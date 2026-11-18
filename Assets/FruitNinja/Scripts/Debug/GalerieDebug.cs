using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Scène de debug : galerie de socles (chaque objet du jeu, à couper) et panneau à onglets pour
/// tester toutes les fonctionnalités : lancers depuis les canons, effets PC / Quest côte à côte,
/// sons, musiques, vibrations, classement, scènes, et réglage en direct de la pomme d'or.
/// Les boutons (BoutonDebug) et curseurs (CurseurDebug) sont reliés au démarrage.
/// </summary>
public class GalerieDebug : MonoBehaviour
{
    [Header("Objets")]
    public GameObject[] fruits;
    public string[] nomsFruits;
    public GameObject bombe;
    public GameObject pommeOr;
    public GameObject fruitStart;

    [Header("Lancers")]
    public LanceurCanon[] canons;
    public float forceLancer = 5.5f;
    public float dureeVieLancer = 4f;

    [Header("Effets (PC à gauche, Quest à droite)")]
    public Transform pointFxPC;
    public Transform pointFxQuest;
    public Transform pointStart;
    public GameObject fxJusPC, fxJusQuest, fxExplosionPC, fxExplosionQuest, fxFumee;

    [Header("Sons et musiques")]
    public AudioClip[] sons;
    public AudioClip[] musiques;
    public AudioClip sonReflet;
    public Transform pointSon;

    [Header("Pomme d'or")]
    public Material materiauOr;
    public Cubemap refletBasse, refletHaute;
    public TMP_Text texteValeursOr;

    [Header("Onglets")]
    public GameObject[] pages;
    public Button[] onglets;
    public Color couleurOngletActif = Color.white;
    public Color couleurOngletInactif = new Color(0.75f, 0.68f, 0.6f);

    private AudioSource sourceMusique;
    private readonly Dictionary<string, Vector4> valeursOrigine = new Dictionary<string, Vector4>();
    private readonly List<(Slider curseur, CurseurDebug info)> curseurs = new List<(Slider, CurseurDebug)>();
    private static readonly string[] ProprietesOr = { "_Teinte", "_Intensite", "_Contraste", "_Ambiant", "_Fresnel", "_Eclat", "_Brillance" };

    private void Awake()
    {
        sourceMusique = gameObject.AddComponent<AudioSource>();
        sourceMusique.loop = true;
        sourceMusique.playOnAwake = false;
        sourceMusique.spatialBlend = 0f;

        foreach (var b in GetComponentsInChildren<BoutonDebug>(true)) Relier(b);
        foreach (var b in FindObjectsByType<BoutonDebug>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!b.transform.IsChildOf(transform)) Relier(b);

        // Pomme d'or : on garde les valeurs d'origine (restaurées en quittant la scène)
        if (materiauOr != null)
            foreach (var p in ProprietesOr)
                valeursOrigine[p] = p == "_Teinte" ? materiauOr.GetVector(p) : new Vector4(materiauOr.GetFloat(p), 0, 0, 0);   // GetVector : sans conversion de couleur (pas d'arrondi)

        foreach (var c in FindObjectsByType<CurseurDebug>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var s = c.GetComponent<Slider>();
            if (s == null) continue;
            curseurs.Add((s, c));
            s.SetValueWithoutNotify(LireOr(c.propriete));
            var info = c;
            s.onValueChanged.AddListener(v => { EcrireOr(info.propriete, v); MajTextesOr(); });
        }
        MajTextesOr();
        Onglet(0);
    }

    private void OnDestroy() => RestaurerOr();
    private void OnApplicationQuit() => RestaurerOr();

    private void Relier(BoutonDebug b)
    {
        var bouton = b.GetComponent<Button>();
        if (bouton == null) return;
        var action = b.action;
        var param = b.parametre;
        bouton.onClick.AddListener(() => Executer(action, param));
    }

    public void Executer(string action, string param)
    {
        switch (action)
        {
            case "onglet": Onglet(int.Parse(param)); break;
            case "lancer": Lancer(param); break;
            case "fx": Effet(param); break;
            case "son": JouerSon(int.Parse(param)); break;
            case "musique": Musique(param); break;
            case "vibration": Vibration(param); break;
            case "score": Score(param); break;
            case "scene": SceneManager.LoadScene(param); break;
            case "or": ActionOr(param); break;
            case "socles": foreach (var s in FindObjectsByType<SocleDebug>(FindObjectsSortMode.None)) s.Faire(); break;
        }
    }

    // ---------- Onglets ----------
    private void Onglet(int i)
    {
        for (int k = 0; k < pages.Length; k++) if (pages[k] != null) pages[k].SetActive(k == i);
        for (int k = 0; k < onglets.Length; k++)
            if (onglets[k] != null) onglets[k].image.color = k == i ? couleurOngletActif : couleurOngletInactif;
    }

    // ---------- Lancers ----------
    private void Lancer(string quoi)
    {
        switch (quoi)
        {
            case "hasard": LancerObjet(fruits[Random.Range(0, fruits.Length)]); break;
            case "bombe": LancerObjet(bombe); break;
            case "or": LancerObjet(pommeOr); break;
            case "vague": StartCoroutine(Vague(5)); break;
            case "start":
                foreach (var old in GameObject.FindGameObjectsWithTag("StartButton")) Destroy(old);
                if (fruitStart != null && pointStart != null) Instantiate(fruitStart, pointStart.position, pointStart.rotation);
                break;
            default:
                if (int.TryParse(quoi, out int i) && i >= 0 && i < fruits.Length) LancerObjet(fruits[i]);
                break;
        }
    }

    private IEnumerator Vague(int n)
    {
        for (int i = 0; i < n; i++)
        {
            LancerObjet(Random.value < 0.15f ? bombe : fruits[Random.Range(0, fruits.Length)]);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void LancerObjet(GameObject prefab)
    {
        if (prefab == null) return;
        if (canons == null || canons.Length == 0) { Tirer(prefab, transform.position + Vector3.up); return; }
        var canon = canons[Random.Range(0, canons.Length)];
        canon.Lancer(() => Tirer(prefab, canon.PointDeSortie));
    }

    private void Tirer(GameObject prefab, Vector3 depart)
    {
        var obj = Instantiate(prefab, depart, Quaternion.identity);
        Destroy(obj, dureeVieLancer);
        var rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return;
        // Vers le haut, un peu vers le joueur pour qu'il puisse couper
        var cam = Camera.main;
        Vector3 versJoueur = cam != null ? cam.transform.position - depart : Vector3.back;
        versJoueur.y = 0f;
        Vector3 dir = Vector3.up + versJoueur.normalized * 0.18f + new Vector3(Random.Range(-0.05f, 0.05f), 0, Random.Range(-0.05f, 0.05f));
        rb.AddForce(dir.normalized * forceLancer, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
    }

    // ---------- Effets ----------
    private void Effet(string quoi)
    {
        switch (quoi)
        {
            case "jus": Paire(fxJusPC, fxJusQuest, new Color(0.9f, 0.15f, 0.2f)); break;
            case "explosion": Paire(fxExplosionPC, fxExplosionQuest, null); break;
            case "fumee":
                if (fxFumee != null) { Detruire(Instantiate(fxFumee, pointFxPC.position, Quaternion.identity)); Detruire(Instantiate(fxFumee, pointFxQuest.position, Quaternion.identity)); }
                break;
            case "canons": foreach (var c in canons) c.Lancer(() => { }); break;
            case "reflet":
                foreach (var b in FindObjectsByType<BladeShine>(FindObjectsSortMode.None)) b.Play(0.8f);
                if (sonReflet != null && Camera.main != null) SonSpatial.Jouer(sonReflet, Camera.main.transform.position + Camera.main.transform.forward * 0.4f, 0.8f);
                Vibrations.Envoyer(0.5f, 0.15f);
                break;
            case "penalite":
                PenaltyPopup.Spawn(pointFxPC.position + Vector3.up * 0.2f, "-3", new Color(1f, 0.2f, 0.15f));
                PenaltyPopup.Spawn(pointFxQuest.position + Vector3.up * 0.2f, "+1", new Color(0.3f, 1f, 0.4f));
                break;
        }
    }

    private void Paire(GameObject pc, GameObject quest, Color? couleur)
    {
        if (pc != null) Colorer(Detruire(Instantiate(pc, pointFxPC.position, Quaternion.identity)), couleur);
        if (quest != null) Colorer(Detruire(Instantiate(quest, pointFxQuest.position, Quaternion.identity)), couleur);
    }

    private static GameObject Detruire(GameObject go) { Destroy(go, 4f); return go; }

    private static void Colorer(GameObject fx, Color? couleur)
    {
        if (fx == null || couleur == null) return;
        foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
        {
            var main = ps.main;
            main.startColor = couleur.Value;
        }
    }

    // ---------- Sons, musiques, vibrations ----------
    private void JouerSon(int i)
    {
        if (sons == null || i < 0 || i >= sons.Length) return;
        Vector3 p = pointSon != null ? pointSon.position : transform.position;
        SonSpatial.Jouer(sons[i], p, 1f, 0f);
    }

    private void Musique(string param)
    {
        if (param == "stop") { sourceMusique.Stop(); return; }
        if (!int.TryParse(param, out int i) || musiques == null || i < 0 || i >= musiques.Length) return;
        sourceMusique.clip = musiques[i];
        sourceMusique.volume = 0.5f * Parametres.VolumeMusique;
        sourceMusique.Play();
    }

    private void Vibration(string force)
    {
        switch (force)
        {
            case "legere": Vibrations.Envoyer(0.25f, 0.08f); break;
            case "moyenne": Vibrations.Envoyer(0.5f, 0.15f); break;
            case "forte": Vibrations.Envoyer(0.9f, 0.35f); break;
        }
    }

    // ---------- Classement ----------
    private void Score(string param)
    {
        var diffs = new[] { Difficulty.Facile, Difficulty.Normal, Difficulty.Hardcore };
        switch (param)
        {
            case "un": Classement.Ajouter(Random.Range(5, 150), diffs[Random.Range(0, 3)]); break;
            case "remplir": for (int i = 0; i < Classement.Taille; i++) Classement.Ajouter(Random.Range(5, 150), diffs[Random.Range(0, 3)]); break;
            case "effacer": Classement.Effacer(); break;
        }
    }

    // ---------- Pomme d'or ----------
    private float LireOr(string prop)
    {
        if (materiauOr == null) return 0f;
        if (prop.Length > 2 && prop[prop.Length - 2] == '.')
        {
            var c = materiauOr.GetColor(prop.Substring(0, prop.Length - 2));
            return prop.EndsWith(".r") ? c.r : prop.EndsWith(".g") ? c.g : c.b;
        }
        return materiauOr.GetFloat(prop);
    }

    private void EcrireOr(string prop, float v)
    {
        if (materiauOr == null) return;
        if (prop.Length > 2 && prop[prop.Length - 2] == '.')
        {
            string nom = prop.Substring(0, prop.Length - 2);
            var c = materiauOr.GetColor(nom);
            if (prop.EndsWith(".r")) c.r = v; else if (prop.EndsWith(".g")) c.g = v; else c.b = v;
            materiauOr.SetColor(nom, c);
        }
        else materiauOr.SetFloat(prop, v);
    }

    private void ActionOr(string param)
    {
        switch (param)
        {
            case "reinit":
                RestaurerOr();
                foreach (var (s, c) in curseurs) s.SetValueWithoutNotify(LireOr(c.propriete));
                break;
            case "basse": if (refletBasse != null) Shader.SetGlobalTexture("_RefletOrGlobal", refletBasse); break;
            case "haute": if (refletHaute != null) Shader.SetGlobalTexture("_RefletOrGlobal", refletHaute); break;
        }
        MajTextesOr();
    }

    private void RestaurerOr()
    {
        if (materiauOr == null) return;
        foreach (var kv in valeursOrigine)
        {
            if (kv.Key == "_Teinte") materiauOr.SetVector(kv.Key, kv.Value);
            else materiauOr.SetFloat(kv.Key, kv.Value.x);
        }
    }

    private void MajTextesOr()
    {
        var ci = CultureInfo.InvariantCulture;
        foreach (var (s, c) in curseurs)
            if (c.valeur != null) c.valeur.text = s.value.ToString(s.maxValue > 20 ? "0" : "0.00", ci);
        if (texteValeursOr != null && materiauOr != null)
        {
            var t = materiauOr.GetColor("_Teinte");
            texteValeursOr.text = string.Format(ci,
                "Teinte ({0:0.00}, {1:0.00}, {2:0.00}) · Intensité {3:0.00} · Contraste {4:0.00} · Ambiant {5:0.00} · Fresnel {6:0.00} · Éclat {7:0.00} · Brillance {8:0}",
                t.r, t.g, t.b, materiauOr.GetFloat("_Intensite"), materiauOr.GetFloat("_Contraste"), materiauOr.GetFloat("_Ambiant"),
                materiauOr.GetFloat("_Fresnel"), materiauOr.GetFloat("_Eclat"), materiauOr.GetFloat("_Brillance"));
        }
    }
}
