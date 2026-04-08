using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class RevenueCatKeysData
{
    public string ios;
    public string android;

    /// <summary>
    /// Picks the SDK key for the active context. In the Editor, <c>UNITY_IOS</c>/<c>UNITY_ANDROID</c> are not
    /// defined the way they are in player builds, so we use the active build target (and fall back to any set key).
    /// </summary>
    public string ApiKeyForCurrentPlatform()
    {
#if UNITY_EDITOR
        BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
        if (bt == BuildTarget.iOS)
            return ios;
        if (bt == BuildTarget.Android)
            return android;
#if UNITY_VISIONOS
        if (bt == BuildTarget.VisionOS)
            return ios;
#endif
        if (!string.IsNullOrEmpty(ios))
            return ios;
        if (!string.IsNullOrEmpty(android))
            return android;
        return null;
#elif UNITY_IOS || UNITY_VISIONOS
        return ios;
#elif UNITY_ANDROID
        return android;
#else
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            return ios;
        if (Application.platform == RuntimePlatform.Android)
            return android;
        return null;
#endif
    }
}
