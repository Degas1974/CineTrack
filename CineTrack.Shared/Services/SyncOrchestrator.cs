using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace CineTrack.Shared.Services;

public interface ISyncOrchestrator
{
    Task<ScrapingResult> ExecutarScrapingSceneSourceAsync(string origem);
    Task<PosterBackfillResult> ReprocessarPostersImdbAsync(string origem, int quantidade = 100);
    Task<BackfillSimulationResult> SimularBackfillTraducaoAsync(string origem, int maxGrupos = 0, decimal precoUsdPorMilhao = 0m, int franquiaGratisCaracteres = 0, int estimativaCaracteresMidiaSemTexto = 600, int estimativaCaracteresEpisodioSemTexto = 350, bool expandirSeriesCompleta = true, int maxTemporadasPorSerie = 8, int estimativaTemporadasPorSerieSemLista = 4, int estimativaEpisodiosPorTemporadaSemLista = 10);
}

public class SyncOrchestrator : ISyncOrchestrator
{
    private const decimal ConfiancaBaseMidia = 80m;
    private const decimal BonusTituloExato = 10m;
    private const decimal BonusAnoCorreto = 5m;
    private const decimal BonusEpisodioAssociado = 5m;
    private const decimal BonusTituloEpisodioExato = 5m;

    private readonly ISyncRepository _syncRepository;
    private readonly ISceneSourceScraper _sceneSourceScraper;
    private readonly IImdbScraper _imdbScraper;
    private readonly IMidiaRepository _midiaRepository;
    private readonly ITemporadaRepository _temporadaRepository;
    private readonly IEpisodioRepository _episodioRepository;
    private readonly ILogger<SyncOrchestrator> _logger;

    public SyncOrchestrator(
        ISyncRepository syncRepository,
        ISceneSourceScraper sceneSourceScraper,
        IImdbScraper imdbScraper,
        IMidiaRepository midiaRepository,
        ITemporadaRepository temporadaRepository,
        IEpisodioRepository episodioRepository,
        ILogger<SyncOrchestrator> logger)
    {
        _syncRepository = syncRepository;
        _sceneSourceScraper = sceneSourceScraper;
        _imdbScraper = imdbScraper;
        _midiaRepository = midiaRepository;
        _temporadaRepository = temporadaRepository;
        _episodioRepository = episodioRepository;
        _logger = logger;
    }

    public async Task<ScrapingResult> ExecutarScrapingSceneSourceAsync(string origem)
    {
        var result = new ScrapingResult();
        var fonteResult = new SourceSyncResult
        {
            Fonte = "SceneSource",
            Status = "Executando",
            Inicio = DateTime.Now
        };
        result.Fontes.Add(fonteResult);

        try
        {
            var confiancaMinimaAutoAssociacao = await _syncRepository.GetConfiancaMinimaAutoAssociacaoAsync();

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.SceneSource,
                Tipo = TipoLog.Info,
                Mensagem = $"{origem}: iniciando scraping",
                DataLog = DateTime.Now
            });

            var capturas = await _sceneSourceScraper.ScrapeRecentAsync();
            fonteResult.ItensProcessados = capturas.Count;
            if (capturas.Count == 0)
            {
                result.Sucesso = true;
                result.Mensagem = "Scraping concluído sem capturas novas na fonte externa.";
                fonteResult.Status = "Sucesso";
                fonteResult.Mensagem = result.Mensagem;
                fonteResult.Fim = DateTime.Now;

                await _syncRepository.InsertLogAsync(new LogCaptura
                {
                    Fonte = FonteCaptura.SceneSource,
                    Tipo = TipoLog.Aviso,
                    Mensagem = $"{origem}: fonte externa respondeu sem posts processáveis",
                    Detalhes = "A rota do SceneSource respondeu com sucesso, mas nenhum post compatível foi encontrado para processamento.",
                    DataLog = DateTime.Now
                });

                await _syncRepository.SetUltimaSyncAsync(DateTime.Now);

                _logger.LogWarning("{Origem}: scraping concluído sem posts processáveis no SceneSource", origem);
                return result;
            }

            foreach (var grupoCapturas in AgruparCapturasPorPreferencia(capturas))
            {
                var capturasInseridas = new List<(int AssociacaoId, AssociacaoSceneSource Captura, bool AutoConfirmar)>();

                foreach (var captura in grupoCapturas)
                {
                    var diagnosticoCaptura = MontarDiagnosticoCaptura(captura);

                    await EnriquecerAssociacaoAsync(captura);
                    var autoConfirmar = DeveAutoConfirmar(captura, confiancaMinimaAutoAssociacao);

                    if (await _syncRepository.AssociacaoJaRegistradaAsync(captura))
                    {
                        result.ItensAtualizados++;
                        continue;
                    }

                    await _syncRepository.InsertLogAsync(new LogCaptura
                    {
                        Fonte = FonteCaptura.SceneSource,
                        Tipo = TipoLog.Info,
                        Mensagem = $"{origem}: captura parseada",
                        Detalhes = diagnosticoCaptura,
                        DataLog = DateTime.Now
                    });

                    _logger.LogInformation("{Origem}: captura parseada {DiagnosticoCaptura}", origem, diagnosticoCaptura);

                    captura.Status = StatusAssociacao.Pendente;
                    captura.DataConfirmacao = null;

                    var associacaoId = await _syncRepository.InsertAssociacaoAsync(captura);
                    capturasInseridas.Add((associacaoId, captura, autoConfirmar));
                    result.NovosItens++;
                }

                var melhorAutoConfirmavel = capturasInseridas
                    .Where(item => item.AutoConfirmar)
                    .OrderByDescending(item => item.Captura.ReleaseScore)
                    .ThenByDescending(item => item.Captura.Confianca)
                    .ThenByDescending(item => item.Captura.DataCaptura)
                    .FirstOrDefault();

                if (melhorAutoConfirmavel.AssociacaoId > 0)
                {
                    await _syncRepository.ConfirmarAssociacaoAsync(melhorAutoConfirmavel.AssociacaoId);
                }
            }

            await _syncRepository.SetUltimaSyncAsync(DateTime.Now);

            result.Sucesso = true;
            result.Mensagem = $"Scraping concluído. Novos: {result.NovosItens}; ignorados por duplicidade: {result.ItensAtualizados}.";
            fonteResult.Status = "Sucesso";
            fonteResult.Mensagem = result.Mensagem;
            fonteResult.NovosItens = result.NovosItens;
            fonteResult.ItensAtualizados = result.ItensAtualizados;
            fonteResult.Fim = DateTime.Now;

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.SceneSource,
                Tipo = TipoLog.Sucesso,
                Mensagem = $"{origem}: {result.Mensagem}",
                DataLog = DateTime.Now
            });

            _logger.LogInformation("{Origem}: scraping concluído. Novos={Novos}; Duplicados={Duplicados}", origem, result.NovosItens, result.ItensAtualizados);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            result.Sucesso = false;
            result.Mensagem = "SceneSource retornou 404 para a rota configurada.";
            result.Erros = 1;
            fonteResult.Status = "Aviso";
            fonteResult.Mensagem = result.Mensagem;
            fonteResult.Erros = result.Erros;
            fonteResult.Fim = DateTime.Now;

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.SceneSource,
                Tipo = TipoLog.Aviso,
                Mensagem = $"{origem}: SceneSource retornou 404",
                Detalhes = ex.Message,
                DataLog = DateTime.Now
            });

            _logger.LogWarning(ex, "{Origem}: SceneSource retornou 404", origem);
        }
        catch (HttpRequestException ex)
        {
            result.Sucesso = false;
            result.Mensagem = ex.Message;
            result.Erros = 1;
            fonteResult.Status = "Erro";
            fonteResult.Mensagem = result.Mensagem;
            fonteResult.Erros = result.Erros;
            fonteResult.Fim = DateTime.Now;

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.SceneSource,
                Tipo = TipoLog.Erro,
                Mensagem = $"{origem}: falha HTTP ao consultar SceneSource",
                Detalhes = ex.Message,
                DataLog = DateTime.Now
            });

            _logger.LogError(ex, "{Origem}: falha HTTP ao consultar SceneSource", origem);
        }
        catch (Exception ex)
        {
            result.Sucesso = false;
            result.Mensagem = ex.Message;
            result.Erros = 1;
            fonteResult.Status = "Erro";
            fonteResult.Mensagem = result.Mensagem;
            fonteResult.Erros = result.Erros;
            fonteResult.Fim = DateTime.Now;

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.SceneSource,
                Tipo = TipoLog.Erro,
                Mensagem = $"{origem}: erro no scraping",
                Detalhes = ex.Message,
                DataLog = DateTime.Now
            });

            _logger.LogError(ex, "{Origem}: erro no scraping", origem);
        }

        return result;
    }

    public async Task<PosterBackfillResult> ReprocessarPostersImdbAsync(string origem, int quantidade = 100)
    {
        var result = new PosterBackfillResult();

        try
        {
            var candidatas = (await _midiaRepository.GetMidiasSemPosterAsync(Math.Max(1, quantidade))).ToList();
            result.ItensProcessados = candidatas.Count;

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.IMDb,
                Tipo = TipoLog.Info,
                Mensagem = $"{origem}: iniciando reprocessamento de posters IMDb",
                Detalhes = $"Mídias candidatas: {candidatas.Count}",
                DataLog = DateTime.Now
            });

            foreach (var midia in candidatas)
            {
                try
                {
                    var detalhesImdb = await _imdbScraper.SearchAndGetDetailsAsync(midia.TituloOriginal ?? midia.Titulo, midia.Ano);
                    if (detalhesImdb == null || string.IsNullOrWhiteSpace(detalhesImdb.ImagemUrl))
                    {
                        result.ItensSemPoster++;
                        continue;
                    }

                    midia.ImagemUrl = detalhesImdb.ImagemUrl;
                    await _midiaRepository.UpdateAsync(midia);
                    result.ItensAtualizados++;
                }
                catch (Exception ex)
                {
                    result.Erros++;
                    _logger.LogWarning(ex, "{Origem}: erro ao reprocessar poster IMDb da mídia {MidiaId}", origem, midia.Id);
                }
            }

            result.Sucesso = result.Erros == 0;
            result.Mensagem = $"Posters IMDb reprocessados. Processadas: {result.ItensProcessados}; atualizadas: {result.ItensAtualizados}; sem poster: {result.ItensSemPoster}; erros: {result.Erros}.";

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.IMDb,
                Tipo = result.Erros > 0 ? TipoLog.Aviso : TipoLog.Sucesso,
                Mensagem = $"{origem}: {result.Mensagem}",
                DataLog = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            result.Sucesso = false;
            result.Erros++;
            result.Mensagem = ex.Message;

            await _syncRepository.InsertLogAsync(new LogCaptura
            {
                Fonte = FonteCaptura.IMDb,
                Tipo = TipoLog.Erro,
                Mensagem = $"{origem}: erro no reprocessamento de posters IMDb",
                Detalhes = ex.Message,
                DataLog = DateTime.Now
            });

            _logger.LogError(ex, "{Origem}: erro no reprocessamento de posters IMDb", origem);
        }

        return result;
    }

    public async Task<BackfillSimulationResult> SimularBackfillTraducaoAsync(string origem, int maxGrupos = 0, decimal precoUsdPorMilhao = 0m, int franquiaGratisCaracteres = 0, int estimativaCaracteresMidiaSemTexto = 600, int estimativaCaracteresEpisodioSemTexto = 350, bool expandirSeriesCompleta = true, int maxTemporadasPorSerie = 8, int estimativaTemporadasPorSerieSemLista = 4, int estimativaEpisodiosPorTemporadaSemLista = 10)
    {
        var result = new BackfillSimulationResult
        {
            PrecoUsdPorMilhao = Math.Max(0m, precoUsdPorMilhao),
            FranquiaGratisCaracteres = Math.Max(0, franquiaGratisCaracteres),
            EstimativaCaracteresMidiaSemTexto = Math.Max(0, estimativaCaracteresMidiaSemTexto),
            EstimativaCaracteresEpisodioSemTexto = Math.Max(0, estimativaCaracteresEpisodioSemTexto),
            ExpandirSeriesCompleta = expandirSeriesCompleta,
            MaxTemporadasPorSerie = Math.Max(1, maxTemporadasPorSerie),
            EstimativaTemporadasPorSerieSemLista = Math.Max(0, estimativaTemporadasPorSerieSemLista),
            EstimativaEpisodiosPorTemporadaSemLista = Math.Max(0, estimativaEpisodiosPorTemporadaSemLista)
        };

        try
        {
            var capturas = await _sceneSourceScraper.ScrapeRecentAsync();
            result.CapturasEncontradas = capturas.Count;

            var grupos = AgruparCapturasPorPreferencia(capturas).ToList();
            result.GruposUnicosEncontrados = grupos.Count;

            var limite = maxGrupos > 0 ? Math.Min(maxGrupos, grupos.Count) : grupos.Count;
            var gruposSelecionados = grupos.Take(limite).ToList();
            result.GruposSimulados = gruposSelecionados.Count;

            var cacheMidia = new Dictionary<string, Midia?>(StringComparer.OrdinalIgnoreCase);
            var cacheEpisodios = new Dictionary<string, List<Episodio>>(StringComparer.OrdinalIgnoreCase);
            var imdbIdsContabilizados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var temporadasContabilizadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seriesExpandidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var grupo in gruposSelecionados)
            {
                var captura = grupo[0];
                var chaveMidia = $"{captura.TituloCapturado}|{captura.AnoCapturado}";

                if (!cacheMidia.TryGetValue(chaveMidia, out var midiaImdb))
                {
                    midiaImdb = await _imdbScraper.SearchAndGetDetailsAsync(captura.TituloCapturado, captura.AnoCapturado, traduzirDescricoes: false);
                    cacheMidia[chaveMidia] = midiaImdb;
                }

                if (midiaImdb == null || string.IsNullOrWhiteSpace(midiaImdb.ImdbId))
                {
                    result.GruposSemMatchImdb++;
                    continue;
                }

                result.GruposComMatchImdb++;

                if (imdbIdsContabilizados.Add(midiaImdb.ImdbId))
                {
                    var caracteresMidia = ContarCaracteres(midiaImdb.Descricao);
                    if (caracteresMidia == 0 && result.EstimativaCaracteresMidiaSemTexto > 0)
                    {
                        caracteresMidia = result.EstimativaCaracteresMidiaSemTexto;
                        result.MidiasSemDescricaoEstimadas++;
                        result.CaracteresEstimadosFallback += caracteresMidia;
                    }

                    result.CaracteresDescricoesMidia += caracteresMidia;

                    if (midiaImdb.Tipo == TipoMidia.Serie)
                    {
                        result.SeriesEstimadas++;
                    }
                    else
                    {
                        result.FilmesEstimados++;
                    }
                }

                if (midiaImdb.Tipo == TipoMidia.Serie && result.ExpandirSeriesCompleta && !string.IsNullOrWhiteSpace(midiaImdb.ImdbId) && seriesExpandidas.Add(midiaImdb.ImdbId))
                {
                    result.SeriesExpandidasCompletas++;
                    await SimularSerieCompletaAsync(midiaImdb.ImdbId, cacheEpisodios, temporadasContabilizadas, result);
                    continue;
                }

                if (midiaImdb.Tipo != TipoMidia.Serie || !captura.TemporadaCapturada.HasValue || !captura.EpisodioCapturado.HasValue)
                {
                    continue;
                }

                var chaveTemporada = $"{midiaImdb.ImdbId}|{captura.TemporadaCapturada.Value}";
                if (!temporadasContabilizadas.Add(chaveTemporada))
                {
                    continue;
                }

                if (!cacheEpisodios.TryGetValue(chaveTemporada, out var episodios))
                {
                    episodios = await _imdbScraper.GetEpisodiosAsync(midiaImdb.ImdbId, captura.TemporadaCapturada.Value, traduzirDescricoes: false);
                    cacheEpisodios[chaveTemporada] = episodios;
                }

                result.TemporadasEstimadas++;
                result.EpisodiosEstimados += episodios.Count;

                foreach (var episodio in episodios)
                {
                    var caracteresEpisodio = ContarCaracteres(episodio.Descricao);
                    if (caracteresEpisodio == 0 && result.EstimativaCaracteresEpisodioSemTexto > 0)
                    {
                        caracteresEpisodio = result.EstimativaCaracteresEpisodioSemTexto;
                        result.EpisodiosSemDescricaoEstimados++;
                        result.CaracteresEstimadosFallback += caracteresEpisodio;
                    }

                    result.CaracteresDescricoesEpisodio += caracteresEpisodio;
                }
            }

            result.CaracteresTotaisTraducao = result.CaracteresDescricoesMidia + result.CaracteresDescricoesEpisodio;
            result.CaracteresCobrados = Math.Max(0, result.CaracteresTotaisTraducao - result.FranquiaGratisCaracteres);
            result.CustoUsdBruto = CalcularCustoTraducao(result.CaracteresTotaisTraducao, result.PrecoUsdPorMilhao);
            result.CustoUsdExcedente = CalcularCustoTraducao(result.CaracteresCobrados, result.PrecoUsdPorMilhao);
            result.Sucesso = result.Erros == 0;
            result.Mensagem = $"Simulação concluída com base em {result.GruposSimulados} grupo(s) do SceneSource. Caracteres estimados: {result.CaracteresTotaisTraducao}; fallback: {result.CaracteresEstimadosFallback}; custo bruto: US$ {result.CustoUsdBruto:0.00}; custo após franquia: US$ {result.CustoUsdExcedente:0.00}.";
        }
        catch (Exception ex)
        {
            result.Sucesso = false;
            result.Erros++;
            result.Mensagem = ex.Message;
            _logger.LogError(ex, "{Origem}: erro na simulação de backfill de tradução", origem);
        }

        return result;
    }

    private async Task SimularSerieCompletaAsync(
        string imdbId,
        IDictionary<string, List<Episodio>> cacheEpisodios,
        ISet<string> temporadasContabilizadas,
        BackfillSimulationResult result)
    {
        var temporadasEncontradas = 0;
        var algumaTemporadaComLista = false;
        var vaziasConsecutivas = 0;

        for (var temporadaNumero = 1; temporadaNumero <= result.MaxTemporadasPorSerie; temporadaNumero++)
        {
            var chaveTemporada = $"{imdbId}|{temporadaNumero}";
            if (!temporadasContabilizadas.Add(chaveTemporada))
            {
                continue;
            }

            if (!cacheEpisodios.TryGetValue(chaveTemporada, out var episodios))
            {
                episodios = await _imdbScraper.GetEpisodiosAsync(imdbId, temporadaNumero, traduzirDescricoes: false);
                cacheEpisodios[chaveTemporada] = episodios;
            }

            if (episodios.Count == 0)
            {
                vaziasConsecutivas++;
                if (vaziasConsecutivas >= 2)
                {
                    break;
                }

                continue;
            }

            vaziasConsecutivas = 0;
            algumaTemporadaComLista = true;
            temporadasEncontradas++;
            result.TemporadasEstimadas++;
            result.EpisodiosEstimados += episodios.Count;

            foreach (var episodio in episodios)
            {
                var caracteresEpisodio = ContarCaracteres(episodio.Descricao);
                if (caracteresEpisodio == 0 && result.EstimativaCaracteresEpisodioSemTexto > 0)
                {
                    caracteresEpisodio = result.EstimativaCaracteresEpisodioSemTexto;
                    result.EpisodiosSemDescricaoEstimados++;
                    result.CaracteresEstimadosFallback += caracteresEpisodio;
                }

                result.CaracteresDescricoesEpisodio += caracteresEpisodio;
            }
        }

        if (algumaTemporadaComLista || result.EstimativaTemporadasPorSerieSemLista <= 0 || result.EstimativaEpisodiosPorTemporadaSemLista <= 0)
        {
            return;
        }

        var temporadasFallback = result.EstimativaTemporadasPorSerieSemLista;
        var episodiosFallback = result.EstimativaEpisodiosPorTemporadaSemLista;

        result.TemporadasEstimadas += temporadasFallback;
        result.EpisodiosEstimados += temporadasFallback * episodiosFallback;

        var caracteresFallback = temporadasFallback * episodiosFallback * result.EstimativaCaracteresEpisodioSemTexto;
        result.EpisodiosSemDescricaoEstimados += temporadasFallback * episodiosFallback;
        result.CaracteresEstimadosFallback += caracteresFallback;
        result.CaracteresDescricoesEpisodio += caracteresFallback;
    }

    private static bool DeveAutoConfirmar(AssociacaoSceneSource captura, decimal confiancaMinima)
    {
        if (captura.Confianca < confiancaMinima || !captura.MidiaId.HasValue)
        {
            return false;
        }

        var requerEpisodio = captura.TemporadaCapturada.HasValue || captura.EpisodioCapturado.HasValue;
        if (requerEpisodio && !captura.EpisodioId.HasValue)
        {
            return false;
        }

        return true;
    }

    private static IEnumerable<IReadOnlyList<AssociacaoSceneSource>> AgruparCapturasPorPreferencia(IEnumerable<AssociacaoSceneSource> capturas)
    {
        return capturas
            .GroupBy(captura => string.IsNullOrWhiteSpace(captura.ChaveAgrupamento)
                ? $"{captura.TituloCapturado}|{captura.AnoCapturado}|{captura.TemporadaCapturada}|{captura.EpisodioCapturado}"
                : captura.ChaveAgrupamento)
            .Select(grupo => (IReadOnlyList<AssociacaoSceneSource>)grupo
                .OrderByDescending(captura => captura.ReleaseScore)
                .ThenByDescending(captura => captura.DataCaptura)
                .ToList())
            .OrderByDescending(grupo => grupo.Max(captura => captura.ReleaseScore))
            .ThenByDescending(grupo => grupo.Max(captura => captura.DataCaptura));
    }

    private static string MontarDiagnosticoCaptura(AssociacaoSceneSource captura)
    {
        return DiagnosticoParserMapper.Serialize(captura);
    }

    private async Task EnriquecerAssociacaoAsync(AssociacaoSceneSource captura)
    {
        var midiaImdb = await _imdbScraper.SearchAndGetDetailsAsync(captura.TituloCapturado, captura.AnoCapturado);
        if (midiaImdb == null || string.IsNullOrWhiteSpace(midiaImdb.ImdbId))
        {
            var midiaLocal = await BuscarMidiaLocalAsync(captura);
            if (midiaLocal == null || string.IsNullOrWhiteSpace(midiaLocal.ImdbId))
            {
                captura.Confianca = 0;
                return;
            }

            captura.ImdbIdCapturado = midiaLocal.ImdbId;
            captura.MidiaId = midiaLocal.Id;
            captura.Confianca = CalcularConfianca(captura, midiaLocal);

            if (midiaLocal.Tipo == TipoMidia.Serie && captura.TemporadaCapturada.HasValue && captura.EpisodioCapturado.HasValue)
            {
                var temporadaLocal = await _temporadaRepository.GetByMidiaAndNumeroAsync(midiaLocal.Id, captura.TemporadaCapturada.Value);
                if (temporadaLocal != null)
                {
                    captura.EpisodioId = (await _episodioRepository.GetByTemporadaAndNumeroAsync(temporadaLocal.Id, captura.EpisodioCapturado.Value))?.Id;
                    if (captura.EpisodioId.HasValue)
                    {
                        captura.Confianca = Math.Min(100m, captura.Confianca + BonusEpisodioAssociado);
                    }
                }
            }

            return;
        }

        captura.ImdbIdCapturado = midiaImdb.ImdbId;

        var midiaPersistida = await _midiaRepository.GetByImdbIdAsync(midiaImdb.ImdbId);
        if (midiaPersistida == null)
        {
            midiaImdb.DataCriacao = DateTime.Now;
            midiaPersistida = midiaImdb;
            midiaPersistida.Id = await _midiaRepository.InsertAsync(midiaPersistida);
        }
        else
        {
            MergeMidia(midiaPersistida, midiaImdb);
            await _midiaRepository.UpdateAsync(midiaPersistida);
        }

        captura.MidiaId = midiaPersistida.Id;
        captura.Confianca = CalcularConfianca(captura, midiaPersistida);

        if (midiaPersistida.Tipo == TipoMidia.Serie && captura.TemporadaCapturada.HasValue && captura.EpisodioCapturado.HasValue)
        {
            captura.EpisodioId = await GarantirEpisodioAssociadoAsync(midiaPersistida, captura);
            if (captura.EpisodioId.HasValue)
            {
                captura.Confianca = Math.Min(100m, captura.Confianca + BonusEpisodioAssociado);

                if (await TituloEpisodioConfereAsync(captura))
                {
                    captura.Confianca = Math.Min(100m, captura.Confianca + BonusTituloEpisodioExato);
                }
            }
        }
    }

    private async Task<int?> GarantirEpisodioAssociadoAsync(Midia midia, AssociacaoSceneSource captura)
    {
        if (string.IsNullOrWhiteSpace(midia.ImdbId) || !captura.TemporadaCapturada.HasValue || !captura.EpisodioCapturado.HasValue)
        {
            return null;
        }

        var temporada = await _temporadaRepository.GetByMidiaAndNumeroAsync(midia.Id, captura.TemporadaCapturada.Value);
        var episodiosImdb = await _imdbScraper.GetEpisodiosAsync(midia.ImdbId, captura.TemporadaCapturada.Value);

        if (temporada == null)
        {
            temporada = new Temporada
            {
                MidiaId = midia.Id,
                Numero = captura.TemporadaCapturada.Value,
                Titulo = null,
                Ano = midia.Ano,
                TotalEpisodios = episodiosImdb.Count > 0 ? episodiosImdb.Count : captura.EpisodioCapturado.Value
            };

            temporada.Id = await _temporadaRepository.InsertAsync(temporada);
        }
        else
        {
            var totalEpisodios = episodiosImdb.Count > 0 ? episodiosImdb.Count : temporada.TotalEpisodios;
            if (temporada.TotalEpisodios != totalEpisodios && totalEpisodios > 0)
            {
                temporada.TotalEpisodios = totalEpisodios;
                await _temporadaRepository.UpdateAsync(temporada);
            }
        }

        foreach (var episodioImdb in episodiosImdb)
        {
            episodioImdb.TemporadaId = temporada.Id;

            var episodioExistente = await _episodioRepository.GetByTemporadaAndNumeroAsync(temporada.Id, episodioImdb.Numero);
            if (episodioExistente == null)
            {
                await _episodioRepository.InsertAsync(episodioImdb);
            }
            else
            {
                MergeEpisodio(episodioExistente, episodioImdb);
                await _episodioRepository.UpdateAsync(episodioExistente);
            }
        }

        Episodio? episodioAssociado = await _episodioRepository.GetByTemporadaAndNumeroAsync(temporada.Id, captura.EpisodioCapturado.Value);

        if (episodioAssociado == null && !string.IsNullOrWhiteSpace(captura.TituloEpisodioCapturado))
        {
            episodioAssociado = episodiosImdb
                .FirstOrDefault(episodio => TituloNormalizado(episodio.Titulo) == TituloNormalizado(captura.TituloEpisodioCapturado));

            if (episodioAssociado != null)
            {
                episodioAssociado = await _episodioRepository.GetByTemporadaAndNumeroAsync(temporada.Id, episodioAssociado.Numero);
            }
        }

        return episodioAssociado?.Id;
    }

    private async Task<bool> TituloEpisodioConfereAsync(AssociacaoSceneSource captura)
    {
        if (!captura.EpisodioId.HasValue || string.IsNullOrWhiteSpace(captura.TituloEpisodioCapturado))
        {
            return false;
        }

        var episodio = await _episodioRepository.GetByIdAsync(captura.EpisodioId.Value);
        return episodio != null
            && TituloNormalizado(episodio.Titulo) == TituloNormalizado(captura.TituloEpisodioCapturado);
    }

    private static decimal CalcularConfianca(AssociacaoSceneSource captura, Midia midia)
    {
        var confianca = ConfiancaBaseMidia;

        if (TituloNormalizado(captura.TituloCapturado) == TituloNormalizado(midia.Titulo) ||
            (!string.IsNullOrWhiteSpace(midia.TituloOriginal) && TituloNormalizado(captura.TituloCapturado) == TituloNormalizado(midia.TituloOriginal)))
        {
            confianca += BonusTituloExato;
        }

        if (captura.AnoCapturado.HasValue && midia.Ano.HasValue && captura.AnoCapturado == midia.Ano)
        {
            confianca += BonusAnoCorreto;
        }

        return Math.Min(100m, confianca);
    }

    private static void MergeMidia(Midia destino, Midia origem)
    {
        destino.Titulo = EscolherTexto(destino.Titulo, origem.Titulo) ?? destino.Titulo;
        destino.TituloOriginal = EscolherTexto(destino.TituloOriginal, origem.TituloOriginal);
        destino.Tipo = origem.Tipo;
        destino.Ano = destino.Ano ?? origem.Ano;
        destino.Descricao = EscolherTexto(destino.Descricao, origem.Descricao);
        destino.ImagemUrl = EscolherTexto(destino.ImagemUrl, origem.ImagemUrl);
        destino.ImdbId = EscolherTexto(destino.ImdbId, origem.ImdbId);
        destino.ImdbRating = destino.ImdbRating ?? origem.ImdbRating;
        destino.ImdbVotes = destino.ImdbVotes ?? origem.ImdbVotes;
        destino.Generos = EscolherTexto(destino.Generos, origem.Generos);
        destino.Duracao = destino.Duracao ?? origem.Duracao;
        destino.Diretor = EscolherTexto(destino.Diretor, origem.Diretor);
        destino.Elenco = EscolherTexto(destino.Elenco, origem.Elenco);
    }

    private static void MergeEpisodio(Episodio destino, Episodio origem)
    {
        destino.Titulo = EscolherTexto(destino.Titulo, origem.Titulo);
        destino.Descricao = EscolherTexto(destino.Descricao, origem.Descricao);
        destino.Duracao = destino.Duracao ?? origem.Duracao;
        destino.DataExibicao = destino.DataExibicao ?? origem.DataExibicao;
        destino.ImdbRating = destino.ImdbRating ?? origem.ImdbRating;
    }

    private static string? EscolherTexto(string? atual, string? novoValor)
    {
        return string.IsNullOrWhiteSpace(atual) ? novoValor : atual;
    }

    private static int ContarCaracteres(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? 0 : valor.Length;
    }

    private static decimal CalcularCustoTraducao(long caracteres, decimal precoUsdPorMilhao)
    {
        if (caracteres <= 0 || precoUsdPorMilhao <= 0)
        {
            return 0;
        }

        return Math.Round((caracteres / 1_000_000m) * precoUsdPorMilhao, 4, MidpointRounding.AwayFromZero);
    }

    private async Task<Midia?> BuscarMidiaLocalAsync(AssociacaoSceneSource captura)
    {
        var candidatas = await _midiaRepository.FindByTitleAsync(captura.TituloCapturado, captura.AnoCapturado, 10);
        return candidatas
            .OrderByDescending(midia => CalcularConfianca(captura, midia))
            .ThenByDescending(midia => midia.ImdbRating)
            .FirstOrDefault();
    }

    private static string TituloNormalizado(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var normalizado = valor.ToLowerInvariant();
        normalizado = Regex.Replace(normalizado, "[^a-z0-9]+", " ");
        normalizado = Regex.Replace(normalizado, "\\s+", " ").Trim();
        return normalizado;
    }
}
