using TMPro;
using UnityEngine;

public enum Difficulty { Facile, Normal, Hardcore }

/// <summary>
/// Fruit de sélection de difficulté (scène de jeu). Quand il est coupé, il disparaît et
/// sa version fantôme reste à sa place ; il réapparaît si une autre difficulté est choisie.
/// </summary>
public class DifficultyFruit : MonoBehaviour
{
    public Difficulty difficulty;
    [Tooltip("Copie fantôme (translucide) affichée à la place du fruit choisi. Désactivée par défaut.")]
    public GameObject phantom;
    public DifficultySelector selector;
    [Tooltip("Étiquette (texte) du fruit, masquée pendant la partie.")]
    public GameObject etiquette;

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private Rigidbody body;
    private TMP_Text texte;
    private string texteBase;

    private void Awake()
    {
        // Position locale : le menu entier peut monter/descendre selon la taille du joueur
        homePosition = transform.localPosition;
        homeRotation = transform.localRotation;
        body = GetComponent<Rigidbody>();
        if (selector == null) selector = FindFirstObjectByType<DifficultySelector>();
        if (etiquette != null)
        {
            texte = etiquette.GetComponentInChildren<TMP_Text>(true);
            if (texte != null) texteBase = texte.text;
        }
    }

    /// <summary>Affiche la durée de la partie sous le nom de la difficulté choisie.</summary>
    public void AfficherDuree(bool choisi, float duree)
    {
        if (texte == null) return;
        texte.text = choisi ? $"{texteBase} <size=60%>· {Mathf.RoundToInt(duree)} s</size>" : texteBase;
    }

    /// <summary>Appelé par SwordSlicer quand la lame coupe ce fruit.</summary>
    public void OnSliced()
    {
        if (selector != null) selector.Select(this);
    }

    public void ShowPhantom()
    {
        gameObject.SetActive(false);
        if (phantom != null) phantom.SetActive(true);
        if (etiquette != null) etiquette.SetActive(true);
    }

    public void ShowFruit()
    {
        if (phantom != null) phantom.SetActive(false);
        transform.localPosition = homePosition;
        transform.localRotation = homeRotation;
        if (body != null && !body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        gameObject.SetActive(true);
        if (etiquette != null) etiquette.SetActive(true);
    }

    public void HideAll()
    {
        gameObject.SetActive(false);
        if (phantom != null) phantom.SetActive(false);
        if (etiquette != null) etiquette.SetActive(false);
    }
}
