using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class PodfileFixer
{
    private const string PODFILE_PATH = "iOS/Podfile";
    private const string UNITYADS_VERSION_OLD = "pod 'UnityAds', '~> 4.12.0'";
    private const string UNITYADS_VERSION_NEW = "pod 'UnityAds', '4.16.3'";

    static PodfileFixer()
    {
        EditorApplication.delayCall += FixPodfileIfNeeded;
        AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
    }

    private static void OnImportPackageCompleted(string packageName)
    {
        EditorApplication.delayCall += FixPodfileIfNeeded;
    }

    [MenuItem("Tools/Fix Podfile UnityAds Version")]
    public static void FixPodfileManually()
    {
        FixPodfileIfNeeded();
    }

    [PostProcessBuild(1)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            FixPodfileIfNeeded();
        }
    }

    private static void FixPodfileIfNeeded()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string podfilePath = Path.Combine(projectRoot, PODFILE_PATH);

        if (!File.Exists(podfilePath))
        {
            return;
        }

        string podfileContent = File.ReadAllText(podfilePath);
        bool needsFix = false;
        string originalContent = podfileContent;

        if (podfileContent.Contains(UNITYADS_VERSION_OLD))
        {
            Debug.Log("Fixing Podfile: Updating UnityAds version from ~> 4.12.0 to 4.16.3");
            podfileContent = podfileContent.Replace(UNITYADS_VERSION_OLD, UNITYADS_VERSION_NEW);
            needsFix = true;
        }
        else
        {
            Regex regex = new Regex(@"pod\s+['""]UnityAds['""]\s*,\s*['""]([^'""]+)['""]");
            Match match = regex.Match(podfileContent);
            if (match.Success)
            {
                string currentVersion = match.Groups[1].Value;
                if (!currentVersion.Contains("4.16.3"))
                {
                    Debug.Log($"Fixing Podfile: Updating UnityAds version from {currentVersion} to 4.16.3");
                    podfileContent = regex.Replace(podfileContent, "pod 'UnityAds', '4.16.3'");
                    needsFix = true;
                }
            }
        }

        if (needsFix && podfileContent != originalContent)
        {
            File.WriteAllText(podfilePath, podfileContent);
            Debug.Log("Podfile fixed successfully! UnityAds version set to 4.16.3");
        }
    }
}

