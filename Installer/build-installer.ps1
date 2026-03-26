param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$publishDir = Join-Path $scriptDir 'build\publish'
$isccPath = 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe'

if (!(Test-Path $isccPath)) {
    throw "Inno Setup Compiler nao encontrado em '$isccPath'."
}

if (Test-Path $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

$dotnet = 'dotnet'
& $dotnet publish (Join-Path $rootDir 'CalculadoraInteligente.csproj') -c $Configuration -r $Runtime --self-contained true /p:PublishSingleFile=false -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw 'Falha ao publicar a aplicacao.'
}

Push-Location $scriptDir
try {
    & $isccPath 'calculadora-inteligente.iss'
    if ($LASTEXITCODE -ne 0) {
        throw 'Falha ao compilar o instalador.'
    }
}
finally {
    Pop-Location
}
