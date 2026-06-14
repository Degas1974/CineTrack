param(
    [string]$WorkspaceRoot = "c:\Solucao CineTrack",
    [string]$ServerInstance = "localhost",
    [string]$DatabaseName = "CineTrackDb",
    [string]$ConnectionString = "Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    [switch]$SkipDbBootstrap,
    [switch]$SkipMobileBuild,
    [switch]$RunMobile,
    [switch]$CleanMobileObj
)

$ErrorActionPreference = "Stop"

Push-Location $WorkspaceRoot

try {
    if (-not $SkipDbBootstrap) {
        & ".\scripts\bootstrap-local-db.ps1" -ServerInstance $ServerInstance -DatabaseName $DatabaseName
    }

    Write-Host "==> Build da API" -ForegroundColor Cyan
    dotnet build ".\CineTrack.API\CineTrack.API.csproj"

    if (-not $SkipMobileBuild) {
        if ($RunMobile) {
            if ($CleanMobileObj) {
                & ".\scripts\run-mobile-android.ps1" -CleanObj -Run
            }
            else {
                & ".\scripts\run-mobile-android.ps1" -Run
            }
        }
        else {
            if ($CleanMobileObj) {
                & ".\scripts\run-mobile-android.ps1" -CleanObj
            }
            else {
                & ".\scripts\run-mobile-android.ps1"
            }
        }
    }

    Write-Host "" 
    Write-Host "Próximos passos:" -ForegroundColor Green
    Write-Host ("1. Suba a API: .\scripts\start-local-api.ps1 -ConnectionString '{0}'" -f $ConnectionString)
    if ($RunMobile) {
        Write-Host "2. O app Android já foi disparado por este script."
    }
    else {
        Write-Host "2. Build do app Android já validado; para rodar use: .\scripts\run-mobile-android.ps1 -Run"
    }
    Write-Host "3. Health check: Invoke-WebRequest http://localhost:5050/health"
}
finally {
    Pop-Location
}