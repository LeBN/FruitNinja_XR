using UnityEngine;

/// <summary>
/// Choix des effets selon la plateforme du build :
/// - build Android (Meta Quest autonome) -> variante "Quest" (allégée) ;
/// - build Windows (PCVR) et éditeur en cible Windows -> variante "PCVR".
/// Le symbole UNITY_ANDROID dépend de la cible de build active : chaque build utilise
/// donc automatiquement la version prévue, y compris dans l'éditeur.
/// </summary>
public static class PlatformFx
{
#if UNITY_ANDROID
    public const bool IsQuest = true;
#else
    public const bool IsQuest = false;
#endif

    public static string PlatformName => IsQuest ? "Quest" : "PCVR";

    public static GameObject Pick(GameObject pcvr, GameObject quest)
    {
        if (IsQuest && quest != null) return quest;
        return pcvr != null ? pcvr : quest;
    }
}
