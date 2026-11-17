using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bouton « Décor détaillé : Oui / Non » du menu Paramètres (option High poly).
/// Changer l'option recharge le menu dans l'autre décor et rouvre le panneau Paramètres.
/// </summary>
public class ReglageDecor : MonoBehaviour
{
    public TMP_Text texte;

    private void OnEnable() => Afficher();

    /// <summary>Appelé par le bouton.</summary>
    public void Basculer()
    {
        Parametres.HighPoly = !Parametres.HighPoly;
        Afficher();
        string menu = Decor.Menu;
        if (menu != SceneManager.GetActiveScene().name)
        {
            MainMenuManager.RouvrirParametres = true;
            SceneManager.LoadScene(menu);
        }
    }

    private void Afficher()
    {
        if (texte != null) texte.text = Parametres.HighPoly ? "Oui" : "Non";
    }
}
