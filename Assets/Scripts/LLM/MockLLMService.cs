using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SilverTongue.LLM
{
    public class MockLLMService : ILLMService
    {
        private readonly List<string> _mockResponses = new List<string>
        {
            "[Thought Process] The opponent seems defensive about their past. I should appeal to their emotional side.\nYou know, I've seen your diary. You wrote about wanting friends — that's not weakness, that's courage. <evidence_used=ev_diary>",
            "[Thought Process] Using logical deduction to corner the argument.\nYour own actions contradict your words. You say you don't care, but the photo album tells a different story. <evidence_used=ev_photo>",
            "[Thought Process] Applying tsundere logic to confuse the opponent.\nI-it's not like I want to be your friend or anything! But... if you happened to stop being evil, I wouldn't hate it. <skill_used=sk_tsundere>",
            "[Thought Process] Appealing to hidden passion.\nThat ramen recipe in your vault? The one with the heart doodles? A true conqueror doesn't need to hide what they love. <evidence_used=ev_ramen>",
            "[Thought Process] Final emotional appeal.\nEveryone deserves a second chance. You've been fighting alone for so long — but you don't have to anymore."
        };

        private int _responseIndex;

        public async Task<LLMResponse> GenerateResponseAsync(LLMRequest request)
        {
            // Simulate network delay
            await Task.Delay(UnityEngine.Random.Range(500, 1500));

            string mockContent = _mockResponses[_responseIndex % _mockResponses.Count];
            _responseIndex++;

            Debug.Log($"[MockLLMService] Returning mock response #{_responseIndex}");

            return new LLMResponse
            {
                Success = true,
                Content = mockContent,
                ThoughtSummary = "Mock thinking process - analyzing opponent's weaknesses and selecting persuasion strategy.",
                Error = null,
                RawResponse = $"{{\"mock\": true, \"content\": \"{mockContent}\"}}"
            };
        }
    }
}
