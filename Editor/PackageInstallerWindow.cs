#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class PackageInstallerWindow : EditorWindow
{
    private static string manifestPath => Path.Combine(Application.dataPath, "../Packages/manifest.json");

    private static readonly Dictionary<string, string> registryPackages = new Dictionary<string, string>
    {
        { "Input System", "com.unity.inputsystem:1.4.4" },
        { "ProBuilder", "com.unity.probuilder:6.0.4" },
        { "Animation Rigging", "com.unity.animation.rigging:1.3.0" },
        { "Cinemachine", "com.unity.cinemachine:3.1.4" },
        { "Addressables", "com.unity.addressables:1.21.12" },
        { "Extenject", "com.extenject:9.2.0" },
        { "Post Processing", "com.unity.postprocessing:3.4.0" },
        { "URP", "com.unity.render-pipelines.universal:14.0.8" },
        { "Behaviour", "com.unity.behaviour:1.1.0" },
        { "SceneReference", "com.eflatun.scenereference:4.1.1" },
        { "Splines", "com.unity.splines:2.6.1" },
        { "Terrain Tools", "com.unity.terrain-tools:5.1.2" },
        { "Timeline", "com.unity.timeline:1.8.8" }
    };

    private static readonly Dictionary<string, string> gitPackages = new Dictionary<string, string>
    {
        {"UniTask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" },
        {"Easy Text Effects", "https://github.com/LeiQiaoZhi/Easy-Text-Effect.git" },
        {"InputSystem Action Prompts", "https://github.com/DrewStriker/InputSystemActionPrompts.git" },
        {"Serializable Interface", "https://github.com/Thundernerd/Unity3D-SerializableInterface.git" },
        {"Libre Fracture", "https://github.com/HunterProduction/unity-libre-fracture-2.0.git" },
        {"AudioClip Editor", "https://github.com/alexandregaudencio/AudioClipEditor.git" },
        {"Serialized Dictionary", "https://github.com/ayellowpaper/SerializedDictionary.git"},
        {"SaintsField Custom Attributes", "https://github.com/TylerTemp/SaintsField.git"},
        {"Transition Kit", "https://github.com/prime31/TransitionKit.git"},
        {"Dotween", "https://github.com/Demigiant/dotween.git" }
    };

    private static readonly Dictionary<string, string> links = new Dictionary<string, string>()
        {
            { "Kinematic Character Controller", "https://assetstore.unity.com/packages/tools/physics/kinematic-character-controller-99131" }
        };

    // private bool category1 = false;
    [MenuItem("Tools/Startup Package Installer")]
    public static void ShowWindow()
    {
        GetWindow<PackageInstallerWindow>("Startup Package Installer");
    }

    private Vector2 scrollPos;
    private void OnGUI()
    {


        GUILayout.Label("Packages", EditorStyles.boldLabel);


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        // if (GUILayout.Button("Category 1 "+ (category1 ?"▲":"▼")  ))
        // {
        //     category1 = !category1;
        // }
        // if (category1) 
        DrawTools1();
        DrawTools2();
        GUILayout.Space(30);
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Reload packages", GUILayout.Height(40)))
        {
            UnityEditor.PackageManager.Client.Resolve();
            AssetDatabase.Refresh();
        }


        GUILayout.Space(20);
        CreateLinks();


        EditorGUILayout.EndScrollView();

        // empurra tudo pra cima, deixando link no rodapé
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (EditorGUILayout.LinkButton("Abrir manifest.json"))
            {
                var path = System.IO.Path.Combine(Application.dataPath, "../Packages/manifest.json");
                Application.OpenURL(path);
            }
        }

    }

    private static void DrawTools1()
    {
        foreach (var pkg in registryPackages)
        {
            var split = pkg.Value.Split(':');
            bool pkgAdded = IsPackageNameInManifest(split[0]);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(pkg.Key, GUILayout.Width(200));
                if (pkgAdded) GUI.backgroundColor = Color.red; else GUI.backgroundColor = Color.green;
                if (GUILayout.Button(pkgAdded ? "Remove" : "Install", GUILayout.Width(150)))
                {
                    if (pkgAdded)
                    {
                        RemoveRegistryPackage(split[0], split[1]);
                    }
                    else
                    {
                        AddRegistryPackage(split[0], split[1]);
                    }
                    //UnityEditor.PackageManager.Client.Resolve();

                }
            }
        }
    }

    private static void DrawTools2()
    {
        foreach (var pkg in gitPackages)
        {
            bool pkgAdded = IsPackageUrlInManifest(pkg.Key, pkg.Value);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(pkg.Key, GUILayout.Width(200));
                if (pkgAdded) GUI.backgroundColor = Color.red; else GUI.backgroundColor = Color.green;

                if (GUILayout.Button(pkgAdded ? "Remove" : "Install", GUILayout.Width(150)))
                {
                    if (pkgAdded)
                    {
                        RemoveGitPackage(pkg.Key, pkg.Value);
                    }
                    else
                    {
                        AddGitPackage(pkg.Key, pkg.Value);
                    }
                    //UnityEditor.PackageManager.Client.Resolve();

                }
            }
        }
    }

    private static void CreateLinks()
    {
        GUILayout.Label("Asset Store ", EditorStyles.boldLabel);
        GUIStyle linkStyle = new GUIStyle(EditorStyles.label);
        linkStyle.normal.textColor = new Color(0, 0.5f, 1f);
        linkStyle.hover.textColor = Color.cyan;
        linkStyle.stretchWidth = true;

        foreach (var kvp in links)
        {
            GUILayout.Space(5);
            string title = kvp.Key;
            string url = kvp.Value;

            Rect linkRect = GUILayoutUtility.GetRect(new GUIContent(title), linkStyle);
            EditorGUI.LabelField(linkRect, title, linkStyle);

            if (Event.current.type == EventType.MouseDown && linkRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL(url);
            }

        }
    }
    private static void AddRegistryPackage(string packageName, string version)
    {
        if (!File.Exists(manifestPath)) return;
        string manifestText = File.ReadAllText(manifestPath);
        if (manifestText.Contains($"\"{packageName}\""))
        {
            Debug.LogWarning($"'{packageName}' already present in manifest.json.");
            return;
        }
        var root = JObject.Parse(manifestText);
        root["dependencies"][packageName] = version;
        File.WriteAllText(manifestPath, root.ToString(Formatting.Indented));
        Debug.Log($"Package '{packageName}' add to manifest.json.");
    }
    private static void RemoveRegistryPackage(string packageName, string version)
    {
        {
            if (!File.Exists(manifestPath)) return;
            string manifestText = File.ReadAllText(manifestPath);
            if (!manifestText.Contains($"\"{packageName}\""))
            {
                Debug.LogWarning($"'{packageName}' is not present in manifest.json.");
                return;
            }
            var root = JObject.Parse(manifestText);
            var deps = (JObject)root["dependencies"];
            deps.Remove(packageName);
            File.WriteAllText(manifestPath, root.ToString(Formatting.Indented));
            Debug.Log($"Package '{packageName}' removed from manifest.json.");
        }

    }

    private static void AddGitPackage(string packageDisplayName, string gitUrl)
    {
        if (!File.Exists(manifestPath)) return;
        string manifestText = File.ReadAllText(manifestPath);
        string packageName = GeneratePackageNameFromUrl(gitUrl);
        AddRegistryPackage(packageName, gitUrl);

    }
    private static void RemoveGitPackage(string packageDisplayName, string gitUrl)
    {
        if (!File.Exists(manifestPath)) return;

        string manifestText = File.ReadAllText(manifestPath);
        string packageName = GeneratePackageNameFromUrl(gitUrl);
        RemoveRegistryPackage(packageName, gitUrl);

    }
    private static string GeneratePackageNameFromUrl(string url)
    {
        var match = Regex.Match(url, @"github\.com/([^/]+)/([^/.]+)");
        if (match.Success)
        {
            return $"com.{match.Groups[1].Value.ToLower()}.{match.Groups[2].Value.ToLower()}";
        }
        throw new System.Exception($"package is not registred in url {url}.");

    }

    public static bool IsPackageNameInManifest(string packageName)
    {
        if (!File.Exists(manifestPath)) return false;

        string manifestText = File.ReadAllText(manifestPath);

        if (manifestText.Contains($"\"{packageName}\""))
        {
            return true;
        }
        return false;
    }
    public static bool IsPackageUrlInManifest(string packageDisplayName, string gitUrl)
    {
        if (!File.Exists(manifestPath)) return false;

        string manifestText = File.ReadAllText(manifestPath);
        string packageName = GeneratePackageNameFromUrl(gitUrl);

        if (manifestText.Contains($"\"{packageName}\""))
        {
            return true;
        }
        return false;
    }
}
#endif
