# 8. Builds et déploiement

> **En bref** : deux builds à partir du même projet, **Windows PCVR** (un dossier avec un `.exe`) et **Meta Quest** (un `.apk` à installer sur le casque). Ce document explique comment les générer, les installer et lire ce qu'il y a dedans.

[← Interface](07-interface.md) · [README](../README.md) · Suivant : [Assets et licences →](09-assets-et-licences.md)

---

## Sommaire

- [Les deux configurations](#les-deux-configurations)
- [Générer les builds](#générer-les-builds)
- [Build Windows PCVR](#build-windows-pcvr)
- [Installeur Windows](#installeur-windows)
- [Build Android pour Meta Quest](#build-android-pour-meta-quest)
- [Installer l'APK sur le casque](#installer-lapk-sur-le-casque)
- [Que contient l'APK ?](#que-contient-lapk-)
- [Problèmes rencontrés et solutions](#problèmes-rencontrés-et-solutions)

---

## Où télécharger les builds

Chaque partie du mini projet a sa **release GitHub**, avec l'**installeur Windows** (`FruityNinjaVR_Installeur_PartN.exe`), le ZIP du build Windows PCVR (sans installation) et l'APK Meta Quest en pièces jointes :
[Part 1](https://github.com/LeBN/FruitNinja_XR/releases/tag/part1).

---

## Les deux configurations

| | Windows PCVR | Meta Quest |
|---|---|---|
| Cible | StandaloneWindows64 | Android (APK) |
| XR | OpenXR (Meta / Oculus Touch) | OpenXR + Meta Quest Support |
| Code compilé avec | Mono | **IL2CPP** (C# converti en C++, puis compilé en natif) |
| Architecture | x86_64 | **ARM64** |
| API graphique | Direct3D 11 | Vulkan (OpenGL ES 3 en secours) |
| Niveau d'API Android | – | min **32**, cible **34** |
| Rendu stéréo | Single Pass Instanced | Single Pass Instanced (Multiview) |
| Identifiant | `com.miniprojetirv.fruityninjavr` | `com.miniprojetirv.fruityninjavr` |
| Scènes | les 5 (menu d'origine en premier) | les 5 |
| Décor par défaut | High poly (ToonScapes) | Low poly |

> [!NOTE]
> **Pourquoi IL2CPP et ARM64 sur Quest ?** Le Quest est un appareil Android 64 bits ; le Meta Store exige ARM64, et IL2CPP est le seul moteur de script qui le permet. IL2CPP est aussi plus rapide que Mono à l'exécution.

---

## Générer les builds

Le menu **Fruity Ninja → Builds** (script `Assets/FruitNinja/Scripts/Editor/ConstructionBuilds.cs`) génère chaque version en un clic. On peut aussi l'appeler en ligne de commande, sans ouvrir l'éditeur :

```bash
# APK Quest
Unity.exe -batchmode -nographics -buildTarget Android -projectPath . -executeMethod ConstructionBuilds.Quest   -logFile build_quest.log

# Windows PCVR
Unity.exe -batchmode -nographics -buildTarget Win64   -projectPath . -executeMethod ConstructionBuilds.Windows -logFile build_windows.log
```

Le dossier de sortie est `Builds/` à côté du projet, ou celui de la variable d'environnement `FRUITNINJA_BUILDS`.

> [!TIP]
> Le premier build Android est long (import des textures en ASTC, Gradle). Les suivants réutilisent le cache et prennent environ **3 à 10 minutes**.

---

## Build Windows PCVR

1. `File > Build Profiles` → **Windows**. Vérifier dans `Project Settings > XR Plug-in Management > Windows` que **OpenXR** est coché, avec le profil des manettes Meta.
2. Vérifier que le **runtime OpenXR** du PC est actif (Meta Quest Link ou SteamVR) et que le casque est connecté.
3. Générer (menu *Fruity Ninja → Builds → Windows PCVR*).
4. Lancer `FruityNinjaVR.exe` avec le casque connecté.

À livrer : **le dossier complet** (l'`.exe` seul ne fonctionne pas sans `FruityNinjaVR_Data/` et les DLL). Le dossier `…_BurstDebugInformation_DoNotShip` n'est pas à livrer.

---

## Installeur Windows

Plutôt qu'un ZIP à décompresser, la version PC est aussi livrée en **installeur** : un seul `.exe` (≈ 200 Mo, contre 280 Mo pour le ZIP) qui installe le jeu, crée les raccourcis et s'ajoute aux *Programmes installés* de Windows pour pouvoir le désinstaller proprement.

Il est construit avec **[Inno Setup 6](https://jrsoftware.org/isinfo.php)** (gratuit, standard pour les jeux Windows), à partir du dossier `Installeur/` du dépôt :

| Fichier | Rôle |
|---|---|
| `FruityNinjaVR.iss` | script de l'installeur : nom, version, dossier d'installation, fichiers, raccourcis, langues |
| `AvantInstallation.txt` | page « Informations » affichée avant l'installation (prérequis VR, runtime OpenXR, contrôles, crédits), copiée aussi en `LISEZMOI.txt` |
| `Images/` | icône (pastèque) et images de l'assistant (logo sur le ciel de nuit), en 100 / 150 / 200 % pour les écrans haute résolution |

Ce que fait l'installeur :

- installation **pour l'utilisateur, sans droits administrateur** (dans `%LOCALAPPDATA%\Programs\Fruity Ninja VR`), avec la possibilité de l'installer pour tous ; le dossier est modifiable ;
- en **français** ou en anglais selon le choix au lancement ;
- raccourcis dans le menu Démarrer (jeu, lisez-moi, désinstallation), raccourci sur le bureau en option ;
- le dossier `…_DoNotShip` de Unity est **exclu** ;
- un **identifiant fixe** (`AppId`) : l'installeur d'une partie suivante met à jour la même installation au lieu d'en créer une seconde.

Pour le reconstruire après un nouveau build Windows :

```powershell
& "A:\Apps\InnoSetup6\ISCC.exe" Installeur\FruityNinjaVR.iss `
  /DSource="A:\Builds\FruitNinjaVR\Windows_PCVR" /DVersion=1.0.0 /DPartie=Part1 `
  /O"A:\Builds\FruitNinjaVR"
```

> [!NOTE]
> La compression (LZMA2 *ultra*, solide) tourne dans un processus 64 bits séparé (`LZMAUseSeparateProcess=yes`) : sur un build de 600 Mo, le compilateur 32 bits d'Inno Setup manquait de mémoire (« Out of memory »).

---

## Build Android pour Meta Quest

1. Vérifier que le module **Android Build Support** (avec SDK, NDK et OpenJDK) est installé pour Unity 6000.4.6f1.
2. `File > Build Profiles` → **Android**. Dans *XR Plug-in Management > Android* : **OpenXR** + la fonctionnalité **Meta Quest Support**.
3. Générer (menu *Fruity Ninja → Builds → Meta Quest (APK)*).

### Toujours la même signature

Android n'accepte une **mise à jour** que si le nouvel APK est signé avec la **même clé** que celui installé ; sinon il faut désinstaller, et les données du jeu (classement, réglages) sont perdues.

- `ConstructionBuilds` signe l'APK avec `Signature/FruityNinjaVR.keystore` s'il existe à la racine du projet. Ce dossier est **ignoré par git** : une clé ne se publie jamais.
- Cette clé est une **copie** de la clé de débogage qui a signé les premiers APK (même empreinte SHA-256) : aucune réinstallation n'a été nécessaire.
- Sans ce fichier (autre machine), le build garde la signature par défaut de Unity.

> [!IMPORTANT]
> Garde une copie de `Signature/FruityNinjaVR.keystore` en lieu sûr : c'est elle qui permet de mettre à jour le jeu sans perdre les données sur le casque.

---

## Installer l'APK sur le casque

Le test en **autonome** (casque débranché) est obligatoire : Quest Link ne le remplace pas.

### Par câble USB (ADB)

1. Activer le **mode développeur** du casque (application Meta Horizon sur le téléphone).
2. Brancher le casque et accepter le **débogage USB** dans le casque.
3. Installer :

```bash
adb devices                                   # le casque doit apparaître
adb install -r FruityNinjaVR_Quest.apk        # -r : remplace la version installée
```

`adb` est fourni avec Unity : `…/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe`.

### À distance (Tailscale)

Si le PC et le casque sont dans le même réseau Tailscale, on peut envoyer l'APK sans câble :

```bash
tailscale file cp FruityNinjaVR_Quest.apk <nom-du-casque>:
```

Le fichier arrive dans les fichiers reçus du casque, d'où on lance l'installation.

L'application apparaît ensuite dans la bibliothèque du Quest, rubrique **Sources inconnues**, avec l'icône pastèque.

---

## Que contient l'APK ?

Un APK est une **archive zip**. On peut l'ouvrir (APK Analyzer d'Android Studio, ou n'importe quel outil zip) pour voir ce qui pèse :

| Catégorie | Contenu | Poids typique |
|---|---|---|
| `lib/arm64-v8a/` | **libil2cpp.so** (tout le code C# du jeu et des paquets, compilé en natif), **libunity.so** (le moteur), plugins (OVRPlugin…) | ~53 Mo |
| `assets/bin/Data/` | scènes, textures, maillages, sons (fichiers `level*`, `sharedassets*`, `.resS`) | dépend du contenu |
| `assets/bin/Data/Managed/` | métadonnées IL2CPP (`global-metadata.dat`) | ~6 Mo |
| `classes.dex` | code Java/Kotlin (activité Android, SDK Meta) | ~6 Mo |
| `res/`, `META-INF/` | ressources Android, signature | < 1 Mo |

Évolution de la taille pendant le projet, et pourquoi :

| Version | APK | Explication |
|---|---|---|
| Décor d'origine seul | 102,5 Mo | base |
| + décor ToonScapes | 206,6 Mo | ~750 objets et leurs textures (limitées à 2048 px sur Android) |
| + éclairage précalculé **avec static batching** | 404,1 Mo | maillages fusionnés dupliqués dans chaque scène |
| static batching retiré | ~221 Mo | lightmaps, sonde de reflets et occlusion (~10 Mo) |

> [!TIP]
> **À retenir** : une analyse d'APK sert à repérer ce qui pèse et **pourquoi**. Ici, elle a révélé qu'une seule option (le static batching) doublait la taille du fichier.

---

## Problèmes rencontrés et solutions

| Problème | Cause | Solution |
|---|---|---|
| Échec Gradle : *namespace com.oculus.Integration utilisé plusieurs fois* | deux bibliothèques Meta (OVRPlugin et SDKTelemetry) déclarent le même namespace, refusé par le Gradle d'Unity 6 | script d'éditeur `MetaAarNamespaceFix` : renomme le namespace de SDKTelemetry pendant le build |
| Échec Jetifier : *only DEFLATED entries can have EXT descriptor* | l'archive `.aar` réécrite avait un format zip non standard | réécriture complète de l'archive en compression standard |
| Vue « sous le sol » sur Quest | caméra cachée dans le modèle FBX du canon | import des caméras désactivé sur le modèle |
| APK de 404 Mo | static batching | option retirée |

---

[← Interface](07-interface.md) · [README](../README.md) · Suivant : [Assets et licences →](09-assets-et-licences.md)
