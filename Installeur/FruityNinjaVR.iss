; Installeur Windows de Fruity Ninja VR (Inno Setup 6)
;
; Compilation (depuis la racine du dépôt) :
;   ISCC.exe Installeur\FruityNinjaVR.iss /DSource="A:\Builds\FruitNinjaVR\Windows_PCVR" /O"A:\Builds\FruitNinjaVR"
; Source = dossier du build Windows produit par Unity (menu Fruity Ninja > Builds).

#define NomJeu "Fruity Ninja VR"
#define Exe "FruityNinjaVR.exe"
#ifndef Version
  #define Version "1.0.0"
#endif
#ifndef Partie
  #define Partie "Part1"
#endif
#ifndef Source
  #define Source "..\Builds\Windows_PCVR"
#endif
#define Depot "https://github.com/LeBN/FruitNinja_XR"

[Setup]
; Identifiant fixe : les versions suivantes mettent à jour la même installation
AppId={{6F1C2A7E-4B8D-4E2F-9C3A-F7D1B2E5A904}
AppName={#NomJeu}
AppVersion={#Version}
AppVerName={#NomJeu} {#Version}
AppPublisher=LeBN
AppPublisherURL={#Depot}
AppSupportURL={#Depot}/issues
AppUpdatesURL={#Depot}/releases
VersionInfoVersion={#Version}
VersionInfoDescription=Installeur de {#NomJeu}
; Installation pour l'utilisateur, sans droits administrateur (proposition de l'installer pour tous)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
DefaultDirName={autopf}\{#NomJeu}
DefaultGroupName={#NomJeu}
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
; Présentation
WizardStyle=modern
WizardImageFile=Images\Assistant_100.bmp,Images\Assistant_150.bmp,Images\Assistant_200.bmp
WizardSmallImageFile=Images\Entete_100.bmp,Images\Entete_150.bmp,Images\Entete_200.bmp
SetupIconFile=Images\FruityNinjaVR.ico
UninstallDisplayIcon={app}\{#Exe}
UninstallDisplayName={#NomJeu}
InfoBeforeFile=AvantInstallation.txt
; Sortie
OutputBaseFilename=FruityNinjaVR_Installeur_{#Partie}
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
; Fermer le jeu s'il est ouvert pendant une mise à jour
CloseApplications=yes

[Languages]
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "bureau"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Tout le build, sauf les dossiers de débogage que Unity demande de ne pas distribuer
Source: "{#Source}\*"; DestDir: "{app}"; Excludes: "*_DoNotShip,*_ButDontShipItWithYourGame"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "AvantInstallation.txt"; DestDir: "{app}"; DestName: "LISEZMOI.txt"; Flags: ignoreversion

[Icons]
Name: "{group}\{#NomJeu}"; Filename: "{app}\{#Exe}"
Name: "{group}\Lisez-moi"; Filename: "{app}\LISEZMOI.txt"
Name: "{group}\{cm:UninstallProgram,{#NomJeu}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#NomJeu}"; Filename: "{app}\{#Exe}"; Tasks: bureau

[Run]
Filename: "{app}\{#Exe}"; Description: "{cm:LaunchProgram,{#NomJeu}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Fichiers créés par le jeu dans son dossier (journaux, cache)
Type: filesandordirs; Name: "{app}\FruityNinjaVR_Data\*.log"
