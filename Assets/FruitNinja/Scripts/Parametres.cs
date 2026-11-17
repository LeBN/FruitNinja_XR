using System;
using UnityEngine;

/// <summary>
/// Paramètres du joueur (menu Paramètres), sauvegardés dans les PlayerPrefs :
/// volumes (général, musique et ambiance, effets) et vibrations.
/// </summary>
public static class Parametres
{
    const string KGeneral = "param_volume_general";
    const string KMusique = "param_volume_musique";
    const string KEffets = "param_volume_effets";
    const string KVibrations = "param_vibrations";

    /// <summary>Appelé à chaque changement de réglage.</summary>
    public static event Action Change;

    public static float VolumeGeneral
    {
        get => PlayerPrefs.GetFloat(KGeneral, 1f);
        set { PlayerPrefs.SetFloat(KGeneral, Mathf.Clamp01(value)); Appliquer(); }
    }

    public static float VolumeMusique
    {
        get => PlayerPrefs.GetFloat(KMusique, 1f);
        set { PlayerPrefs.SetFloat(KMusique, Mathf.Clamp01(value)); Appliquer(); }
    }

    public static float VolumeEffets
    {
        get => PlayerPrefs.GetFloat(KEffets, 1f);
        set { PlayerPrefs.SetFloat(KEffets, Mathf.Clamp01(value)); Appliquer(); }
    }

    public static bool Vibrations
    {
        get => PlayerPrefs.GetInt(KVibrations, 1) == 1;
        set { PlayerPrefs.SetInt(KVibrations, value ? 1 : 0); Appliquer(); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AuDemarrage() => AudioListener.volume = VolumeGeneral;

    private static void Appliquer()
    {
        AudioListener.volume = VolumeGeneral;
        PlayerPrefs.Save();
        Change?.Invoke();
    }
}
