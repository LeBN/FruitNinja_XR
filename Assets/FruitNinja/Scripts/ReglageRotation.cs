using TMPro;
using UnityEngine;

/// <summary>Bouton « Rotation : Instantanée / Continue » du menu Paramètres.</summary>
public class ReglageRotation : MonoBehaviour
{
    public TMP_Text texte;

    private void OnEnable() => Afficher();

    /// <summary>Appelé par le bouton.</summary>
    public void Basculer()
    {
        Parametres.RotationContinue = !Parametres.RotationContinue;
        Afficher();
    }

    private void Afficher()
    {
        if (texte != null) texte.text = Parametres.RotationContinue ? "Continue" : "Instantanée";
    }
}
