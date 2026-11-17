using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sur Quest (décor ToonScapes), garde le bloom mais réduit sa portée : étalement plus court,
/// moins de niveaux de flou (aussi moins coûteux) et seuil un peu plus haut pour les surfaces
/// brillantes du pack. Le profil est copié au lancement, l'asset d'origine n'est pas modifié.
/// </summary>
[RequireComponent(typeof(Volume))]
public class BloomQuest : MonoBehaviour
{
    [Range(0f, 1f)] public float etalement = 0.3f;
    [Range(2, 8)] public int niveauxDeFlou = 4;
    public float seuil = 1.15f;

    private void Awake()
    {
        if (!PlatformFx.IsQuest) return;
        var volume = GetComponent<Volume>();
        if (volume.profile == null || !volume.profile.TryGet(out Bloom bloom)) return;
        bloom.scatter.Override(etalement);
        bloom.maxIterations.Override(niveauxDeFlou);
        bloom.threshold.Override(Mathf.Max(bloom.threshold.value, seuil));
    }
}
