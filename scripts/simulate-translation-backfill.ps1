param(
    [int]$MaxGrupos = 0,
    [decimal]$PrecoUsdPorMilhao = 0,
    [int]$FranquiaGratisCaracteres = 0,
    [int]$EstimativaCaracteresMidiaSemTexto = 600,
    [int]$EstimativaCaracteresEpisodioSemTexto = 350,
    [bool]$ExpandirSeriesCompleta = $true,
    [int]$MaxTemporadasPorSerie = 8,
    [int]$EstimativaTemporadasPorSerieSemLista = 4,
    [int]$EstimativaEpisodiosPorTemporadaSemLista = 10,
    [string]$BaseUrl = 'http://localhost:5050'
)

$endpoint = "$BaseUrl/api/sync/backfill/simulacao?maxGrupos=$MaxGrupos&precoUsdPorMilhao=$PrecoUsdPorMilhao&franquiaGratisCaracteres=$FranquiaGratisCaracteres&estimativaCaracteresMidiaSemTexto=$EstimativaCaracteresMidiaSemTexto&estimativaCaracteresEpisodioSemTexto=$EstimativaCaracteresEpisodioSemTexto&expandirSeriesCompleta=$ExpandirSeriesCompleta&maxTemporadasPorSerie=$MaxTemporadasPorSerie&estimativaTemporadasPorSerieSemLista=$EstimativaTemporadasPorSerieSemLista&estimativaEpisodiosPorTemporadaSemLista=$EstimativaEpisodiosPorTemporadaSemLista"

Write-Host "==> Simulando backfill de tradução (maxGrupos=$MaxGrupos, precoUsd=$PrecoUsdPorMilhao/M, franquia=$FranquiaGratisCaracteres, fallbackMidia=$EstimativaCaracteresMidiaSemTexto, fallbackEpisodio=$EstimativaCaracteresEpisodioSemTexto, expandirSeries=$ExpandirSeriesCompleta, maxTemporadas=$MaxTemporadasPorSerie, fallbackTemporadas=$EstimativaTemporadasPorSerieSemLista, fallbackEps=$EstimativaEpisodiosPorTemporadaSemLista)"
$response = Invoke-RestMethod -Method Get -Uri $endpoint -TimeoutSec 1800
$response | ConvertTo-Json -Depth 6
