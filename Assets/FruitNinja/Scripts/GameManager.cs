using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public FruitSpawnerManager spawner;
    public GameObject startFruitPrefab;
    public Transform startFruitSpawnPoint;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Game settings")]
    public float gameDuration = 60f;

    private bool gameStarted = false;
    private int score = 0;
    private float timeRemaining;
    private Coroutine timerCoroutine;

    [Header("High Score UI")]
    public TextMeshProUGUI highScoreText;

    [Header("Audio")]
    public GameMusicManager music;

    [Header("Séquence de départ")]
    [Tooltip("Texte \"Prêt ?\" affiché avant la partie (désactivé par défaut).")]
    public GameObject readyText;
    public float readyDuration = 1.2f;
    [Tooltip("Reflets lumineux des lames, lancés en même temps que le son.")]
    public BladeShine[] bladeShines;
    public AudioSource shineSource;
    public AudioClip shineClip;
    [Tooltip("Durée du balayage du reflet sur la lame (s).")]
    public float shineSweepDuration = 0.8f;

    private Coroutine startSequence;


    private void Start()
    {
        if (startFruitPrefab == null)
        {
            startFruitPrefab = Resources.Load<GameObject>("StartFruit");
            if (startFruitPrefab == null)
                Debug.LogWarning("Impossible de trouver StartFruit dans Resources.");
        }

        if (startFruitSpawnPoint == null)
        {
            GameObject found = GameObject.Find("StartFruit") ?? GameObject.Find("StartFruitSpawn");
            if (found != null)
                startFruitSpawnPoint = found.transform;
        }

        if (music == null)
            music = FindFirstObjectByType<GameMusicManager>();

        UpdateScoreUI();
        UpdateTimerUI();
        UpdateHighScoreUI();

    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Touche R pressée - Respawn StartFruit");
            RespawnStartFruit();
        }

        UnityEngine.XR.InputDevice rightHand = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
        bool buttonPressed = false;
        if (rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out buttonPressed) && buttonPressed)
        {
            RespawnStartFruit();
        }
    }

    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        score = 0;
        UpdateScoreUI();

        timeRemaining = gameDuration;
        UpdateTimerUI();

        if (music != null)
            music.OnGameStart();

        if (startSequence != null)
            StopCoroutine(startSequence);
        startSequence = StartCoroutine(StartSequence());
    }

    // "Prêt ?" puis reflet sur les lames (synchronisé avec le son), puis la partie démarre.
    private IEnumerator StartSequence()
    {
        if (readyText != null)
            readyText.SetActive(true);

        yield return new WaitForSeconds(readyDuration);

        if (shineSource != null && shineClip != null)
            shineSource.PlayOneShot(shineClip);
        if (bladeShines != null)
            foreach (var shine in bladeShines)
                if (shine != null)
                    shine.Play(shineSweepDuration);

        float shineTime = shineClip != null ? Mathf.Max(shineSweepDuration, shineClip.length * 0.8f) : shineSweepDuration;
        yield return new WaitForSeconds(shineTime);

        if (readyText != null)
            readyText.SetActive(false);

        if (spawner != null)
            spawner.StartSpawning();

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(GameTimer());
        startSequence = null;
    }

    private IEnumerator GameTimer()
    {
        while (gameStarted && timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
            UpdateTimerUI();
        }

        if (gameStarted)
        {
            Debug.Log("Timer terminé - appel de EndGame()");
            EndGame();
        }

        timerCoroutine = null;
    }

    public void EndGame()
    {
        if (!gameStarted) return;
        gameStarted = false;
        Debug.Log("Fin de partie : chrono terminé");

        if (spawner != null)
            spawner.StopSpawning();

        if (music != null)
            music.OnGameEnd();

        foreach (var rb in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
        {
            if (rb.CompareTag("Sliceable") || rb.CompareTag("Bomb"))
                Destroy(rb.gameObject);
        }

        RespawnStartFruit();


        // Mise à jour du High Score
        int previousHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > previousHighScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
            Debug.Log("Nouveau High Score enregistré : " + score);
        }
        UpdateHighScoreUI();

    }

    public void RespawnStartFruit()
    {
        GameObject existingStart = GameObject.FindWithTag("StartButton");
        if (existingStart != null)
        {
            Destroy(existingStart);
            Debug.Log("Ancien StartFruit supprimé avant respawn.");
        }

        // Vérifie que le spawn point est valide
        if (startFruitSpawnPoint == null)
        {
            // Essaie de le retrouver dans la scène
            GameObject foundSpawn = GameObject.Find("StartFruitSpawnPoint");
            if (foundSpawn != null)
            {
                startFruitSpawnPoint = foundSpawn.transform;
                Debug.Log("StartFruitSpawnPoint retrouvé automatiquement.");
            }
            else
            {
                Debug.LogWarning("Aucun StartFruitSpawnPoint trouvé, utilisation position par défaut.");
            }
        }

        // Vérifie que le prefab est valide
        if (startFruitPrefab == null)
        {
            Debug.LogWarning("StartFruitPrefab manquant, tentative de chargement depuis Resources...");
            startFruitPrefab = Resources.Load<GameObject>("StartFruit");

            if (startFruitPrefab == null)
            {
                Debug.LogError("Aucun StartFruit trouvé !");
                return;
            }
        }

        // Calcule la position finale
        Vector3 spawnPos = startFruitSpawnPoint != null
            ? startFruitSpawnPoint.position
            : new Vector3(0, 1.2f, 0);

        Quaternion spawnRot = startFruitSpawnPoint != null
    ? startFruitSpawnPoint.rotation
    : Quaternion.identity;

        Instantiate(startFruitPrefab, spawnPos, spawnRot);

        Debug.Log($"StartFruit respawné à {spawnPos}");
    }


    public void AddScore(int amount)
    {
        score += amount;

        if (score < 0)
            score = 0;

        UpdateScoreUI();
    }


    public void ResetFruitsOnBomb()
    {
        foreach (var rb in FindObjectsOfType<Rigidbody>())
        {
            if (rb.CompareTag("Sliceable"))
                Destroy(rb.gameObject);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = "Temps : " + Mathf.Ceil(timeRemaining);
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            int high = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "Meilleur score : " + high;
        }
    }

    private bool TagExists(string tag)
    {
        try
        {
            // Unity lève une exception si le tag n'existe pas
            GameObject temp = new GameObject();
            bool hasTag = temp.CompareTag(tag);
            Destroy(temp);
            return true;
        }
        catch
        {
            return false;
        }
    }

}
