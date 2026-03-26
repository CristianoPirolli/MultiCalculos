# MultiCálculos

Projeto desktop WPF em arquitetura de camadas para facilitar manutenção.

## Estrutura

- `UI/`: telas e eventos de interface
- `Application/`: fluxo da aplicação
- `Core/`: regras de cálculo puras
- `Installer/`: script do Inno Setup

## Executar

```powershell
$dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
& $dotnet restore .\CalculadoraInteligente.csproj
& $dotnet run --project .\CalculadoraInteligente.csproj
```

## Publicar EXE self-contained

```powershell
$dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
& $dotnet publish .\CalculadoraInteligente.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=false
```

Executável final:

`bin\Release\net8.0-windows\win-x64\publish\MultiCalculos.exe`

## Instalador

Compile `Installer\calculadora-inteligente.iss` no Inno Setup.
