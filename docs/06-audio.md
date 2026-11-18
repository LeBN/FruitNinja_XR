# 6. Audio

> **En bref** : la **musique** et l'**ambiance** sont des fonds sonores en 2D ; **tous les effets** (coupes, bombes, canons, sabres, reflet des lames) sont joués **en 3D**, à l'endroit exact où ils se produisent.

[← Effets multiplateforme](05-effets-multiplateforme.md) · [README](../README.md) · Suivant : [Interface →](07-interface.md)

---

## Sommaire

- [2D ou 3D : la règle](#2d-ou-3d--la-règle)
- [Les sons du jeu](#les-sons-du-jeu)
- [`SonSpatial` : un son 3D en une ligne](#sonspatial--un-son-3d-en-une-ligne)
- [La musique de partie](#la-musique-de-partie)
- [Les réglages de volume](#les-réglages-de-volume)

---

## 2D ou 3D : la règle

| Type de son | Spatialisation | Pourquoi |
|---|---|---|
| Musique, ambiance de forêt | **2D** (`spatialBlend = 0`) | c'est un fond sonore : il ne vient d'aucun endroit précis |
| Effets (coupe, bombe, canon, sabre…) | **3D** (`spatialBlend = 1`) | en VR, entendre **d'où** vient un son aide à réagir (un canon à gauche, une bombe derrière) |

> [!TIP]
> **À retenir** : un son 3D doit être **mono**. Un fichier stéréo ne peut pas être placé correctement dans l'espace : il est fusionné ou mal positionné. Les sons ajoutés pour ce projet ont été convertis en mono à l'import.

---

## Les sons du jeu

| Son | Fichier(s) | Où il est joué |
|---|---|---|
| Musique du menu | `Menu_MysticalGlades.ogg` | 2D, en boucle |
| Départ de partie | `Jeu_Depart_EnGarde.ogg` (stinger) | 2D |
| Musique de combat | `Jeu_Combat_DesertBattle.ogg` | 2D, en boucle, enchaînée au stinger |
| Ambiance | `Ambiance_ForetNuit_PluieGrillons.ogg` | 2D, en boucle |
| Coupe d'un fruit | `Fruit_Decoupe_Splash.mp3` (volume 0,45) | 3D, au point de contact |
| Bombe | `Bombe_Explosion.mp3` | 3D, au point de contact |
| Reflet des lames | `Sabre_Reflet_Depart.wav` | 3D, sur chaque lame |
| Sabre, geste lent / rapide | `Sabre_Swing_Lent_1-3` / `Sabre_Swing_Rapide_1-3` | 3D, sur le sabre |
| Tir d'un canon | `Lanceur_Pop_1-4.ogg` | 3D, à la bouche du canon |
| Apparition / disparition d'un canon | `Chariot_Apparition_Boing_1-3.ogg` / `Chariot_Disparition_Pouf.ogg` | 3D, au pied du canon |
| Apparition du fruit Start | `Start_Apparition_Magique.wav` (synthétisé : whoosh qui monte, puis impact grave, carillon et étincelles) | 3D, au point d'apparition |

Quand il existe plusieurs variantes d'un son, on en tire une **au hasard**, avec une légère variation de hauteur (±5 %) : la répétition s'entend beaucoup moins.

---

## `SonSpatial` : un son 3D en une ligne

```csharp
SonSpatial.Jouer(clip, position, volume);
SonSpatial.JouerAuHasard(clips, position, volume);
```

Le principe :

1. on crée un petit objet temporaire **à la position du son** ;
2. on lui ajoute une `AudioSource` réglée en 3D (atténuation logarithmique de 0,6 à 20 m, sans effet Doppler) ;
3. on joue le son, puis l'objet se détruit tout seul à la fin du clip.

> [!NOTE]
> Avant, les sons de coupe et d'explosion étaient joués par une source **2D** collée au sabre : on les entendait « dans la tête », sans direction. Centraliser la création des sons 3D dans une seule fonction garantit que **tous** les effets ont le même réglage.

---

## La musique de partie

`GameMusicManager` enchaîne le stinger et la musique de combat **à l'échantillon près**, grâce à `AudioSource.PlayScheduled` et à l'horloge audio (`AudioSettings.dspTime`) plutôt qu'au temps du jeu :

```csharp
double debut = AudioSettings.dspTime + 0.05;
stinger.PlayScheduled(debut);
musique.PlayScheduled(debut + dureeStinger - battleMusicLead);   // la musique démarre 2 s avant la fin du stinger
```

En fin de partie, la musique de combat descend en fondu (1,5 s) ; l'ambiance continue.

---

## Les réglages de volume

Le menu **Paramètres** propose trois curseurs, sauvegardés dans les PlayerPrefs (voir [Interface](07-interface.md#le-menu-paramètres)) :

| Curseur | Ce qu'il règle | Comment |
|---|---|---|
| Volume général | tout le son | `AudioListener.volume` |
| Musique | musiques et ambiance | composant `VolumeMusique` sur ces sources : volume de la scène × réglage |
| Effets sonores | tous les effets 3D et les sabres | multiplié dans `SonSpatial` et `SwordSwingSound` |

Les réglages s'appliquent **en direct** : la classe `Parametres` déclenche un évènement `Change` à chaque modification, et les composants concernés se mettent à jour.

---

[← Effets multiplateforme](05-effets-multiplateforme.md) · [README](../README.md) · Suivant : [Interface →](07-interface.md)
