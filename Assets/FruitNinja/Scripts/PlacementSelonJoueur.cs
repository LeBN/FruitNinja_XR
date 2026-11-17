using System.Collections;
using UnityEngine;

/// <summary>
/// Place un groupe (menu de difficulté) à hauteur de sabre selon la taille du joueur :
/// le groupe monte ou descend pour que sa hauteur de référence soit à « sousLesYeux » sous les yeux.
/// </summary>
public class PlacementSelonJoueur : MonoBehaviour
{
    [Tooltip("Distance sous les yeux du joueur (m).")]
    public float sousLesYeux = 0.45f;
    [Tooltip("Limites au-dessus du sol (m).")]
    public float minAuSol = 0.9f;
    public float maxAuSol = 1.5f;
    [Tooltip("Hauteur monde des fruits telle que placée dans la scène (moyenne), utilisée sans suivi du casque.")]
    public float hauteurReference = 2f;
    [Tooltip("Temps max d'attente du suivi du casque au chargement (s).")]
    public float attenteSuiviMax = 3f;

    private Vector3 positionScene;

    private void Awake()
    {
        positionScene = transform.position;
    }

    private void Start()
    {
        StartCoroutine(AjusterQuandSuivi());
    }

    /// <summary>Recalcule la hauteur (appelé quand le menu réapparaît).</summary>
    public void Ajuster()
    {
        float cible = HauteurJoueur.Cible(sousLesYeux, minAuSol, maxAuSol, hauteurReference);
        transform.position = positionScene + Vector3.up * (cible - hauteurReference);
    }

    private IEnumerator AjusterQuandSuivi()
    {
        float t = 0f;
        yield return null;
        while (t < attenteSuiviMax && !HauteurJoueur.Lire(out _, out _))
        {
            t += Time.deltaTime;
            yield return null;
        }
        Ajuster();
    }
}
