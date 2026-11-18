using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Canon cartoon d'un lanceur de fruits : il surgit avec un « boing » (étirement / écrasement)
/// dans un nuage de fumée, tire le fruit vers le haut (recul + fumée à la bouche + « pop »),
/// puis disparaît de la même façon une demi-seconde après le dernier tir.
/// </summary>
public class LanceurCanon : MonoBehaviour
{
    [Tooltip("Modèle du canon (enfant, caché au repos).")]
    [FormerlySerializedAs("chariot")]
    public Transform modele;
    public GameObject fumeePrefab;
    [Tooltip("Hauteur de la bouche du canon au-dessus du lanceur (m) : les fruits sortent de là.")]
    public float hauteurSortie = 0.35f;

    [Header("Sons (3D)")]
    public AudioClip[] sonsApparition;
    public AudioClip sonDisparition;
    public AudioClip[] sonsLancer;
    [Range(0f, 1f)] public float volume = 0.8f;

    [Header("Timing (s)")]
    public float dureeApparition = 0.4f;
    public float dureeDisparition = 0.3f;
    [Tooltip("Délai entre le début de l'apparition et le tir.")]
    public float delaiAvantLancer = 0.22f;
    [Tooltip("Le canon reste visible ce temps après le dernier tir.")]
    public float resteApresLancer = 0.5f;

    private enum Etat { Cache, Apparition, Visible, Disparition }
    private Etat etat = Etat.Cache;
    private Vector3 echelle;
    private float finVisible;
    private Coroutine anim;
    private Coroutine recul;

    public Vector3 PointDeSortie => transform.position + Vector3.up * hauteurSortie;

    private void Awake()
    {
        if (modele == null) return;
        echelle = modele.localScale;
        modele.gameObject.SetActive(false);
    }

    /// <summary>Fait apparaître le canon si besoin, puis appelle <paramref name="lancer"/> (qui crée le fruit).</summary>
    public void Lancer(Action lancer)
    {
        StartCoroutine(SequenceLancer(lancer));
    }

    /// <summary>Fin de partie : le canon disparaît tout de suite, les tirs en attente sont annulés.</summary>
    public void Cacher()
    {
        StopAllCoroutines();
        anim = null;
        recul = null;
        if (etat == Etat.Cache || modele == null) return;
        anim = StartCoroutine(Disparition());
    }

    private IEnumerator SequenceLancer(Action lancer)
    {
        finVisible = Mathf.Max(finVisible, Time.time + delaiAvantLancer + resteApresLancer);
        if (modele != null && (etat == Etat.Cache || etat == Etat.Disparition))
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Apparition());
            yield return new WaitForSeconds(delaiAvantLancer);
        }

        lancer?.Invoke();
        SonSpatial.JouerAuHasard(sonsLancer, PointDeSortie, volume);
        Fumee(PointDeSortie, 0.7f);
        if (modele != null && etat == Etat.Visible)
        {
            if (recul != null) StopCoroutine(recul);
            recul = StartCoroutine(Recul());
        }
        finVisible = Mathf.Max(finVisible, Time.time + resteApresLancer);
    }

    private IEnumerator Apparition()
    {
        etat = Etat.Apparition;
        modele.gameObject.SetActive(true);
        Fumee(transform.position + Vector3.up * 0.05f, 1f);
        SonSpatial.JouerAuHasard(sonsApparition, transform.position, volume);

        for (float t = 0f; t < 1f; t += Time.deltaTime / dureeApparition)
        {
            AppliquerEchelle(Ressort(t), Ressort(Mathf.Min(1f, t + 0.02f)) - Ressort(t));
            yield return null;
        }
        modele.localScale = echelle;
        etat = Etat.Visible;

        while (Time.time < finVisible) yield return null;
        anim = StartCoroutine(Disparition());
    }

    private IEnumerator Disparition()
    {
        etat = Etat.Disparition;
        if (recul != null) { StopCoroutine(recul); recul = null; }
        Fumee(transform.position + Vector3.up * 0.05f, 1f);
        SonSpatial.Jouer(sonDisparition, transform.position, volume);

        for (float t = 0f; t < 1f; t += Time.deltaTime / dureeDisparition)
        {
            // Anticipation (le canon s'écrase), puis il s'étire vers le haut en rétrécissant.
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
            modele.localScale = Vector3.Scale(echelle, new Vector3(xz, y, xz));
            yield return null;
        }
        modele.localScale = echelle;
        modele.gameObject.SetActive(false);
        etat = Etat.Cache;
        anim = null;
    }

    // Recul cartoon au tir : écrasé puis rebond.
    private IEnumerator Recul()
    {
        const float duree = 0.25f;
        for (float t = 0f; t < 1f; t += Time.deltaTime / duree)
        {
            float d = Mathf.Sin(t * Mathf.PI) * (1f - t) * 0.45f;
            modele.localScale = Vector3.Scale(echelle, new Vector3(1f + d * 0.5f, 1f - d, 1f + d * 0.5f));
            yield return null;
        }
        if (etat == Etat.Visible) modele.localScale = echelle;
        recul = null;
    }

    // Courbe « boing » : 0 → dépasse 1 → rebondit → se stabilise à 1.
    private static float Ressort(float t) => 1f - Mathf.Cos(t * Mathf.PI * 2.5f) * Mathf.Exp(-5f * t);

    // Écrasement / étirement selon la vitesse de la courbe (étiré en montant, écrasé en retombant).
    private void AppliquerEchelle(float s, float vitesse)
    {
        float d = Mathf.Clamp(vitesse * 12f, -0.35f, 0.35f);
        modele.localScale = Vector3.Scale(echelle, new Vector3(s * (1f - d * 0.5f), s * (1f + d), s * (1f - d * 0.5f)));
    }

    private void Fumee(Vector3 position, float taille)
    {
        if (fumeePrefab == null) return;
        var fx = Instantiate(fumeePrefab, position, Quaternion.identity);
        fx.transform.localScale = Vector3.one * taille;
        Destroy(fx, 2.5f);
    }
}
