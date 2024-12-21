using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeHaptics
{
    [DllImport("__Internal")]
    private static extern void TriggerHapticFeedback(int type);

    public static void TriggerLightHaptic()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(0); // Light impact
        }
    }

    public static void TriggerMediumHaptic()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(1); // Medium impact
        }
    }

    public static void TriggerHeavyHaptic()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(2); // Heavy impact
        }
    }

    public static void TriggerSelectionHaptic()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(3); // Selection feedback
        }
    }

    public static void TriggerSuccessNotification()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(4); // Notification success
        }
    }

    public static void TriggerWarningNotification()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(5); // Notification warning
        }
    }

    public static void TriggerErrorNotification()
    {
        if (IsVibrationEnabled() && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            TriggerHapticFeedback(6); // Notification error
        }
    }

    private static bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
    }
}