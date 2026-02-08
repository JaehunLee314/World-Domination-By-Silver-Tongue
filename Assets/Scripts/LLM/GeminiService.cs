using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SilverTongue.LLM
{
public class GeminiService : ILLMService
{
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly int _maxRetries;
    private readonly float _retryDelaySeconds;
    private const string BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/";

    // Event for API status updates (can be subscribed to by UI)
    public static event Action<string> OnApiStatusChanged;
    public static event Action<string> OnApiError;

    public GeminiService(string apiKey, string modelName = "gemini-3-flash-preview")
    {
        _apiKey = apiKey;
        _modelName = modelName;
        _maxRetries = ConfigLoader.GetApiRetryCount();
        _retryDelaySeconds = ConfigLoader.GetApiRetryDelaySeconds();
    }

    public async Task<LLMResponse> GenerateResponseAsync(LLMRequest request)
    {
        var response = new LLMResponse();
        string url = $"{BASE_URL}{_modelName}:generateContent";
        string jsonPayload = BuildPayload(request);

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    float delay = _retryDelaySeconds * (float)Math.Pow(2, attempt - 1); // Exponential backoff
                    string retryMsg = $"Retry {attempt}/{_maxRetries} in {delay:F1}s...";
                    Debug.LogWarning($"[GeminiService] {retryMsg}");
                    OnApiStatusChanged?.Invoke(retryMsg);
                    await Task.Delay((int)(delay * 1000));
                }

                OnApiStatusChanged?.Invoke($"Calling API (attempt {attempt + 1})...");
                Debug.Log($"[GeminiService] Sending request to {_modelName} (attempt {attempt + 1})");
                Debug.Log($"[GeminiService] URL: {url}");
                Debug.Log($"[GeminiService] Request payload:\n{jsonPayload}");

                using var webRequest = new UnityWebRequest(url, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("x-goog-api-key", _apiKey);
                webRequest.timeout = 60;

                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    await Awaitable.NextFrameAsync();
                }

                response.RawResponse = webRequest.downloadHandler.text;
                long responseCode = webRequest.responseCode;

                Debug.Log($"[GeminiService] Response code: {responseCode}");
                Debug.Log($"[GeminiService] Raw response:\n{response.RawResponse}");

                // Check for retryable errors (503, 429, 500)
                if (responseCode == 503 || responseCode == 429 || responseCode == 500)
                {
                    string errorMsg = GetErrorMessage(responseCode);
                    Debug.LogWarning($"[GeminiService] {errorMsg} (attempt {attempt + 1}/{_maxRetries + 1})");
                    OnApiError?.Invoke(errorMsg);

                    if (attempt < _maxRetries)
                    {
                        continue; // Retry
                    }

                    // Max retries exceeded
                    response.Success = false;
                    response.Error = $"{errorMsg} (max retries exceeded)";
                    OnApiError?.Invoke(response.Error);
                    return response;
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    response.Success = false;
                    response.Error = $"{webRequest.error}: {webRequest.downloadHandler.text}";
                    Debug.LogError($"[GeminiService] Error: {response.Error}");
                    OnApiError?.Invoke(response.Error);
                    return response;
                }

                // Success!
                var (content, thoughtSummary) = ParseResponse(webRequest.downloadHandler.text);
                response.Content = content;
                response.ThoughtSummary = thoughtSummary;
                response.Success = true;
                OnApiStatusChanged?.Invoke("API OK");
                Debug.Log($"[GeminiService] Parsed content ({content.Length} chars):\n{content}");
                Debug.Log($"[GeminiService] Parsed thought ({thoughtSummary?.Length ?? 0} chars):\n{thoughtSummary}");
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Error = ex.Message;
                Debug.LogError($"[GeminiService] Exception: {ex.Message}\n{ex.StackTrace}");
                OnApiError?.Invoke($"Exception: {ex.Message}");

                if (attempt >= _maxRetries)
                {
                    return response;
                }
            }
        }

        return response;
    }

    private string GetErrorMessage(long responseCode)
    {
        return responseCode switch
        {
            503 => "API Overloaded (503)",
            429 => "Rate Limited (429)",
            500 => "Server Error (500)",
            _ => $"HTTP Error ({responseCode})"
        };
    }

    private string BuildPayload(LLMRequest request)
    {
        var sb = new StringBuilder();
        sb.Append("{");

        // System instruction
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            sb.Append("\"system_instruction\":{\"parts\":[{\"text\":");
            sb.Append(JsonEscape(request.SystemPrompt));
            sb.Append("}]},");
        }

        // Contents (conversation history)
        sb.Append("\"contents\":[");
        for (int i = 0; i < request.History.Count; i++)
        {
            var msg = request.History[i];
            if (i > 0) sb.Append(",");
            sb.Append("{\"role\":\"");
            sb.Append(msg.Role);
            sb.Append("\",\"parts\":[{\"text\":");
            sb.Append(JsonEscape(msg.Content));
            sb.Append("}]}");
        }
        sb.Append("],");

        // Generation config for Gemini 3
        // Per docs: keep temperature at 1.0 (changing it may cause unexpected behavior)
        sb.Append("\"generationConfig\":{");
        sb.Append("\"temperature\":1.0,");
        sb.Append("\"maxOutputTokens\":2048,");  // Increased: thinking tokens count toward limit

        // Thinking config for Gemini 3 Flash
        // Options: minimal (fastest), low, medium, high (slowest)
        string thinkingLevel = request.ThinkingEffort?.ToLower() switch
        {
            "minimal" => "minimal",
            "low" => "low",
            "medium" => "medium",
            "high" => "high",
            _ => "low"
        };
        sb.Append("\"thinkingConfig\":{");
        sb.Append($"\"thinkingLevel\":\"{thinkingLevel}\",");
        sb.Append("\"includeThoughts\":true");  // Get thought summaries
        sb.Append("}");

        sb.Append("}");

        sb.Append("}");
        return sb.ToString();
    }

    private string JsonEscape(string text)
    {
        if (string.IsNullOrEmpty(text)) return "\"\"";

        var sb = new StringBuilder("\"");
        foreach (char c in text)
        {
            switch (c)
            {
                case '\"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default: sb.Append(c); break;
            }
        }
        sb.Append("\"");
        return sb.ToString();
    }

    private (string content, string thoughtSummary) ParseResponse(string json)
    {
        // Parse Gemini response - looking for content and thought summary
        // Structure: candidates[0].content.parts[] where:
        //   - parts with "thought": true contain thinking process
        //   - parts without "thought": true contain actual response
        string content = "";
        string thoughtSummary = "";

        try
        {
            // Find the candidates array
            int candidatesStart = json.IndexOf("\"candidates\"");
            if (candidatesStart < 0) return ("", "");

            // Find the parts array
            int partsStart = json.IndexOf("\"parts\"", candidatesStart);
            if (partsStart < 0) return ("", "");

            // Parse through parts array
            int searchPos = partsStart;
            while (true)
            {
                // Find next part object
                int partStart = json.IndexOf("{", searchPos);
                if (partStart < 0 || partStart > json.IndexOf("]", partsStart)) break;

                // Find the end of this part object
                int partEnd = FindClosingBrace(json, partStart);
                if (partEnd < 0) break;

                string partJson = json.Substring(partStart, partEnd - partStart + 1);

                // Check if this part has "thought": true
                bool isThought = partJson.Contains("\"thought\"") && 
                                (partJson.Contains("\"thought\": true") || partJson.Contains("\"thought\":true"));

                // Extract text from this part
                int textStart = partJson.IndexOf("\"text\"");
                if (textStart >= 0)
                {
                    string text = ExtractJsonStringFromSubstring(partJson, textStart);
                    
                    if (isThought)
                    {
                        thoughtSummary = text;
                    }
                    else
                    {
                        content = text;
                    }
                }

                searchPos = partEnd + 1;
            }

            return (content, thoughtSummary);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[GeminiService] Parse error: {ex.Message}");
            return (json, "");
        }
    }

    private int FindClosingBrace(string json, int openBracePos)
    {
        int depth = 0;
        bool inString = false;
        bool escaped = false;

        for (int i = openBracePos; i < json.Length; i++)
        {
            char c = json[i];

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString) continue;

            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }

        return -1;
    }

    private string ExtractJsonStringFromSubstring(string json, int fieldStart)
    {
        int colonPos = json.IndexOf(":", fieldStart);
        if (colonPos < 0) return "";

        int quoteStart = json.IndexOf("\"", colonPos + 1);
        if (quoteStart < 0) return "";

        var sb = new StringBuilder();
        int i = quoteStart + 1;
        while (i < json.Length)
        {
            if (json[i] == '\\' && i + 1 < json.Length)
            {
                char next = json[i + 1];
                switch (next)
                {
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    case 'u':
                        if (i + 5 < json.Length)
                        {
                            string hex = json.Substring(i + 2, 4);
                            if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int unicode))
                            {
                                sb.Append((char)unicode);
                                i += 4;
                            }
                        }
                        break;
                    default: sb.Append(next); break;
                }
                i += 2;
            }
            else if (json[i] == '"')
            {
                break;
            }
            else
            {
                sb.Append(json[i]);
                i++;
            }
        }
        return sb.ToString();
    }

}
}

