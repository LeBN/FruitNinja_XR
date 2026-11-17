using UnityEngine;

/// <summary>
/// Sons d'effets en 3D : chaque son est joué depuis l'endroit où il se produit
/// (point de coupe, lanceur, chariot...), avec une atténuation selon la distance.
/// La musique et l'ambiance restent en 2D (fond sonore, pas de position dans la scène).
/// </summary>
public static class SonSpatial
{
    public const float DistanceMin = 0.6f;
    public const float DistanceMax = 20f;

    /// <summary>Joue un son à une position, sur une source 3D temporaire.</summary>
    public static AudioSource Jouer(AudioClip clip, Vector3 position, float volume = 1f, float variationPitch = 0.05f)
    {
        if (clip == null) return null;
        var go = new GameObject("Son_" + clip.name);
        go.transform.position = position;
        var source = go.AddComponent<AudioSource>();
        Configurer3D(source);
        source.clip = clip;
        source.volume = volume;
        source.pitch = 1f + Random.Range(-variationPitch, variationPitch);
        source.Play();
        Object.Destroy(go, clip.length / source.pitch + 0.1f);
        return source;
    }

    /// <summary>Joue un son choisi au hasard dans une liste.</summary>
    public static AudioSource JouerAuHasard(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return null;
        return Jouer(clips[Random.Range(0, clips.Length)], position, volume);
    }

    /// <summary>Règle une source existante en son 3D.</summary>
    public static void Configurer3D(AudioSource source, float distanceMin = DistanceMin, float distanceMax = DistanceMax)
    {
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = distanceMin;
        source.maxDistance = distanceMax;
        source.dopplerLevel = 0f;
    }
}
