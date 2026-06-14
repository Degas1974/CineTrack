param(
    [string]$WorkspaceRoot = "c:\Solucao CineTrack",
    [switch]$Run,
    [switch]$CleanObj
)

$ErrorActionPreference = "Stop"

Push-Location $WorkspaceRoot

try {
    $objPath = Join-Path $WorkspaceRoot "CineTrack.Mobile\obj\Debug\net10.0-android"

    if ($CleanObj -and (Test-Path $objPath)) {
        Write-Host "==> Limpando artefatos Android em '$objPath'" -ForegroundColor Cyan
        Remove-Item $objPath -Recurse -Force
    }

    if ($Run) {
        Write-Host "==> Executando app Android (device/emulador precisa estar disponível)" -ForegroundColor Cyan
        dotnet build ".\CineTrack.Mobile\CineTrack.Mobile.csproj" -t:Run -f net10.0-android
        return
    }

    Write-Host "==> Build do app Android" -ForegroundColor Cyan
    dotnet build ".\CineTrack.Mobile\CineTrack.Mobile.csproj"
}
finally {
    Pop-Location
}