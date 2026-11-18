using System.Collections;
using UnityEngine;

/// <summary>
/// Apparition « éclair magique » du fruit START : trois anneaux de lumière se resserrent sur le
/// fruit en tournant (montée du son), puis éclair, gerbe d'étincelles et « pop » élastique du
/// fruit (impact du son), avec une petite vibration des manettes.
/// Le fruit ne peut pas être coupé avant la fin de l'apparition.
/// </summary>
public class ApparitionMagique : MonoBehaviour
{
    [Header("Son (3D, au point d'apparition)")]
    public AudioClip son;
    [Range(0f, 1f)] public float volume = 0.9f;

    [Header("Visuel")]
    public Material materiauAnneau;
    public Material materiauEclat;
    public Material materiauEtincelle;
    [Tooltip("Teinte des anneaux et étincelles (l'intensité lumineuse, pour le bloom, vient des matériaux).")]
    public Color couleur = new Color(0.25f, 0.85f, 1f, 1f);
    [Tooltip("Intensité de l'éclair (HDR, déclenche le bloom).")]
    public float intensiteEclat = 4f;
    [Tooltip("Durée de la convergence des anneaux, synchronisée avec l'impact du son (s).")]
    public float dureeConvergence = 0.65f;
    public float rayonDepart = 1.1f;

    private const int Points = 48;
    private Vector3 echelle;
    private Collider[] colliders;
    private Transform racineFx;

    private void Start()
    {
        echelle = transform.localScale;
        colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;
        transform.localScale = Vector3.zero;
        StartCoroutine(Jouer());
    }

    private IEnumerator Jouer()
    {
        Vector3 centre = transform.position;
        SonSpatial.Jouer(son, centre, volume, 0f);

        // Les effets vivent à côté du fruit (pas en enfant) pour ne pas suivre son échelle
        racineFx = new GameObject("FX_Apparition_Start").transform;
        racineFx.position = centre;

        var anneaux = new LineRenderer[3];
        var inclinaisons = new[] { Quaternion.Euler(90, 0, 0), Quaternion.Euler(30, 0, 60), Quaternion.Euler(-30, 0, -60) };
        for (int i = 0; i < anneaux.Length; i++)
        {
            var lr = new GameObject("Anneau_" + i).AddComponent<LineRenderer>();
            lr.transform.SetParent(racineFx, false);
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = Points;
            lr.sharedMaterial = materiauAnneau;
            lr.textureMode = LineTextureMode.Stretch;
            lr.alignment = LineAlignment.View;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            anneaux[i] = lr;
        }

        for (float t = 0f; t < 1f; t += Time.deltaTime / dureeConvergence)
        {
            float e = t * t * (3f - 2f * t);                                  // accélération douce
            float rayon = Mathf.Lerp(rayonDepart, 0.12f, e);
            float largeur = Mathf.Lerp(0.012f, 0.045f, e);
            Color c = new Color(couleur.r, couleur.g, couleur.b, Mathf.Lerp(0.2f, 1f, t * t));
            for (int i = 0; i < anneaux.Length; i++)
            {
                var lr = anneaux[i];
                lr.transform.localRotation = inclinaisons[i] * Quaternion.Euler(0, 0, (i % 2 == 0 ? 1 : -1) * 540f * e + i * 40f);
                for (int p = 0; p < Points; p++)
                {
                    float a = p * Mathf.PI * 2f / Points;
                    lr.SetPosition(p, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * rayon);
                }
                lr.widthMultiplier = largeur;
                lr.startColor = lr.endColor = c;
            }
            yield return null;
        }
        foreach (var lr in anneaux) Destroy(lr.gameObject);

        // Impact : éclair, étincelles, pop du fruit, vibration
        StartCoroutine(Eclat(centre));
        Etincelles(centre);
        Vibrations.Envoyer(0.45f, 0.12f);
        foreach (var c in colliders) if (c != null) c.enabled = true;

        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.35f)
        {
            // Élastique : dépasse puis revient (pop cartoon)
            float s = 1f + Mathf.Sin(t * Mathf.PI * 2.5f) * Mathf.Exp(-t * 4f) * 0.35f;
            transform.localScale = echelle * Mathf.Min(1f, t * 4f) * s;
            yield return null;
        }
        transform.localScale = echelle;
        Destroy(racineFx.gameObject, 1.2f);
    }

    private IEnumerator Eclat(Vector3 centre)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(q.GetComponent<Collider>());
        q.name = "Eclat";
        q.transform.SetParent(racineFx, false);
        var r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = materiauEclat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var bloc = new MaterialPropertyBlock();
        var cam = Camera.main;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
        {
            if (cam != null) q.transform.rotation = Quaternion.LookRotation(q.transform.position - cam.transform.position);
            q.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.8f, Mathf.Sqrt(t));
            var ce = couleur * intensiteEclat * (1f - t);
            ce.a = 1f - t;
            bloc.SetColor("_BaseColor", ce);
            r.SetPropertyBlock(bloc);
            yield return null;
        }
        Destroy(q);
    }

    private void Etincelles(Vector3 centre)
    {
        var go = new GameObject("Etincelles");
        go.transform.SetParent(racineFx, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = new ParticleSystem.MinMaxGradient(couleur, Color.white);
        main.gravityModifier = 0.35f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 64;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(PlatformFx.IsQuest ? 24 : 48)) });
        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 0.05f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;
        var sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
        var pr = go.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = materiauEtincelle;
        pr.renderMode = ParticleSystemRenderMode.Stretch;
        pr.velocityScale = 0.04f;
        pr.lengthScale = 1.5f;
        pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ps.Play();
    }

    private void OnDestroy()
    {
        if (racineFx != null) Destroy(racineFx.gameObject);
    }
}
