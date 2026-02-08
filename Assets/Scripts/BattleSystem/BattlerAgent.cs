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
            sb.AppendLine(player.lore);
            sb.AppendLine();

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

            if (!string.IsNullOrWhiteSpace(strategy.StrategyText))
            {
                sb.AppendLine("[PLAYER STRATEGY]:");
                sb.AppendLine(strategy.StrategyText);
                sb.AppendLine();
            }

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
            sb.AppendLine("- You may include <accept_defeat/> if you choose to concede.");
            sb.AppendLine("- You may use <inner_thought>...</inner_thought> for private reasoning.");
            sb.AppendLine("- Keep responses to 1-2 sentences of dialogue only. No narration.");
            if (player.loseConditions != null && player.loseConditions.Length > 0)
            {
                sb.AppendLine("- AVOID doing any of these (your lose conditions):");
                foreach (var cond in player.loseConditions)
                    sb.AppendLine($"  - \"{cond}\"");
            }

            return sb.ToString();
        }

        public static string BuildOpponentPrompt(CharacterSO player, CharacterSO opponent)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"You are {opponent.characterName}.");
            sb.AppendLine(opponent.lore);
            sb.AppendLine();
            sb.AppendLine("RULES:");
            sb.AppendLine($"- You are being debated by {player.characterName}.");
            sb.AppendLine("- Stay in character. Defend your position.");
            sb.AppendLine("- React naturally to persuasive arguments and evidence presented.");
            sb.AppendLine("- You may include <accept_defeat/> if you choose to concede.");
            sb.AppendLine("- You may use <inner_thought>...</inner_thought> for private reasoning.");
            sb.AppendLine("- Keep responses to 1-2 sentences of dialogue only. No narration.");
            if (opponent.loseConditions != null && opponent.loseConditions.Length > 0)
            {
                sb.AppendLine("- You lose if you do all of these:");
                foreach (var cond in opponent.loseConditions)
                    sb.AppendLine($"  - \"{cond}\"");
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

        /// <summary>
        /// Strips all recognized XML tags from dialogue text.
        /// Returns clean display text; extracted tag data is returned via out parameter.
        /// </summary>
        public static string StripXmlTags(string raw, out ParsedTags tags)
        {
            tags = new ParsedTags();
            if (string.IsNullOrEmpty(raw)) return raw ?? "";

            tags.AcceptDefeat = Regex.IsMatch(raw, @"<accept_defeat\s*/>");

            var evMatch = Regex.Match(raw, @"<evidence_used=([^>]+)>");
            tags.EvidenceUsed = evMatch.Success ? evMatch.Groups[1].Value : null;

            var thoughtMatch = Regex.Match(raw, @"<inner_thought>(.*?)</inner_thought>", RegexOptions.Singleline);
            tags.InnerThought = thoughtMatch.Success ? thoughtMatch.Groups[1].Value.Trim() : null;

            string clean = raw;
            clean = Regex.Replace(clean, @"<accept_defeat\s*/>", "");
            clean = Regex.Replace(clean, @"<evidence_used=[^>]+>", "");
            clean = Regex.Replace(clean, @"<inner_thought>.*?</inner_thought>", "", RegexOptions.Singleline);
            return clean.Trim();
        }
    }
}
