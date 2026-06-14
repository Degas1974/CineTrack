param(
    [string]$BaseUrl = "http://localhost:5050/",
    [string]$ApiKey = "",
    [int]$TimeoutSec = 15
)

$ErrorActionPreference = "Stop"

if (-not $BaseUrl.EndsWith("/")) {
    $BaseUrl = "$BaseUrl/"
}

$headers = @{}
if (-not [string]::IsNullOrWhiteSpace($ApiKey)) {
    $headers["X-Api-Key"] = $ApiKey
}

$tests = @(
    @{ Name = "Health"; Url = "${BaseUrl}health"; Auth = $false },
    @{ Name = "Fontes"; Url = "${BaseUrl}api/sync/fontes"; Auth = $true },
    @{ Name = "Estatisticas"; Url = "${BaseUrl}api/estatisticas"; Auth = $true },
    @{ Name = "Busca inicial"; Url = "${BaseUrl}api/midias"; Auth = $true },
    @{ Name = "Diagnostico"; Url = "${BaseUrl}api/sync/diagnostico"; Auth = $true }
)

foreach ($test in $tests) {
    $requestHeaders = if ($test.Auth) { $headers } else { @{} }
    Write-Host "==> $($test.Name): $($test.Url)" -ForegroundColor Cyan
    $response = Invoke-WebRequest -Uri $test.Url -Headers $requestHeaders -UseBasicParsing -TimeoutSec $TimeoutSec
    if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 300) {
        throw "$($test.Name) retornou HTTP $($response.StatusCode)."
    }
    Write-Host "OK: HTTP $($response.StatusCode)" -ForegroundColor Green
}

Write-Host "Smoke test da API direta concluido." -ForegroundColor Green
