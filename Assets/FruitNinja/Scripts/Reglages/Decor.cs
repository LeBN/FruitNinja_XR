using UnityEngine;

/// <summary>
/// Choix du décor selon l'option « Décor détaillé » (High poly) des Paramètres :
/// décor ToonScapes (High poly, activé par défaut sur PC) ou décor d'origine low poly (par défaut sur Quest).
/// </summary>
public static class Decor
{
    public const string MenuLowPoly = "MainMenu";
    public const string MenuHighPoly = "MainMenu_ToonScapes";
    public const string JeuLowPoly = "Game";
    public const string JeuHighPoly = "Game_ToonScapes";

    static bool Disponible(string scene) => Application.CanStreamedLevelBeLoaded(scene);

    /// <summary>Menu principal du décor choisi (repli sur le décor d'origine s'il n'est pas dans le build).</summary>
    public static string Menu => Parametres.HighPoly && Disponible(MenuHighPoly) ? MenuHighPoly : MenuLowPoly;

    /// <summary>Scène de jeu du décor choisi.</summary>
    public static string Jeu => Parametres.HighPoly && Disponible(JeuHighPoly) ? JeuHighPoly : JeuLowPoly;
}
