using TMPro;
using UnityEngine;

/// <summary>
/// Curseur du panneau de debug réglant une propriété du matériau de la pomme d'or.
/// « propriete » : nom de la propriété du shader ; pour une couleur, suffixe .r / .g / .b.
/// </summary>
public class CurseurDebug : MonoBehaviour
{
    public string propriete;
    public TMP_Text valeur;
}
