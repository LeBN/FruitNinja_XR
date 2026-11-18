using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Classement des meilleurs scores (score, difficulté, date), sauvegardé dans les PlayerPrefs.
/// </summary>
public static class Classement
{
    public const int Taille = 20;
    const string Cle = "classement_scores";

    [Serializable]
    public class Entree
    {
        public int score;
        public string difficulte;
        public string date;
        public long id;
    }

    [Serializable]
    private class Liste { public List<Entree> entrees = new List<Entree>(); }

    /// <summary>Appelé quand le classement change.</summary>
    public static event Action Change;

    /// <summary>Identifiant du dernier score ajouté (surligné dans l'affichage).</summary>
    public static long DernierId { get; private set; } = -1;

    public static List<Entree> Lire()
    {
        var json = PlayerPrefs.GetString(Cle, "");
        if (string.IsNullOrEmpty(json)) return new List<Entree>();
        try { return JsonUtility.FromJson<Liste>(json)?.entrees ?? new List<Entree>(); }
        catch { return new List<Entree>(); }
    }

    /// <summary>Ajoute un score ; renvoie son rang (1 = premier) ou 0 s'il n'entre pas dans le classement.</summary>
    public static int Ajouter(int score, Difficulty difficulte)
    {
        if (score <= 0) return 0;
        var e = new Entree { score = score, difficulte = difficulte.ToString(), date = DateTime.Now.ToString("dd/MM"), id = DateTime.Now.Ticks };
        var liste = Lire();
        liste.Add(e);
        liste = liste.OrderByDescending(x => x.score).ThenBy(x => x.id).Take(Taille).ToList();
        PlayerPrefs.SetString(Cle, JsonUtility.ToJson(new Liste { entrees = liste }));
        PlayerPrefs.Save();
        int rang = liste.FindIndex(x => x.id == e.id) + 1;
        DernierId = rang > 0 ? e.id : -1;
        Change?.Invoke();
        return rang;
    }

    public static void Effacer()
    {
        PlayerPrefs.DeleteKey(Cle);
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();
        DernierId = -1;
        Change?.Invoke();
    }
}
