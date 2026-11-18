using System.Collections;
using UnityEngine;

/// <summary>
/// Anime le reflet lumineux (shader FruitNinja/BladeShine) le long de la lame.
/// Le matériau du reflet est en second slot du MeshRenderer du sabre.
/// </summary>
public class BladeShine : MonoBehaviour
{
    public Renderer target;
    [Tooltip("Index du matériau BladeShine dans le MeshRenderer.")]
    public int shineMaterialIndex = 1;

    private static readonly int ProgressId = Shader.PropertyToID("_Progress");
    private MaterialPropertyBlock block;
    private Coroutine routine;

    private void Awake()
    {
        if (target == null) target = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        SetProgress(-1f);
    }

    /// <summary>Lance un balayage de la garde à la pointe en <paramref name="duration"/> secondes.</summary>
    public void Play(float duration)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Sweep(Mathf.Max(0.05f, duration)));
    }

    private IEnumerator Sweep(float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            // Départ rapide puis ralentissement, comme un éclat qui file sur la lame
            float p = 1f - Mathf.Pow(1f - t / duration, 2f);
            SetProgress(p);
            yield return null;
        }
        SetProgress(-1f);
        routine = null;
    }

    private void SetProgress(float p)
    {
        if (target == null || target.sharedMaterials.Length <= shineMaterialIndex) return;
        target.GetPropertyBlock(block, shineMaterialIndex);
        block.SetFloat(ProgressId, p);
        target.SetPropertyBlock(block, shineMaterialIndex);
    }
}
