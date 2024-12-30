using UnityEngine;
using System.Runtime.InteropServices;

public static class NativeHaptics
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void TriggerHapticFeedback(int type);
#endif

    public static void TriggerLightHaptic()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(0); 
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    public static void TriggerMediumHaptic()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(1); 
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    public static void TriggerHeavyHaptic()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(2); 
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    public static void TriggerSelectionHaptic()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(3);
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    public static void TriggerSuccessNotification()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(4); 
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    public static void TriggerWarningNotification()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(5); 
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    public static void TriggerErrorNotification()
    {
        if (!IsVibrationEnabled()) return;

#if UNITY_IOS && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(6); 
        }
#endif

        if (Application.platform == RuntimePlatform.Android)
        {
            // Do nothing for now on Android
        }
    }

    private static bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
    }
}