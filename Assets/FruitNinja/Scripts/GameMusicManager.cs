using System.Collections;
using UnityEngine;

/// <summary>
/// Musique de la scène de jeu :
/// - au démarrage d'une partie : stinger "En Garde", puis la musique de combat en boucle,
///   enchaînée exactement à la fin du stinger (PlayScheduled) ;
/// - en fin de partie : fondu de sortie de la musique (l'ambiance continue).
/// Appelé par GameManager.StartGame() / EndGame().
/// </summary>
public class GameMusicManager : MonoBehaviour
{
    [Header("Sources")]
    public AudioSource stingerSource;
    public AudioSource musicSource;

    [Header("Clips")]
    public AudioClip startStinger;
    public AudioClip battleMusic;

    [Header("Réglages")]
    [Range(0f, 1f)] public float musicVolume = 0.45f;
    [Tooltip("La musique de combat démarre ce nombre de secondes avant la fin du stinger.")]
    public float battleMusicLead = 2f;
    public float fadeOutDuration = 1.5f;

    private Coroutine fadeRoutine;

    public void OnGameStart()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        double startTime = AudioSettings.dspTime + 0.05;

        if (stingerSource != null && startStinger != null)
        {
            stingerSource.clip = startStinger;
            stingerSource.loop = false;
            stingerSource.PlayScheduled(startTime);
            double stingerLength = (double)startStinger.samples / startStinger.frequency;
            startTime += System.Math.Max(0.0, stingerLength - battleMusicLead);
        }

        if (musicSource != null && battleMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = battleMusic;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.PlayScheduled(startTime);
        }
    }

    public void OnGameEnd()
    {
        if (musicSource == null) return;
        if (stingerSource != null) stingerSource.Stop();
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float start = musicSource.volume;
        for (float t = 0f; t < fadeOutDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(start, 0f, t / fadeOutDuration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = musicVolume;
        fadeRoutine = null;
    }
}
