using System;
using UnityEngine;

[Serializable]
public class DifficultySettings
{
    public Difficulty difficulty;
    [Tooltip("Intervalle moyen entre deux vagues (s).")]
    public float spawnInterval = 1.5f;
    [Range(0f, 1f)] public float bombChance = 0.15f;
    public float launchForce = 6f;
    public int minPerWave = 1;
    public int maxPerWave = 3;
}

/// <summary>
/// Sélection de la difficulté en coupant un fruit : le fruit choisi laisse un fantôme,
/// les autres (ré)apparaissent, et les réglages sont appliqués au spawner.
/// Le premier choix fait apparaître le fruit de départ (GameManager).
/// </summary>
public class DifficultySelector : MonoBehaviour
{
    public DifficultyFruit[] fruits;
    public GameManager gameManager;
    public FruitSpawnerManager spawner;
    [Tooltip("Fruit Retour (menu principal), masqué pendant la partie.")]
    public FruitRetour retour;
    [Tooltip("Place le menu à hauteur de sabre selon la taille du joueur.")]
    public PlacementSelonJoueur placement;

    public DifficultySettings[] settings =
    {
        new DifficultySettings { difficulty = Difficulty.Facile, spawnInterval = 1.9f, bombChance = 0.08f, launchForce = 5.5f, minPerWave = 1, maxPerWave = 2 },
        new DifficultySettings { difficulty = Difficulty.Normal, spawnInterval = 1.5f, bombChance = 0.15f, launchForce = 6f, minPerWave = 1, maxPerWave = 3 },
        new DifficultySettings { difficulty = Difficulty.Hardcore, spawnInterval = 1.0f, bombChance = 0.25f, launchForce = 7f, minPerWave = 2, maxPerWave = 4 },
    };

    public bool HasChoice { get; private set; }
    public Difficulty Current { get; private set; }

    private DifficultyFruit chosen;

    public void Select(DifficultyFruit fruit)
    {
        bool first = !HasChoice;
        chosen = fruit;
        Current = fruit.difficulty;
        HasChoice = true;

        foreach (var f in fruits)
        {
            if (f == null) continue;
            if (f == fruit) f.ShowPhantom();
            else f.ShowFruit();
        }

        Apply(fruit.difficulty);
        if (gameManager != null) gameManager.OnDifficultyChosen(first);
    }

    /// <summary>Masque le menu de difficulté pendant la partie, le réaffiche ensuite.</summary>
    public void SetMenuVisible(bool visible)
    {
        if (visible && placement != null) placement.Ajuster();
        if (retour != null) retour.SetVisible(visible);
        foreach (var f in fruits)
        {
            if (f == null) continue;
            if (!visible) f.HideAll();
            else if (f == chosen) f.ShowPhantom();
            else f.ShowFruit();
        }
    }

    private void Apply(Difficulty d)
    {
        if (spawner == null) return;
        var s = Array.Find(settings, x => x.difficulty == d);
        if (s == null) return;
        spawner.spawnInterval = s.spawnInterval;
        spawner.bombSpawnChance = s.bombChance;
        spawner.launchForce = s.launchForce;
        spawner.minObjectsPerWave = s.minPerWave;
        spawner.maxObjectsPerWave = s.maxPerWave;
        Debug.Log($"Difficulté : {d}");
    }
}
