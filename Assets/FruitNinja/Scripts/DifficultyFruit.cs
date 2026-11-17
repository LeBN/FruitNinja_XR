using UnityEngine;

public enum Difficulty { Facile, Normal, Hardcore }

/// <summary>
/// Fruit de sélection de difficulté (scène de jeu). Quand il est coupé, il disparaît et
/// sa version fantôme reste à sa place ; il réapparaît si une autre difficulté est choisie.
/// </summary>
public class DifficultyFruit : MonoBehaviour
{
    public Difficulty difficulty;
    [Tooltip("Copie fantôme (translucide) affichée à la place du fruit choisi. Désactivée par défaut.")]
    public GameObject phantom;
    public DifficultySelector selector;

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private Rigidbody body;

    private void Awake()
    {
        homePosition = transform.position;
        homeRotation = transform.rotation;
        body = GetComponent<Rigidbody>();
        if (selector == null) selector = FindFirstObjectByType<DifficultySelector>();
    }

    /// <summary>Appelé par SlideObject quand la lame coupe ce fruit.</summary>
    public void OnSliced()
    {
        if (selector != null) selector.Select(this);
    }

    public void ShowPhantom()
    {
        gameObject.SetActive(false);
        if (phantom != null) phantom.SetActive(true);
    }

    public void ShowFruit()
    {
        if (phantom != null) phantom.SetActive(false);
        transform.SetPositionAndRotation(homePosition, homeRotation);
        if (body != null && !body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        gameObject.SetActive(true);
    }

    public void HideAll()
    {
        gameObject.SetActive(false);
        if (phantom != null) phantom.SetActive(false);
    }
}
