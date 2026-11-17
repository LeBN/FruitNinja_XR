using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal : boutons "Jouer", "Quitter" et engrenage "Paramètres"
/// (canvas world-space, cliquables au rayon XR). Le panneau Paramètres donne accès à la scène de debug.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Nom de la scène de jeu (doit être dans les Build Profiles / Build Settings).")]
    public string gameSceneName = "Game";
    [Tooltip("Scène de debug (affichage des fruits), ouverte depuis les Paramètres.")]
    public string debugSceneName = "Debug_Fruits";

    [Header("Paramètres")]
    [Tooltip("Éléments de l'accueil masqués quand le panneau Paramètres est ouvert.")]
    public GameObject[] accueil;
    public GameObject panneauParametres;

    private void Start()
    {
        if (panneauParametres != null) panneauParametres.SetActive(false);
    }

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OuvrirParametres() => Afficher(true);

    public void FermerParametres() => Afficher(false);

    public void OuvrirDebug()
    {
        SceneManager.LoadScene(debugSceneName);
    }

    private void Afficher(bool parametres)
    {
        foreach (var go in accueil)
            if (go != null) go.SetActive(!parametres);
        if (panneauParametres != null) panneauParametres.SetActive(parametres);
    }
}
