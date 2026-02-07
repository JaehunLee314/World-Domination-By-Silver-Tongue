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
            "[Thought Process] Trying a softer approach.\nI-it's not like I want to be your friend or anything! But... if you happened to stop being evil, I wouldn't hate it.",
            "[Thought Process] Appealing to hidden passion.\nThat ramen recipe in your vault? The one with the heart doodles? A true conqueror doesn't need to hide what they love. <evidence_used=ev_ramen>",
            "[Thought Process] Final emotional appeal.\nEveryone deserves a second chance. You've been fighting alone for so long — but you don't have to anymore."
        };

        private readonly List<string> _opponentResponses = new List<string>
        {
            "Silence, mortal! I am the Demon King — I have no need for such sentimental drivel!",
            "You dare bring up my past?! Those... those were merely strategic documents!",
            "Hmph. Your words are meaningless. World domination requires no friends, only power.",
            "That recipe means nothing! It was... research. For poisoning enemies. Obviously.",
            "I... I don't need your pity. But perhaps... a temporary ceasefire wouldn't be the worst idea."
        };

        // Alternating judge results to demonstrate different damage types
        private readonly List<string> _judgeResponses = new List<string>
        {
            "{\"reasoning\": \"The player presented a logical argument backed by evidence from the diary. The opponent was visibly shaken.\", \"damage_type\": \"Normal Hit\", \"damage_dealt\": 20, \"prophet_current_sanity\": 80, \"status\": \"ONGOING\"}",
            "{\"reasoning\": \"The player used the photo album as emotional leverage. Critical strike — the opponent's composure cracked.\", \"damage_type\": \"Critical Hit\", \"damage_dealt\": 50, \"prophet_current_sanity\": 30, \"status\": \"ONGOING\"}",
            "{\"reasoning\": \"The player's argument lacked evidence. Just empty rhetoric.\", \"damage_type\": \"Ineffective\", \"damage_dealt\": 0, \"prophet_current_sanity\": 30, \"status\": \"ONGOING\"}",
            "{\"reasoning\": \"The ramen evidence struck a deep chord. The opponent's defenses are crumbling.\", \"damage_type\": \"Normal Hit\", \"damage_dealt\": 20, \"prophet_current_sanity\": 10, \"status\": \"ONGOING\"}",
            "{\"reasoning\": \"The final appeal broke through. The opponent can no longer maintain their stance.\", \"damage_type\": \"Critical Hit\", \"damage_dealt\": 50, \"prophet_current_sanity\": 0, \"status\": \"PLAYER_WINS\"}"
        };

        private int _playerIndex;
        private int _opponentIndex;
        private int _judgeIndex;

        public async Task<LLMResponse> GenerateResponseAsync(LLMRequest request)
        {
            await Task.Delay(Random.Range(500, 1500));

            bool isPlayerSide = request.SystemPrompt != null && request.SystemPrompt.Contains("skilled debater");
            bool isJudge = request.SystemPrompt != null && request.SystemPrompt.Contains("Game Engine & Referee");

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
