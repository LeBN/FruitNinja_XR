using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// En partie (scènes de jeu), seuls les sabres comptent : aucun pointeur ne doit s'afficher.
/// Le ControllerInputActionManager des Starter Assets réactive le rayon des menus (Near-Far)
/// à chaque fin de visée de téléportation : désactiver l'objet dans la scène ne suffit donc pas.
/// Ce composant coupe, à chaque image et avant le rendu, le rayon des menus, la téléportation
/// (arc + réticule ; on se déplace au joystick) et le « poke ». Les menus et la scène de debug
/// gardent leurs rayons.
/// </summary>
[DefaultExecutionOrder(1000)]
public class MasquerPointeurs : MonoBehaviour
{
    [Tooltip("Préfixe des scènes où les pointeurs sont masqués (scènes de jeu).")]
    public string prefixeScenesJeu = "Game";

    private NearFarInteractor[] nearFar;
    private XRRayInteractor[] rayons;
    private XRPokeInteractor[] pokes;
    private bool actif;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += Changement;
        Preparer();
    }

    private void OnDisable() => SceneManager.activeSceneChanged -= Changement;

    private void Changement(Scene a, Scene b) => Preparer();

    private void Preparer()
    {
        actif = SceneManager.GetActiveScene().name.StartsWith(prefixeScenesJeu);
        nearFar = GetComponentsInChildren<NearFarInteractor>(true);
        rayons = GetComponentsInChildren<XRRayInteractor>(true);   // téléportation
        pokes = GetComponentsInChildren<XRPokeInteractor>(true);
        Appliquer();
    }

    private void LateUpdate() => Appliquer();

    private void Appliquer()
    {
        if (!actif) return;
        foreach (var i in nearFar) if (i != null && i.gameObject.activeSelf) i.gameObject.SetActive(false);
        foreach (var i in rayons) if (i != null && i.gameObject.activeSelf) i.gameObject.SetActive(false);
        foreach (var i in pokes) if (i != null && i.gameObject.activeSelf) i.gameObject.SetActive(false);
    }
}
