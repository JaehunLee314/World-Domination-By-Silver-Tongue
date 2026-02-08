using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SilverTongue.LLM
{
    public class MockLLMService : ILLMService
    {
        private readonly List<string> _playerResponses = new List<string>
        {
            "[Thought Process] Appeal to their emotional side.\nI've seen your diary. You wrote about wanting friends — that's not weakness, that's courage. <evidence_used=ev_diary>",
            "[Thought Process] Using logical deduction.\nYour own actions contradict your words. You say you don't care, but the photo album tells a different story. <evidence_used=ev_photo>",
            "[Thought Process] Trying a softer approach.\n<inner_thought>Maybe a gentler approach will work better here.</inner_thought>I-it's not like I want to be your friend or anything! But... if you happened to stop being evil, I wouldn't hate it.",
            "[Thought Process] Appealing to hidden passion.\nThat ramen recipe in your vault? The one with the heart doodles? A true conqueror doesn't need to hide what they love. <evidence_used=ev_ramen>",
            "[Thought Process] Final emotional appeal.\nEveryone deserves a second chance. You've been fighting alone for so long — but you don't have to anymore."
        };

        private readonly List<string> _opponentResponses = new List<string>
        {
            "Silence, mortal! I am the Demon King — I have no need for such sentimental drivel!",
            "You dare bring up my past?! Those... those were merely strategic documents!",
            "Hmph. Your words are meaningless. World domination requires no friends, only power.",
            "That recipe means nothing! It was... research. For poisoning enemies. Obviously.",
            "<inner_thought>They're right... I am tired of being alone.</inner_thought>I... I don't need your pity. But perhaps... a temporary ceasefire wouldn't be the worst idea. <accept_defeat/>"
        };

        // Judge responses in new format: per-condition evaluation + damage
        private readonly List<string> _judgeResponses = new List<string>
        {
            "{\"player_conditions\": [{\"index\": 0, \"is_met\": false, \"reasoning\": \"Player has not triggered any lose condition.\"}], \"opponent_conditions\": [{\"index\": 0, \"is_met\": false, \"reasoning\": \"Not yet persuaded.\"}, {\"index\": 1, \"is_met\": false, \"reasoning\": \"Still defensive.\"}], \"damage\": 20, \"reasoning\": \"The diary evidence shook the opponent. Decent progress.\"}",
            "{\"player_conditions\": [{\"index\": 0, \"is_met\": false, \"reasoning\": \"Player remains focused.\"}], \"opponent_conditions\": [{\"index\": 0, \"is_met\": true, \"reasoning\": \"The photo album broke through emotional defenses.\"}, {\"index\": 1, \"is_met\": false, \"reasoning\": \"Not fully conceded.\"}], \"damage\": 40, \"reasoning\": \"Strong emotional leverage. First condition met.\"}",
            "{\"player_conditions\": [{\"index\": 0, \"is_met\": false, \"reasoning\": \"Player stayed on track.\"}], \"opponent_conditions\": [{\"index\": 0, \"is_met\": true, \"reasoning\": \"Previously met.\"}, {\"index\": 1, \"is_met\": false, \"reasoning\": \"Argument lacked evidence.\"}], \"damage\": 0, \"reasoning\": \"Ineffective turn. No evidence used.\"}",
            "{\"player_conditions\": [{\"index\": 0, \"is_met\": false, \"reasoning\": \"No violation.\"}], \"opponent_conditions\": [{\"index\": 0, \"is_met\": true, \"reasoning\": \"Previously met.\"}, {\"index\": 1, \"is_met\": true, \"reasoning\": \"The ramen evidence struck deep. Opponent's passion exposed.\"}], \"damage\": 30, \"reasoning\": \"Ramen evidence was compelling. All opponent conditions now met.\"}",
            "{\"player_conditions\": [{\"index\": 0, \"is_met\": false, \"reasoning\": \"Player held strong.\"}], \"opponent_conditions\": [{\"index\": 0, \"is_met\": true, \"reasoning\": \"Previously met.\"}, {\"index\": 1, \"is_met\": true, \"reasoning\": \"Previously met.\"}], \"damage\": 50, \"reasoning\": \"Final appeal broke through completely.\"}"
        };

        private int _playerIndex;
        private int _opponentIndex;
        private int _judgeIndex;

        public async Task<LLMResponse> GenerateResponseAsync(LLMRequest request)
        {
            await Task.Delay(Random.Range(500, 1500));

            bool isPlayerSide = request.SystemPrompt != null && request.SystemPrompt.Contains("skilled debater");
            bool isJudge = request.SystemPrompt != null && request.SystemPrompt.Contains("impartial Judge");

            string mockContent;
            if (isJudge)
            {
                mockContent = _judgeResponses[_judgeIndex % _judgeResponses.Count];
                _judgeIndex++;
            }
            else if (isPlayerSide)
            {
                mockContent = _playerResponses[_playerIndex % _playerResponses.Count];
                _playerIndex++;
            }
            else
            {
                mockContent = _opponentResponses[_opponentIndex % _opponentResponses.Count];
                _opponentIndex++;
            }

            Debug.Log($"[MockLLMService] Returning mock response ({(isJudge ? "judge" : isPlayerSide ? "player" : "opponent")})");

            return new LLMResponse
            {
                Success = true,
                Content = mockContent,
                ThoughtSummary = "Mock thinking process.",
                Error = null,
                RawResponse = "{\"mock\": true}"
            };
        }
    }
}
