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
///
/// Signature de l'APK : si « Signature/FruityNinjaVR.keystore » existe à la racine du projet (dossier
/// ignoré par git), l'APK est signé avec cette clé, toujours la même, pour que les mises à jour gardent
/// les données du jeu sur le casque (classement, réglages). Sans ce fichier, Unity signe avec la clé
/// de débogage de la machine. La clé du projet est une copie de la clé de débogage d'origine
/// (mot de passe « android », alias « androiddebugkey ») : même signature que les APK déjà installés.
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

    static string Racine => Directory.GetParent(Application.dataPath).FullName;

    static string Sortie => Environment.GetEnvironmentVariable("FRUITNINJA_BUILDS") ?? Path.Combine(Racine, "Builds");

    static string CleProjet => Path.Combine(Racine, "Signature", "FruityNinjaVR.keystore");

    [MenuItem("Fruity Ninja/Builds/Meta Quest (APK)")]
    public static void Quest()
    {
        EditorUserBuildSettings.buildAppBundle = false;
        bool cle = File.Exists(CleProjet);
        if (cle)
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = CleProjet;
            PlayerSettings.Android.keystorePass = "android";
            PlayerSettings.Android.keyaliasName = "androiddebugkey";
            PlayerSettings.Android.keyaliasPass = "android";
            Debug.Log("[Build] APK signé avec la clé du projet (Signature/FruityNinjaVR.keystore)");
        }
        BuildResult resultat;
        try
        {
            resultat = Construire(BuildTarget.Android, Path.Combine(Sortie, "Quest", "FruityNinjaVR_Quest.apk"));
        }
        finally
        {
            // Réglage remis par défaut : aucun chemin local enregistré dans les réglages du projet
            if (cle)
            {
                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.Android.keystoreName = "";
                AssetDatabase.SaveAssets();
            }
        }
        if (Application.isBatchMode) EditorApplication.Exit(resultat == BuildResult.Succeeded ? 0 : 1);
    }

    [MenuItem("Fruity Ninja/Builds/Windows PCVR")]
    public static void Windows()
    {
        var resultat = Construire(BuildTarget.StandaloneWindows64, Path.Combine(Sortie, "Windows_PCVR", "FruityNinjaVR.exe"));
        if (Application.isBatchMode) EditorApplication.Exit(resultat == BuildResult.Succeeded ? 0 : 1);
    }

    static BuildResult Construire(BuildTarget cible, string chemin)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(chemin));
        var scenes = Scenes.Where(File.Exists).ToArray();
        var rapport = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = chemin, target = cible });
        var r = rapport.summary;
        Debug.Log($"[Build] {cible} : {r.result}, {r.totalErrors} erreur(s), {r.totalTime:mm\\:ss}, sortie {chemin}");
        return r.result;
    }
}
#endif
