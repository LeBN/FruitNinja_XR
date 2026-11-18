using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Planche du classement (scène de jeu) : rang, score, difficulté et date ;
/// le dernier score ajouté est surligné en doré.
/// Si la liste dépasse la planche, elle défile toute seule (pause en haut, descente lente,
/// pause en bas, remontée), car les rayons sont masqués en jeu. Après une partie, la liste
/// se place d'abord sur le nouveau score.
/// </summary>
public class AffichageClassement : MonoBehaviour
{
    public TMP_Text lignes;
    public Color couleurDernier = new Color(1f, 0.84f, 0.3f);

    [Header("Défilement automatique")]
    [Tooltip("Zone visible (avec RectMask2D) qui contient le texte des lignes.")]
    public RectTransform fenetre;
    [Tooltip("Vitesse de défilement (unités de la planche par seconde).")]
    public float vitesse = 45f;
    [Tooltip("Pause en haut et en bas de la liste (s).")]
    public float pause = 3f;
    [Tooltip("Temps pendant lequel le nouveau score reste affiché avant de reprendre le défilement (s).")]
    public float maintienNouveauScore = 8f;

    private enum Etat { PauseHaut, Descente, PauseBas, Montee, Maintien }
    private Etat etat = Etat.PauseHaut;
    private float minuteur;
    private float decalage;

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
        int indexDernier = -1;
        if (liste.Count == 0)
        {
            lignes.text = "<align=center>Aucun score pour l'instant.\nÀ toi de jouer !</align>";
        }
        else
        {
            var sb = new StringBuilder();
            string dore = ColorUtility.ToHtmlStringRGB(couleurDernier);
            for (int i = 0; i < liste.Count; i++)
            {
                var e = liste[i];
                string ligne = $"{i + 1}.<pos=14%>{e.score}<pos=38%>{e.difficulte}<pos=78%>{e.date}";
                bool dernier = e.id == Classement.DernierId;
                if (dernier) indexDernier = i;
                sb.AppendLine(dernier ? $"<color=#{dore}>{ligne}</color>" : ligne);
            }
            lignes.text = sb.ToString();
        }
        AjusterHauteur();

        // Nouveau score : on le centre dans la fenêtre, puis le défilement reprend
        if (indexDernier >= 0 && Debordement() > 0f)
        {
            lignes.ForceMeshUpdate();
            var info = lignes.textInfo;
            if (indexDernier < info.lineCount)
            {
                var l = info.lineInfo[indexDernier];
                float centreLigne = -(l.ascender + l.descender) * 0.5f;      // distance depuis le haut du texte
                decalage = Mathf.Clamp(centreLigne - fenetre.rect.height * 0.5f, 0f, Debordement());
                etat = Etat.Maintien;
                minuteur = maintienNouveauScore;
                Appliquer();
                return;
            }
        }
        etat = Etat.PauseHaut;
        minuteur = pause;
        decalage = 0f;
        Appliquer();
    }

    private void AjusterHauteur()
    {
        if (fenetre == null) return;
        var rt = lignes.rectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Max(fenetre.rect.height, lignes.preferredHeight));
    }

    private float Debordement() => fenetre == null ? 0f : Mathf.Max(0f, lignes.rectTransform.rect.height - fenetre.rect.height);

    private void Appliquer()
    {
        if (fenetre == null) return;
        var rt = lignes.rectTransform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, decalage);
    }

    private void Update()
    {
        float max = Debordement();
        if (max <= 0f) { if (decalage != 0f) { decalage = 0f; Appliquer(); } return; }

        switch (etat)
        {
            case Etat.PauseHaut:
            case Etat.PauseBas:
            case Etat.Maintien:
                minuteur -= Time.deltaTime;
                if (minuteur <= 0f) etat = etat == Etat.PauseBas ? Etat.Montee : Etat.Descente;
                break;
            case Etat.Descente:
                decalage = Mathf.MoveTowards(decalage, max, vitesse * Time.deltaTime);
                if (decalage >= max) { etat = Etat.PauseBas; minuteur = pause; }
                break;
            case Etat.Montee:
                decalage = Mathf.MoveTowards(decalage, 0f, vitesse * 3f * Time.deltaTime);
                if (decalage <= 0f) { etat = Etat.PauseHaut; minuteur = pause; }
                break;
        }
        Appliquer();
    }
}
