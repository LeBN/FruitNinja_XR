using UnityEngine;
using EzySlice;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Découpe avec le sabre : un segment (startSlicePoint -> endSlicePoint) détecte les objets
/// "Sliceable". Fruit -> coupé en deux + jus au point de contact ; bombe -> explosion + pénalité ;
/// fruit de départ -> lance la partie ; fruit de difficulté -> sélection.
/// Les effets ont une variante PCVR et une variante Quest (voir PlatformFx).
/// </summary>
public class SwordSlicer : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public LayerMask sliceableLayer;
    public Material crossSectionMaterial;
    public float cutForce = 2000f;

    [Header("Effets (variante PCVR / variante Quest)")]
    [Tooltip("Explosion de bombe - version PCVR.")]
    public GameObject explosionVFX;
    [Tooltip("Explosion de bombe - version Quest (allégée).")]
    public GameObject explosionVFXQuest;
    [Tooltip("Jus de fruit à la coupe - version PCVR.")]
    public GameObject fruitJuiceFXPrefab;
    [Tooltip("Jus de fruit à la coupe - version Quest (allégée).")]
    public GameObject fruitJuiceFXPrefabQuest;
    public float fxLifetime = 3f;

    [Header("Sons")]
    public AudioClip fruitCutSound;
    public AudioClip bombExplosionSound;

    [Header("Pénalité bombe")]
    public int bombPenalty = 3;
    [Range(0f, 1f)] public float bombHapticAmplitude = 0.9f;
    public float bombHapticDuration = 0.35f;

    public string bombTag = "Bomb";
    public string startButtonTag = "StartButton";
    public GameObject gameManager;

    private AudioSource audioSource;

    // Liste des objets deja coupes recemment
    private HashSet<GameObject> recentlySliced = new HashSet<GameObject>();
    private float sliceCooldown = 0.15f; // 150 ms d'intervalle de securite

    private GameObject ExplosionFx => PlatformFx.Pick(explosionVFX, explosionVFXQuest);
    private GameObject JuiceFx => PlatformFx.Pick(fruitJuiceFXPrefab, fruitJuiceFXPrefabQuest);

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (gameManager == null)
        {
            GameManager gmSearch = FindFirstObjectByType<GameManager>();
            if (gmSearch != null)
                gameManager = gmSearch.gameObject;
        }

        Debug.Log($"[FX] {name} : variante {PlatformFx.PlatformName} (jus = {(JuiceFx ? JuiceFx.name : "aucun")}, explosion = {(ExplosionFx ? ExplosionFx.name : "aucune")})");
    }

    void FixedUpdate()
    {
        if (Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer))
        {
            GameObject target = hit.collider.gameObject;
            if (!recentlySliced.Contains(target))
            {
                StartCoroutine(RegisterSliceCooldown(target)); // Marque cet objet comme "deja coupe" pendant un court delai
                SliceObject(target, hit.point);
            }
        }
    }

    private IEnumerator RegisterSliceCooldown(GameObject obj)
    {
        recentlySliced.Add(obj);
        yield return new WaitForSeconds(sliceCooldown);
        recentlySliced.Remove(obj);
    }

    public void SliceObject(GameObject target) => SliceObject(target, target != null ? target.transform.position : Vector3.zero);

    public void SliceObject(GameObject target, Vector3 contactPoint)
    {
        if (target == null) return;

        GameManager gm = gameManager != null ? gameManager.GetComponent<GameManager>() : null;

        // Fruit de démarrage
        if (target.CompareTag(startButtonTag))
        {
            if (gm != null)
                gm.StartGame();

            SpawnFx(JuiceFx, contactPoint, FruitColor(target));
            PlaySound(fruitCutSound);
            Destroy(target);
            return;
        }

        // Bombe : explosion au point de contact + pénalité (score, texte, vibration)
        if (target.CompareTag(bombTag))
        {
            PlaySound(bombExplosionSound);
            SpawnFx(ExplosionFx, contactPoint, null);
            PenaltyPopup.Spawn(contactPoint + Vector3.up * 0.15f, "-" + bombPenalty, new Color(1f, 0.2f, 0.15f));
            Haptics(bombHapticAmplitude, bombHapticDuration);

            if (gm != null)
            {
                gm.AddScore(-bombPenalty);
                gm.ResetFruitsOnBomb();
            }

            Destroy(target);
            return;
        }

        // Fruit de sélection de difficulté : coupé comme un fruit (sans score), puis sélection
        DifficultyFruit difficultyFruit = target.GetComponentInParent<DifficultyFruit>();
        if (difficultyFruit != null)
        {
            SliceIntoPieces(target, contactPoint);
            difficultyFruit.OnSliced();
            return;
        }

        // Fruit normal
        if (!SliceIntoPieces(target, contactPoint)) return;

        if (gm != null)
            gm.AddScore(1);

        Destroy(target);
    }

    // Coupe l'objet en deux moitiés physiques, joue le son et l'effet de jus au point de contact.
    // Ne détruit pas l'objet d'origine.
    private bool SliceIntoPieces(GameObject target, Vector3 contactPoint)
    {
        SlicedHull hull = target.Slice(target.transform.position, Vector3.up, crossSectionMaterial);
        if (hull == null) return false;

        GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
        GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);

        Rigidbody originalRb = target.GetComponent<Rigidbody>();
        Vector3 originalVelocity = originalRb ? originalRb.linearVelocity : Vector3.zero;
        Vector3 originalAngularVelocity = originalRb ? originalRb.angularVelocity : Vector3.zero;

        AddPhysicsToSlice(upperHull, originalVelocity, originalAngularVelocity);
        AddPhysicsToSlice(lowerHull, originalVelocity, originalAngularVelocity);

        PlaySound(fruitCutSound);
        SpawnFx(JuiceFx, contactPoint, FruitColor(target));
        return true;
    }

    private static Color? FruitColor(GameObject target)
    {
        var rend = target.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null) return null;
        var mat = rend.sharedMaterial;
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_Color")) return mat.color;
        return null;
    }

    // Instancie un effet et, si une couleur est donnée, teinte tous ses sous-systèmes (jus du fruit).
    private void SpawnFx(GameObject prefab, Vector3 position, Color? tint)
    {
        if (prefab == null) return;
        GameObject fx = Instantiate(prefab, position, Quaternion.identity);

        if (tint.HasValue)
        {
            foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = ps.main;
                var c = main.startColor.color;
                main.startColor = new Color(tint.Value.r * c.r, tint.Value.g * c.g, tint.Value.b * c.b, c.a);
            }
        }

        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
            if (ps.main.playOnAwake == false) ps.Play();

        Destroy(fx, fxLifetime);
    }

    private static void Haptics(float amplitude, float duration)
    {
        foreach (var node in new[] { UnityEngine.XR.XRNode.LeftHand, UnityEngine.XR.XRNode.RightHand })
        {
            var device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid) device.SendHapticImpulse(0u, amplitude, duration);
        }
    }

    private void AddPhysicsToSlice(GameObject slicedObject, Vector3 inheritedVelocity, Vector3 inheritedAngularVelocity)
    {
        if (slicedObject == null) return;

        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        slicedObject.AddComponent<SphereCollider>();

        rb.linearVelocity = inheritedVelocity;
        rb.angularVelocity = inheritedAngularVelocity;
        rb.AddExplosionForce(cutForce, slicedObject.transform.position, 5f);
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
        Destroy(slicedObject, 8f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
