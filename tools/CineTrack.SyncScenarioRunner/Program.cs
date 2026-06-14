using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using CineTrack.Shared.Services;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;

var options = ParseOptions(args);
if (string.IsNullOrWhiteSpace(options.ConnectionString))
{
	throw new InvalidOperationException("Informe a connection string via --connection-string.");
}

var runId = $"sync-test-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
var exactTitle = $"Auto Confirm Serie {runId}";
var pendingCaptureTitle = $"Pending Capture {runId}";
var pendingCanonicalTitle = $"Canonical Pending {runId}";
var exactImdbId = $"tt9{DateTime.UtcNow:MMddHHmmss}1";
var pendingImdbId = $"tt9{DateTime.UtcNow:MMddHHmmss}2";

var sceneCaptures = new List<AssociacaoSceneSource>
{
	new()
	{
		TituloCapturado = exactTitle,
		AnoCapturado = 2026,
		TemporadaCapturada = 1,
		EpisodioCapturado = 1,
		LinkSceneSource = $"https://www.scnsrc.me/{runId}-exact/",
		Status = StatusAssociacao.Pendente,
		DataCaptura = DateTime.Now
	},
	new()
	{
		TituloCapturado = pendingCaptureTitle,
		AnoCapturado = null,
		LinkSceneSource = $"https://www.scnsrc.me/{runId}-pending/",
		Status = StatusAssociacao.Pendente,
		DataCaptura = DateTime.Now
	}
};

var imdbCatalog = new Dictionary<string, Midia>(StringComparer.OrdinalIgnoreCase)
{
	[exactTitle] = new Midia
	{
		Titulo = exactTitle,
		TituloOriginal = exactTitle,
		Tipo = TipoMidia.Serie,
		Ano = 2026,
		ImdbId = exactImdbId,
		Descricao = "Teste controlado de auto-confirmação",
		Generos = "Drama",
		DataCriacao = DateTime.Now
	},
	[pendingCaptureTitle] = new Midia
	{
		Titulo = pendingCanonicalTitle,
		TituloOriginal = pendingCanonicalTitle,
		Tipo = TipoMidia.Filme,
		Ano = 2026,
		ImdbId = pendingImdbId,
		Descricao = "Teste controlado de pendência por limiar",
		Generos = "Thriller",
		DataCriacao = DateTime.Now
	}
};

var episodeCatalog = new Dictionary<string, List<Episodio>>(StringComparer.OrdinalIgnoreCase)
{
	[exactImdbId] = new List<Episodio>
	{
		new()
		{
			Numero = 1,
			Titulo = $"Piloto {runId}",
			Descricao = "Primeiro episódio do cenário controlado.",
			ImdbRating = 8.9m
		}
	}
};

var syncRepository = new SyncRepository(options.ConnectionString);
var midiaRepository = new MidiaRepository(options.ConnectionString);
var temporadaRepository = new TemporadaRepository(options.ConnectionString);
var episodioRepository = new EpisodioRepository(options.ConnectionString);

var orchestrator = new SyncOrchestrator(
	syncRepository,
	new FakeSceneSourceScraper(sceneCaptures),
	new FakeImdbScraper(imdbCatalog, episodeCatalog),
	midiaRepository,
	temporadaRepository,
	episodioRepository,
	NullLogger<SyncOrchestrator>.Instance);

try
{
	var threshold = await syncRepository.GetConfiancaMinimaAutoAssociacaoAsync();
	var scrapingResult = await orchestrator.ExecutarScrapingSceneSourceAsync("Teste Controlado");

	await using var connection = new SqlConnection(options.ConnectionString);
	await connection.OpenAsync();

	var exactAssociation = await connection.QuerySingleOrDefaultAsync<AssociationProbe>(
		@"SELECT TOP 1 Id, Status, DataConfirmacao, Confianca, MidiaId, EpisodioId
		  FROM AssociacaoSceneSource
		  WHERE LinkSceneSource = @Link
		  ORDER BY Id DESC",
		new { Link = sceneCaptures[0].LinkSceneSource });

	var pendingAssociation = await connection.QuerySingleOrDefaultAsync<AssociationProbe>(
		@"SELECT TOP 1 Id, Status, DataConfirmacao, Confianca, MidiaId, EpisodioId
		  FROM AssociacaoSceneSource
		  WHERE LinkSceneSource = @Link
		  ORDER BY Id DESC",
		new { Link = sceneCaptures[1].LinkSceneSource });

	if (exactAssociation == null)
	{
		throw new InvalidOperationException("A associação de alta confiança não foi gravada no banco.");
	}

	if (pendingAssociation == null)
	{
		throw new InvalidOperationException("A associação de baixa confiança não foi gravada no banco.");
	}

	if (exactAssociation.Status != (int)StatusAssociacao.Confirmado || exactAssociation.DataConfirmacao == null)
	{
		throw new InvalidOperationException("A associação de alta confiança não foi auto-confirmada como esperado.");
	}

	if (pendingAssociation.Status != (int)StatusAssociacao.Pendente)
	{
		throw new InvalidOperationException("A associação de baixa confiança não permaneceu pendente como esperado.");
	}

	var usuarioEpisodio = await connection.QuerySingleOrDefaultAsync<UsuarioEpisodioProbe>(
		@"SELECT TOP 1 Status, DataAssistido
		  FROM UsuarioEpisodio
		  WHERE EpisodioId = @EpisodioId",
		new { exactAssociation.EpisodioId });

	var usuarioMidia = await connection.QuerySingleOrDefaultAsync<UsuarioMidiaProbe>(
		@"SELECT TOP 1 Status, DataInicio
		  FROM UsuarioMidia
		  WHERE MidiaId = @MidiaId",
		new { exactAssociation.MidiaId });

	if (usuarioEpisodio == null || usuarioEpisodio.Status != (int)StatusEpisodio.Assistido)
	{
		throw new InvalidOperationException("A confirmação automática não marcou o episódio como assistido.");
	}

	if (usuarioMidia == null || usuarioMidia.Status != (int)StatusMidia.Assistindo)
	{
		throw new InvalidOperationException("A confirmação automática não atualizou a série para Assistindo.");
	}

	Console.WriteLine($"Threshold usado: {threshold}");
	Console.WriteLine($"Resultado do scraping: sucesso={scrapingResult.Sucesso}; novos={scrapingResult.NovosItens}; atualizados={scrapingResult.ItensAtualizados}");
	Console.WriteLine($"Alta confiança: status={exactAssociation.Status}; confianca={exactAssociation.Confianca}; confirmadoEm={exactAssociation.DataConfirmacao:O}");
	Console.WriteLine($"Baixa confiança: status={pendingAssociation.Status}; confianca={pendingAssociation.Confianca}; confirmadoEm={(pendingAssociation.DataConfirmacao?.ToString("O") ?? "null")}");
	Console.WriteLine("Teste controlado concluído com sucesso.");
}
finally
{
	if (!options.KeepData)
	{
		await CleanupAsync(options.ConnectionString, new[] { exactImdbId, pendingImdbId }, sceneCaptures.Select(x => x.LinkSceneSource!).ToArray());
	}
}

static RunnerOptions ParseOptions(string[] args)
{
	string? connectionString = null;
	var keepData = false;

	for (var index = 0; index < args.Length; index++)
	{
		switch (args[index])
		{
			case "--connection-string" when index + 1 < args.Length:
				connectionString = args[++index];
				break;
			case "--keep-data":
				keepData = true;
				break;
		}
	}

	return new RunnerOptions(connectionString ?? string.Empty, keepData);
}

static async Task CleanupAsync(string connectionString, IReadOnlyCollection<string> imdbIds, IReadOnlyCollection<string> links)
{
	await using var connection = new SqlConnection(connectionString);
	await connection.OpenAsync();

	var midiaIds = (await connection.QueryAsync<int>(
		"SELECT Id FROM Midia WHERE ImdbId IN @ImdbIds",
		new { ImdbIds = imdbIds }))
		.ToArray();

	if (midiaIds.Length == 0)
	{
		await connection.ExecuteAsync(
			"DELETE FROM AssociacaoSceneSource WHERE LinkSceneSource IN @Links",
			new { Links = links });
		return;
	}

	var temporadaIds = (await connection.QueryAsync<int>(
		"SELECT Id FROM Temporada WHERE MidiaId IN @MidiaIds",
		new { MidiaIds = midiaIds }))
		.ToArray();

	var episodioIds = temporadaIds.Length == 0
		? Array.Empty<int>()
		: (await connection.QueryAsync<int>(
			"SELECT Id FROM Episodio WHERE TemporadaId IN @TemporadaIds",
			new { TemporadaIds = temporadaIds }))
			.ToArray();

	if (episodioIds.Length > 0)
	{
		await connection.ExecuteAsync("DELETE FROM UsuarioEpisodio WHERE EpisodioId IN @EpisodioIds", new { EpisodioIds = episodioIds });
	}

	await connection.ExecuteAsync("DELETE FROM AssociacaoSceneSource WHERE LinkSceneSource IN @Links", new { Links = links });
	await connection.ExecuteAsync("DELETE FROM UsuarioMidia WHERE MidiaId IN @MidiaIds", new { MidiaIds = midiaIds });

	if (episodioIds.Length > 0)
	{
		await connection.ExecuteAsync("DELETE FROM Episodio WHERE Id IN @EpisodioIds", new { EpisodioIds = episodioIds });
	}

	if (temporadaIds.Length > 0)
	{
		await connection.ExecuteAsync("DELETE FROM Temporada WHERE Id IN @TemporadaIds", new { TemporadaIds = temporadaIds });
	}

	await connection.ExecuteAsync("DELETE FROM Midia WHERE Id IN @MidiaIds", new { MidiaIds = midiaIds });
}

file sealed record RunnerOptions(string ConnectionString, bool KeepData);

file sealed class AssociationProbe
{
	public int Id { get; set; }
	public int Status { get; set; }
	public DateTime? DataConfirmacao { get; set; }
	public decimal Confianca { get; set; }
	public int? MidiaId { get; set; }
	public int? EpisodioId { get; set; }
}

file sealed class UsuarioEpisodioProbe
{
	public int Status { get; set; }
	public DateTime? DataAssistido { get; set; }
}

file sealed class UsuarioMidiaProbe
{
	public int Status { get; set; }
	public DateTime? DataInicio { get; set; }
}

file sealed class FakeSceneSourceScraper : ISceneSourceScraper
{
	private readonly List<AssociacaoSceneSource> _capturas;

	public FakeSceneSourceScraper(IEnumerable<AssociacaoSceneSource> capturas)
	{
		_capturas = capturas.Select(captura => new AssociacaoSceneSource
		{
			TituloCapturado = captura.TituloCapturado,
			AnoCapturado = captura.AnoCapturado,
			ModoParsingCapturado = captura.ModoParsingCapturado,
			TemporadaCapturada = captura.TemporadaCapturada,
			EpisodioCapturado = captura.EpisodioCapturado,
			LinkSceneSource = captura.LinkSceneSource,
			ImdbIdCapturado = captura.ImdbIdCapturado,
			MidiaId = captura.MidiaId,
			EpisodioId = captura.EpisodioId,
			Confianca = captura.Confianca,
			Status = captura.Status,
			DataCaptura = captura.DataCaptura
		}).ToList();
	}

	public Task<List<AssociacaoSceneSource>> ScrapeRecentAsync() => Task.FromResult(_capturas);
}

file sealed class FakeImdbScraper : IImdbScraper
{
	private readonly Dictionary<string, Midia> _midias;
	private readonly Dictionary<string, List<Episodio>> _episodios;

	public FakeImdbScraper(Dictionary<string, Midia> midias, Dictionary<string, List<Episodio>> episodios)
	{
		_midias = midias;
		_episodios = episodios;
	}

	public Task<Midia?> SearchAndGetDetailsAsync(string titulo, int? ano = null, bool traduzirDescricoes = true)
	{
		if (!_midias.TryGetValue(titulo, out var midia))
		{
			return Task.FromResult<Midia?>(null);
		}

		return Task.FromResult<Midia?>(CloneMidia(midia));
	}

	public Task<Midia?> GetByIdAsync(string imdbId, bool traduzirDescricoes = true)
	{
		var midia = _midias.Values.FirstOrDefault(item => string.Equals(item.ImdbId, imdbId, StringComparison.OrdinalIgnoreCase));
		return Task.FromResult<Midia?>(midia == null ? null : CloneMidia(midia));
	}

	public Task<List<Episodio>> GetEpisodiosAsync(string imdbId, int temporada, bool traduzirDescricoes = true)
	{
		if (!_episodios.TryGetValue(imdbId, out var episodios))
		{
			return Task.FromResult(new List<Episodio>());
		}

		return Task.FromResult(episodios.Select(episodio => new Episodio
		{
			Numero = episodio.Numero,
			Titulo = episodio.Titulo,
			Descricao = episodio.Descricao,
			Duracao = episodio.Duracao,
			DataExibicao = episodio.DataExibicao,
			ImdbRating = episodio.ImdbRating
		}).ToList());
	}

	private static Midia CloneMidia(Midia origem)
	{
		return new Midia
		{
			Titulo = origem.Titulo,
			TituloOriginal = origem.TituloOriginal,
			Tipo = origem.Tipo,
			Ano = origem.Ano,
			Descricao = origem.Descricao,
			ImagemUrl = origem.ImagemUrl,
			ImdbId = origem.ImdbId,
			ImdbRating = origem.ImdbRating,
			ImdbVotes = origem.ImdbVotes,
			Tomatometer = origem.Tomatometer,
			Popcornmeter = origem.Popcornmeter,
			RottenTomatoesUrl = origem.RottenTomatoesUrl,
			Generos = origem.Generos,
			Duracao = origem.Duracao,
			Diretor = origem.Diretor,
			Elenco = origem.Elenco,
			Ativo = origem.Ativo,
			DataCriacao = origem.DataCriacao,
			DataAtualizacao = origem.DataAtualizacao
		};
	}
}
