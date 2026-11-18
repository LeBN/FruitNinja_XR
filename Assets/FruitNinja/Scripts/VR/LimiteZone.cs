using Unity.XR.CoreUtils;
using UnityEngine;

/// <summary>
/// Garde le joueur dans un cercle autour de sa position de départ (7 m par défaut) :
/// si la tête sort du cercle (joystick, téléportation ou marche réelle), le rig est ramené au bord.
/// À placer sur le XR Origin.
/// </summary>
[RequireComponent(typeof(XROrigin))]
public class LimiteZone : MonoBehaviour
{
    [Tooltip("Rayon de la zone autorisée autour du point de départ (m).")]
    public float rayon = 7f;

    private XROrigin origine;
    private Vector3 centre;

    private void Start()
    {
        origine = GetComponent<XROrigin>();
        centre = transform.position;
    }

    private void LateUpdate()
    {
        if (origine == null || origine.Camera == null) return;
        Vector3 ecart = origine.Camera.transform.position - centre;
        ecart.y = 0f;
        float distance = ecart.magnitude;
        if (distance <= rayon) return;
        transform.position -= ecart / distance * (distance - rayon);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.8f);
        Vector3 c = Application.isPlaying ? centre : transform.position;
        const int n = 48;
        for (int i = 0; i < n; i++)
        {
            float a0 = i * Mathf.PI * 2f / n, a1 = (i + 1) * Mathf.PI * 2f / n;
            Gizmos.DrawLine(c + new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)) * rayon, c + new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * rayon);
        }
    }
}
