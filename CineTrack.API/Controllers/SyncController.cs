using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using CineTrack.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncRepository _syncRepository;
    private readonly ISyncOrchestrator _syncOrchestrator;
    private readonly IImdbDatasetImporter _imdbDatasetImporter;
    private readonly IRottenTomatoesProvider _rottenTomatoesProvider;

    public SyncController(
        ISyncRepository syncRepository,
        ISyncOrchestrator syncOrchestrator,
        IImdbDatasetImporter imdbDatasetImporter,
        IRottenTomatoesProvider rottenTomatoesProvider)
    {
        _syncRepository = syncRepository;
        _syncOrchestrator = syncOrchestrator;
        _imdbDatasetImporter = imdbDatasetImporter;
        _rottenTomatoesProvider = rottenTomatoesProvider;
    }

    [HttpGet("fontes")]
    public async Task<ActionResult<IEnumerable<FonteSyncStatus>>> GetFontes()
    {
        var fontes = await _syncRepository.GetFontesStatusAsync();
        return Ok(fontes);
    }

    [HttpGet("associacoes/pendentes")]
    public async Task<ActionResult<IEnumerable<AssociacaoPendenteVM>>> GetAssociacoesPendentes()
    {
        var associacoes = await _syncRepository.GetAssociacoesPendentesAsync();
        return Ok(associacoes);
    }

    [HttpGet("associacoes/resolvidas")]
    public async Task<ActionResult<IEnumerable<AssociacaoPendenteVM>>> GetAssociacoesResolvidas([FromQuery] int quantidade = 100)
    {
        var associacoes = await _syncRepository.GetAssociacoesResolvidasAsync(Math.Max(quantidade, 1));
        return Ok(associacoes);
    }

    [HttpPost("associacoes/{id}/confirmar")]
    public async Task<IActionResult> ConfirmarAssociacao(int id)
    {
        await _syncRepository.ConfirmarAssociacaoAsync(id);
        return Ok();
    }

    [HttpPost("associacoes/{id}/selecionar")]
    public async Task<IActionResult> SelecionarAssociacao(int id)
    {
        await _syncRepository.SelecionarAssociacaoAsync(id);
        return Ok();
    }

    [HttpPost("associacoes/{id}/rejeitar")]
    public async Task<IActionResult> RejeitarAssociacao(int id)
    {
        await _syncRepository.RejeitarAssociacaoAsync(id);
        return Ok();
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<LogCaptura>>> GetLogs([FromQuery] int quantidade = 50)
    {
        var logs = await _syncRepository.GetLogsAsync(quantidade);
        return Ok(logs);
    }

    [HttpGet("logs/diagnostico")]
    public async Task<ActionResult<IEnumerable<DiagnosticoParserVM>>> GetLogsDiagnostico([FromQuery] int quantidade = 20)
    {
        var logs = await _syncRepository.GetLogsAsync(Math.Max(quantidade * 5, quantidade));
        var diagnosticos = logs
            .Where(log => log.Tipo == TipoLog.Info
                && log.Mensagem.Contains("captura parseada", StringComparison.OrdinalIgnoreCase))
            .Select(DiagnosticoParserMapper.FromLog)
            .Take(quantidade);

        return Ok(diagnosticos);
    }

    [HttpGet("diagnostico")]
    public async Task<ActionResult<IEnumerable<DiagnosticoParserVM>>> GetDiagnostico([FromQuery] int quantidade = 20)
    {
        return await GetLogsDiagnostico(quantidade);
    }

    [HttpGet("ultima-sync")]
    public async Task<ActionResult<object>> GetUltimaSync()
    {
        var ultimaSync = await _syncRepository.GetUltimaSyncAsync();
        return Ok(new { UltimaSync = ultimaSync });
    }

    [HttpPost("scraping/executar")]
    public async Task<ActionResult<ScrapingResult>> ExecutarScraping()
    {
        var result = await _syncOrchestrator.ExecutarScrapingSceneSourceAsync("API");
        return Ok(result);
    }

    [HttpPost("posters/reprocessar")]
    public async Task<ActionResult<PosterBackfillResult>> ReprocessarPosters([FromQuery] int quantidade = 100)
    {
        var result = await _syncOrchestrator.ReprocessarPostersImdbAsync("API", Math.Max(1, quantidade));
        return Ok(result);
    }

    [HttpPost("imdb/importar")]
    public async Task<ActionResult<ImdbDatasetImportResult>> ImportarImdbDatasets([FromBody] ImdbDatasetImportRequest? request = null)
    {
        var result = await _imdbDatasetImporter.ImportAsync(request ?? new ImdbDatasetImportRequest(), HttpContext.RequestAborted);
        return result.Sucesso ? Ok(result) : BadRequest(result);
    }

    [HttpPost("ratings/reprocessar")]
    public async Task<ActionResult<RottenTomatoesRatingsResult>> ReprocessarRatings([FromBody] RatingsReprocessRequest? request = null)
    {
        request ??= new RatingsReprocessRequest();
        var result = await _rottenTomatoesProvider.ReprocessarRatingsAsync(
            Math.Max(1, request.Quantidade),
            request.Forcar,
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("backfill/simulacao")]
    public async Task<ActionResult<BackfillSimulationResult>> SimularBackfill(
        [FromQuery] int maxGrupos = 0,
        [FromQuery] decimal precoUsdPorMilhao = 0m,
        [FromQuery] int franquiaGratisCaracteres = 0,
        [FromQuery] int estimativaCaracteresMidiaSemTexto = 600,
        [FromQuery] int estimativaCaracteresEpisodioSemTexto = 350,
        [FromQuery] bool expandirSeriesCompleta = true,
        [FromQuery] int maxTemporadasPorSerie = 8,
        [FromQuery] int estimativaTemporadasPorSerieSemLista = 4,
        [FromQuery] int estimativaEpisodiosPorTemporadaSemLista = 10)
    {
        var result = await _syncOrchestrator.SimularBackfillTraducaoAsync(
            "API",
            Math.Max(0, maxGrupos),
            precoUsdPorMilhao,
            Math.Max(0, franquiaGratisCaracteres),
            Math.Max(0, estimativaCaracteresMidiaSemTexto),
            Math.Max(0, estimativaCaracteresEpisodioSemTexto),
            expandirSeriesCompleta,
            Math.Max(1, maxTemporadasPorSerie),
            Math.Max(0, estimativaTemporadasPorSerieSemLista),
            Math.Max(0, estimativaEpisodiosPorTemporadaSemLista));

        return Ok(result);
    }
}
