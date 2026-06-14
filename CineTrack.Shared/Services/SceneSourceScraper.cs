using CineTrack.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.RegularExpressions;

namespace CineTrack.Shared.Services;

public class SceneSourceScraperOptions
{
    public string[] Categories { get; set; } = ["tv", "films/bluray", "tv/miniseries"];
    public int MaxPagesPerCategory { get; set; } = 3;
    public int MaxPostsPerPage { get; set; } = 20;
    public bool StorePostUrlOnly { get; set; } = true;
}

public interface ISceneSourceScraper
{
    Task<List<AssociacaoSceneSource>> ScrapeRecentAsync();
}

public class SceneSourceScraper : ISceneSourceScraper
{
    private readonly HttpClient _httpClient;
    private readonly SceneSourceScraperOptions _options;
    private const string BaseUrl = "https://www.scnsrc.me";
    private static readonly string[] ReleaseMarkers =
    [
        "720p", "1080p", "2160p", "4K", "BluRay", "WEB-DL", "WEBRip", "HDRip", "BRRip", "HDTV", "PDTV",
        "DSNP", "AMZN", "NF", "HULU", "ATVP", "MAX", "x264", "x265", "HEVC", "AV1", "AAC", "AC3", "DDP", "DTS"
    ];

    private sealed record ScrapedCandidate(AssociacaoSceneSource Associacao, string SemanticKey, int ReleaseScore);
    private sealed record ReleaseMetadata(string Categoria, string? Qualidade, string? Fonte, string? Codec, string? Provider, string? Grupo);

    public SceneSourceScraper(HttpClient httpClient, IOptions<SceneSourceScraperOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async Task<List<AssociacaoSceneSource>> ScrapeRecentAsync()
    {
        var candidatos = new List<ScrapedCandidate>();

        foreach (var category in GetCategoryPaths())
        {
            candidatos.AddRange(await ScrapeCategoryAsync(category));
        }

        var resultados = candidatos
            .Where(x => !string.IsNullOrWhiteSpace(x.Associacao.TituloCapturado))
            .GroupBy(
                x => x.Associacao.LinkSceneSource
                    ?? x.Associacao.TituloBrutoCapturado
                    ?? $"{x.SemanticKey}|{x.ReleaseScore}",
                StringComparer.OrdinalIgnoreCase)
            .Select(x => x
                .OrderByDescending(candidate => candidate.ReleaseScore)
                .ThenByDescending(candidate => candidate.Associacao.LinkSceneSource?.Length ?? 0)
                .First()
                .Associacao)
            .ToList();

        return resultados;
    }

    private IEnumerable<string> GetCategoryPaths()
    {
        var categories = _options.Categories is { Length: > 0 }
            ? _options.Categories
            : ["tv", "films/bluray", "tv/miniseries"];

        return categories
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim().Trim('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(category => $"/category/{category}/");
    }

    private async Task<List<ScrapedCandidate>> ScrapeCategoryAsync(string categoryPath)
    {
        var resultados = new List<ScrapedCandidate>();
        var paginasVisitadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var paginaAtual = new Uri(new Uri(BaseUrl), categoryPath).ToString();
        var paginasProcessadas = 0;
        var limitePaginas = Math.Max(1, _options.MaxPagesPerCategory);
        var limitePostsPorPagina = Math.Max(1, _options.MaxPostsPerPage);

        while (!string.IsNullOrWhiteSpace(paginaAtual)
            && paginasProcessadas < limitePaginas
            && paginasVisitadas.Add(paginaAtual))
        {
            using var response = await _httpClient.GetAsync(paginaAtual);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpRequestException($"SceneSource retornou 404 para {paginaAtual}.", null, response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"SceneSource retornou status {(int)response.StatusCode} ({response.ReasonPhrase}) para {paginaAtual}.",
                    null,
                    response.StatusCode);
            }

            var html = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(html))
            {
                break;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var posts = doc.DocumentNode.SelectNodes("//article[contains(@class, 'post')] | //div[contains(@class, 'post') and .//h2/a]");
            if (posts == null)
            {
                break;
            }

            foreach (var post in posts.Take(limitePostsPorPagina))
            {
                var titleNode = post.SelectSingleNode(".//h2/a | .//header//h2/a");
                if (titleNode == null)
                {
                    continue;
                }

                var titulo = HtmlEntity.DeEntitize(titleNode.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    continue;
                }

                var link = titleNode.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrWhiteSpace(link) && Uri.TryCreate(new Uri(BaseUrl), link, out var absoluteLink))
                {
                    link = absoluteLink.ToString();
                }

                var parsed = ParseTitle(titulo);
                var metadata = ExtractReleaseMetadata(titulo);
                var semanticKey = BuildSemanticKey(titulo, associacao: null, parsed.Titulo, parsed.ModoParsing, parsed.Ano, parsed.Temporada, parsed.Episodio);
                var releaseScore = CalculateReleaseScore(titulo);
                var associacao = new AssociacaoSceneSource
                {
                    TituloCapturado = parsed.Titulo,
                    TituloBrutoCapturado = titulo,
                    TituloEpisodioCapturado = parsed.TituloEpisodio,
                    ModoParsingCapturado = parsed.ModoParsing,
                    AnoCapturado = parsed.Ano,
                    TemporadaCapturada = parsed.Temporada,
                    EpisodioCapturado = parsed.Episodio,
                    LinkSceneSource = string.IsNullOrWhiteSpace(link) ? null : link,
                    CategoriaCapturada = metadata.Categoria,
                    QualidadeCapturada = metadata.Qualidade,
                    FonteReleaseCapturada = metadata.Fonte,
                    CodecCapturado = metadata.Codec,
                    ProviderCapturado = metadata.Provider,
                    GrupoReleaseCapturado = metadata.Grupo,
                    ChaveAgrupamento = semanticKey,
                    ReleaseScore = releaseScore,
                    Confianca = 0,
                    Status = StatusAssociacao.Pendente,
                    DataCaptura = DateTime.Now
                };

                resultados.Add(new ScrapedCandidate(
                    associacao,
                    semanticKey,
                    releaseScore));
            }

            paginasProcessadas++;
            paginaAtual = ResolveNextPageUrl(doc, paginaAtual);
        }

        return resultados;
    }

    private static string BuildSemanticKey(
        string rawTitle,
        AssociacaoSceneSource? associacao = null,
        string? tituloCapturado = null,
        string? modoParsing = null,
        int? anoCapturado = null,
        int? temporadaCapturada = null,
        int? episodioCapturado = null)
    {
        var titulo = NormalizeSemanticToken(associacao?.TituloCapturado ?? tituloCapturado);
        var dateToken = ExtractDateToken(rawTitle);
        var temporada = associacao?.TemporadaCapturada ?? temporadaCapturada;
        var episodio = associacao?.EpisodioCapturado ?? episodioCapturado;
        var modo = associacao?.ModoParsingCapturado ?? modoParsing;
        var ano = associacao?.AnoCapturado ?? anoCapturado;

        if (temporada.HasValue && episodio.HasValue)
        {
            return $"episode|{titulo}|{temporada.Value}|{episodio.Value}";
        }

        if (string.Equals(modo, "Data", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(dateToken))
        {
            return $"date|{titulo}|{dateToken}";
        }

        if (temporada.HasValue)
        {
            return $"season|{titulo}|{temporada.Value}";
        }

        if (ano.HasValue)
        {
            return $"title-year|{titulo}|{ano.Value}";
        }

        return $"raw|{NormalizeSemanticToken(rawTitle)}";
    }

    private static ReleaseMetadata ExtractReleaseMetadata(string rawTitle)
    {
        var tokens = new List<string>();

        var resolution = Regex.Match(rawTitle, @"\b(2160p|4K|1080p|720p)\b", RegexOptions.IgnoreCase);
        string? qualidade = null;
        if (resolution.Success)
        {
            qualidade = resolution.Value.ToUpperInvariant();
            tokens.Add(qualidade);
        }

        var source = Regex.Match(rawTitle, @"\b(BluRay|WEB-DL|WEBRip|HDTV|PDTV|BDRip|HDRip|BRRip)\b", RegexOptions.IgnoreCase);
        string? fonte = null;
        if (source.Success)
        {
            fonte = source.Value;
            tokens.Add(fonte);
        }

        var provider = Regex.Match(rawTitle, @"\b(DSNP|AMZN|NF|HULU|ATVP|MAX)\b", RegexOptions.IgnoreCase);
        string? providerValue = null;
        if (provider.Success)
        {
            providerValue = provider.Value.ToUpperInvariant();
            tokens.Add(providerValue);
        }

        var codec = Regex.Match(rawTitle, @"\b(x265|x264|H264|HEVC|AV1)\b", RegexOptions.IgnoreCase);
        string? codecValue = null;
        if (codec.Success)
        {
            codecValue = codec.Value.ToUpperInvariant();
            tokens.Add(codecValue);
        }

        var group = Regex.Match(rawTitle, @"-([A-Za-z0-9]+)\s*$");
        var grupo = group.Success ? group.Groups[1].Value : null;

        var categoria = tokens.Count > 0
            ? string.Join(" • ", tokens.Distinct(StringComparer.OrdinalIgnoreCase))
            : "Release padrão";

        return new ReleaseMetadata(categoria, qualidade, fonte, codecValue, providerValue, grupo);
    }

    private static string NormalizeSemanticToken(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var normalizado = valor.ToLowerInvariant();
        normalizado = Regex.Replace(normalizado, @"[^a-z0-9]+", " ");
        return Regex.Replace(normalizado, @"\s+", " ").Trim();
    }

    private static string? ExtractDateToken(string rawTitle)
    {
        var match = Regex.Match(rawTitle, @"\b((19|20)\d{2})[\.\-_ ](0[1-9]|1[0-2])[\.\-_ ](0[1-9]|[12]\d|3[01])\b", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        return $"{match.Groups[1].Value}-{match.Groups[3].Value}-{match.Groups[4].Value}";
    }

    private static int CalculateReleaseScore(string rawTitle)
    {
        var score = 0;

        score += Regex.IsMatch(rawTitle, @"\b1080p\b", RegexOptions.IgnoreCase) ? 50 : 0;
        score += Regex.IsMatch(rawTitle, @"\b(2160p|4K)\b", RegexOptions.IgnoreCase) ? 35 : 0;
        score += Regex.IsMatch(rawTitle, @"\b720p\b", RegexOptions.IgnoreCase) ? 20 : 0;

        score += Regex.IsMatch(rawTitle, @"\bBluRay\b", RegexOptions.IgnoreCase) ? 15 : 0;
        score += Regex.IsMatch(rawTitle, @"\bWEB-DL\b", RegexOptions.IgnoreCase) ? 12 : 0;
        score += Regex.IsMatch(rawTitle, @"\bWEBRip\b", RegexOptions.IgnoreCase) ? 10 : 0;
        score += Regex.IsMatch(rawTitle, @"\bHDTV\b", RegexOptions.IgnoreCase) ? 8 : 0;
        score += Regex.IsMatch(rawTitle, @"\b(BDRip|HDRip|BRRip)\b", RegexOptions.IgnoreCase) ? 6 : 0;
        score += Regex.IsMatch(rawTitle, @"\bPDTV\b", RegexOptions.IgnoreCase) ? 4 : 0;

        score += Regex.IsMatch(rawTitle, @"\b(HEVC|AV1|x265)\b", RegexOptions.IgnoreCase) ? 4 : 0;
        score += Regex.IsMatch(rawTitle, @"\b(x264|H264)\b", RegexOptions.IgnoreCase) ? 2 : 0;
        score += Regex.IsMatch(rawTitle, @"\b(REPACK|PROPER|REAL)\b", RegexOptions.IgnoreCase) ? 1 : 0;

        return score;
    }

    private static string? ResolveNextPageUrl(HtmlDocument doc, string paginaAtual)
    {
        var nextPageNode = doc.DocumentNode.SelectSingleNode(
            "//a[contains(@class, 'next') or contains(@rel, 'next') or normalize-space()='Next Page' or contains(translate(normalize-space(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'next page')]");

        if (nextPageNode == null)
        {
            return null;
        }

        var href = nextPageNode.GetAttributeValue("href", string.Empty);
        if (string.IsNullOrWhiteSpace(href))
        {
            return null;
        }

        return Uri.TryCreate(new Uri(paginaAtual), href, out var absoluteNext)
            ? absoluteNext.ToString()
            : null;
    }

    private (string Titulo, string? TituloEpisodio, string ModoParsing, int? Ano, int? Temporada, int? Episodio) ParseTitle(string rawTitle)
    {
        var titulo = rawTitle;
        string? tituloEpisodio = null;
        var modoParsing = "Filme";
        int? ano = null;
        int? temporada = null;
        int? episodio = null;
        var possuiEpisodioPorData = false;

        // Extrair ano (4 dígitos entre 1900-2099)
        var anoMatch = Regex.Match(rawTitle, @"\b(19|20)\d{2}\b");
        if (anoMatch.Success)
        {
            ano = int.Parse(anoMatch.Value);
        }

        var dateEpisodeMatch = Regex.Match(rawTitle, @"\b(19|20)\d{2}[\.\-_ ](0[1-9]|1[0-2])[\.\-_ ](0[1-9]|[12]\d|3[01])\b", RegexOptions.IgnoreCase);
        if (dateEpisodeMatch.Success)
        {
            possuiEpisodioPorData = true;
            modoParsing = "Data";
            ano = int.Parse(dateEpisodeMatch.Value[..4]);
        }

        // Extrair temporada/episódio (S01E01)
        var seMatch = Regex.Match(rawTitle, @"S(\d{1,2})E(\d{1,2})", RegexOptions.IgnoreCase);
        if (seMatch.Success)
        {
            temporada = int.Parse(seMatch.Groups[1].Value);
            episodio = int.Parse(seMatch.Groups[2].Value);
            tituloEpisodio = ExtrairTituloEpisodio(rawTitle, seMatch.Index + seMatch.Length);
            modoParsing = "SxxEyy";
        }
        else
        {
            var seasonMatch = Regex.Match(rawTitle, @"\bS(\d{1,2})\b", RegexOptions.IgnoreCase);
            if (seasonMatch.Success)
            {
                temporada = int.Parse(seasonMatch.Groups[1].Value);
                modoParsing = "Temporada";
            }
        }

        // Limpar título
        if (possuiEpisodioPorData && dateEpisodeMatch.Index > 0)
        {
            titulo = rawTitle[..dateEpisodeMatch.Index];
        }

        titulo = Regex.Replace(titulo, @"S\d{1,2}E\d{1,2}", "", RegexOptions.IgnoreCase);
        titulo = Regex.Replace(titulo, @"\bS\d{1,2}\b", "", RegexOptions.IgnoreCase);
        titulo = Regex.Replace(titulo, @"\b\d{4}[\.\-_ ]\d{2}[\.\-_ ]\d{2}\b", "", RegexOptions.IgnoreCase);
        if (!possuiEpisodioPorData)
        {
            titulo = Regex.Replace(titulo, @"\b(19|20)\d{2}\b", "");
        }

        titulo = Regex.Replace(titulo, @"\b(REPACK|PROPER|INTERNAL|SUBBED|READNFO|REAL)\b", "", RegexOptions.IgnoreCase);
        titulo = Regex.Replace(titulo, @"(720p|1080p|2160p|4K|BluRay|WEB-DL|WEBRip|HDRip|BRRip|HDTV|PDTV|DSNP|AMZN|NF|HULU|ATVP|MAX|x264|x265|HEVC|AV1|AAC|AC3|DDP5?\.?(1)?|DTS).*", "", RegexOptions.IgnoreCase);
        titulo = Regex.Replace(titulo, @"[\.\-_]", " ");
        titulo = Regex.Replace(titulo, @"\s+", " ").Trim();

        return (titulo, tituloEpisodio, modoParsing, ano, temporada, episodio);
    }

    private static string? ExtrairTituloEpisodio(string rawTitle, int startIndex)
    {
        if (startIndex >= rawTitle.Length)
        {
            return null;
        }

        var trecho = rawTitle[startIndex..].Trim();
        if (string.IsNullOrWhiteSpace(trecho))
        {
            return null;
        }

        trecho = Regex.Replace(trecho, @"^(REPACK|PROPER|INTERNAL|SUBBED|READNFO|REAL)\b", string.Empty, RegexOptions.IgnoreCase).Trim();
        if (string.IsNullOrWhiteSpace(trecho))
        {
            return null;
        }

        var markerIndex = ReleaseMarkers
            .Select(marker => Regex.Match(trecho, $@"\b{Regex.Escape(marker)}\b", RegexOptions.IgnoreCase))
            .Where(match => match.Success)
            .Select(match => match.Index)
            .DefaultIfEmpty(-1)
            .Min();

        if (markerIndex >= 0)
        {
            trecho = trecho[..markerIndex];
        }

        trecho = Regex.Replace(trecho, @"\b(REPACK|PROPER|INTERNAL|SUBBED|READNFO|REAL)\b", string.Empty, RegexOptions.IgnoreCase);
        trecho = Regex.Replace(trecho, @"[\.\-_]", " ");
        trecho = Regex.Replace(trecho, @"\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(trecho) ? null : trecho;
    }
}
