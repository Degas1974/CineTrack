param(
    [string]$BaseUrl = 'http://localhost:5050'
)

$ErrorActionPreference = 'Stop'

$scenarios = @(
    @{
        Nome = 'Leve'
        Query = 'maxGrupos=25&expandirSeriesCompleta=false&estimativaCaracteresMidiaSemTexto=450&estimativaCaracteresEpisodioSemTexto=250'
    },
    @{
        Nome = 'Media'
        Query = 'maxGrupos=75&expandirSeriesCompleta=true&maxTemporadasPorSerie=4&estimativaTemporadasPorSerieSemLista=2&estimativaEpisodiosPorTemporadaSemLista=8&estimativaCaracteresMidiaSemTexto=600&estimativaCaracteresEpisodioSemTexto=300'
    },
    @{
        Nome = 'Pesada'
        Query = 'maxGrupos=0&expandirSeriesCompleta=true&maxTemporadasPorSerie=8&estimativaTemporadasPorSerieSemLista=4&estimativaEpisodiosPorTemporadaSemLista=10&estimativaCaracteresMidiaSemTexto=600&estimativaCaracteresEpisodioSemTexto=350'
    },
    @{
        Nome = 'Extrema'
        Query = 'maxGrupos=0&expandirSeriesCompleta=true&maxTemporadasPorSerie=12&estimativaTemporadasPorSerieSemLista=6&estimativaEpisodiosPorTemporadaSemLista=12&estimativaCaracteresMidiaSemTexto=800&estimativaCaracteresEpisodioSemTexto=450'
    }
)

$results = foreach ($scenario in $scenarios) {
    $uri = "$BaseUrl/api/sync/backfill/simulacao?$($scenario.Query)"
    $parsed = Invoke-RestMethod -Method Get -Uri $uri -TimeoutSec 1800

    [pscustomobject]@{
        Cenario = $scenario.Nome
        Grupos = $parsed.gruposSimulados
        MatchImdb = $parsed.gruposComMatchImdb
        Filmes = $parsed.filmesEstimados
        Series = $parsed.seriesEstimadas
        Temporadas = $parsed.temporadasEstimadas
        Episodios = $parsed.episodiosEstimados
        Caracteres = $parsed.caracteresTotaisTraducao
        CustoBrutoUSD = [math]::Round([decimal]$parsed.custoUsdBruto, 3)
        CustoExcedenteUSD = [math]::Round([decimal]$parsed.custoUsdExcedente, 3)
    }
}

$results | Format-Table -AutoSize
