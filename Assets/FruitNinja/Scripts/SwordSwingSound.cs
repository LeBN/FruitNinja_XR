using UnityEngine;

/// <summary>
/// Son de fendage d'air du sabre. Déclenché quand la lame se déplace dans le sens du
/// tranchant (largeur de la lame) :
///  - mouvement lent  → un des sons "Big" au hasard ;
///  - mouvement rapide → un des sons "Medium" au hasard.
/// La vitesse est mesurée sur un point de la lame ; on garde le pic de vitesse sur une
/// courte fenêtre avant de choisir le son, pour bien classer les mouvements rapides.
/// </summary>
public class SwordSwingSound : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip[] slowSwings;
    public AudioClip[] fastSwings;

    [Header("Géométrie de la lame (espace local du sabre)")]
    [Tooltip("Point de mesure de la vitesse (vers les 3/4 de la lame).")]
    public Vector3 samplePointLocal = new Vector3(0f, 0.8f, 0f);
    [Tooltip("Direction du tranchant (largeur de la lame).")]
    public Vector3 edgeAxisLocal = Vector3.right;

    [Header("Seuils (m/s, au point de mesure)")]
    public float minSpeed = 1.6f;
    public float fastSpeed = 4f;
    [Tooltip("Part minimale de la vitesse qui doit être dans le sens du tranchant.")]
    [Range(0f, 1f)] public float minEdgeRatio = 0.6f;
    public float cooldown = 0.3f;
    public float decisionWindow = 0.06f;

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 0.8f;

    private AudioSource source;
    private Vector3 lastPos;
    private Vector3 smoothedVelocity;
    private bool armed = true;
    private bool deciding;
    private float decideUntil;
    private float peakSpeed;
    private float nextAllowed;
    private int lastSlow = -1, lastFast = -1;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.minDistance = 0.5f;
        source.maxDistance = 12f;
    }

    private void OnEnable()
    {
        lastPos = transform.TransformPoint(samplePointLocal);
        smoothedVelocity = Vector3.zero;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 pos = transform.TransformPoint(samplePointLocal);
        Vector3 v = (pos - lastPos) / dt;
        lastPos = pos;
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, v, 1f - Mathf.Exp(-dt * 30f));

        Vector3 edge = transform.TransformDirection(edgeAxisLocal).normalized;
        float edgeSpeed = Mathf.Abs(Vector3.Dot(smoothedVelocity, edge));
        float total = smoothedVelocity.magnitude;
        bool alongEdge = total > 0f && edgeSpeed >= minEdgeRatio * total;

        if (deciding)
        {
            peakSpeed = Mathf.Max(peakSpeed, edgeSpeed);
            if (Time.time >= decideUntil)
            {
                deciding = false;
                Play(peakSpeed >= fastSpeed);
            }
            return;
        }

        if (edgeSpeed < minSpeed * 0.5f) armed = true;

        if (armed && Time.time >= nextAllowed && edgeSpeed >= minSpeed && alongEdge)
        {
            armed = false;
            deciding = true;
            decideUntil = Time.time + decisionWindow;
            peakSpeed = edgeSpeed;
        }
    }

    private void Play(bool fast)
    {
        var clips = fast ? fastSwings : slowSwings;
        if (clips == null || clips.Length == 0) return;

        int last = fast ? lastFast : lastSlow;
        int i = Random.Range(0, clips.Length);
        if (clips.Length > 1 && i == last) i = (i + 1) % clips.Length;
        if (fast) lastFast = i; else lastSlow = i;

        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clips[i], volume);
        nextAllowed = Time.time + cooldown;
    }
}
