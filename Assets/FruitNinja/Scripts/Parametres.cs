using System;
using UnityEngine;

/// <summary>
/// Paramètres du joueur (menu Paramètres), sauvegardés dans les PlayerPrefs :
/// volumes (général, musique et ambiance, effets), vibrations, décor détaillé et type de rotation.
/// </summary>
public static class Parametres
{
    const string KGeneral = "param_volume_general";
    const string KMusique = "param_volume_musique";
    const string KEffets = "param_volume_effets";
    const string KVibrations = "param_vibrations";
    const string KHighPoly = "param_high_poly";
    const string KRotation = "param_rotation_continue";

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

    /// <summary>Décor détaillé ToonScapes (« High poly ») : activé par défaut sur PC, désactivé sur Quest.</summary>
    public static bool HighPoly
    {
        get => PlayerPrefs.GetInt(KHighPoly, PlatformFx.IsQuest ? 0 : 1) == 1;
        set { PlayerPrefs.SetInt(KHighPoly, value ? 1 : 0); Appliquer(); }
    }

    /// <summary>Rotation au joystick : continue (true) ou instantanée par crans (false, par défaut, plus confortable).</summary>
    public static bool RotationContinue
    {
        get => PlayerPrefs.GetInt(KRotation, 0) == 1;
        set { PlayerPrefs.SetInt(KRotation, value ? 1 : 0); Appliquer(); }
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
