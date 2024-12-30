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

            VibrateAndroid(30);
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

            VibrateAndroid(60);
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

            VibrateAndroid(100);
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

            VibrateAndroid(20);
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
            VibrateAndroid(50);
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
            VibrateAndroid(70);
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
            VibrateAndroid(100);
        }
    }

    private static bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
    }


    private static void VibrateAndroid(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity != null)
                {
                    AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    if (vibrator != null)
                    {
                        using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
                        {
                            int sdkInt = version.GetStatic<int>("SDK_INT");
                            if (sdkInt >= 26)
                            {
                                using (AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                                {
                                    AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                                        "createOneShot",
                                        new object[] { milliseconds, vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE") }
                                    );
                                    vibrator.Call("vibrate", effect);
                                }
                            }
                            else
                            {
                                vibrator.Call("vibrate", milliseconds);
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("VibrateAndroid failed: " + e.Message);
        }
#endif
    }
}