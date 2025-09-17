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
        { "Cinemachine", "com.unity.cinemachine:2.9.13" },
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
            if (GUILayout.Button($"Install {pkg.Key}"))
            {
                var split = pkg.Value.Split(':');
                AddRegistryPackage(split[0], split[1]);
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Git-based Packages", EditorStyles.boldLabel);
        foreach (var pkg in gitPackages)
        {
            if (GUILayout.Button($"Install {pkg.Key}"))
            {
                AddGitPackage(pkg.Key, pkg.Value);
            }
        }
    }

    private static void AddRegistryPackage(string packageName, string version)
    {
        if (!File.Exists(manifestPath)) return;

        string manifestText = File.ReadAllText(manifestPath);

        if (manifestText.Contains($"\"{packageName}\""))
        {
            Debug.LogWarning($"'{packageName}' já está presente no manifest.json.");
            return;
        }

        string newLine = $",\n    \"{packageName}\": \"{version}\"";

        int depIndex = manifestText.IndexOf("\"dependencies\"");
        int insertIndex = manifestText.IndexOf('}', depIndex);
        manifestText = manifestText.Insert(insertIndex, newLine);
        File.WriteAllText(manifestPath, manifestText);

        AssetDatabase.Refresh();
        Debug.Log($"Pacote '{packageName}' adicionado ao manifest.json com sucesso.");
    }

    private static void AddGitPackage(string packageDisplayName, string gitUrl)
    {
        if (!File.Exists(manifestPath)) return;

        string manifestText = File.ReadAllText(manifestPath);
        string packageName = GeneratePackageNameFromUrl(gitUrl);

        if (manifestText.Contains($"\"{packageName}\""))
        {
            Debug.LogWarning($"'{packageDisplayName}' já está presente no manifest.json.");
            return;
        }

        string newLine = $",\n    \"{packageName}\": \"{gitUrl}\"";

        int depIndex = manifestText.IndexOf("\"dependencies\"");
        int insertIndex = manifestText.IndexOf('}', depIndex);
        manifestText = manifestText.Insert(insertIndex, newLine);
        File.WriteAllText(manifestPath, manifestText);

        AssetDatabase.Refresh();
        Debug.Log($"Pacote '{packageDisplayName}' adicionado ao manifest.json com sucesso.");
    }

    // Gerador de nome de pacote simples baseado na URL
    private static string GeneratePackageNameFromUrl(string url)
    {
        var match = Regex.Match(url, @"github\.com/([^/]+)/([^/.]+)");
        if (match.Success)
        {
            return $"com.{match.Groups[1].Value.ToLower()}.{match.Groups[2].Value.ToLower()}";
        }

        return $"com.custom.package{Random.Range(1000, 9999)}";
    }
}
#endif
