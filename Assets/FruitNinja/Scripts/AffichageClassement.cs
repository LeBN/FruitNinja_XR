using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Planche du classement (scène de jeu) : rang, score, difficulté et date, en blanc ;
/// le dernier score ajouté est surligné en doré.
/// </summary>
public class AffichageClassement : MonoBehaviour
{
    public TMP_Text lignes;
    public Color couleurDernier = new Color(1f, 0.84f, 0.3f);

    private void OnEnable()
    {
        Classement.Change += Rafraichir;
        Rafraichir();
    }

    private void OnDisable() => Classement.Change -= Rafraichir;

    /// <summary>Appelé par le bouton « Effacer scores » des Paramètres.</summary>
    public void Effacer() => Classement.Effacer();

    private void Rafraichir()
    {
        if (lignes == null) return;
        var liste = Classement.Lire();
        if (liste.Count == 0)
        {
            lignes.text = "<align=center>Aucun score pour l'instant.\nÀ toi de jouer !</align>";
            return;
        }
        var sb = new StringBuilder();
        string dore = ColorUtility.ToHtmlStringRGB(couleurDernier);
        for (int i = 0; i < liste.Count; i++)
        {
            var e = liste[i];
            string ligne = $"{i + 1}.<pos=14%>{e.score}<pos=38%>{e.difficulte}<pos=78%>{e.date}";
            sb.AppendLine(e.id == Classement.DernierId ? $"<color=#{dore}>{ligne}</color>" : ligne);
        }
        lignes.text = sb.ToString();
    }
}
