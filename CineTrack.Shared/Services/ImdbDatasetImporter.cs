using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace CineTrack.Shared.Services;

public interface IImdbDatasetImporter
{
    Task<ImdbDatasetImportResult> ImportAsync(ImdbDatasetImportRequest request, CancellationToken cancellationToken = default);
}

public sealed class ImdbDatasetImporter : IImdbDatasetImporter
{
    private readonly IMidiaRepository _midiaRepository;
    private readonly ITemporadaRepository _temporadaRepository;
    private readonly IEpisodioRepository _episodioRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImdbDatasetImporter> _logger;

    public ImdbDatasetImporter(
        IMidiaRepository midiaRepository,
        ITemporadaRepository temporadaRepository,
        IEpisodioRepository episodioRepository,
        IConfiguration configuration,
        ILogger<ImdbDatasetImporter> logger)
    {
        _midiaRepository = midiaRepository;
        _temporadaRepository = temporadaRepository;
        _episodioRepository = episodioRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ImdbDatasetImportResult> ImportAsync(ImdbDatasetImportRequest request, CancellationToken cancellationToken = default)
    {
        var result = new ImdbDatasetImportResult();
        var datasetDirectory = ResolveDatasetDirectory(request);

        if (string.IsNullOrWhiteSpace(datasetDirectory) || !Directory.Exists(datasetDirectory))
        {
            result.Sucesso = false;
            result.Erros = 1;
            result.Mensagem = "Diretório dos datasets IMDb não encontrado. Configure ImdbDatasets:Directory ou informe DatasetDirectory.";
            return result;
        }

        var basicsPath = Path.Combine(datasetDirectory, "title.basics.tsv");
        if (!File.Exists(basicsPath))
        {
            result.Sucesso = false;
            result.Erros = 1;
            result.Mensagem = "Arquivo title.basics.tsv não encontrado no diretório configurado.";
            return result;
        }

        try
        {
            var ratings = request.ImportRatings
                ? await LoadRatingsAsync(Path.Combine(datasetDirectory, "title.ratings.tsv"), cancellationToken)
                : new Dictionary<string, RatingRow>(StringComparer.OrdinalIgnoreCase);

            var midiasPorImdb = await ImportTitlesAsync(basicsPath, ratings, request.MaxTitles, result, cancellationToken);

            if (request.ImportEpisodes && midiasPorImdb.Count > 0)
            {
                var episodes = await LoadEpisodesAsync(Path.Combine(datasetDirectory, "title.episode.tsv"), midiasPorImdb.Keys, cancellationToken);
                await ImportEpisodesAsync(basicsPath, episodes, midiasPorImdb, ratings, result, cancellationToken);
            }

            await ImportNamesAsync(datasetDirectory, midiasPorImdb, result, cancellationToken);

            result.Sucesso = result.Erros == 0;
            result.Mensagem = $"IMDb datasets importados. Títulos: {result.TitulosProcessados}; mídias novas: {result.MidiasInseridas}; mídias atualizadas: {result.MidiasAtualizadas}; episódios novos: {result.EpisodiosInseridos}; episódios atualizados: {result.EpisodiosAtualizados}; ratings: {result.RatingsAplicados}.";
        }
        catch (Exception ex)
        {
            result.Sucesso = false;
            result.Erros++;
            result.Mensagem = ex.Message;
            _logger.LogError(ex, "Falha ao importar datasets IMDb de {DatasetDirectory}", datasetDirectory);
        }

        return result;
    }

    private string? ResolveDatasetDirectory(ImdbDatasetImportRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.DatasetDirectory))
        {
            return request.DatasetDirectory;
        }

        var configured = _configuration["ImdbDatasets:Directory"];
        return string.IsNullOrWhiteSpace(configured) ? null : configured;
    }

    private async Task<Dictionary<string, int>> ImportTitlesAsync(
        string basicsPath,
        IReadOnlyDictionary<string, RatingRow> ratings,
        int maxTitles,
        ImdbDatasetImportResult result,
        CancellationToken cancellationToken)
    {
        var midiasPorImdb = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        await foreach (var row in ReadTsvAsync(basicsPath, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (!IsSupportedMediaType(row.GetValueOrDefault("titleType")))
            {
                continue;
            }

            if (maxTitles > 0 && result.TitulosProcessados >= maxTitles)
            {
                break;
            }

            var imdbId = row.GetValueOrDefault("tconst");
            if (string.IsNullOrWhiteSpace(imdbId))
            {
                continue;
            }

            var midia = MapMidia(row, ratings.GetValueOrDefault(imdbId));
            var existing = await _midiaRepository.GetByImdbIdAsync(imdbId);

            if (existing == null)
            {
                midia.Id = await _midiaRepository.InsertAsync(midia);
                result.MidiasInseridas++;
            }
            else
            {
                MergeDatasetMidia(existing, midia);
                await _midiaRepository.UpdateAsync(existing);
                midia.Id = existing.Id;
                result.MidiasAtualizadas++;
            }

            if (ratings.ContainsKey(imdbId))
            {
                result.RatingsAplicados++;
            }

            result.TitulosProcessados++;
            midiasPorImdb[imdbId] = midia.Id;
        }

        return midiasPorImdb;
    }

    private async Task ImportEpisodesAsync(
        string basicsPath,
        IReadOnlyDictionary<string, EpisodeIndexRow> episodes,
        IReadOnlyDictionary<string, int> midiasPorImdb,
        IReadOnlyDictionary<string, RatingRow> ratings,
        ImdbDatasetImportResult result,
        CancellationToken cancellationToken)
    {
        if (episodes.Count == 0)
        {
            return;
        }

        await foreach (var row in ReadTsvAsync(basicsPath, cancellationToken))
        {
            if (!string.Equals(row.GetValueOrDefault("titleType"), "tvEpisode", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var imdbId = row.GetValueOrDefault("tconst");
            if (string.IsNullOrWhiteSpace(imdbId) || !episodes.TryGetValue(imdbId, out var episodeIndex))
            {
                continue;
            }

            if (!midiasPorImdb.TryGetValue(episodeIndex.ParentTconst, out var midiaId)
                || episodeIndex.SeasonNumber is null
                || episodeIndex.EpisodeNumber is null)
            {
                continue;
            }

            var temporada = await _temporadaRepository.GetByMidiaAndNumeroAsync(midiaId, episodeIndex.SeasonNumber.Value);
            if (temporada == null)
            {
                temporada = new Temporada
                {
                    MidiaId = midiaId,
                    Numero = episodeIndex.SeasonNumber.Value,
                    TotalEpisodios = Math.Max(episodeIndex.EpisodeNumber.Value, 1)
                };

                temporada.Id = await _temporadaRepository.InsertAsync(temporada);
                result.TemporadasInseridas++;
            }
            else if (temporada.TotalEpisodios < episodeIndex.EpisodeNumber.Value)
            {
                temporada.TotalEpisodios = episodeIndex.EpisodeNumber.Value;
                await _temporadaRepository.UpdateAsync(temporada);
                result.TemporadasAtualizadas++;
            }

            var episodio = MapEpisodio(row, ratings.GetValueOrDefault(imdbId), episodeIndex.EpisodeNumber.Value);
            episodio.TemporadaId = temporada.Id;

            var existing = await _episodioRepository.GetByTemporadaAndNumeroAsync(temporada.Id, episodio.Numero);
            if (existing == null)
            {
                await _episodioRepository.InsertAsync(episodio);
                result.EpisodiosInseridos++;
            }
            else
            {
                MergeDatasetEpisodio(existing, episodio);
                await _episodioRepository.UpdateAsync(existing);
                result.EpisodiosAtualizados++;
            }

            if (ratings.ContainsKey(imdbId))
            {
                result.RatingsAplicados++;
            }
        }
    }

    private async Task ImportNamesAsync(
        string datasetDirectory,
        IReadOnlyDictionary<string, int> midiasPorImdb,
        ImdbDatasetImportResult result,
        CancellationToken cancellationToken)
    {
        var crewPath = Path.Combine(datasetDirectory, "title.crew.tsv");
        var principalsPath = Path.Combine(datasetDirectory, "title.principals.tsv");
        var namesPath = Path.Combine(datasetDirectory, "name.basics.tsv");

        if (!File.Exists(namesPath) || (!File.Exists(crewPath) && !File.Exists(principalsPath)))
        {
            return;
        }

        var directorsByTitle = File.Exists(crewPath)
            ? await LoadCrewAsync(crewPath, midiasPorImdb.Keys, cancellationToken)
            : new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var castByTitle = File.Exists(principalsPath)
            ? await LoadCastAsync(principalsPath, midiasPorImdb.Keys, cancellationToken)
            : new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var neededNames = directorsByTitle.Values.SelectMany(x => x)
            .Concat(castByTitle.Values.SelectMany(x => x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (neededNames.Count == 0)
        {
            return;
        }

        var names = await LoadNamesAsync(namesPath, neededNames, cancellationToken);
        foreach (var (imdbId, midiaId) in midiasPorImdb)
        {
            var midia = await _midiaRepository.GetByImdbIdAsync(imdbId);
            if (midia == null)
            {
                continue;
            }

            var directors = directorsByTitle.GetValueOrDefault(imdbId)?
                .Select(id => names.GetValueOrDefault(id))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();

            var cast = castByTitle.GetValueOrDefault(imdbId)?
                .Select(id => names.GetValueOrDefault(id))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .ToList();

            var changed = false;
            if ((directors?.Count ?? 0) > 0 && string.IsNullOrWhiteSpace(midia.Diretor))
            {
                midia.Diretor = string.Join(", ", directors!);
                changed = true;
            }

            if ((cast?.Count ?? 0) > 0 && string.IsNullOrWhiteSpace(midia.Elenco))
            {
                midia.Elenco = string.Join(", ", cast!);
                changed = true;
            }

            if (changed)
            {
                await _midiaRepository.UpdateAsync(midia);
                result.MidiasAtualizadas++;
            }

            _ = midiaId;
        }
    }

    private static Midia MapMidia(IReadOnlyDictionary<string, string> row, RatingRow? rating)
    {
        var titleType = row.GetValueOrDefault("titleType");
        return new Midia
        {
            ImdbId = row.GetValueOrDefault("tconst") ?? string.Empty,
            Titulo = CleanValue(row.GetValueOrDefault("primaryTitle")) ?? "Sem título",
            TituloOriginal = CleanValue(row.GetValueOrDefault("originalTitle")),
            Tipo = string.Equals(titleType, "movie", StringComparison.OrdinalIgnoreCase)
                || string.Equals(titleType, "tvMovie", StringComparison.OrdinalIgnoreCase)
                    ? TipoMidia.Filme
                    : TipoMidia.Serie,
            Ano = ParseInt(row.GetValueOrDefault("startYear")),
            Generos = CleanValue(row.GetValueOrDefault("genres"))?.Replace(",", ", "),
            Duracao = ParseInt(row.GetValueOrDefault("runtimeMinutes")),
            ImdbRating = rating?.AverageRating,
            ImdbVotes = rating?.NumVotes,
            Ativo = true,
            DataCriacao = DateTime.Now
        };
    }

    private static Episodio MapEpisodio(IReadOnlyDictionary<string, string> row, RatingRow? rating, int numero)
    {
        return new Episodio
        {
            Numero = numero,
            Titulo = CleanValue(row.GetValueOrDefault("primaryTitle")),
            Duracao = ParseInt(row.GetValueOrDefault("runtimeMinutes")),
            DataExibicao = ParseYearDate(row.GetValueOrDefault("startYear")),
            ImdbRating = rating?.AverageRating
        };
    }

    private static void MergeDatasetMidia(Midia target, Midia source)
    {
        target.Titulo = ChooseText(target.Titulo, source.Titulo) ?? target.Titulo;
        target.TituloOriginal = ChooseText(target.TituloOriginal, source.TituloOriginal);
        target.Tipo = source.Tipo;
        target.Ano = target.Ano ?? source.Ano;
        target.Generos = ChooseText(target.Generos, source.Generos);
        target.Duracao = target.Duracao ?? source.Duracao;
        target.ImdbRating = source.ImdbRating ?? target.ImdbRating;
        target.ImdbVotes = source.ImdbVotes ?? target.ImdbVotes;
    }

    private static void MergeDatasetEpisodio(Episodio target, Episodio source)
    {
        target.Titulo = ChooseText(target.Titulo, source.Titulo);
        target.Duracao = target.Duracao ?? source.Duracao;
        target.DataExibicao = target.DataExibicao ?? source.DataExibicao;
        target.ImdbRating = source.ImdbRating ?? target.ImdbRating;
    }

    private static async Task<Dictionary<string, RatingRow>> LoadRatingsAsync(string path, CancellationToken cancellationToken)
    {
        var ratings = new Dictionary<string, RatingRow>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            return ratings;
        }

        await foreach (var row in ReadTsvAsync(path, cancellationToken))
        {
            var tconst = row.GetValueOrDefault("tconst");
            var rating = ParseDecimal(row.GetValueOrDefault("averageRating"));
            var votes = ParseInt(row.GetValueOrDefault("numVotes"));
            if (!string.IsNullOrWhiteSpace(tconst) && rating.HasValue)
            {
                ratings[tconst] = new RatingRow(rating.Value, votes);
            }
        }

        return ratings;
    }

    private static async Task<Dictionary<string, EpisodeIndexRow>> LoadEpisodesAsync(string path, IEnumerable<string> parentIds, CancellationToken cancellationToken)
    {
        var parentSet = parentIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var episodes = new Dictionary<string, EpisodeIndexRow>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            return episodes;
        }

        await foreach (var row in ReadTsvAsync(path, cancellationToken))
        {
            var parent = row.GetValueOrDefault("parentTconst");
            var episode = row.GetValueOrDefault("tconst");
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(episode) || !parentSet.Contains(parent))
            {
                continue;
            }

            episodes[episode] = new EpisodeIndexRow(
                parent,
                ParseInt(row.GetValueOrDefault("seasonNumber")),
                ParseInt(row.GetValueOrDefault("episodeNumber")));
        }

        return episodes;
    }

    private static async Task<Dictionary<string, List<string>>> LoadCrewAsync(string path, IEnumerable<string> titleIds, CancellationToken cancellationToken)
    {
        var titleSet = titleIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var crew = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        await foreach (var row in ReadTsvAsync(path, cancellationToken))
        {
            var tconst = row.GetValueOrDefault("tconst");
            if (string.IsNullOrWhiteSpace(tconst) || !titleSet.Contains(tconst))
            {
                continue;
            }

            crew[tconst] = SplitIds(row.GetValueOrDefault("directors")).Take(3).ToList();
        }

        return crew;
    }

    private static async Task<Dictionary<string, List<string>>> LoadCastAsync(string path, IEnumerable<string> titleIds, CancellationToken cancellationToken)
    {
        var titleSet = titleIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cast = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        await foreach (var row in ReadTsvAsync(path, cancellationToken))
        {
            var tconst = row.GetValueOrDefault("tconst");
            var nconst = row.GetValueOrDefault("nconst");
            var category = row.GetValueOrDefault("category");
            if (string.IsNullOrWhiteSpace(tconst)
                || string.IsNullOrWhiteSpace(nconst)
                || !titleSet.Contains(tconst)
                || !IsCastCategory(category))
            {
                continue;
            }

            if (!cast.TryGetValue(tconst, out var list))
            {
                list = [];
                cast[tconst] = list;
            }

            if (list.Count < 8)
            {
                list.Add(nconst);
            }
        }

        return cast;
    }

    private static async Task<Dictionary<string, string>> LoadNamesAsync(string path, ISet<string> neededNames, CancellationToken cancellationToken)
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        await foreach (var row in ReadTsvAsync(path, cancellationToken))
        {
            var nconst = row.GetValueOrDefault("nconst");
            if (string.IsNullOrWhiteSpace(nconst) || !neededNames.Contains(nconst))
            {
                continue;
            }

            var name = CleanValue(row.GetValueOrDefault("primaryName"));
            if (!string.IsNullOrWhiteSpace(name))
            {
                names[nconst] = name;
            }
        }

        return names;
    }

    private static async IAsyncEnumerable<Dictionary<string, string>> ReadTsvAsync(
        string path,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            yield break;
        }

        var headers = headerLine.Split('\t');
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = line.Split('\t');
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length && i < values.Length; i++)
            {
                row[headers[i]] = values[i];
            }

            yield return row;
        }
    }

    private static bool IsSupportedMediaType(string? titleType) =>
        string.Equals(titleType, "movie", StringComparison.OrdinalIgnoreCase)
        || string.Equals(titleType, "tvMovie", StringComparison.OrdinalIgnoreCase)
        || string.Equals(titleType, "tvSeries", StringComparison.OrdinalIgnoreCase)
        || string.Equals(titleType, "tvMiniSeries", StringComparison.OrdinalIgnoreCase);

    private static bool IsCastCategory(string? category) =>
        string.Equals(category, "actor", StringComparison.OrdinalIgnoreCase)
        || string.Equals(category, "actress", StringComparison.OrdinalIgnoreCase)
        || string.Equals(category, "self", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> SplitIds(string? value) =>
        CleanValue(value)?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? [];

    private static string? CleanValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == @"\N")
        {
            return null;
        }

        return value.Trim();
    }

    private static string? ChooseText(string? current, string? candidate) =>
        string.IsNullOrWhiteSpace(current) ? candidate : current;

    private static int? ParseInt(string? value) =>
        int.TryParse(CleanValue(value), out var parsed) ? parsed : null;

    private static decimal? ParseDecimal(string? value) =>
        decimal.TryParse(CleanValue(value), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static DateTime? ParseYearDate(string? value) =>
        int.TryParse(CleanValue(value), out var year) && year > 0
            ? new DateTime(year, 1, 1)
            : null;

    private sealed record RatingRow(decimal AverageRating, int? NumVotes);
    private sealed record EpisodeIndexRow(string ParentTconst, int? SeasonNumber, int? EpisodeNumber);
}
