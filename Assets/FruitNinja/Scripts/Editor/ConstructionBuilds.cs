#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds du jeu en un clic (menu « Fruity Ninja > Builds ») ou en ligne de commande :
///   Unity.exe -batchmode -nographics -buildTarget Android -projectPath . -executeMethod ConstructionBuilds.Quest
///   Unity.exe -batchmode -nographics -buildTarget Win64   -projectPath . -executeMethod ConstructionBuilds.Windows
/// Les deux versions contiennent les 5 scènes ; le décor affiché par défaut dépend de la plateforme
/// (option « Décor détaillé » : activée sur PC, désactivée sur Quest).
/// Dossier de sortie : variable d'environnement FRUITNINJA_BUILDS, sinon « Builds/ » à côté du projet.
/// </summary>
public static class ConstructionBuilds
{
    static readonly string[] Scenes =
    {
        "Assets/FruitNinja/Scenes/MainMenu.unity",
        "Assets/FruitNinja/Scenes/Game.unity",
        "Assets/FruitNinja/Scenes/Debug_Fruits.unity",
        "Assets/FruitNinja/Scenes/MainMenu_ToonScapes.unity",
        "Assets/FruitNinja/Scenes/Game_ToonScapes.unity",
    };

    static string Sortie => Environment.GetEnvironmentVariable("FRUITNINJA_BUILDS")
                            ?? Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");

    [MenuItem("Fruity Ninja/Builds/Meta Quest (APK)")]
    public static void Quest()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        Construire(BuildTarget.Android, Path.Combine(Sortie, "Quest", "FruityNinjaVR_Quest.apk"));
    }

    [MenuItem("Fruity Ninja/Builds/Windows PCVR")]
    public static void Windows() => Construire(BuildTarget.StandaloneWindows64, Path.Combine(Sortie, "Windows_PCVR", "FruityNinjaVR.exe"));

    static void Construire(BuildTarget cible, string chemin)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(chemin));
        var scenes = Scenes.Where(File.Exists).ToArray();
        var rapport = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = chemin, target = cible });
        var r = rapport.summary;
        Debug.Log($"[Build] {cible} : {r.result}, {r.totalErrors} erreur(s), {r.totalTime:mm\\:ss}, sortie {chemin}");
        if (Application.isBatchMode) EditorApplication.Exit(r.result == BuildResult.Succeeded ? 0 : 1);
    }
}
#endif
