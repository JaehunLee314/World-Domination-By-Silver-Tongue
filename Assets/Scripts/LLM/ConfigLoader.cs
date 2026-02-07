using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SilverTongue.LLM
{
public static class ConfigLoader
{
    private static Dictionary<string, string> _config;
    private static bool _loaded = false;

    public static void Load()
    {
        _config = new Dictionary<string, string>();
        
        // Project root = one level above Application.dataPath (which points to Assets/)
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        string[] possiblePaths = new[]
        {
            Path.Combine(projectRoot, ".env"),
            Path.Combine(Application.dataPath, ".env"),
            Path.Combine(Application.persistentDataPath, ".env")
        };

        bool found = false;
        foreach (var path in possiblePaths)
        {
            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                ParseEnvFile(fullPath);
                Debug.Log($"[ConfigLoader] Loaded config from: {fullPath}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"[ConfigLoader] No .env file found. Searched: {string.Join(", ", possiblePaths)}");
        }

        _loaded = true;
    }

    private static void ParseEnvFile(string path)
    {
        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            
            // Skip comments and empty lines
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            var equalsIndex = trimmed.IndexOf('=');
            if (equalsIndex > 0)
            {
                var key = trimmed.Substring(0, equalsIndex).Trim();
                var value = trimmed.Substring(equalsIndex + 1).Trim();
                
                // Remove quotes if present
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                
                _config[key] = value;
            }
        }
    }

    public static string Get(string key, string defaultValue = "")
    {
        if (!_loaded) Load();
        return _config.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public static string GetGeminiApiKey()
    {
        return Get("GEMINI_API_KEY", "");
    }

    public static string GetGeminiModel()
    {
        return Get("GEMINI_MODEL", "gemini-3-flash-preview");
    }

    public static bool HasGeminiApiKey()
    {
        var key = GetGeminiApiKey();
        return !string.IsNullOrEmpty(key);
    }

    public static string GetLogPath()
    {
        // Allow custom path via config, default to project Logs folder
        var customPath = Get("LOG_PATH", "");
        if (!string.IsNullOrEmpty(customPath))
        {
            return customPath;
        }

        // Default: project root/Logs folder (next to Assets)
        return Path.Combine(Application.dataPath, "..", "Logs");
    }

    public static int GetApiRetryCount()
    {
        var value = Get("API_RETRY_COUNT", "3");
        return int.TryParse(value, out int count) ? count : 3;
    }

    public static float GetApiRetryDelaySeconds()
    {
        var value = Get("API_RETRY_DELAY", "2.0");
        return float.TryParse(value, out float delay) ? delay : 2.0f;
    }
}
}
