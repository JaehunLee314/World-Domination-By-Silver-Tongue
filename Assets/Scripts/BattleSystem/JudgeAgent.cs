using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using SilverTongue.Data;
using SilverTongue.LLM;

namespace SilverTongue.BattleSystem
{
    public class JudgeAgent
    {
        private const string JudgeSystemPrompt =
            "You are the impartial Judge in a persuasion battle. " +
            "Analyze the conversation, determine whether specific lose conditions have been met, " +
            "and assign sanity damage based on argument quality. " +
            "Output ONLY valid JSON.";

        public async Task<JudgeResult> Evaluate(
            ILLMService llm,
            IReadOnlyList<ConversationEntry> history,
            ConditionStatus[] playerUnmetConditions,
            ConditionStatus[] opponentUnmetConditions,
            string playerName,
            string opponentName,
            int currentSanity,
            int maxSanity)
        {
            string judgeMessage = BuildJudgePrompt(
                history, playerUnmetConditions, opponentUnmetConditions,
                playerName, opponentName, currentSanity, maxSanity);

            var request = new LLMRequest
            {
                SystemPrompt = JudgeSystemPrompt,
                ThinkingEffort = "high"
            };
            request.History.Add(new LLMMessage("user", judgeMessage));

            var response = await llm.GenerateResponseAsync(request);

            if (response.Success)
                return ParseJudgeResult(response.Content, playerUnmetConditions, opponentUnmetConditions);

            return new JudgeResult
            {
                playerEval = new JudgeEvaluation { conditions = System.Array.Empty<ConditionStatus>() },
                opponentEval = new JudgeEvaluation { conditions = System.Array.Empty<ConditionStatus>() },
                damage = 0,
                reasoning = "Judge evaluation failed."
            };
        }

        // ─── Prompt Builder ─────────────────────────────────────────────

        public static string BuildJudgePrompt(
            IReadOnlyList<ConversationEntry> history,
            ConditionStatus[] playerUnmetConditions,
            ConditionStatus[] opponentUnmetConditions,
            string playerName,
            string opponentName,
            int currentSanity,
            int maxSanity)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[DIALOGUE HISTORY]");
            foreach (var entry in history)
            {
                string text = !string.IsNullOrEmpty(entry.RawText) ? entry.RawText : entry.SpeechText;
                sb.AppendLine($"[{entry.SpeakerName}]: {text}");
            }
            sb.AppendLine();

            sb.AppendLine($"[PLAYER: {playerName}]");
            sb.AppendLine("[PLAYER UNMET LOSE CONDITIONS]:");
            if (playerUnmetConditions != null && playerUnmetConditions.Length > 0)
            {
                for (int i = 0; i < playerUnmetConditions.Length; i++)
                    sb.AppendLine($"  {i}: \"{playerUnmetConditions[i].Condition}\"");
            }
            else
            {
                sb.AppendLine("  (none remaining)");
            }
            sb.AppendLine();

            sb.AppendLine($"[OPPONENT: {opponentName}]");
            sb.AppendLine("[OPPONENT UNMET LOSE CONDITIONS]:");
            if (opponentUnmetConditions != null && opponentUnmetConditions.Length > 0)
            {
                for (int i = 0; i < opponentUnmetConditions.Length; i++)
                    sb.AppendLine($"  {i}: \"{opponentUnmetConditions[i].Condition}\"");
            }
            else
            {
                sb.AppendLine("  (none remaining)");
            }
            sb.AppendLine();

            sb.AppendLine($"[OPPONENT SANITY]: {currentSanity}/{maxSanity}");
            sb.AppendLine();

            sb.AppendLine("EVALUATION RULES:");
            sb.AppendLine("- For each unmet condition, analyze the FULL conversation to determine if it has been met.");
            sb.AppendLine("- A condition is met when the character clearly demonstrates the behavior/concession described.");
            sb.AppendLine("- Be strict: vague hints or partial fulfillment do NOT count.");
            sb.AppendLine("- Only evaluate conditions listed above. Do not invent new ones.");
            sb.AppendLine();

            sb.AppendLine("DAMAGE GUIDELINES (to opponent sanity):");
            sb.AppendLine("- 0: Arguments were ineffective or player fell into a trap.");
            sb.AppendLine("- 10-20: Decent argument, some progress.");
            sb.AppendLine("- 30-40: Strong argument backed by evidence or newly met condition.");
            sb.AppendLine("- 50: Devastating argument, critical breakthrough.");
            sb.AppendLine();

            sb.AppendLine("OUTPUT JSON ONLY (no markdown, no code fences):");
            sb.AppendLine("{");
            sb.AppendLine("  \"player_conditions\": [");
            sb.AppendLine("    { \"index\": 0, \"is_met\": true/false, \"reasoning\": \"...\" },");
            sb.AppendLine("    ...");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"opponent_conditions\": [");
            sb.AppendLine("    { \"index\": 0, \"is_met\": true/false, \"reasoning\": \"...\" },");
            sb.AppendLine("    ...");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"damage\": <integer 0-50>,");
            sb.AppendLine("  \"reasoning\": \"Overall assessment of this exchange.\"");
            sb.AppendLine("}");

            return sb.ToString();
        }

        // ─── Parser ──────────────────────────────────────────────────────

        public static JudgeResult ParseJudgeResult(
            string raw,
            ConditionStatus[] playerUnmetConditions,
            ConditionStatus[] opponentUnmetConditions)
        {
            raw = raw.Trim();
            if (raw.StartsWith("```"))
            {
                raw = Regex.Replace(raw, @"^```\w*\n?", "");
                raw = Regex.Replace(raw, @"\n?```$", "");
                raw = raw.Trim();
            }

            var result = new JudgeResult
            {
                playerEval = new JudgeEvaluation
                {
                    conditions = new ConditionStatus[playerUnmetConditions?.Length ?? 0]
                },
                opponentEval = new JudgeEvaluation
                {
                    conditions = new ConditionStatus[opponentUnmetConditions?.Length ?? 0]
                },
                damage = 0,
                reasoning = ""
            };

            try
            {
                var dmgMatch = Regex.Match(raw, @"""damage""\s*:\s*(-?\d+)");
                if (dmgMatch.Success) int.TryParse(dmgMatch.Groups[1].Value, out result.damage);

                var reasonMatch = Regex.Match(raw, @"""reasoning""\s*:\s*""((?:[^""\\]|\\.)*)""");
                if (reasonMatch.Success) result.reasoning = reasonMatch.Groups[1].Value;

                ParseConditionArray(raw, "player_conditions", playerUnmetConditions, result.playerEval.conditions);
                ParseConditionArray(raw, "opponent_conditions", opponentUnmetConditions, result.opponentEval.conditions);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[JudgeAgent] Failed to parse judge JSON: {e.Message}\n{raw}");
            }

            return result;
        }

        private static void ParseConditionArray(
            string raw, string arrayName,
            ConditionStatus[] unmetConditions,
            ConditionStatus[] output)
        {
            if (unmetConditions == null || output == null) return;

            var arrayMatch = Regex.Match(raw,
                $@"""{arrayName}""\s*:\s*\[(.*?)\]",
                RegexOptions.Singleline);
            if (!arrayMatch.Success) return;

            string arrayContent = arrayMatch.Groups[1].Value;

            var entryMatches = Regex.Matches(arrayContent,
                @"\{\s*""index""\s*:\s*(\d+)\s*,\s*""is_met""\s*:\s*(true|false)\s*,\s*""reasoning""\s*:\s*""((?:[^""\\]|\\.)*)""[^}]*\}",
                RegexOptions.Singleline);

            foreach (Match m in entryMatches)
            {
                if (!int.TryParse(m.Groups[1].Value, out int index)) continue;
                if (index < 0 || index >= output.Length) continue;

                bool isMet = m.Groups[2].Value == "true";
                string reasoning = m.Groups[3].Value;

                output[index] = new ConditionStatus
                {
                    Condition = unmetConditions[index].Condition,
                    IsMet = isMet,
                    Reasoning = reasoning,
                    MetOnTurn = 0
                };
            }
        }
    }
}
