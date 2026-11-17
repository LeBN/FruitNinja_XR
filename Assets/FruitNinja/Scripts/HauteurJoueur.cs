using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// Hauteur des yeux du joueur au-dessus du sol (origine du rig XR), pour placer les fruits
/// à portée de sabre quelle que soit la taille du joueur (debout ou assis).
/// </summary>
public static class HauteurJoueur
{
    /// <summary>
    /// Lit la hauteur du sol et des yeux. Renvoie false tant que le casque n'est pas suivi
    /// (tête à moins de 80 cm du sol : premières images, casque posé...).
    /// </summary>
    public static bool Lire(out float sol, out float yeux)
    {
        var origine = Object.FindFirstObjectByType<XROrigin>();
        var camera = origine != null && origine.Camera != null ? origine.Camera : Camera.main;
        sol = origine != null ? origine.transform.position.y : 0f;
        yeux = camera != null ? camera.transform.position.y : sol;
        return origine != null && camera != null && yeux - sol > 0.8f;
    }

    /// <summary>
    /// Hauteur (monde) à <paramref name="sousLesYeux"/> mètres sous les yeux, limitée entre
    /// <paramref name="minAuSol"/> et <paramref name="maxAuSol"/> au-dessus du sol.
    /// Si le casque n'est pas suivi, renvoie <paramref name="parDefaut"/>.
    /// </summary>
    public static float Cible(float sousLesYeux, float minAuSol, float maxAuSol, float parDefaut)
    {
        if (!Lire(out float sol, out float yeux)) return parDefaut;
        return Mathf.Clamp(yeux - sousLesYeux, sol + minAuSol, sol + maxAuSol);
    }
}
