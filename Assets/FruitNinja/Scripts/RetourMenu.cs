using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bouton « Retour » (scène de debug) : revient au menu principal qui l'a ouverte
/// (menu classique ou menu ToonScapes), sinon au menu indiqué ici.
/// </summary>
public class RetourMenu : MonoBehaviour
{
    public string sceneMenu = "MainMenu";

    /// <summary>Dernier menu principal ouvert (renseigné par MainMenuManager).</summary>
    public static string DernierMenu;

    public void Retour() => SceneManager.LoadScene(string.IsNullOrEmpty(DernierMenu) ? sceneMenu : DernierMenu);
}
