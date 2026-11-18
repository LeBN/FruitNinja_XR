# 3. VR et interactions

> **En bref** : le jeu utilise **OpenXR** (le standard ouvert de la VR) avec les extensions **Meta**, le **XR Interaction Toolkit** pour le rig et les menus, et quelques scripts maison pour les sensations (vibrations, son des sabres).

[← Gameplay](02-gameplay.md) · [README](../README.md) · Suivant : [Rendu et shaders →](04-rendu-et-shaders.md)

---

## Sommaire

- [La pile XR du projet](#la-pile-xr-du-projet)
- [Le rig du joueur](#le-rig-du-joueur)
- [Rayons dans les menus, sabres en jeu](#rayons-dans-les-menus-sabres-en-jeu)
- [Le rendu stéréo : Single Pass Instanced](#le-rendu-stéréo--single-pass-instanced)
- [Les vibrations](#les-vibrations)
- [Le son des sabres](#le-son-des-sabres)
- [Pièges rencontrés](#pièges-rencontrés)

---

## La pile XR du projet

| Couche | Paquet | Version | Rôle |
|---|---|---|---|
| Gestion XR | XR Plugin Management | 4.5.2 | choisit et démarre le « loader » XR |
| Runtime | OpenXR | 1.16.1 | communique avec le casque (PC : Link ou SteamVR ; Quest : runtime Meta) |
| Extensions Meta | Meta XR SDK (all) + Unity OpenXR Meta | 203.0.0 / 2.5.0 | fonctionnalités Quest (écran de démarrage système, profils de manettes…) |
| Interactions | XR Interaction Toolkit | 3.4.1 | rig, rayons, poke, UI en VR, vibrations |
| Mains | XR Hands | 1.7.3 | suivi des mains (disponible, non utilisé en jeu) |
| Entrées | Input System | 1.19.0 | actions des manettes |

> [!NOTE]
> **Pourquoi OpenXR plutôt que le plugin Oculus ?** OpenXR est un standard : le même projet tourne en PC VR (Link, SteamVR…) et sur Quest, et les fonctionnalités propres à Meta s'ajoutent sous forme d'extensions OpenXR, sans dupliquer la configuration.

---

## Le rig du joueur

Le prefab `XR_Rig_Joueur` (dans `Prefabs/XR`) contient :

```text
XR_Rig_Joueur        XR Origin (l'origine = le sol du joueur)
└── Camera Offset
    ├── Main Camera          suivie par le casque (TrackedPoseDriver)
    ├── Left Controller      manette gauche → Sabre_Gauche
    └── Right Controller     manette droite → Sabre_Droit
Locomotion                   déplacement, rotation, gravité…
```

Chaque manette porte un `HapticImpulsePlayer` (vibrations) et ses interacteurs : *Near-Far* (rayon + saisie de près), *Poke* (appui du doigt) et *Teleport*.

---

## Rayons dans les menus, sabres en jeu

| Scène | Rayons (*Near-Far Interactor*) | Pourquoi |
|---|---|---|
| Menus, Debug | **activés** | il faut viser les boutons, les curseurs |
| Jeu | **désactivés**, ainsi que l'arc de téléportation et le poke | on joue au sabre ; aucun pointeur ne doit gêner la vue (déplacement au joystick) |

> [!WARNING]
> Désactiver le rayon dans la scène ne suffit pas : le `ControllerInputActionManager` des Starter Assets le **réactive** à chaque fin de visée de téléportation. `MasquerPointeurs` (sur le rig) coupe donc, à chaque image et juste avant le rendu, le rayon, la téléportation et le poke dans les scènes `Game*`.

Les menus sont des **canvas en espace monde** (`World Space`) avec un `TrackedDeviceGraphicRaycaster` : les rayons des manettes y cliquent comme une souris, et un `EventSystem` avec `XRUIInputModule` fait le lien.

---

## Le rendu stéréo : Single Pass Instanced

En VR, chaque image est dessinée **deux fois**, une par œil. Il y a deux façons de faire :

| Mode | Principe | Coût |
|---|---|---|
| Multi Pass | on dessine toute la scène pour l'œil gauche, puis pour l'œil droit | ×2 sur les appels de dessin |
| **Single Pass Instanced** (Multiview sur Quest) | chaque objet est dessiné **une fois pour les deux yeux** grâce à l'instanciation GPU | presque ×1 |

Le projet est en **Single Pass Instanced sur PC et sur Quest**. La version Quest était au départ en Multi Pass, réglage repris d'un ancien projet ; une question pendant les tests l'a fait repérer et corriger.

> [!IMPORTANT]
> En Single Pass Instanced, **chaque shader doit gérer l'œil courant**. Sinon, l'objet n'apparaît que dans un œil. Tous les shaders maison du projet contiennent ces macros :
> ```hlsl
> UNITY_VERTEX_INPUT_INSTANCE_ID           // dans les attributs du sommet
> UNITY_VERTEX_OUTPUT_STEREO               // dans la structure transmise au fragment
> UNITY_SETUP_INSTANCE_ID(v);              // au début du vertex shader
> UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
> UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // au début du fragment shader
> ```

---

## Les vibrations

`Vibrations.Envoyer(amplitude, durée)` fait vibrer les deux manettes :

1. d'abord via les `HapticImpulsePlayer` du rig XRI, la méthode fiable en OpenXR ;
2. sinon via l'ancienne API `InputDevices`, en secours ;
3. rien du tout si l'option **Vibrations** est désactivée dans les Paramètres.

| Événement | Amplitude | Durée |
|---|---|---|
| Bombe touchée | 0,9 | 0,35 s |
| Reflet des lames au départ | 0,5 | 0,8 s |
| Activation de l'option Vibrations | 0,5 | 0,15 s |

---

## Le son des sabres

`SwordSwingSound` joue un son de fendage d'air quand la lame **coupe l'air dans le sens du tranchant** :

1. il mesure la vitesse **au bout de la lame** (le point de découpe le plus éloigné de la poignée), là où un vrai sabre siffle ;
2. il ne garde que la part de vitesse **dans l'axe du tranchant** : un sabre qu'on déplace à plat ne siffle pas ;
3. il garde le **pic de vitesse** pendant 0,1 s avant de choisir le son, pour bien classer les gestes rapides ;
4. au-dessus de **1,5 m/s** : son grave (geste lent) ; au-dessus de **3 m/s** : son aigu (geste rapide).

> [!TIP]
> Pourquoi au bout de la lame ? Pour une même rotation du poignet, le bout d'une lame d'un mètre va beaucoup plus vite que le milieu. Mesurer au milieu rendait le son aigu presque impossible à obtenir.

---

## Rotation et zone de jeu

| Réglage | Fonctionnement |
|---|---|
| **Rotation** (Paramètres) | *Instantanée* (par crans, par défaut, plus confortable) ou *Continue*. `RotationJoueur` applique le choix au `ControllerInputActionManager` des manettes (propriété `smoothTurnEnabled`) à chaque chargement de scène et à chaque changement. |
| **Zone de 7 m** | `LimiteZone`, sur le rig, mesure la position de la **tête** par rapport au point de départ. Si elle sort du cercle (joystick, téléportation ou marche réelle), le rig est ramené au bord. Le cercle est visible dans l'éditeur en sélectionnant le rig. |

> [!TIP]
> On limite la **tête** et pas seulement le rig : dans un espace de jeu réel, on peut aussi sortir de la zone en marchant.

---

## Pièges rencontrés

| Symptôme | Cause | Correction |
|---|---|---|
| La vue « sautait » sous le sol toutes les secondes sur Quest | le modèle FBX du canon contenait une **caméra** (exportée par erreur depuis Blender) : chaque canon qui apparaissait prenait l'affichage | import des caméras et lumières désactivé sur le FBX |
| Le fruit de départ était trop haut | position fixe à 1,93 m du sol | placement selon la hauteur des yeux |
| Objet visible dans un seul œil (risque) | shader sans macros stéréo | macros SPI dans tous les shaders maison |
| Éclairs très lumineux dans les arbres, seulement dans le casque | pixels invalides ou démesurés (calcul en demi-précision sur la puce mobile) étalés par le bloom | caméra en **Stop NaN** + bloom plafonné (`clamp`) |

---

[← Gameplay](02-gameplay.md) · [README](../README.md) · Suivant : [Rendu et shaders →](04-rendu-et-shaders.md)
