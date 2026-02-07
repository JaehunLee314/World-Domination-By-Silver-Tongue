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
            "You are the Game Engine & Referee. " +
            "Analyze the latest exchange in this debate, calculate Sanity Damage, and check Victory/Defeat Conditions. " +
            "Output ONLY valid JSON.";

        public async Task<JudgeResult> Evaluate(
            ILLMService llm,
            IReadOnlyList<ConversationEntry> history,
            CharacterSO opponent,
            int currentSanity,
            int maxSanity)
        {
            string judgeMessage = BuildJudgePrompt(history, opponent, currentSanity, maxSanity);

            var request = new LLMRequest
            {
                SystemPrompt = JudgeSystemPrompt,
                ThinkingEffort = "high"
            };
            request.History.Add(new LLMMessage("user", judgeMessage));

            var response = await llm.GenerateResponseAsync(request);

            if (response.Success)
                return ParseJudgeResult(response.Content, currentSanity);

            return new JudgeResult
            {
                reasoning = "Judge evaluation failed.",
                damage_type = "Ineffective",
                damage_dealt = 0,
                prophet_current_sanity = currentSanity,
                status = BattleStatus.Ongoing
            };
        }

        // ─── Static Helpers ──────────────────────────────────────────────

        public static string BuildJudgePrompt(
            IReadOnlyList<ConversationEntry> history,
            CharacterSO opponent,
            int currentSanity,
            int maxSanity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[DIALOGUE HISTORY]");
            foreach (var entry in history)
            {
                if (entry.SpeakerName == "JUDGE") continue;
                sb.AppendLine($"[{entry.SpeakerName}]: {entry.SpeechText}");
            }

            sb.AppendLine();
            sb.AppendLine($"[CURRENT OPPONENT]: {opponent.characterName}");
            sb.AppendLine($"[OPPONENT CURRENT SANITY]: {currentSanity}/{maxSanity}");
            sb.AppendLine();

            sb.AppendLine("[LOSE CONDITIONS]:");
            if (opponent.loseConditions != null)
            {
                foreach (var cond in opponent.loseConditions)
                    sb.AppendLine($"- {cond}");
            }

            sb.AppendLine();
            sb.AppendLine("EVALUATION RULES:");
            sb.AppendLine("1. LOGIC CHECK: Does the argument + Evidence directly satisfy a lose condition?");
            sb.AppendLine("   - IF YES: Higher damage.");
            sb.AppendLine("2. RISK CHECK: Did the opponent successfully trap the player (Hypocrisy/Worthlessness)?");
            sb.AppendLine("   - IF YES: Heal opponent sanity instead.");
            sb.AppendLine();
            sb.AppendLine("SCORING:");
            sb.AppendLine("- Ineffective (0): Insults without logic.");
            sb.AppendLine("- Normal Hit (-20): Logical argument.");
            sb.AppendLine("- Critical Hit (-50): Emotional/evidence-backed strike.");
            sb.AppendLine("- Trap Trigger (+20 Heal to Enemy): Player falls for a trap.");
            sb.AppendLine();
            sb.AppendLine("OUTPUT JSON ONLY (no markdown, no code fences):");
            sb.AppendLine("{");
            sb.AppendLine("  \"reasoning\": \"Step-by-step analysis.\",");
            sb.AppendLine("  \"damage_type\": \"Ineffective | Normal Hit | Critical Hit | Trap Trigger\",");
            sb.AppendLine("  \"damage_dealt\": <integer>,");
            sb.AppendLine($"  \"prophet_current_sanity\": <calculated from {currentSanity}>,");
            sb.AppendLine("  \"status\": \"ONGOING | PLAYER_WINS | OPPONENT_WINS\"");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static JudgeResult ParseJudgeResult(string raw, int currentSanity)
        {
            raw = raw.Trim();
            if (raw.StartsWith("```"))
            {
                raw = Regex.Replace(raw, @"^```\w*\n?", "");
                raw = Regex.Replace(raw, @"\n?```$", "");
                raw = raw.Trim();
            }

            try
            {
                return JsonUtility.FromJson<JudgeResult>(raw);
            }
            catch
            {
                Debug.LogWarning($"[JudgeAgent] Failed to parse judge JSON: {raw}");

                int damage = 0;
                var dmgMatch = Regex.Match(raw, @"""damage_dealt""\s*:\s*(-?\d+)");
                if (dmgMatch.Success) int.TryParse(dmgMatch.Groups[1].Value, out damage);

                string status = BattleStatus.Ongoing;
                if (raw.Contains(BattleStatus.PlayerWins)) status = BattleStatus.PlayerWins;
                else if (raw.Contains(BattleStatus.OpponentWins)) status = BattleStatus.OpponentWins;

                string damageType = "Ineffective";
                if (raw.Contains("Critical Hit")) damageType = "Critical Hit";
                else if (raw.Contains("Normal Hit")) damageType = "Normal Hit";
                else if (raw.Contains("Trap Trigger")) damageType = "Trap Trigger";

                return new JudgeResult
                {
                    reasoning = "Parsed from partial response.",
                    damage_type = damageType,
                    damage_dealt = damage,
                    prophet_current_sanity = currentSanity - damage,
                    status = status
                };
            }
        }

        public static bool CheckLoseConditions(string[] conditions, string dialogue)
        {
            if (conditions == null) return false;
            foreach (var cond in conditions)
            {
                if (string.IsNullOrWhiteSpace(cond)) continue;
                if (Regex.IsMatch(dialogue, Regex.Escape(cond), RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
