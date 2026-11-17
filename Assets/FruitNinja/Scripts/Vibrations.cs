using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

/// <summary>
/// Vibrations des manettes. Passe par les HapticImpulsePlayer du rig XRI (fiables en OpenXR),
/// sinon par l'API InputDevices. Désactivables dans les paramètres.
/// </summary>
public static class Vibrations
{
    /// <summary>Vibration des deux manettes.</summary>
    public static void Envoyer(float amplitude, float duree)
    {
        if (!Parametres.Vibrations) return;

        bool envoye = false;
        foreach (var player in Object.FindObjectsByType<HapticImpulsePlayer>(FindObjectsSortMode.None))
            envoye |= player.SendHapticImpulse(amplitude, duree);
        if (envoye) return;

        foreach (var node in new[] { UnityEngine.XR.XRNode.LeftHand, UnityEngine.XR.XRNode.RightHand })
        {
            var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid) device.SendHapticImpulse(0u, amplitude, duree);
        }
    }
}
