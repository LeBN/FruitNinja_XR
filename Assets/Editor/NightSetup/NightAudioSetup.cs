// Script temporaire : ambiance de nuit (lune, ciel étoilé, ambiant, brume) + audio des scènes.
// Lancé en batchmode via -executeMethod, puis supprimé.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class NightAudioSetup
{
    const string SkyTexPath = "Assets/FruitNinja/Textures/Ciel/Ciel_NuitEtoilee.png";
    const string SkyMatPath = "Assets/FruitNinja/Materials/Skybox_NuitEtoilee.mat";
    const string MenuMusic = "Assets/FruitNinja/Sons/Musiques/Menu_MysticalGlades.ogg";
    const string StartStinger = "Assets/FruitNinja/Sons/Musiques/Start_EnGarde.ogg";
    const string BattleMusic = "Assets/FruitNinja/Sons/Musiques/Jeu_DesertBattle.ogg";
    const string NightAmbience = "Assets/FruitNinja/Sons/Ambiances/Nuit_Forest_Rain_Crickets_Frogs.ogg";

    // Lune : devant le joueur (+Z), un peu à droite, assez haute pour être visible
    const float MoonAzimuthDeg = 20f;
    const float MoonElevationDeg = 22f;
    const float MoonRadiusDeg = 2.4f;

    static Vector3 MoonDir
    {
        get
        {
            float az = MoonAzimuthDeg * Mathf.Deg2Rad, el = MoonElevationDeg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(az) * Mathf.Cos(el), Mathf.Sin(el), Mathf.Cos(az) * Mathf.Cos(el)).normalized;
        }
    }

    public static void Run()
    {
        try
        {
            ConfigureAudioImport();
            var sky = CreateSkybox();
            SetupScene("Assets/FruitNinja/Scenes/MainMenu.unity", sky, isMenu: true);
            SetupScene("Assets/FruitNinja/Scenes/Game.unity", sky, isMenu: false);
            AssetDatabase.SaveAssets();
            Debug.Log("[Night] OK");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError("[Night] " + e);
            EditorApplication.Exit(1);
        }
    }

    // ---------------------------------------------------------------- Audio
    static void ConfigureAudioImport()
    {
        void Set(string path, AudioClipLoadType load, float quality, bool preload)
        {
            var imp = (AudioImporter)AssetImporter.GetAtPath(path) ?? throw new Exception("Audio introuvable : " + path);
            var s = imp.defaultSampleSettings;
            s.loadType = load;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = quality;
            s.preloadAudioData = preload;
            imp.defaultSampleSettings = s;
            imp.loadInBackground = load == AudioClipLoadType.Streaming;
            imp.SaveAndReimport();
            Debug.Log($"[Night] import {Path.GetFileName(path)} : {load}, Vorbis {quality}");
        }
        Set(MenuMusic, AudioClipLoadType.Streaming, 0.6f, false);
        Set(BattleMusic, AudioClipLoadType.Streaming, 0.6f, false);
        Set(NightAmbience, AudioClipLoadType.Streaming, 0.5f, false);
        Set(StartStinger, AudioClipLoadType.DecompressOnLoad, 0.7f, true);
    }

    static AudioSource AddSource(Transform parent, string name, string clipPath, float volume, bool loop, bool playOnAwake)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var src = go.AddComponent<AudioSource>();
        src.clip = clipPath != null ? AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath) : null;
        if (clipPath != null && src.clip == null) throw new Exception("Clip introuvable : " + clipPath);
        src.volume = volume;
        src.loop = loop;
        src.playOnAwake = playOnAwake;
        src.spatialBlend = 0f; // musique / ambiance non spatialisées
        return src;
    }

    // ---------------------------------------------------------------- Ciel
    static Material CreateSkybox()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SkyTexPath));
        File.WriteAllBytes(SkyTexPath, GenerateStarfield(4096, 2048).EncodeToPNG());
        AssetDatabase.ImportAsset(SkyTexPath, ImportAssetOptions.ForceSynchronousImport);

        var ti = (TextureImporter)AssetImporter.GetAtPath(SkyTexPath);
        ti.textureType = TextureImporterType.Default;
        ti.textureShape = TextureImporterShape.Texture2D;
        ti.sRGBTexture = true;
        ti.mipmapEnabled = false;
        ti.wrapModeU = TextureWrapMode.Repeat;
        ti.wrapModeV = TextureWrapMode.Clamp;
        ti.filterMode = FilterMode.Bilinear;
        ti.maxTextureSize = 4096;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(SkyTexPath);
        var mat = new Material(Shader.Find("Skybox/Panoramic") ?? throw new Exception("Shader Skybox/Panoramic introuvable"));
        mat.SetTexture("_MainTex", tex);
        mat.SetFloat("_Mapping", 1f);   // Latitude-Longitude
        mat.SetFloat("_ImageType", 0f); // 360°
        mat.SetFloat("_Exposure", 1f);
        mat.SetFloat("_Rotation", 0f);
        mat.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 0.5f));
        Directory.CreateDirectory(Path.GetDirectoryName(SkyMatPath));
        AssetDatabase.CreateAsset(mat, SkyMatPath);
        Debug.Log("[Night] skybox créée : " + SkyMatPath);
        return mat;
    }

    // Direction monde d'un pixel, selon la projection de Skybox/Panoramic (lat-long)
    static Vector3 PixelDir(float u, float v)
    {
        float lat = (1f - v) * Mathf.PI;
        float lon = (0.5f - u) * 2f * Mathf.PI;
        return new Vector3(Mathf.Sin(lat) * Mathf.Cos(lon), Mathf.Cos(lat), Mathf.Sin(lat) * Mathf.Sin(lon));
    }

    static Vector2 DirToUV(Vector3 d)
    {
        float u = 0.5f - Mathf.Atan2(d.z, d.x) / (2f * Mathf.PI);
        float v = 1f - Mathf.Acos(Mathf.Clamp(d.y, -1f, 1f)) / Mathf.PI;
        return new Vector2(Mathf.Repeat(u, 1f), v);
    }

    static Texture2D GenerateStarfield(int W, int H)
    {
        var col = new Color[W * H];
        var rng = new System.Random(20251110);
        float R() => (float)rng.NextDouble();

        Color zenith = new Color(0.010f, 0.016f, 0.050f);
        Color horizon = new Color(0.055f, 0.085f, 0.175f);
        Color below = new Color(0.020f, 0.028f, 0.050f);
        Color moonGlow = new Color(0.55f, 0.65f, 0.85f);
        Vector3 moon = MoonDir;
        Vector3 milkyN = new Vector3(0.35f, 0.55f, -0.76f).normalized; // plan de la Voie lactée

        // Fond : dégradé + halo lunaire + lueur diffuse de la Voie lactée
        for (int y = 0; y < H; y++)
        {
            float v = (y + 0.5f) / H;
            for (int x = 0; x < W; x++)
            {
                var d = PixelDir((x + 0.5f) / W, v);
                Color c = d.y >= 0f
                    ? Color.Lerp(horizon, zenith, Mathf.Pow(d.y, 0.55f))
                    : Color.Lerp(horizon * 0.8f, below, Mathf.Pow(-d.y, 0.4f));

                float a = Mathf.Acos(Mathf.Clamp(Vector3.Dot(d, moon), -1f, 1f));
                c += moonGlow * (0.20f * Mathf.Exp(-Mathf.Pow(a / 0.35f, 2f)) + 0.35f * Mathf.Exp(-Mathf.Pow(a / 0.07f, 2f)));

                if (d.y > 0f)
                {
                    float band = Mathf.Exp(-Mathf.Pow(Vector3.Dot(d, milkyN) / 0.16f, 2f));
                    float wobble = 0.6f + 0.4f * Mathf.Sin(d.x * 7.1f + d.z * 5.3f) * Mathf.Sin(d.y * 9.7f + d.x * 3.1f);
                    c += new Color(0.07f, 0.075f, 0.10f) * band * wobble * Mathf.SmoothStep(0f, 0.3f, d.y);
                }
                col[y * W + x] = c;
            }
        }

        // Étoiles
        void Star(Vector3 d, float brightness, float radiusPx, Color tint)
        {
            if (d.y < 0.01f) return;
            brightness *= Mathf.SmoothStep(0f, 0.25f, d.y); // extinction près de l'horizon
            var uv = DirToUV(d);
            float cx = uv.x * W, cy = uv.y * H;
            float sinLat = Mathf.Max(Mathf.Sqrt(1f - d.y * d.y), 0.05f);
            float rx = radiusPx / sinLat, ry = radiusPx;
            int ix0 = Mathf.FloorToInt(cx - rx - 1), ix1 = Mathf.CeilToInt(cx + rx + 1);
            int iy0 = Mathf.Max(0, Mathf.FloorToInt(cy - ry - 1)), iy1 = Mathf.Min(H - 1, Mathf.CeilToInt(cy + ry + 1));
            for (int py = iy0; py <= iy1; py++)
                for (int pxi = ix0; pxi <= ix1; pxi++)
                {
                    float dx = (pxi + 0.5f - cx) / rx, dy = (py + 0.5f - cy) / ry;
                    float w = Mathf.Exp(-(dx * dx + dy * dy) * 2.2f);
                    if (w < 0.01f) continue;
                    int px = ((pxi % W) + W) % W;
                    col[py * W + px] += tint * (brightness * w);
                }
        }

        Vector3 RandomDir()
        {
            float z = R() * 2f - 1f, t = R() * 2f * Mathf.PI, r = Mathf.Sqrt(1f - z * z);
            return new Vector3(r * Mathf.Cos(t), z, r * Mathf.Sin(t));
        }

        Color[] tints = { new Color(1f, 1f, 1f), new Color(0.80f, 0.88f, 1f), new Color(1f, 0.93f, 0.80f), new Color(0.90f, 0.95f, 1f) };
        for (int i = 0; i < 9000; i++)
        {
            float b = Mathf.Pow(R(), 4f);
            Star(RandomDir(), 0.25f + 1.6f * b, 0.7f + 1.4f * b, tints[rng.Next(tints.Length)]);
        }
        // Étoiles faibles concentrées dans la Voie lactée
        for (int i = 0; i < 14000; i++)
        {
            var d = RandomDir();
            d = (d - milkyN * Vector3.Dot(d, milkyN) * (1f - Mathf.Abs((R() - 0.5f) * 0.3f))).normalized;
            Star(d, 0.08f + 0.25f * Mathf.Pow(R(), 3f), 0.6f, tints[rng.Next(tints.Length)]);
        }

        // Lune : disque avec assombrissement du bord et quelques "mers"
        Vector3 t1 = Vector3.Cross(Vector3.up, moon).normalized, t2 = Vector3.Cross(moon, t1);
        float rad = MoonRadiusDeg * Mathf.Deg2Rad;
        var craters = Enumerable.Range(0, 9).Select(_ => new Vector3(R() * 1.3f - 0.65f, R() * 1.3f - 0.65f, 0.12f + R() * 0.22f)).ToArray();
        var uvMoon = DirToUV(moon);
        int span = Mathf.CeilToInt(rad / Mathf.PI * H) + 4;
        int mcx = Mathf.RoundToInt(uvMoon.x * W), mcy = Mathf.RoundToInt(uvMoon.y * H);
        for (int py = mcy - span; py <= mcy + span; py++)
            for (int pxi = mcx - span * 2; pxi <= mcx + span * 2; pxi++)
            {
                if (py < 0 || py >= H) continue;
                int px = ((pxi % W) + W) % W;
                var d = PixelDir((px + 0.5f) / W, (py + 0.5f) / H);
                float lx = Vector3.Dot(d, t1) / Mathf.Tan(rad), ly = Vector3.Dot(d, t2) / Mathf.Tan(rad);
                float r2 = lx * lx + ly * ly;
                float edge = Mathf.Clamp01((1f - Mathf.Sqrt(r2)) * 25f);
                if (edge <= 0f || Vector3.Dot(d, moon) < 0f) continue;
                float mu = Mathf.Sqrt(Mathf.Max(0f, 1f - r2));
                float shade = 0.72f + 0.28f * mu;
                foreach (var cr in craters)
                {
                    float cd = ((lx - cr.x) * (lx - cr.x) + (ly - cr.y) * (ly - cr.y)) / (cr.z * cr.z);
                    if (cd < 1f) shade *= 1f - 0.16f * (1f - cd);
                }
                var moonCol = new Color(0.96f, 0.95f, 0.88f) * shade;
                col[py * W + px] = Color.Lerp(col[py * W + px], moonCol, edge);
            }

        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        tex.SetPixels(col.Select(c => new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b))).ToArray());
        tex.Apply();
        return tex;
    }

    // ---------------------------------------------------------------- Scènes
    static GameObject Root(Scene s, string name) =>
        s.GetRootGameObjects().FirstOrDefault(g => g.name == name) ?? throw new Exception($"'{name}' introuvable dans {s.name}");

    static void SetupScene(string path, Material sky, bool isMenu)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // Lune
        var lights = Root(scene, "--Lights&Effects--");
        var lightTr = lights.transform.Find("Directional Light") ?? throw new Exception("Directional Light introuvable");
        var light = lightTr.GetComponent<Light>();
        lightTr.rotation = Quaternion.LookRotation(-MoonDir, Vector3.up);
        light.color = new Color(0.66f, 0.76f, 1f);
        light.intensity = 0.9f;
        light.shadowStrength = 0.75f;
        EditorUtility.SetDirty(light);

        // Ciel, ambiant, brume
        RenderSettings.skybox = sky;
        RenderSettings.sun = light;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.22f, 0.27f, 0.45f);
        RenderSettings.ambientEquatorColor = new Color(0.14f, 0.17f, 0.30f);
        RenderSettings.ambientGroundColor = new Color(0.06f, 0.07f, 0.11f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.035f, 0.05f, 0.10f);
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        DynamicGI.UpdateEnvironment();

        // Audio
        var audio = new GameObject("--Audio--");
        SceneManager.MoveGameObjectToScene(audio, scene);
        AddSource(audio.transform, "Ambiance_Nuit", NightAmbience, 0.55f, loop: true, playOnAwake: true);

        if (isMenu)
        {
            AddSource(audio.transform, "Musique_Menu", MenuMusic, 0.5f, loop: true, playOnAwake: true);
        }
        else
        {
            var musicGo = new GameObject("GameMusicManager");
            musicGo.transform.SetParent(audio.transform, false);
            var mm = musicGo.AddComponent<GameMusicManager>();
            mm.stingerSource = AddSource(musicGo.transform, "Stinger_Start", null, 0.8f, loop: false, playOnAwake: false);
            mm.musicSource = AddSource(musicGo.transform, "Musique_Combat", null, 0.45f, loop: true, playOnAwake: false);
            mm.startStinger = AssetDatabase.LoadAssetAtPath<AudioClip>(StartStinger);
            mm.battleMusic = AssetDatabase.LoadAssetAtPath<AudioClip>(BattleMusic);
            if (mm.startStinger == null || mm.battleMusic == null) throw new Exception("Clips de jeu introuvables");

            var gm = Root(scene, "--Managers--").GetComponentInChildren<GameManager>(true) ?? throw new Exception("GameManager introuvable");
            gm.music = mm;
            EditorUtility.SetDirty(gm);
        }

        // Place --Audio-- juste après --Lights&Effects--
        audio.transform.SetSiblingIndex(lights.transform.GetSiblingIndex() + 1);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Sauvegarde impossible : " + path);
        Debug.Log($"[Night] {scene.name} : " + string.Join(" | ", scene.GetRootGameObjects().Select(r => r.name)));
    }
}
#endif
