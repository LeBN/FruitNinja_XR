using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Chariot d'un lanceur de fruits : il surgit avec un « boing » cartoon (étirement / écrasement)
/// et un nuage de fumée, lance le fruit, puis disparaît de la même façon une demi-seconde après.
/// </summary>
public class LanceurChariot : MonoBehaviour
{
    [Tooltip("Modèle du chariot (enfant, caché au repos).")]
    public Transform chariot;
    public GameObject fumeePrefab;
    [Tooltip("Hauteur de sortie des fruits au-dessus du lanceur (m).")]
    public float hauteurSortie = 0.35f;

    [Header("Sons (3D)")]
    public AudioClip[] sonsApparition;
    public AudioClip sonDisparition;
    public AudioClip[] sonsLancer;
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("Timing (s)")]
    public float dureeApparition = 0.4f;
    public float dureeDisparition = 0.3f;
    [Tooltip("Délai entre le début de l'apparition et le lancer.")]
    public float delaiAvantLancer = 0.18f;
    [Tooltip("Le chariot reste visible ce temps après le dernier lancer.")]
    public float resteApresLancer = 0.5f;

    private enum Etat { Cache, Apparition, Visible, Disparition }
    private Etat etat = Etat.Cache;
    private Vector3 echelle;
    private float finVisible;
    private Coroutine anim;

    public Vector3 PointDeSortie => transform.position + Vector3.up * hauteurSortie;

    private void Awake()
    {
        if (chariot == null) return;
        echelle = chariot.localScale;
        chariot.gameObject.SetActive(false);
    }

    /// <summary>Fait apparaître le chariot si besoin, puis appelle <paramref name="lancer"/> (qui crée le fruit).</summary>
    public void Lancer(Action lancer)
    {
        StartCoroutine(SequenceLancer(lancer));
    }

    /// <summary>Fin de partie : le chariot disparaît tout de suite, les lancers en attente sont annulés.</summary>
    public void Cacher()
    {
        StopAllCoroutines();
        anim = null;
        if (etat == Etat.Cache || chariot == null) return;
        anim = StartCoroutine(Disparition());
    }

    private IEnumerator SequenceLancer(Action lancer)
    {
        finVisible = Mathf.Max(finVisible, Time.time + delaiAvantLancer + resteApresLancer);
        if (chariot != null && (etat == Etat.Cache || etat == Etat.Disparition))
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Apparition());
            yield return new WaitForSeconds(delaiAvantLancer);
        }

        lancer?.Invoke();
        SonSpatial.JouerAuHasard(sonsLancer, PointDeSortie, volume);
        finVisible = Mathf.Max(finVisible, Time.time + resteApresLancer);
    }

    private IEnumerator Apparition()
    {
        etat = Etat.Apparition;
        chariot.gameObject.SetActive(true);
        Fumee();
        SonSpatial.JouerAuHasard(sonsApparition, transform.position, volume);

        for (float t = 0f; t < 1f; t += Time.deltaTime / dureeApparition)
        {
            AppliquerEchelle(Ressort(t), Ressort(Mathf.Min(1f, t + 0.02f)) - Ressort(t));
            yield return null;
        }
        chariot.localScale = echelle;
        etat = Etat.Visible;

        while (Time.time < finVisible) yield return null;
        anim = StartCoroutine(Disparition());
    }

    private IEnumerator Disparition()
    {
        etat = Etat.Disparition;
        Fumee();
        SonSpatial.Jouer(sonDisparition, transform.position, volume);

        for (float t = 0f; t < 1f; t += Time.deltaTime / dureeDisparition)
        {
            // Anticipation (le chariot s'écrase), puis il s'étire vers le haut en rétrécissant.
            float y, xz;
            if (t < 0.35f)
            {
                float k = t / 0.35f;
                y = 1f - 0.3f * k;
                xz = 1f + 0.2f * k;
            }
            else
            {
                float k = (t - 0.35f) / 0.65f;
                y = (1f - k) * (0.7f + 0.9f * k);
                xz = 1.2f * Mathf.Pow(1f - k, 1.5f);
            }
            chariot.localScale = Vector3.Scale(echelle, new Vector3(xz, y, xz));
            yield return null;
        }
        chariot.localScale = echelle;
        chariot.gameObject.SetActive(false);
        etat = Etat.Cache;
        anim = null;
    }

    // Courbe « boing » : 0 → dépasse 1 → rebondit → se stabilise à 1.
    private static float Ressort(float t) => 1f - Mathf.Cos(t * Mathf.PI * 2.5f) * Mathf.Exp(-5f * t);

    // Écrasement / étirement selon la vitesse de la courbe (étiré en montant, écrasé en retombant).
    private void AppliquerEchelle(float s, float vitesse)
    {
        float d = Mathf.Clamp(vitesse * 12f, -0.35f, 0.35f);
        chariot.localScale = Vector3.Scale(echelle, new Vector3(s * (1f - d * 0.5f), s * (1f + d), s * (1f - d * 0.5f)));
    }

    private void Fumee()
    {
        if (fumeePrefab == null) return;
        var fx = Instantiate(fumeePrefab, transform.position + Vector3.up * 0.08f, Quaternion.identity);
        Destroy(fx, 2f);
    }
}
