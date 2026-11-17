using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sur Quest, coupe le bloom du post-traitement de cette scène (décor ToonScapes) :
/// effet coûteux sur la puce mobile et trop fort sur les surfaces brillantes du pack.
/// Le profil est copié au lancement, l'asset d'origine n'est pas modifié.
/// </summary>
[RequireComponent(typeof(Volume))]
public class BloomQuest : MonoBehaviour
{
    public bool desactiverSurQuest = true;

    private void Awake()
    {
        if (!PlatformFx.IsQuest || !desactiverSurQuest) return;
        var volume = GetComponent<Volume>();
        if (volume.profile != null && volume.profile.TryGet(out Bloom bloom))
            bloom.active = false;
    }
}
