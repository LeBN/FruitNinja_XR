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

    /// <summary>Rouvre le panneau Paramètres après un changement de décor (rechargement du menu).</summary>
    public static bool RouvrirParametres;

    private void Awake()
    {
#if !UNITY_EDITOR
        // Au lancement du jeu : menu du décor choisi (ToonScapes sur PC par défaut, d'origine sur Quest)
        string menu = Decor.Menu;
        if (menu != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(menu);
            return;
        }
#endif
    }

    private void Start()
    {
        Afficher(RouvrirParametres);
        RouvrirParametres = false;
    }

    public void Play()
    {
        // Scène de jeu du décor choisi (option « Décor détaillé » des Paramètres)
        SceneManager.LoadScene(Decor.Jeu);
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
