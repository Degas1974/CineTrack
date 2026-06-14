param(
    [string]$WorkspaceRoot = "c:\Solucao CineTrack",
    [string]$ConnectionString = "Server=localhost;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Push-Location $WorkspaceRoot

try {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:CINETRACK_SQL_CONNECTION = $ConnectionString

    if (-not $SkipBuild) {
        Write-Host "==> Build da API" -ForegroundColor Cyan
        dotnet build ".\CineTrack.API\CineTrack.API.csproj"
    }

    Write-Host "==> Iniciando API em http://localhost:5050" -ForegroundColor Cyan
    Write-Host "==> Connection string ativa: $ConnectionString" -ForegroundColor DarkGray
    dotnet run --project ".\CineTrack.API\CineTrack.API.csproj" --no-launch-profile
}
finally {
    Pop-Location
}