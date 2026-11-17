using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Bouton « Retour » (scène de debug) : revient au menu principal.</summary>
public class RetourMenu : MonoBehaviour
{
    public string sceneMenu = "MainMenu";

    public void Retour() => SceneManager.LoadScene(sceneMenu);
}
