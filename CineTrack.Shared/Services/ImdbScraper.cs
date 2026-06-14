using CineTrack.Shared.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace CineTrack.Shared.Services;

public sealed class ImdbScraperOptions
{
    public bool FallbackWebEnabled { get; set; }
}

public interface IImdbScraper
{
    Task<Midia?> SearchAndGetDetailsAsync(string titulo, int? ano = null, bool traduzirDescricoes = true);
    Task<Midia?> GetByIdAsync(string imdbId, bool traduzirDescricoes = true);
    Task<List<Episodio>> GetEpisodiosAsync(string imdbId, int temporada, bool traduzirDescricoes = true);
}

public class ImdbScraper : IImdbScraper
{
    private readonly HttpClient _httpClient;
    private readonly ITextTranslationService _textTranslationService;
    private readonly ImdbScraperOptions _options;
    private const string BaseUrl = "https://www.imdb.com";
    private const string SuggestionBaseUrl = "https://v2.sg.media-imdb.com/suggestion";

    public ImdbScraper(HttpClient httpClient, ITextTranslationService textTranslationService, IOptions<ImdbScraperOptions> options)
    {
        _httpClient = httpClient;
        _textTranslationService = textTranslationService;
        _options = options.Value;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
    }

    public async Task<Midia?> SearchAndGetDetailsAsync(string titulo, int? ano = null, bool traduzirDescricoes = true)
    {
        if (!_options.FallbackWebEnabled)
        {
            return null;
        }

        try
        {
            var suggestion = await SearchSuggestionAsync(titulo, ano);
            if (suggestion == null)
            {
                var searchTerm = HttpUtility.UrlEncode(titulo);
                var searchUrl = $"{BaseUrl}/find/?q={searchTerm}&s=tt&ttype=ft,tv";

                var html = await _httpClient.GetStringAsync(searchUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var firstResult = doc.DocumentNode.SelectSingleNode("//a[contains(@href, '/title/tt')]");
                if (firstResult == null) return null;

                var href = firstResult.GetAttributeValue("href", "");
                var imdbIdMatch = Regex.Match(href, @"tt\d+");
                if (!imdbIdMatch.Success) return null;

                return await GetByIdAsync(imdbIdMatch.Value, traduzirDescricoes);
            }

            var detalhes = await GetByIdAsync(suggestion.Id, traduzirDescricoes);
            if (detalhes != null)
            {
                detalhes.ImagemUrl ??= suggestion.ImageUrl;
                detalhes.Ano ??= suggestion.Year;
                detalhes.Titulo = string.IsNullOrWhiteSpace(detalhes.Titulo) || detalhes.Titulo == "Sem título"
                    ? suggestion.Title
                    : detalhes.Titulo;
                detalhes.Tipo = MapSuggestionType(suggestion.Qid, detalhes.Tipo);
                return detalhes;
            }

            return new Midia
            {
                ImdbId = suggestion.Id,
                Titulo = suggestion.Title,
                Ano = suggestion.Year,
                ImagemUrl = suggestion.ImageUrl,
                Tipo = MapSuggestionType(suggestion.Qid, TipoMidia.Filme)
            };
        }
        catch (TextTranslationException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    public async Task<Midia?> GetByIdAsync(string imdbId, bool traduzirDescricoes = true)
    {
        if (!_options.FallbackWebEnabled)
        {
            return null;
        }

        try
        {
            var url = $"{BaseUrl}/title/{imdbId}/";
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var midia = new Midia { ImdbId = imdbId };

            // Título
            var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@data-testid='hero__pageTitle']");
            midia.Titulo = titleNode?.InnerText.Trim() ?? "Sem título";

            // Título Original
            var originalTitleNode = doc.DocumentNode.SelectSingleNode("//div[@data-testid='hero__pageTitle']//following-sibling::div");
            if (originalTitleNode != null)
            {
                var text = originalTitleNode.InnerText;
                if (text.Contains("Original title:"))
                {
                    midia.TituloOriginal = text.Replace("Original title:", "").Trim();
                }
            }

            // Ano
            var yearNode = doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'releaseinfo')]");
            if (yearNode != null && int.TryParse(Regex.Match(yearNode.InnerText, @"\d{4}").Value, out var ano))
            {
                midia.Ano = ano;
            }

            // Rating
            var ratingNode = doc.DocumentNode.SelectSingleNode("//span[@class='sc-eb51e184-1 ljxVSS']");
            if (ratingNode != null && decimal.TryParse(ratingNode.InnerText.Replace(",", "."), out var rating))
            {
                midia.ImdbRating = rating;
            }

            // Tipo (série se tem episódios)
            var episodesLink = doc.DocumentNode.SelectSingleNode("//a[contains(@href, 'episodes')]");
            midia.Tipo = episodesLink != null ? TipoMidia.Serie : TipoMidia.Filme;

            // Descrição
            var descNode = doc.DocumentNode.SelectSingleNode("//span[@data-testid='plot-l']");
            var descricaoOriginal = descNode?.InnerText.Trim();
            midia.Descricao = traduzirDescricoes
                ? await _textTranslationService.TranslateAsync(descricaoOriginal)
                : descricaoOriginal;

            // Imagem
            var imgNode = doc.DocumentNode.SelectSingleNode("//img[@data-testid='hero-media__poster']")
                ?? doc.DocumentNode.SelectSingleNode("//div[@data-testid='hero-media__poster']//img")
                ?? doc.DocumentNode.SelectSingleNode("//img[contains(@class, 'ipc-image') and contains(@src, 'media-amazon')]")
                ?? doc.DocumentNode.SelectSingleNode("//img[contains(@class, 'ipc-image')]");

            midia.ImagemUrl = imgNode?.GetAttributeValue("src", null)
                ?? imgNode?.GetAttributeValue("data-src", null);

            // Gêneros
            var genreNodes = doc.DocumentNode.SelectNodes("//span[@class='ipc-chip__text']");
            if (genreNodes != null)
            {
                midia.Generos = string.Join(", ", genreNodes.Take(5).Select(n => n.InnerText.Trim()));
            }

            return midia;
        }
        catch (TextTranslationException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private async Task<ImdbSuggestionItem?> SearchSuggestionAsync(string titulo, int? ano)
    {
        var query = titulo.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var firstChar = char.ToLowerInvariant(query[0]);
        if (!char.IsLetterOrDigit(firstChar))
        {
            firstChar = '_';
        }

        var encoded = Uri.EscapeDataString(query.ToLowerInvariant());
        var url = $"{SuggestionBaseUrl}/{firstChar}/{encoded}.json";
        var json = await _httpClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("d", out var results) || results.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var candidates = new List<ImdbSuggestionItem>();
        foreach (var item in results.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idProp))
            {
                continue;
            }

            var id = idProp.GetString();
            var title = item.TryGetProperty("l", out var titleProp) ? titleProp.GetString() : null;
            var qid = item.TryGetProperty("qid", out var qidProp) ? qidProp.GetString() : null;
            var yearValue = item.TryGetProperty("y", out var yearProp) && yearProp.TryGetInt32(out var parsedYear)
                ? parsedYear
                : (int?)null;
            string? imageUrl = null;

            if (item.TryGetProperty("i", out var imageProp)
                && imageProp.ValueKind == JsonValueKind.Object
                && imageProp.TryGetProperty("imageUrl", out var imageUrlProp))
            {
                imageUrl = imageUrlProp.GetString();
            }

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            candidates.Add(new ImdbSuggestionItem(id, title, qid, yearValue, imageUrl));
        }

        return candidates
            .OrderByDescending(item => MatchSuggestionScore(item, titulo, ano))
            .FirstOrDefault();
    }

    private static int MatchSuggestionScore(ImdbSuggestionItem item, string titulo, int? ano)
    {
        var score = 0;
        var normalizedQuery = NormalizeTitle(titulo);
        var normalizedCandidate = NormalizeTitle(item.Title);

        if (normalizedQuery == normalizedCandidate)
        {
            score += 100;
        }
        else if (normalizedCandidate.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || normalizedQuery.Contains(normalizedCandidate, StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }

        if (ano.HasValue && item.Year.HasValue)
        {
            score += ano == item.Year ? 40 : Math.Max(0, 10 - Math.Abs(ano.Value - item.Year.Value));
        }

        score += item.Qid switch
        {
            "movie" => 10,
            "tvSeries" => 10,
            "tvMiniSeries" => 8,
            _ => 0
        };

        score += string.IsNullOrWhiteSpace(item.ImageUrl) ? 0 : 5;
        return score;
    }

    private static string NormalizeTitle(string value)
    {
        var normalized = value.ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^a-z0-9]+", " ");
        return Regex.Replace(normalized, @"\s+", " ").Trim();
    }

    private static TipoMidia MapSuggestionType(string? qid, TipoMidia fallback)
    {
        return qid switch
        {
            "tvSeries" or "tvMiniSeries" => TipoMidia.Serie,
            "movie" => TipoMidia.Filme,
            _ => fallback
        };
    }

    private sealed record ImdbSuggestionItem(string Id, string Title, string? Qid, int? Year, string? ImageUrl);

    public async Task<List<Episodio>> GetEpisodiosAsync(string imdbId, int temporada, bool traduzirDescricoes = true)
    {
        var episodios = new List<Episodio>();

        if (!_options.FallbackWebEnabled)
        {
            return episodios;
        }

        try
        {
            var url = $"{BaseUrl}/title/{imdbId}/episodes?season={temporada}";
            var html = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var episodeNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'episode-item')]");
            if (episodeNodes == null) return episodios;

            foreach (var node in episodeNodes)
            {
                var ep = new Episodio();

                var numNode = node.SelectSingleNode(".//div[contains(@class, 'episode-title')]");
                if (numNode != null)
                {
                    var match = Regex.Match(numNode.InnerText, @"E(\d+)");
                    if (match.Success) ep.Numero = int.Parse(match.Groups[1].Value);
                }

                var titleNode = node.SelectSingleNode(".//a[@data-testid='title-link']");
                ep.Titulo = titleNode?.InnerText.Trim();

                var descNode = node.SelectSingleNode(".//div[contains(@class, 'plot')]");
                var descricaoOriginal = descNode?.InnerText.Trim();
                ep.Descricao = traduzirDescricoes
                    ? await _textTranslationService.TranslateAsync(descricaoOriginal)
                    : descricaoOriginal;

                var ratingNode = node.SelectSingleNode(".//span[contains(@class, 'rating')]");
                if (ratingNode != null && decimal.TryParse(ratingNode.InnerText.Replace(",", "."), out var rating))
                {
                    ep.ImdbRating = rating;
                }

                if (ep.Numero > 0) episodios.Add(ep);
            }
        }
        catch (TextTranslationException)
        {
            throw;
        }
        catch { }

        return episodios;
    }
}
