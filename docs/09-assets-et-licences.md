# 9. Assets et licences

> **En bref** : d'où vient chaque ressource, ce qui est **versionné** dans ce dépôt public, et ce qu'il faut installer soi-même.

[← Builds et déploiement](08-builds-et-deploiement.md) · [README](../README.md) · Suivant : [Utilisation de l'IA →](10-utilisation-ia.md)

---

## Ce qui est versionné et ce qui ne l'est pas

Le dépôt est **public** : on n'y met que ce qu'on a le droit de redistribuer, et ce qui est nécessaire pour ouvrir et construire le jeu.

| Ressource | Versionné ? | Remarque |
|---|---|---|
| `Assets/FruitNinja/` (code, scènes, prefabs, matériaux, UI, sons choisis) | ✅ | le cœur du projet |
| Packs déjà présents au S7 (fruits, nature, sabre, EzySlice, Ball Pack…) | ✅ | repris tels quels |
| **Vefects** (effets de particules) | ⚠️ en partie | seules les **textures et le maillage utilisés** par les effets sont versionnés |
| **ToonScapes – Spring Isles** (décor high poly) | ❌ | pack acheté, 1,8 Go, licence non redistribuable : **à importer soi-même** dans `Assets/ToonScapes/` |
| Copies de matériaux ToonScapes ajustés pour la nuit | ✅ | dans `Assets/FruitNinja/Materials/ToonScapes/` (ils pointent vers les textures du pack) |
| Builds (`.exe`, `.apk`) | ❌ | générés à la demande (voir [Builds](08-builds-et-deploiement.md)) |

> [!IMPORTANT]
> Sans le pack ToonScapes, **tout le jeu fonctionne** avec le décor low poly. Seules les scènes `MainMenu_ToonScapes` et `Game_ToonScapes` ont des objets manquants, et l'option **Décor détaillé** retombe automatiquement sur le décor d'origine si ces scènes ne sont pas dans le build.

---

## Inventaire des ressources

### Modèles et décors

| Ressource | Source | Utilisation |
|---|---|---|
| Low Poly Fruits | pack du S7 | tous les fruits |
| Ball Pack | pack du S7 | la bombe |
| JTS – The Samurai Sword | pack du S7 | les deux sabres (Nodachi) |
| Simple Nature Pack | pack du S7 | décor low poly (arbres, rochers, sol, collines) |
| ToonScapes – Spring Isles | Unity Asset Store (acheté) | décor high poly |
| Canon stylisé (`stylized_canon.glb`) | modèle **Sketchfab** (export glTF), converti avec Blender — ⚠️ auteur et licence à vérifier et à créditer | canons lanceurs |

### Effets

| Ressource | Source | Utilisation |
|---|---|---|
| Vefects (Anime Stylized VFX, Stylized AoE VFX) | Unity Asset Store | textures des effets de jus, d'explosion et de fumée, réutilisées avec un shader maison |

### Interface

| Ressource | Source / licence | Utilisation |
|---|---|---|
| Logo « Fruity Ninja VR » | image générée par IA | menu, écran de démarrage |
| UI en bois et or | planche d'éléments d'UI (JPG, probablement Freepik d'après le nom du fichier) — ⚠️ licence et attribution à vérifier | boutons, planches, curseurs |
| Lilita One | Google Fonts, **SIL Open Font License 1.1** (`Polices/LilitaOne_Licence_OFL.txt`) | tous les textes |
| Émoji pastèque | **Noto Emoji** (Google) (`UI/Icone/NotoEmoji_Licence.txt`) | icône de l'application |

### Sons

| Ressource | Source | Utilisation |
|---|---|---|
| Musiques (*Mystical Glades*, *En Garde*, *Desert Battle*) | bibliothèque personnelle (Humble Bundle) | menu, départ, combat |
| Ambiance de forêt de nuit (pluie, grillons) | bibliothèque personnelle (Humble Bundle) | fond sonore |
| Sons de sabre, coupe, explosion | packs du projet | effets |
| « Pop », « boing », « pouf » des canons | bibliothèque personnelle (Humble Bundle : Mini Games SFX, Game SFX Treasury, Fantasy Game) | canons |

### Code tiers

| Ressource | Licence | Utilisation |
|---|---|---|
| EzySlice | MIT | découpe des maillages |
| XR Interaction Toolkit, OpenXR, URP, Input System | Unity Companion License | paquets Unity |
| Meta XR SDK | Oculus SDK License | fonctionnalités Quest |

> [!WARNING]
> Deux ressources ont une **origine à confirmer** (canon et planche d'UI). Avant une diffusion publique, il faut retrouver leur page d'origine et ajouter l'attribution demandée par leur licence (souvent « CC BY » sur Sketchfab, « Designed by Freepik » sur Freepik).

---

## Installer le pack ToonScapes

1. Acheter et télécharger **ToonScapes – Spring Isles** depuis l'Asset Store.
2. L'importer : il doit se trouver dans `Assets/ToonScapes/` (structure d'origine du pack, sans le déplacer).
3. Ouvrir `MainMenu_ToonScapes` : les objets doivent apparaître. Les références sont faites par **GUID**, que le pack garde d'une installation à l'autre.

---

[← Builds et déploiement](08-builds-et-deploiement.md) · [README](../README.md) · Suivant : [Utilisation de l'IA →](10-utilisation-ia.md)
