#if UNITY_IOS
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

// Writes every SKAdNetwork ID our ad networks need into the exported
// Info.plist. Without them, iOS 14+ installs driven by an ad cannot be
// attributed to it, so networks bid less for our inventory.
//
// The list is committed instead of fetched at build time: LevelPlay 9's own
// "add installed networks' SKAdNetwork IDs" toggle downloads the lists on
// every build and lives in an Editor-only settings asset that the 9.x
// migration resets. This list is the union of LevelPlay's published lists
// for ironSource, Unity Ads and AdMob plus the "missing IDs" list on the
// Unity Ads dashboard (Monetization -> Settings), as of 2026-11-17.
// Refresh it from those sources every few months; IDs are only ever added.
//
// Existing entries are kept, so it is safe to run after any other
// post-processor that also writes SKAdNetworkItems.
public static class SkAdNetworkIds
{
    private static readonly string[] Ids =
    {
        "22mmun2rn5.skadnetwork",
        "238da6jt44.skadnetwork",
        "252b5q8x7y.skadnetwork",
        "294l99pt4k.skadnetwork",
        "2fnua5tdw4.skadnetwork",
        "2u9pt9hc89.skadnetwork",
        "32z4fx6l9h.skadnetwork",
        "3qcr597p9d.skadnetwork",
        "3qy4746246.skadnetwork",
        "3rd42ekr43.skadnetwork",
        "3sh42y64q3.skadnetwork",
        "424m5254lk.skadnetwork",
        "4468km3ulz.skadnetwork",
        "44jx6755aq.skadnetwork",
        "47vhws6wlr.skadnetwork",
        "488r3q3dtq.skadnetwork",
        "4dzt52r2t5.skadnetwork",
        "4fzdc2evr5.skadnetwork",
        "4pfyvq9l8r.skadnetwork",
        "4w7y6s5ca2.skadnetwork",
        "578prtvx9j.skadnetwork",
        "5a6flpkh64.skadnetwork",
        "5f5u5tfb26.skadnetwork",
        "5l3tpt7t6e.skadnetwork",
        "5lm9lj6jb7.skadnetwork",
        "5tjdwbrq8w.skadnetwork",
        "6yxyv74ff7.skadnetwork",
        "77y3x8wds4.skadnetwork",
        "7fmhfwg9en.skadnetwork",
        "7ug5zh24hu.skadnetwork",
        "8c4e2ghe7u.skadnetwork",
        "8s468mfl3y.skadnetwork",
        "97r2b46745.skadnetwork",
        "9nlqeag3gk.skadnetwork",
        "9rd848q2bz.skadnetwork",
        "9t245vhmpl.skadnetwork",
        "a2p9lx4jpn.skadnetwork",
        "a8cz6cu7e5.skadnetwork",
        "av6w8kgt66.skadnetwork",
        "c3frkrj4fj.skadnetwork",
        "c6k4g5qg8m.skadnetwork",
        "cp8zw746q7.skadnetwork",
        "cstr6suwn9.skadnetwork",
        "cwn433xbcr.skadnetwork",
        "e5fvkxwrpn.skadnetwork",
        "f38h382jlk.skadnetwork",
        "f73kdq92p3.skadnetwork",
        "f7s53z58qe.skadnetwork",
        "feyaarzu9v.skadnetwork",
        "g6gcrrvk4p.skadnetwork",
        "glqzh8vgby.skadnetwork",
        "gta9lk7p23.skadnetwork",
        "hs6bdukanm.skadnetwork",
        "k674qkevps.skadnetwork",
        "k6y4y55b64.skadnetwork",
        "kbd757ywx3.skadnetwork",
        "kbmxgpxpgc.skadnetwork",
        "klf5c3l5u5.skadnetwork",
        "lr83yxwka7.skadnetwork",
        "ludvb6z3bs.skadnetwork",
        "m8dbw4sv7c.skadnetwork",
        "mj797d8u6f.skadnetwork",
        "mlmmfzh3r3.skadnetwork",
        "mp6xlyr22a.skadnetwork",
        "mqn7fxpca7.skadnetwork",
        "n38lu8286q.skadnetwork",
        "n9x2a789qt.skadnetwork",
        "p78axxw29g.skadnetwork",
        "ppxm28t8ap.skadnetwork",
        "prcb7njmu6.skadnetwork",
        "pwa73g5rt2.skadnetwork",
        "qqp299437r.skadnetwork",
        "s39g8k73mm.skadnetwork",
        "su67r6k2v3.skadnetwork",
        "t38b2kh725.skadnetwork",
        "tl55sbb4fm.skadnetwork",
        "uw77j35x4d.skadnetwork",
        "v4nxqhlyqp.skadnetwork",
        "v72qych5uu.skadnetwork",
        "v79kvwwj4g.skadnetwork",
        "v9wttpbfk9.skadnetwork",
        "vhf287vqwu.skadnetwork",
        "vutu7akeur.skadnetwork",
        "w9q455wk68.skadnetwork",
        "wg4vff78zm.skadnetwork",
        "wzmmz9fp6w.skadnetwork",
        "x44k69ngh6.skadnetwork",
        "xga6mpmplv.skadnetwork",
        "y5ghdn5j9k.skadnetwork",
        "yclnxrl5pm.skadnetwork",
        "ydx93a7ass.skadnetwork",
        "zmvfpc5aq8.skadnetwork",
        "zq492l623r.skadnetwork",
    };

    [PostProcessBuild(200)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        if (!File.Exists(plistPath))
        {
            Debug.LogWarning($"SKADNETWORK: no Info.plist at {plistPath}; IDs not added.");
            return;
        }

        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementArray items = plist.root.values.TryGetValue("SKAdNetworkItems", out PlistElement existing)
            ? existing.AsArray()
            : plist.root.CreateArray("SKAdNetworkItems");

        var present = new HashSet<string>();
        foreach (PlistElement item in items.values)
        {
            if (item is PlistElementDict dict && dict.values.TryGetValue("SKAdNetworkIdentifier", out PlistElement id))
                present.Add(id.AsString().ToLowerInvariant());
        }

        int added = 0;
        foreach (string id in Ids)
        {
            if (present.Add(id))
            {
                items.AddDict().SetString("SKAdNetworkIdentifier", id);
                added++;
            }
        }

        plist.WriteToFile(plistPath);
        Debug.Log($"SKADNETWORK: added {added} IDs, {present.Count} in Info.plist.");
    }
}
#endif
