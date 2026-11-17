using TMPro;
using UnityEngine;

/// <summary>
/// Texte flottant (ex. "-3" rouge quand une bombe est touchée) : apparaît au point d'impact,
/// monte, se tourne vers le joueur et s'efface.
/// </summary>
public class PenaltyPopup : MonoBehaviour
{
    public float lifetime = 1.2f;
    public float riseSpeed = 0.6f;

    private TextMeshPro text;
    private float age;
    private Color baseColor;

    public static void Spawn(Vector3 position, string message, Color color, float size = 1.2f)
    {
        var go = new GameObject("PenaltyPopup");
        go.transform.position = position;
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.rectTransform.sizeDelta = new Vector2(1f, 0.5f);
        var popup = go.AddComponent<PenaltyPopup>();
        popup.text = tmp;
        popup.baseColor = color;
    }

    private void Update()
    {
        age += Time.deltaTime;
        transform.position += Vector3.up * (riseSpeed * Time.deltaTime);

        var cam = Camera.main;
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        if (text != null)
        {
            var c = baseColor;
            c.a = Mathf.Clamp01(1f - age / lifetime);
            text.color = c;
        }

        if (age >= lifetime) Destroy(gameObject);
    }
}
