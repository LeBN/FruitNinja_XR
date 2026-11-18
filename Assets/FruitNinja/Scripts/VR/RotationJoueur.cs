using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Applique le type de rotation choisi dans les Paramètres (instantanée par crans ou continue)
/// aux manettes du rig XRI, à chaque chargement de scène et à chaque changement de réglage.
/// Passe par la propriété « smoothTurnEnabled » du ControllerInputActionManager des Starter Assets.
/// </summary>
public static class RotationJoueur
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialiser()
    {
        SceneManager.sceneLoaded += (_, __) => Appliquer();
        Parametres.Change += Appliquer;
        Appliquer();
    }

    public static void Appliquer()
    {
        bool continue_ = Parametres.RotationContinue;
        foreach (var comp in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (comp == null || comp.GetType().Name != "ControllerInputActionManager") continue;
            var prop = comp.GetType().GetProperty("smoothTurnEnabled");
            if (prop != null && prop.CanWrite) prop.SetValue(comp, continue_);
        }
    }
}
