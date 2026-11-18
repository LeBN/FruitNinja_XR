using UnityEngine;

/// <summary>
/// Applique le réglage « Musique et ambiance » (menu Paramètres) à une source audio :
/// volume = volume réglé dans la scène × volume choisi par le joueur.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VolumeMusique : MonoBehaviour
{
    private AudioSource source;
    private float volumeScene;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        volumeScene = source.volume;
    }

    private void OnEnable()
    {
        Parametres.Change += Appliquer;
        Appliquer();
    }

    private void OnDisable() => Parametres.Change -= Appliquer;

    private void Appliquer()
    {
        if (source != null) source.volume = volumeScene * Parametres.VolumeMusique;
    }
}
