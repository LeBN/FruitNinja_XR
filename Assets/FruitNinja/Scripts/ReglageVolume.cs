using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Curseur de volume du menu Paramètres (barre bois : fond vide + remplissage doré + poignée).
/// Le remplissage suit la valeur ; la valeur est enregistrée dans Parametres.
/// </summary>
[RequireComponent(typeof(Slider))]
public class ReglageVolume : MonoBehaviour
{
    public enum Reglage { General, Musique, Effets }

    public Reglage reglage;
    [Tooltip("Image de remplissage (type Filled, horizontal).")]
    public Image remplissage;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.SetValueWithoutNotify(Lire());
        MettreAJour(slider.value);
        slider.onValueChanged.AddListener(Changer);
    }

    private void OnEnable()
    {
        if (slider == null) return;
        slider.SetValueWithoutNotify(Lire());
        MettreAJour(slider.value);
    }

    private float Lire() => reglage switch
    {
        Reglage.General => Parametres.VolumeGeneral,
        Reglage.Musique => Parametres.VolumeMusique,
        _ => Parametres.VolumeEffets,
    };

    private void Changer(float v)
    {
        switch (reglage)
        {
            case Reglage.General: Parametres.VolumeGeneral = v; break;
            case Reglage.Musique: Parametres.VolumeMusique = v; break;
            default: Parametres.VolumeEffets = v; break;
        }
        MettreAJour(v);
    }

    private void MettreAJour(float v)
    {
        if (remplissage != null) remplissage.fillAmount = v;
    }
}
