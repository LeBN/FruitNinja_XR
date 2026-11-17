using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal : boutons "Jouer" et "Quitter" (canvas world-space, cliquables au rayon XR).
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Nom de la scène de jeu (doit être dans les Build Profiles / Build Settings).")]
    public string gameSceneName = "Game";

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
}
