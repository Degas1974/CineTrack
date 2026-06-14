param(
    [int]$Quantidade = 100,
    [string]$BaseUrl = 'http://localhost:5050'
)

$endpoint = "$BaseUrl/api/sync/posters/reprocessar?quantidade=$Quantidade"

Write-Host "==> Reprocessando posters IMDb em lote ($Quantidade itens)"
$response = Invoke-RestMethod -Method Post -Uri $endpoint -TimeoutSec 600
$response | ConvertTo-Json -Depth 5