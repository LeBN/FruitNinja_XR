using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Bouton « Retour » (scène de debug) : revient au menu principal du décor choisi.</summary>
public class RetourMenu : MonoBehaviour
{
    public void Retour() => SceneManager.LoadScene(Decor.Menu);
}
