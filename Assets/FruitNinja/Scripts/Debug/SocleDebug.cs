using UnityEngine;

/// <summary>
/// Socle de la galerie de debug : expose un objet du jeu (fruit, bombe, pomme d'or, START...)
/// agrandi et en rotation lente. On peut le couper avec les sabres : il réapparaît ensuite.
/// </summary>
public class SocleDebug : MonoBehaviour
{
    public GameObject prefab;
    [Tooltip("Point où l'objet est posé (au-dessus du socle).")]
    public Transform point;
    [Tooltip("Agrandissement par rapport à la taille en jeu.")]
    public float agrandissement = 1.6f;
    public float vitesseRotation = 35f;
    public float delaiReapparition = 1.2f;

    private GameObject objet;
    private float minuteur;

    private void Start() => Faire();

    private void Update()
    {
        if (objet != null)
        {
            objet.transform.Rotate(Vector3.up, vitesseRotation * Time.deltaTime, Space.World);
            return;
        }
        minuteur -= Time.deltaTime;
        if (minuteur <= 0f) Faire();
    }

    /// <summary>Remplace l'objet exposé (bouton « Réinitialiser » de la galerie).</summary>
    public void Faire()
    {
        if (objet != null) Destroy(objet);
        if (prefab == null) return;
        Transform p = point != null ? point : transform;
        objet = Instantiate(prefab, p.position, p.rotation);
        objet.transform.localScale *= agrandissement;
        foreach (var rb in objet.GetComponentsInChildren<Rigidbody>())
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        minuteur = delaiReapparition;   // décompté seulement une fois l'objet coupé
    }
}
