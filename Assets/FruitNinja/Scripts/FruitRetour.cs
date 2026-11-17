using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fruit « Retour » (à gauche du menu de difficulté) : le couper ramène au menu principal.
/// Masqué pendant la partie pour ne pas quitter par erreur.
/// </summary>
public class FruitRetour : MonoBehaviour
{
    public string sceneMenu = "MainMenu";
    [Tooltip("Délai avant le changement de scène, pour voir les morceaux tomber (s).")]
    public float delai = 0.7f;
    [Tooltip("Étiquette « RETOUR » affichée au-dessus du fruit.")]
    public GameObject etiquette;

    private bool coupe;

    /// <summary>Appelé par SwordSlicer quand la lame coupe ce fruit.</summary>
    public void OnSliced()
    {
        if (coupe) return;
        coupe = true;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (etiquette != null) etiquette.SetActive(false);
        StartCoroutine(Retour());
    }

    public void SetVisible(bool visible)
    {
        if (coupe) return;
        gameObject.SetActive(visible);
        if (etiquette != null) etiquette.SetActive(visible);
    }

    private IEnumerator Retour()
    {
        yield return new WaitForSeconds(delai);
        SceneManager.LoadScene(sceneMenu);
    }
}
