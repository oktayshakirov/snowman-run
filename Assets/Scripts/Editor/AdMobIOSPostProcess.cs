#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

// Injects the AdMob application id into the built Xcode Info.plist. The Google
// Mobile Ads SDK (pulled in through the LevelPlay AdMob adapter) requires
// GADApplicationIdentifier at launch or the app crashes on startup. LevelPlay
// cannot supply this value itself since it is app-specific, so it is set here.
// SKAdNetwork ids are handled separately by LevelPlay's build post-processor
// (AddIronsourceSkadnetworkID enabled in IronSourceMediationSettings).
public static class AdMobIOSPostProcess
{
    private const string AdMobIOSAppId = "ca-app-pub-5852582960793521~7828873764";
    private const string TrackingUsageDescription =
        "This identifier will be used to deliver personalized ads to you.";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        if (!File.Exists(plistPath))
            return;

        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("GADApplicationIdentifier", AdMobIOSAppId);
        plist.root.SetString("NSUserTrackingUsageDescription", TrackingUsageDescription);
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
#endif
