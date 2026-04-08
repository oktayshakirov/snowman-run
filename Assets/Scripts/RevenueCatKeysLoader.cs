using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class RevenueCatKeysLoader
{
    private const string FileName = "revenuecat_keys.json";

    public static RevenueCatKeysData LoadFromStreamingAssetsSync()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);
            if (!File.Exists(path))
                return null;
            string json = File.ReadAllText(path);
            return ParseJson(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RevenueCat] Could not read {FileName}: {e.Message}");
            return null;
        }
    }

    /// <summary>Android: StreamingAssets live inside a jar; use UnityWebRequest.</summary>
    public static IEnumerator LoadFromStreamingAssets(Action<RevenueCatKeysData> onComplete)
    {
        string path = Path.Combine(Application.streamingAssetsPath, FileName);
        using (UnityWebRequest req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogWarning($"[RevenueCat] Could not load {FileName}: {req.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            onComplete?.Invoke(ParseJson(req.downloadHandler.text));
        }
    }

    private static RevenueCatKeysData ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonUtility.FromJson<RevenueCatKeysData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[RevenueCat] Invalid JSON in {FileName}: {e.Message}");
            return null;
        }
    }
}
