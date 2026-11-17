using TMPro;
using UnityEngine;

/// <summary>Bouton « Vibrations : Oui / Non » du menu Paramètres.</summary>
public class ReglageVibrations : MonoBehaviour
{
    public TMP_Text texte;

    private void OnEnable() => Afficher();

    /// <summary>Appelé par le bouton.</summary>
    public void Basculer()
    {
        Parametres.Vibrations = !Parametres.Vibrations;
        Afficher();
        if (Parametres.Vibrations) Vibrations.Envoyer(0.5f, 0.15f);
    }

    private void Afficher()
    {
        if (texte != null) texte.text = Parametres.Vibrations ? "Oui" : "Non";
    }
}
