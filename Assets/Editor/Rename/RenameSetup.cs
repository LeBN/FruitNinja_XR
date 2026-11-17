// Script temporaire : tonemapping/brouillard, boutons, renommages (assets + objets), vérification.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RenameSetup
{
    const string R = "Assets/FruitNinja/";
    static readonly Color HorizonColor = new Color(0.055f, 0.085f, 0.175f); // couleur de l'horizon du ciel étoilé

    static readonly (string from, string to)[] AssetMoves =
    {
        // Prefabs
        (R + "Prefabs/Cube.prefab", R + "Prefabs/Debug/Debug_Cube.prefab"),
        (R + "Prefabs/SliceableCube.prefab", R + "Prefabs/Debug/Debug_CubeDecoupable.prefab"),
        (R + "Prefabs/Explosion.prefab", R + "Prefabs/FX/FX_Explosion.prefab"),
        (R + "Prefabs/FruitJuiceFX.prefab", R + "Prefabs/FX/FX_Fruit_JusDecoupe.prefab"),
        (R + "Prefabs/Fruits/apple.prefab", R + "Prefabs/Fruits/Fruit_Pomme.prefab"),
        (R + "Prefabs/Fruits/avocado.prefab", R + "Prefabs/Fruits/Fruit_Avocat.prefab"),
        (R + "Prefabs/Fruits/banana.prefab", R + "Prefabs/Fruits/Fruit_Banane.prefab"),
        (R + "Prefabs/Fruits/cherries.prefab", R + "Prefabs/Fruits/Fruit_Cerises.prefab"),
        (R + "Prefabs/Fruits/lemon.prefab", R + "Prefabs/Fruits/Fruit_Citron.prefab"),
        (R + "Prefabs/Fruits/peach.prefab", R + "Prefabs/Fruits/Fruit_Peche.prefab"),
        (R + "Prefabs/Fruits/peanut.prefab", R + "Prefabs/Fruits/Fruit_Cacahuete.prefab"),
        (R + "Prefabs/Fruits/pear.prefab", R + "Prefabs/Fruits/Fruit_Poire.prefab"),
        (R + "Prefabs/Fruits/strawberry.prefab", R + "Prefabs/Fruits/Fruit_Fraise.prefab"),
        (R + "Prefabs/Fruits/watermelon.prefab", R + "Prefabs/Fruits/Fruit_Pasteque.prefab"),
        (R + "Prefabs/Fruits/GoldenFruit.prefab", R + "Prefabs/Fruits/Fruit_Dore_Bonus.prefab"),
        (R + "Prefabs/Fruits/BombBall.prefab", R + "Prefabs/Bombes/Bombe.prefab"),
        (R + "Prefabs/Scene/Directional Light.prefab", R + "Prefabs/Environnement/Env_Lumiere_Lune.prefab"),
        (R + "Prefabs/Scene/Global Volume.prefab", R + "Prefabs/Environnement/Env_Volume_PostProcess.prefab"),
        (R + "Prefabs/Scene/Plane.prefab", R + "Prefabs/Environnement/Env_Sol.prefab"),
        (R + "Prefabs/XR/XR Interaction Manager.prefab", R + "Prefabs/XR/XR_InteractionManager.prefab"),
        (R + "Prefabs/XR/XR Origin (XR Rig).prefab", R + "Prefabs/XR/XR_Rig_Joueur.prefab"),
        (R + "Resources/StartFruit.prefab", R + "Resources/Fruit_Demarrage.prefab"),
        // Matériaux
        (R + "Materials/BladeShine.mat", R + "Materials/Mat_Sabre_Reflet.mat"),
        (R + "Materials/Fantome.mat", R + "Materials/Mat_Fruit_Fantome.mat"),
        (R + "Materials/Red Emissive.mat", R + "Materials/Mat_Fruit_InterieurCoupe.mat"),
        (R + "Materials/Skybox_NuitEtoilee.mat", R + "Materials/Mat_Skybox_NuitEtoilee.mat"),
        // Scènes
        (R + "Scenes/ScèneFruit.scenetemplate", R + "Scenes/Template_Game.scenetemplate"),
        // Sons
        (R + "Sons/bomb.mp3", R + "Sons/Effets/Bombe_Explosion.mp3"),
        (R + "Sons/splash-fruit.mp3", R + "Sons/Effets/Fruit_Decoupe_Splash.mp3"),
        (R + "Sons/Effets/Lame_Shine.wav", R + "Sons/Effets/Sabre_Reflet_Depart.wav"),
        (R + "Sons/Effets/Swing_Lent_1.wav", R + "Sons/Effets/Sabre_Swing_Lent_1.wav"),
        (R + "Sons/Effets/Swing_Lent_2.wav", R + "Sons/Effets/Sabre_Swing_Lent_2.wav"),
        (R + "Sons/Effets/Swing_Lent_3.wav", R + "Sons/Effets/Sabre_Swing_Lent_3.wav"),
        (R + "Sons/Effets/Swing_Rapide_1.wav", R + "Sons/Effets/Sabre_Swing_Rapide_1.wav"),
        (R + "Sons/Effets/Swing_Rapide_2.wav", R + "Sons/Effets/Sabre_Swing_Rapide_2.wav"),
        (R + "Sons/Effets/Swing_Rapide_3.wav", R + "Sons/Effets/Sabre_Swing_Rapide_3.wav"),
        (R + "Sons/Musiques/Start_EnGarde.ogg", R + "Sons/Musiques/Jeu_Depart_EnGarde.ogg"),
        (R + "Sons/Musiques/Jeu_DesertBattle.ogg", R + "Sons/Musiques/Jeu_Combat_DesertBattle.ogg"),
        (R + "Sons/Ambiances/Nuit_Forest_Rain_Crickets_Frogs.ogg", R + "Sons/Ambiances/Ambiance_ForetNuit_PluieGrillons.ogg"),
        // Ressources non utilisées
        (R + "Terrain/New Terrain.asset", R + "_NonUtilise/Terrain/Terrain_NonUtilise_1.asset"),
        (R + "Terrain/New Terrain 1.asset", R + "_NonUtilise/Terrain/Terrain_NonUtilise_2.asset"),
        (R + "Terrain/New Terrain 2.asset", R + "_NonUtilise/Terrain/Terrain_NonUtilise_3.asset"),
        (R + "Terrain/New Terrain 3.asset", R + "_NonUtilise/Terrain/Terrain_NonUtilise_4.asset"),
        (R + "Terrain/TerrainData_7abb20e4-b73f-4537-8b05-e992ba87dbab.asset", R + "_NonUtilise/Terrain/TerrainData_NonUtilise_A.asset"),
        (R + "Terrain/TerrainData_b570a5ae-1df0-4992-b887-1b3c21533851.asset", R + "_NonUtilise/Terrain/TerrainData_NonUtilise_B.asset"),
        // UI
        (R + "UI/Studio.png", R + "UI/Logo_Studio.png"),
        (R + "UI/Boutons/Bouton_Bois.png", R + "UI/Boutons/Bouton_Bois_Vide.png"),
        // Réglages de rendu
        ("Assets/Settings/SampleSceneProfile.asset", "Assets/Settings/PostProcess_Nuit.asset"),
        ("Assets/Settings/DefaultVolumeProfile.asset", "Assets/Settings/URP_VolumeProfile_Defaut.asset"),
        ("Assets/Settings/PC_RPAsset.asset", "Assets/Settings/URP_Asset_PC.asset"),
        ("Assets/Settings/PC_Renderer.asset", "Assets/Settings/URP_Renderer_PC.asset"),
        ("Assets/Settings/Mobile_RPAsset.asset", "Assets/Settings/URP_Asset_Mobile.asset"),
        ("Assets/Settings/Mobile_Renderer.asset", "Assets/Settings/URP_Renderer_Mobile.asset"),
    };

    public static void Run()
    {
        try
        {
            TuneTonemap();
            MoveAssets();
            foreach (var p in new[] { R + "Scenes/MainMenu.unity", R + "Scenes/Game.unity" })
                SetupScene(p);
            RenameDebugScene();
            AssetDatabase.SaveAssets();
            Verify();
            ListPoint3();
            Debug.Log("[Ren] OK");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("[Ren] " + e);
            EditorApplication.Exit(1);
        }
    }

    // ---------------------------------------------------------------- Rendu
    static void TuneTonemap()
    {
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
        if (profile.TryGet<ColorAdjustments>(out var ca)) { ca.postExposure.Override(1.1f); ca.contrast.Override(6f); }
        if (profile.TryGet<LiftGammaGain>(out var lgg)) lgg.gamma.Override(new Vector4(1f, 1f, 1f, 0.06f));
        EditorUtility.SetDirty(profile);
        foreach (var c in profile.components) EditorUtility.SetDirty(c);
    }

    static void ApplyFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 6f;
        RenderSettings.fogEndDistance = 45f;
        RenderSettings.fogColor = HorizonColor;
    }

    // ---------------------------------------------------------------- Assets
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    static void MoveAssets()
    {
        foreach (var (from, to) in AssetMoves)
        {
            var guid = AssetDatabase.AssetPathToGUID(from);
            if (string.IsNullOrEmpty(guid)) throw new Exception("Asset introuvable : " + from);
            EnsureFolder(Path.GetDirectoryName(to).Replace('\\', '/'));
            var err = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(err)) throw new Exception($"{from} -> {to} : {err}");
            if (AssetDatabase.AssetPathToGUID(to) != guid) throw new Exception("GUID modifié : " + to);
        }
        Debug.Log($"[Ren] {AssetMoves.Length} assets déplacés/renommés (GUID conservés)");

        AssetDatabase.DeleteAsset(R + "UI/Boutons/Bouton_Bois_Fleche.png");
        foreach (var f in new[] { R + "Prefabs/Scene", R + "Terrain" })
            if (AssetDatabase.IsValidFolder(f) && !Directory.EnumerateFiles(f, "*", SearchOption.AllDirectories).Any(x => !x.EndsWith(".meta")))
                AssetDatabase.DeleteAsset(f);
        AssetDatabase.Refresh();
    }

    // ---------------------------------------------------------------- Objets de scène
    static List<Transform> All(Scene s) => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToList();

    static void Rename(Scene s, string from, string to)
    {
        var hits = All(s).Where(t => t.name == from).ToList();
        foreach (var t in hits) t.name = to;
        if (hits.Count > 0) Debug.Log($"[Ren] {s.name} : '{from}' -> '{to}' ({hits.Count})");
    }

    static void SetupScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        ApplyFog();

        Rename(scene, "Directional Light", "Lumiere_Lune");
        Rename(scene, "Global Volume", "Volume_PostProcess");
        Rename(scene, "Plane", "Sol");
        Rename(scene, "XR Origin (XR Rig)", "XR_Rig_Joueur");
        Rename(scene, "XR Interaction Manager", "XR_InteractionManager");
        Rename(scene, "-Decor-", "-Decor_Nature-");

        if (scene.name == "MainMenu")
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(R + "UI/Boutons/Bouton_Bois_Vide.png");
            foreach (var name in new[] { "JouerButton", "QuitterButton" })
            {
                var go = All(scene).First(t => t.name == name).gameObject;
                go.GetComponent<Image>().sprite = sprite;
                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(176f * sprite.rect.width / sprite.rect.height, 176f);
                var trt = (RectTransform)go.GetComponentInChildren<TMPro.TextMeshProUGUI>().transform;
                trt.offsetMin = new Vector2(20f, 10f);
                trt.offsetMax = new Vector2(-20f, -10f);
            }
            Rename(scene, "JouerButton", "Bouton_Jouer");
            Rename(scene, "QuitterButton", "Bouton_Quitter");
            Rename(scene, "MenuCanvas", "Canvas_MenuPrincipal");
            Rename(scene, "Logo", "Logo_FruityNinjaVR");
            Rename(scene, "Ambiance_Nuit", "Audio_Ambiance_Nuit");
            Rename(scene, "Musique_Menu", "Audio_Musique_Menu");
        }
        else
        {
            // Sabres (instances du prefab Nodachi de JTS, attachés aux contrôleurs)
            foreach (var t in All(scene).Where(t => t.name == "Nodachi").ToList())
                t.name = t.parent != null && t.parent.name.StartsWith("Left") ? "Sabre_Gauche" : "Sabre_Droit";

            // Menu de difficulté : fruits, fantômes, étiquettes (associées au fruit le plus proche)
            var fruits = All(scene).Select(t => t.GetComponent<DifficultyFruit>()).Where(d => d != null).ToList();
            var menu = fruits[0].transform.parent;
            foreach (var label in Enumerable.Range(0, menu.childCount).Select(i => menu.GetChild(i)).Where(c => c.GetComponent<Canvas>() != null).ToList())
            {
                var near = fruits.OrderBy(f => Vector3.Distance(f.transform.position, label.GetComponentInChildren<RectTransform>().position)).First();
                label.name = "Etiquette_" + near.difficulty;
            }
            foreach (var f in fruits)
            {
                f.gameObject.name = "FruitDifficulte_" + f.difficulty;
                if (f.phantom != null) f.phantom.name = "FruitDifficulte_" + f.difficulty + "_Fantome";
            }
            menu.name = "MenuDifficulte";

            Rename(scene, "StartFruitSpawnPoint ", "PointApparition_FruitDemarrage");
            Rename(scene, "FruitJuiceFX", "FX_Fruit_JusDecoupe_Modele");
            var spawners = All(scene).FirstOrDefault(t => t.name == "-Spawners-");
            if (spawners != null)
            {
                for (int i = 0; i < spawners.childCount; i++) spawners.GetChild(i).name = $"Lanceur_{i + 1:00}";
                spawners.name = "-Lanceurs_Fruits-";
            }
            Rename(scene, "UI_Canva", "Canvas_HUD");
            Rename(scene, "HighScore", "HighScoreText");
            Rename(scene, "ReadyText", "PretText");
            Rename(scene, "Ambiance_Nuit", "Audio_Ambiance_Nuit");
            Rename(scene, "GameMusicManager", "Audio_GameMusicManager");
            Rename(scene, "Stinger_Start", "Audio_Stinger_Depart");
            Rename(scene, "Musique_Combat", "Audio_Musique_Combat");
            Rename(scene, "SFX_Shine", "Audio_Sabre_Reflet");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Sauvegarde impossible : " + path);
        Debug.Log($"[Ren] {scene.name} : " + string.Join(" | ", scene.GetRootGameObjects().Select(r => r.name)));
    }

    static void RenameDebugScene()
    {
        var scene = EditorSceneManager.OpenScene(R + "Scenes/Debug_Fruits.unity", OpenSceneMode.Single);
        Rename(scene, "-Fruits-", "-Anciens_Modeles_Spawner-");
        Rename(scene, "-Fruits_Affichage-", "-Fruits_Affichage_Debug-");
        Rename(scene, "BombBall_Affichage", "Bombe_Affichage_Debug");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Vérification (point 4)
    static int CheckObject(UnityEngine.Object o, string where, List<string> issues)
    {
        int n = 0;
        if (o is GameObject go)
        {
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missing > 0) { issues.Add($"{where} : {missing} script(s) manquant(s)"); n++; }
            foreach (var r in go.GetComponents<Renderer>())
                if (r.sharedMaterials.Any(m => m == null)) { issues.Add($"{where} : matériau manquant sur {r.GetType().Name}"); n++; }
            foreach (var c in go.GetComponents<Component>().Where(c => c != null))
                n += CheckSerialized(c, where + "/" + c.GetType().Name, issues);
        }
        else n += CheckSerialized(o, where, issues);
        return n;
    }

    static int CheckSerialized(UnityEngine.Object o, string where, List<string> issues)
    {
        int n = 0;
        var so = new SerializedObject(o);
        var p = so.GetIterator();
        while (p.NextVisible(true))
        {
            if (p.propertyType == SerializedPropertyType.ObjectReference && p.objectReferenceValue == null && p.objectReferenceInstanceIDValue != 0)
            {
                issues.Add($"{where} : référence cassée '{p.propertyPath}'");
                n++;
            }
        }
        return n;
    }

    static void Verify()
    {
        var issues = new List<string>();
        int checkedCount = 0;
        foreach (var path in new[] { "MainMenu", "Game", "Debug_Fruits" }.Select(n => $"{R}Scenes/{n}.unity"))
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var t in All(scene)) { CheckObject(t.gameObject, $"{scene.name}:{t.name}", issues); checkedCount++; }
        }
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/FruitNinja" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) { CheckObject(t.gameObject, $"{Path.GetFileName(path)}:{t.name}", issues); checkedCount++; }
        }
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/FruitNinja" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader") issues.Add($"{path} : shader introuvable");
            CheckSerialized(m, path, issues);
            checkedCount++;
        }
        foreach (var s in EditorBuildSettings.scenes)
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path) == null) issues.Add("Build Settings : scène introuvable " + s.path);

        Debug.Log($"[Verif] {checkedCount} objets/prefabs/matériaux vérifiés, {issues.Count} problème(s)");
        foreach (var i in issues.Distinct()) Debug.Log("[Verif] " + i);
        Debug.Log("[Verif] Build : " + string.Join(" ; ", EditorBuildSettings.scenes.Select((s, i) => $"{i}:{s.path} ({(s.enabled ? "activée" : "désactivée")})")));
    }

    // ---------------------------------------------------------------- Liste du point 3
    static void ListPoint3()
    {
        var scene = EditorSceneManager.OpenScene(R + "Scenes/Game.unity", OpenSceneMode.Single);
        var spawner = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<FruitSpawnerManager>(true)).First();
        Debug.Log("[P3] Fruits lancés : " + string.Join(", ", spawner.fruitPrefabs.Select(AssetDatabase.GetAssetPath)));
        Debug.Log("[P3] Bonus : " + AssetDatabase.GetAssetPath(spawner.goldenFruitPrefab) + " ; Bombe : " + AssetDatabase.GetAssetPath(spawner.bombPrefab));
        var gm = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<GameManager>(true)).First();
        Debug.Log("[P3] Fruit de départ : " + AssetDatabase.GetAssetPath(gm.startFruitPrefab));
        foreach (var s in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SwordSlicer>(true)))
            Debug.Log($"[P3] SwordSlicer sur '{s.name}' : explosion={AssetDatabase.GetAssetPath(s.explosionVFX)} jus={AssetDatabase.GetAssetPath(s.fruitJuiceFXPrefab)} coupe={AssetDatabase.GetAssetPath(s.crossSectionMaterial)}");
        foreach (var t in All(scene).Where(t => t.name.StartsWith("Sabre_")))
            Debug.Log($"[P3] {t.name} : prefab source {AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject))}, parent {t.parent?.name}");
    }
}
#endif
