param(
    [string]$WorkspaceRoot = "d:\Solucao TaskList",
    [string]$ApiBaseUrl = "http://localhost:5050",
    [string]$ApiKey = "",
    [switch]$SkipBuild,
    [switch]$SkipHttp
)

$ErrorActionPreference = "Stop"

function Invoke-Step {
    param(
        [string]$Title,
        [scriptblock]$Action
    )

    Write-Host "==> $Title" -ForegroundColor Cyan
    & $Action
    Write-Host "OK: $Title" -ForegroundColor Green
    Write-Host ""
}

Push-Location $WorkspaceRoot

try {
    if (-not $SkipBuild) {
        Invoke-Step "Build solução principal" {
            dotnet build "$WorkspaceRoot\CineTrack.sln"
        }

    }

    if (-not $SkipHttp) {
        Invoke-Step "Health da API" {
            $apiHealth = Invoke-WebRequest -Uri "$ApiBaseUrl/health" -UseBasicParsing -TimeoutSec 10
            if ($apiHealth.StatusCode -ne 200) {
                throw "API não respondeu 200 em /health."
            }
        }

        Invoke-Step "Fontes da API" {
            $headers = @{}
            if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
                $headers["X-Api-Key"] = $ApiKey
            }

            $fontes = Invoke-WebRequest -Uri "$ApiBaseUrl/api/sync/fontes" -Headers $headers -UseBasicParsing -TimeoutSec 10
            if ($fontes.StatusCode -ne 200) {
                throw "API não respondeu 200 em /api/sync/fontes."
            }
        }
    }

    Write-Host "Smoke check concluído com sucesso." -ForegroundColor Green
}
finally {
    Pop-Location
}
