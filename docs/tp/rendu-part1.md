# Rendu du TP – Mini projet VR, Part 1 : reprise et déploiement multiplateforme

> Cette page suit **exactement** la structure du sujet (*ST2VAAS 2026-2027, Fruit Ninja VR, Part 1*).
> Pour chaque point : ce qui a été fait, la preuve, et un état : ✅ fait · 🟡 partiel · ⏳ à faire.
> Les explications détaillées sont dans la [documentation](../../README.md#documentation-détaillée) ; cette page renvoie vers elles.

[← README](../../README.md)

---

## Vue d'ensemble

| Task | Sujet | État |
|---|---|---|
| [1](#task-1--reprise-et-vérification-du-projet-s7) | Reprise et vérification du projet S7 | 🟡 tableau de l'état initial à compléter |
| [2](#task-2--organisation-du-projet-unity) | Organisation du projet Unity | ✅ |
| [3](#task-3--particle-systems-pour-pcvr-et-quest) | Particle Systems pour PCVR et Quest | ✅ (impact mesuré : ⏳) |
| [4](#task-4--génération-et-test-des-builds) | Génération et test des builds | 🟡 builds faits, campagne de test à compléter |
| [5](#task-5--analyse-de-lapk-et-comparaison-des-builds) | Analyse de l'APK et comparaison des builds | 🟡 captures de l'APK Analyzer à ajouter |

---

## Task 1 – Reprise et vérification du projet S7

**1. Version, pipeline, packages ; copie du projet initial** ✅

| Élément | Avant (S7) | Après |
|---|---|---|
| Éditeur | Unity 6000.2.7f2 | **Unity 6000.4.6f1** |
| Pipeline | URP | URP 17.4 |
| XR | OpenXR + XR Interaction Toolkit | OpenXR 1.16.1, XRI 3.4.1, XR Hands 1.7.3, Meta XR SDK 203, Unity OpenXR Meta 2.5.0 |

Copie du projet initial : **fork GitHub** du dépôt du S7 (l'historique d'origine est conservé), puis conversion vers Unity 6000.4.6f1.

**2. XR Plugin Management, OpenXR, XRI, XR Origin, actions ; dépendances de plateforme** ✅

Configuration reprise de mon projet Birdverse : OpenXR sur Windows et Android, profils de manettes Meta, XR Origin avec actions XRI. Dépendance propre au Quest identifiée : fonctionnalité *Meta Quest Support* (Android uniquement). → [VR et interactions](../03-vr-et-interactions.md#la-pile-xr-du-projet)

**3. Tests : suivi, sabres, lancement, coupes, bonus, bombes, score, effets, pénalités** 🟡

Premier test dans le Meta XR Simulator, puis tests dans le casque au fil du projet. Les bugs trouvés ont été corrigés (voir le point 5).

**4. Fonctions S7 : difficultés, entraînement, menu, volume, classement, sauvegarde JSON** ✅ / 🟡

| Fonction S7 | État observé au départ | Après |
|---|---|---|
| Difficultés | présentes visuellement, **sans effet** | ✅ vraies valeurs (durée, cadence, bombes, force), choix en coupant un fruit |
| Entraînement | **absent** | ⏳ non réalisé (la scène `Debug_Fruits` sert seulement de vitrine des fruits) |
| Menu | **absent** du dépôt (scène d'origine introuvable) | ✅ menu principal recréé |
| Réglage du volume | **absent** | ✅ menu Paramètres (général, musique, effets) |
| Classement | **absent** (seulement un meilleur score) | ✅ top 8 avec difficulté et date |
| Sauvegarde JSON | **absente** | ✅ classement sérialisé en JSON (PlayerPrefs) |

**5. Tableau de l'état initial (fonctionnalité, état, correction) ; erreurs restantes** 🟡

| Fonctionnalité | État observé | Correction apportée |
|---|---|---|
| Bombes | n'apparaissaient presque jamais (un `else if` écrasait la bombe par un fruit) | logique de tirage corrigée |
| Difficultés | sans effet sur le jeu | réglages appliqués au spawner |
| Pomme d'or | matériau **jamais commité** au S7 : affichage sombre | matériau or métallique (`OrReflet`) |
| Effet d'explosion | 3 matériaux manquants | nouveaux effets (Task 3) |
| `StartFruitSpawnPoint` | espace à la fin du nom : `GameObject.Find` échouait | renommé `PointApparition_FruitDemarrage` |
| Scripts | accents illisibles (encodage) | convertis en UTF-8 |
| Fuite des effets d'explosion | objets jamais détruits | destruction après 3 s |

> ⏳ **À compléter** : tableau rempli à partir d'un test complet de la version S7 d'origine dans le casque.

> **Résultat attendu** : base jouable (fruits, bombes, sabres, score), projet sans erreur de compilation. → ✅ atteint.

---

## Task 2 – Organisation du projet Unity

**1. Regrouper scènes, scripts, prefabs, ressources** ✅ — tout ce qui est propre au jeu est dans `Assets/FruitNinja/`. → [README, organisation du dépôt](../../README.md#organisation-du-dépôt)

**2. Dossiers dédiés, rien à la racine d'Assets** ✅ — `Scenes`, `Scripts`, `Shaders`, `Prefabs`, `Materials`, `Textures`, `UI`, `Sons`, `Modeles`, `Polices`, `Settings`. Les packs tiers restent dans leurs propres dossiers.

**3. Prefabs des fruits, bombes, sabres, effets ; noms explicites** ✅

| Catégorie | Prefabs |
|---|---|
| Fruits | `Prefabs/Fruits/Fruit_Pomme`, `Fruit_Avocat`, `Fruit_Banane`, `Fruit_Cerises`, `Fruit_Citron`, `Fruit_Peche`, `Fruit_Cacahuete`, `Fruit_Poire`, `Fruit_Fraise`, `Fruit_Pasteque`, `Fruit_Dore_Bonus` ; `Resources/Fruit_Demarrage` |
| Bombe | `Prefabs/Bombes/Bombe` |
| Sabres | `JTS_TheSamuraiSword/Prefabs/Nodachi` (pack), instancié en `Sabre_Gauche` / `Sabre_Droit` |
| Effets | `Prefabs/FX/FX_Fruit_Jus_PCVR`, `FX_Fruit_Jus_Quest`, `FX_Bombe_Explosion_PCVR`, `FX_Bombe_Explosion_Quest`, `FX_Canon_Fumee` |
| Joueur, décor | `Prefabs/XR/XR_Rig_Joueur`, `Prefabs/Environnement/Env_Canon_Lanceur`, `Env_Lumiere_Lune`, `Env_Volume_PostProcess`, `Env_Sol` |

Convention : un préfixe par rôle (`Fruit_`, `FX_`, `Env_`, `XR_`, `Mat_`…), noms en français.

**4. Déplacements dans l'éditeur ; vérification des références** ✅ — déplacements par l'`AssetDatabase` de Unity (les GUID et `.meta` suivent), puis un script de vérification de toutes les scènes, prefabs et matériaux : **1608 éléments vérifiés, 0 problème** (après correction de 4 références cassées).

**5. Scène de démarrage, scènes nécessaires, ordre de build** ✅

| Index | Scène |
|---|---|
| 0 | `MainMenu` (démarrage) |
| 1 | `Game` |
| 2 | `Debug_Fruits` |
| 3 | `MainMenu_ToonScapes` |
| 4 | `Game_ToonScapes` |

→ [Architecture](../01-architecture.md#les-scènes-et-leur-enchaînement)

> **Résultat attendu** : projet organisé, références valides, démarrage identifié. → ✅ atteint.

---

## Task 3 – Particle Systems pour PCVR et Quest

**1. Deux types d'effets** ✅ — jus de fruit (éclaboussure) et explosion de bombe (pack Vefects, adapté à l'URP avec un shader maison).

**2. Reliés au gameplay** ✅ — jus **au point de contact** de la lame, à la couleur du fruit ; explosion au point de contact, avec la pénalité (−3, texte « −3 », vibration, son).

**3. Une version PCVR et une version Quest, chacune utilisée par le bon build** ✅ — prefabs `*_PCVR` / `*_Quest`, choix à la compilation par `PlatformFx` (`UNITY_ANDROID`).

**4. Versions Quest allégées, pas de collisions coûteuses** ✅ — moins de couches, bursts ×0,4, durée de vie ×0,7, pas de Noise ni de lumière, textures 256 px ASTC, aucune collision.

**5. Paramètres, justifications, impact, captures** 🟡 — tableau complet, justifications et captures comparables dans [Effets multiplateforme](../05-effets-multiplateforme.md). Impact : **5 fois moins de particules** (134 → 27 pour le jus, 106 → 22 pour l'explosion). Impact mesuré en FPS : ⏳.

> **Résultat attendu** : deux effets × deux plateformes, déclenchés correctement. → ✅ atteint (mesure de performance à faire).

---

## Task 4 – Génération et test des builds

### Build Windows PCVR

| Point | État | Détail |
|---|---|---|
| 1. Cible Windows, OpenXR, runtime, casque | ✅ | OpenXR coché pour Windows |
| 2. Scènes, effets PCVR, dossier dédié | ✅ | 5 scènes, effets PCVR, dossier `Windows_PCVR` |
| 3. Lancement de l'exécutable avec le casque | ⏳ | à faire et noter (hors éditeur) |

### Build Android pour Meta Quest

| Point | État | Détail |
|---|---|---|
| 1. Android Build Support, SDK, NDK ; casque en mode développeur | ✅ | installés avec Unity 6000.4.6f1 |
| 2. IL2CPP, ARM64, OpenXR + Meta Quest, API | ✅ | IL2CPP, ARM64, API min 32 / cible 34, Vulkan, Single Pass Instanced |
| 3. Scènes, effets Quest, validation XR, APK | ✅ | erreurs Gradle corrigées (namespace Meta), APK généré |
| 4. Installation (ADB / SideQuest), test autonome | ✅ | installé par Tailscale / ADB et testé en autonome |

→ [Builds et déploiement](../08-builds-et-deploiement.md)

### Vérifications sur les deux plateformes

Matériel : PC Intel Core i7-11700K, NVIDIA GeForce RTX 3080, 64 Go de RAM, Windows 11 ; casque **Meta Quest 3**.

| Élément | PCVR (Quest 3 via Link) | Quest 3 autonome |
|---|---|---|
| Tracking et sabres | ⏳ à noter | ✅ suivi et sabres OK (après correction de la caméra cachée dans le modèle du canon) |
| Gameplay | ⏳ à noter | ✅ fruits, bonus, bombes, coupes, score |
| Effets visuels (variante du build) | ⏳ à noter | ✅ variante Quest |
| UI et fonctions héritées | ⏳ à noter | ✅ menus, paramètres, classement |
| Exécution (chargement, fluidité, crash) | ⏳ à noter | 🟡 décor détaillé (high poly) : **~40 FPS** observés, bloom trop fort → bloom réduit sur Quest ; décor low poly : par défaut |

> FPS : valeur observée à l'œil dans le casque, **sans outil de mesure**. ⏳ Mesure à refaire avec l'**OVR Metrics Tool** (FPS, temps GPU), en jeu pendant une vague de fruits, pour les deux décors.

> **Résultat attendu** : deux builds générés et testés. → 🟡 builds générés ; test du build Windows et mesures à compléter.

---

## Task 5 – Analyse de l'APK et comparaison des builds

**1. Taille et catégories (APK Analyzer)** 🟡 — APK de **221,7 Mo** (1095 fichiers, 489,1 Mo décompressé). Catégories relevées en lisant l'APK comme une archive zip (mêmes informations que l'APK Analyzer) :

| Catégorie | Dans l'APK | Décompressé | Fichiers |
|---|---|---|---|
| Données Unity (`assets/bin/Data`) | 152,3 Mo | 276,7 Mo | 651 |
| Bibliothèques natives (`lib/`) | 53,4 Mo | 179,0 Mo | 21 |
| Code Java/Kotlin (`classes.dex`) | 6,2 Mo | 6,2 Mo | 1 |
| Métadonnées IL2CPP | 5,7 Mo | 20,4 Mo | 5 |
| Autres assets (dont l'écran de démarrage) | 3,5 Mo | 6,3 Mo | 30 |
| Ressources Android, signature | 0,4 Mo | 0,5 Mo | 378 |

> ⏳ Captures de l'APK Analyzer d'Android Studio à ajouter.

**2. `assets/bin/Data` et `libil2cpp.so`** ✅ — `assets/bin/Data` contient les données Unity : scènes et assets sérialisés (`level*`, `sharedassets*`), et les gros contenus (textures, maillages, sons) dans des fichiers `.resource` / `.resS`. `libil2cpp.so` (32,6 Mo, 129 Mo décompressé) est **tout le code C#** du jeu et des paquets, converti en C++ par IL2CPP puis compilé en ARM64. **Limite de l'outil** : il voit les fichiers Unity comme des blocs ; il ne dit pas quelle texture ou quel maillage occupe la place dans un `sharedassets`. Il faut pour cela le rapport de build d'Unity (*Editor.log* / *Build Report*).

**3. Ce qui prend le plus de place** ✅ — les **données Unity** (152 Mo), surtout les **textures du décor ToonScapes** (incluses pour l'option High poly, limitées à 2048 px en ASTC 6x6 sur Android), puis le **code natif** (53 Mo). L'analyse a aussi montré qu'une option (le static batching) **doublait l'APK** à 404 Mo. → [Que contient l'APK ?](../08-builds-et-deploiement.md#que-contient-lapk-)

**4. Comparaison APK / ZIP Windows / dossier Windows** 🟡

| Version | Taille | Remarque |
|---|---|---|
| APK Quest | 221,7 Mo | déjà compressé (zip), textures ASTC |
| ZIP Windows | 279,0 Mo | dossier compressé |
| Dossier Windows complet | 575,5 Mo | non compressé : Mono, Direct3D, plugins x86_64, textures d'origine |

Unités : Mo = 1 048 576 octets. L'APK étant lui-même une archive compressée, il se compare au **ZIP** Windows, pas au dossier.

**5. Lien avec les tests de la Task 4** 🟡 — la version Quest utilise des effets allégés, le décor low poly par défaut et un bloom de portée réduite en décor détaillé : c'est la réponse aux 40 FPS observés en high poly. La version PC garde les effets complets et le décor détaillé par défaut. ⏳ À confirmer par des mesures (OVR Metrics Tool).

> **Résultat attendu** : comparaison argumentée (taille, qualité, fonctionnement). → 🟡 tailles et qualité faites, mesures de fluidité à compléter.

---

## Livrables

| Livrable demandé | Où |
|---|---|
| 1. Projet source sur GitHub (Assets + `.meta`, Packages, ProjectSettings) + notice (Unity, scène de démarrage, contrôles, lancement) | ce dépôt ; la notice est le [README](../../README.md#démarrage-rapide) |
| 2. Builds testés (ZIP Windows complet + APK Quest) | générés (voir [Builds](../08-builds-et-deploiement.md)) ; à joindre au rendu |
| 3. Rapport PDF synthétique | rapport Word du projet, à exporter en PDF |

## Difficultés, solutions et limitations

| Difficulté | Solution | Limitation restante |
|---|---|---|
| Effets Vefects roses en URP | shader `ParticleMask` maison | — |
| Build Android : namespace Meta en double, Jetifier | correctif Gradle `MetaAarNamespaceFix` | — |
| Version Android en Multi Pass | passée en Single Pass Instanced | — |
| Caméra cachée dans le modèle du canon | import des caméras désactivé | — |
| APK doublé par le static batching | option retirée | — |
| 40 FPS en décor détaillé sur Quest | décor low poly par défaut, bloom réduit | mesure précise à faire |
| — | — | mode **entraînement** non réalisé ; pomme d'or = +1 point comme un fruit |

---

[← README](../../README.md)
