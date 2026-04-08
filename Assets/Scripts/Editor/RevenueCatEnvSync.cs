#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class RevenueCatEnvSync
{
    private const string StreamingFileName = "revenuecat_keys.json";

    [MenuItem("Revenue Cat/Sync keys from .env")]
    public static void SyncFromEnv()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
        {
            EditorUtility.DisplayDialog("RevenueCat", "Could not resolve project root.", "OK");
            return;
        }

        string envPath = Path.Combine(projectRoot, ".env");
        if (!File.Exists(envPath))
        {
            EditorUtility.DisplayDialog("RevenueCat",
                $"Missing `.env` at:\n{envPath}\n\nCopy `env.example` to `.env` and add your public SDK keys.",
                "OK");
            return;
        }

        string[] lines = File.ReadAllLines(envPath);
        string ios = "";
        string android = "";
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                continue;
            int eq = trimmed.IndexOf('=');
            if (eq <= 0)
                continue;
            string key = trimmed.Substring(0, eq).Trim().TrimStart('\uFEFF');
            string value = Unquote(trimmed.Substring(eq + 1).Trim());
            if (string.Equals(key, "REVENUECAT_IOS_PUBLIC_API_KEY", StringComparison.OrdinalIgnoreCase))
                ios = value;
            else if (string.Equals(key, "REVENUECAT_ANDROID_PUBLIC_API_KEY", StringComparison.OrdinalIgnoreCase))
                android = value;
        }

        string streamingDir = Path.Combine(Application.dataPath, "StreamingAssets");
        Directory.CreateDirectory(streamingDir);
        string outPath = Path.Combine(streamingDir, StreamingFileName);

        var data = new RevenueCatKeysData { ios = ios, android = android };
        File.WriteAllText(outPath, JsonUtility.ToJson(data, true), Encoding.UTF8);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("RevenueCat", $"Wrote keys to:\nAssets/StreamingAssets/{StreamingFileName}", "OK");
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[value.Length - 1] == '"') ||
             (value[0] == '\'' && value[value.Length - 1] == '\'')))
            return value.Substring(1, value.Length - 2);
        return value;
    }
}
#endif
