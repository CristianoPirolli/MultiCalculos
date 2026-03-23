[Setup]
AppName=Calculadora Inteligente
AppVersion=1.0.0
AppPublisher=Cristiano Pirolli
AppPublisherURL=https://github.com/CristianoPirolli
AppSupportURL=https://github.com/CristianoPirolli
AppUpdatesURL=https://github.com/CristianoPirolli
DefaultDirName={autopf}\CalculadoraInteligente
DefaultGroupName=Calculadora Inteligente
OutputDir=.
OutputBaseFilename=CalculadoraInteligente-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=classic
SetupIconFile=..\UI\image\calculator_icon.ico
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
ShowLanguageDialog=no

[Files]
Source: "..\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Calculadora Inteligente"; Filename: "{app}\CalculadoraInteligente.exe"; IconFilename: "{app}\CalculadoraInteligente.exe"
Name: "{autodesktop}\Calculadora Inteligente"; Filename: "{app}\CalculadoraInteligente.exe"; IconFilename: "{app}\CalculadoraInteligente.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Area de Trabalho"; GroupDescription: "Atalhos adicionais:"

[Run]
Filename: "{app}\CalculadoraInteligente.exe"; Description: "Executar Calculadora Inteligente"; Flags: nowait postinstall skipifsilent
