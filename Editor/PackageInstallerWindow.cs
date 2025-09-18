#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class PackageInstallerWindow : EditorWindow
{
    private static string manifestPath => Path.Combine(Application.dataPath, "../Packages/manifest.json");

    // Pacotes do Unity Registry (nome + versão)
    private static readonly Dictionary<string, string> registryPackages = new Dictionary<string, string>
    {
        { "TextMeshPro", "com.unity.textmeshpro:3.0.6" },
        { "Input System", "com.unity.inputsystem:1.4.4" },
        { "ProBuilder", "com.unity.probuilder:6.0.4" },
        { "Animation Rigging", "com.unity.animation.rigging:1.3.0" },
        { "Cinemachine", "com.unity.cinemachine:3.1.4" },
        { "Addressables", "com.unity.addressables:1.21.12" },
        { "DoTween", "com.demigiant.dotween:1.2.705" },
        { "Extenject", "com.extenject:9.2.0" },
        { "Post Processing", "com.unity.postprocessing:3.4.0" },
        { "URP", "com.unity.render-pipelines.universal:14.0.8" },
        { "Behaviour", "com.unity.behaviour:1.1.0" },
        { "SceneReference", "com.eflatun.scenereference:4.1.1" },
        { "Splines", "com.unity.splines:2.6.1" },
        { "Terrain Tools", "com.unity.terrain-tools:5.1.2" },
        { "Timeline", "com.unity.timeline:1.8.8" }
    };

    // Pacotes Git (nome + URL)
    private static readonly Dictionary<string, string> gitPackages = new Dictionary<string, string>
    {
        { "UniTask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" },
        { "Easy Text Effects", "https://github.com/LeiQiaoZhi/Easy-Text-Effect.git" },
        { "InputSystem Action Prompts", "https://github.com/DrewStriker/InputSystemActionPrompts.git" },
        { "Serializable Interface", "https://github.com/Thundernerd/Unity3D-SerializableInterface.git" },
        { "Libre Fracture", "https://github.com/HunterProduction/unity-libre-fracture-2.0.git" },
        { "AudioClip Editor", "https://github.com/alexandregaudencio/AudioClipEditor.git" },
        {"Serialized Dictionary", "https://github.com/ayellowpaper/SerializedDictionary.git"},
        {"SaintsField Custom Attributes", "https://github.com/TylerTemp/SaintsField.git"}
    };

    [MenuItem("Tools/Package Installer")]
    public static void ShowWindow()
    {
        GetWindow<PackageInstallerWindow>("Package Installer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Registry Packages", EditorStyles.boldLabel);
        foreach (var pkg in registryPackages)
        {
            bool isInstalled = IsPackageInstalled(pkg.Value.Split(':')[0]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(pkg.Key))
            {
                var split = pkg.Value.Split(':');
                AddRegistryPackage(split[0], split[1]);
            }
            GUILayout.Label(isInstalled ? "Installed" : "Not Installed", GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("Git-based Packages", EditorStyles.boldLabel);
        foreach (var pkg in gitPackages)
        {
            string packageName = GeneratePackageNameFromUrl(pkg.Value);
            bool isInstalled = IsPackageInstalled(packageName);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(pkg.Key))
            {
                AddGitPackage(pkg.Key, pkg.Value);
            }
            GUILayout.Label(isInstalled ? "Installed" : "Not Installed", GUILayout.Width(100));
            if (GUILayout.Button("about", GUILayout.Width(50)))
            {
                Application.OpenURL(pkg.Value);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    private static bool IsPackageInstalled(string packageName)
    {
        if (!File.Exists(manifestPath)) return false;

        string manifestText = File.ReadAllText(manifestPath);
        return manifestText.Contains($"\"{packageName}\"");
    }

    private static void AddRegistryPackage(string packageName, string version)
    {
        if (!File.Exists(manifestPath)) return;

        string manifestText
