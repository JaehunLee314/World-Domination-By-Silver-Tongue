using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilverTongue.LLM
{
    public class LLMMessage
    {
        public string Role { get; set; }  // "user" or "model"
        public string Content { get; set; }

        public LLMMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    public class LLMRequest
    {
        public string SystemPrompt { get; set; }
        public List<LLMMessage> History { get; set; } = new List<LLMMessage>();
        public string ThinkingEffort { get; set; } = "medium"; // low, medium, high
    }

    public class LLMResponse
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public string ThoughtSummary { get; set; }  // Gemini 3 thought summary (from includeThoughts)
        public string Error { get; set; }
        public string RawResponse { get; set; }
    }

    public interface ILLMService
    {
        Task<LLMResponse> GenerateResponseAsync(LLMRequest request);
    }
}

