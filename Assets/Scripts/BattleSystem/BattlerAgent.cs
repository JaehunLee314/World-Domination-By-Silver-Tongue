using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SilverTongue.Data;
using SilverTongue.LLM;

namespace SilverTongue.BattleSystem
{
    public class BattlerAgent
    {
        public CharacterSO Character { get; }
        public List<LLMMessage> History { get; } = new List<LLMMessage>();
        public string SystemPrompt { get; private set; }
        public string ThinkingEffort { get; }

        public BattlerAgent(CharacterSO character, string systemPrompt, string thinkingEffort)
        {
            Character = character;
            SystemPrompt = systemPrompt;
            ThinkingEffort = thinkingEffort;
        }

        public void UpdateSystemPrompt(string systemPrompt)
        {
            SystemPrompt = systemPrompt;
        }

        public void AddToHistory(string role, string content)
        {
            History.Add(new LLMMessage(role, content));
        }

        public async Task<LLMResponse> GenerateResponse(ILLMService llm)
        {
            var request = new LLMRequest
            {
                SystemPrompt = SystemPrompt,
                ThinkingEffort = ThinkingEffort,
                History = new List<LLMMessage>(History)
            };
            return await llm.GenerateResponseAsync(request);
        }

        // ─── Static Prompt Builders ──────────────────────────────────────

        public static string BuildPlayerPrompt(CharacterSO player, CharacterSO opponent, StrategyEntry strategy)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"You are {player.characterName}, a skilled debater.");
            sb.AppendLine(player.systemPromptLore);
            sb.AppendLine($"Your speaking style: {player.voiceTone}");
            sb.AppendLine();

            // Character skills
            if (player.skills != null && player.skills.Length > 0)
            {
                sb.AppendLine("YOUR SKILLS:");
                foreach (var skill in player.skills)
                {
                    sb.AppendLine($"- {skill.skillName}: {skill.description}");
                    sb.AppendLine($"  [Technique: {skill.promptModifier}]");
                }
                sb.AppendLine();
            }

            // Strategy from strategy phase
            if (!string.IsNullOrWhiteSpace(strategy.StrategyText))
            {
                sb.AppendLine("[PLAYER STRATEGY]:");
                sb.AppendLine(strategy.StrategyText);
                sb.AppendLine();
            }

            // Evidence items — inject factInjection if available, fallback to description
            if (strategy.Items != null && strategy.Items.Count > 0)
            {
                sb.AppendLine("[EQUIPPED ITEMS]:");
                foreach (var item in strategy.Items)
                {
                    string injection = !string.IsNullOrWhiteSpace(item.factInjection)
                        ? item.factInjection
                        : $"[ITEM: {item.itemName}] {item.description}";
                    sb.AppendLine(injection);
                }
                sb.AppendLine();
            }

            sb.AppendLine("RULES:");
            sb.AppendLine($"- You are debating against {opponent.characterName} to persuade them.");
            sb.AppendLine("- STRICTLY follow the [PLAYER STRATEGY].");
            sb.AppendLine("- Use [EQUIPPED ITEMS] to prove your point.");
            sb.AppendLine("- When using evidence, include the tag: <evidence_used=ID>");
            sb.AppendLine("- Keep responses to 1-2 sentences of dialogue only. No narration.");
            if (player.loseConditions != null && player.loseConditions.Length > 0)
            {
                sb.Append("- NEVER do any of these (instant loss):");
                foreach (var cond in player.loseConditions)
                    sb.Append($" \"{cond}\",");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string BuildOpponentPrompt(CharacterSO player, CharacterSO opponent)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"You are {opponent.characterName}.");
            sb.AppendLine(opponent.systemPromptLore);
            sb.AppendLine($"Your speaking style: {opponent.voiceTone}");
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine($"- You are being debated by {player.characterName}.");
            sb.AppendLine("- Stay in character. Defend your position.");
            sb.AppendLine("- React naturally to persuasive arguments and evidence presented.");
            sb.AppendLine("- Keep responses to 1-2 sentences of dialogue only. No narration.");
            if (opponent.loseConditions != null && opponent.loseConditions.Length > 0)
            {
                sb.Append("- You lose if you do any of these:");
                foreach (var cond in opponent.loseConditions)
                    sb.Append($" \"{cond}\",");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GetThinkingEffort(CharacterSO character)
        {
            var max = character.thinkingEffort;
            if (character.skills != null)
            {
                foreach (var skill in character.skills)
                    if (skill.thinkingEffort > max)
                        max = skill.thinkingEffort;
            }
            return max.ToString().ToLower();
        }

        // ─── Static Utilities ────────────────────────────────────────────

        public static string CleanDialogue(string raw)
        {
            var match = Regex.Match(raw, @"\[Thought Process\].*?\n(.+)", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
        }

        public static string ParseTag(string text, string tagName)
        {
            var match = Regex.Match(text, $"<{tagName}=([^>]+)>");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
