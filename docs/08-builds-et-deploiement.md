# 8. Builds et déploiement

> **En bref** : deux builds à partir du même projet, **Windows PCVR** (un dossier avec un `.exe`) et **Meta Quest** (un `.apk` à installer sur le casque). Ce document explique comment les générer, les installer et lire ce qu'il y a dedans.

[← Interface](07-interface.md) · [README](../README.md) · Suivant : [Assets et licences →](09-assets-et-licences.md)

---

## Sommaire

- [Les deux configurations](#les-deux-configurations)
- [Générer les builds](#générer-les-builds)
- [Build Windows PCVR](#build-windows-pcvr)
- [Build Android pour Meta Quest](#build-android-pour-meta-quest)
- [Installer l'APK sur le casque](#installer-lapk-sur-le-casque)
- [Que contient l'APK ?](#que-contient-lapk-)
- [Problèmes rencontrés et solutions](#problèmes-rencontrés-et-solutions)

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

## Build Android pour Meta Quest

1. Vérifier que le module **Android Build Support** (avec SDK, NDK et OpenJDK) est installé pour Unity 6000.4.6f1.
2. `File > Build Profiles` → **Android**. Dans *XR Plug-in Management > Android* : **OpenXR** + la fonctionnalité **Meta Quest Support**.
3. Générer (menu *Fruity Ninja → Builds → Meta Quest (APK)*).

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
