namespace CineTrack.Shared.Models;

public class MidiaCompletaVM
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? TituloOriginal { get; set; }
    public TipoMidia Tipo { get; set; }
    public string TipoDescricao { get; set; } = string.Empty;
    public int? Ano { get; set; }
    public string? Descricao { get; set; }
    public string? ImagemUrl { get; set; }
    public string? ImdbId { get; set; }
    public decimal? ImdbRating { get; set; }
    public int? ImdbVotes { get; set; }
    public int? Tomatometer { get; set; }
    public int? Popcornmeter { get; set; }
    public string? RottenTomatoesUrl { get; set; }
    public string? Generos { get; set; }
    public int? Duracao { get; set; }
    public string? Diretor { get; set; }
    public string? Elenco { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public StatusMidia StatusUsuario { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public int? EstrelaUsuario { get; set; }
    public decimal? NotaUsuario { get; set; }
    public string? ComentarioUsuario { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int? TotalTemporadas { get; set; }
    public int? TotalEpisodios { get; set; }
    public int? EpisodiosAssistidos { get; set; }
    public string ImagemExibicao { get; set; } = string.Empty;

    public string ProgressoTexto => Tipo == TipoMidia.Serie && TotalEpisodios > 0
        ? $"{EpisodiosAssistidos ?? 0}/{TotalEpisodios} episódios"
        : string.Empty;

    public double ProgressoPercentual => Tipo == TipoMidia.Serie && TotalEpisodios > 0
        ? (double)(EpisodiosAssistidos ?? 0) / TotalEpisodios.Value * 100
        : 0;
}

public class TemporadaCompletaVM
{
    public int Id { get; set; }
    public int MidiaId { get; set; }
    public int Numero { get; set; }
    public string? Titulo { get; set; }
    public int? Ano { get; set; }
    public int TotalEpisodios { get; set; }
    public int EpisodiosDisponiveis { get; set; }
    public int EpisodiosAssistidos { get; set; }
    public decimal PercentualCompleto { get; set; }
    public bool Completa { get; set; }

    public string NomeExibicao => string.IsNullOrEmpty(Titulo)
        ? $"Temporada {Numero}"
        : $"Temporada {Numero}: {Titulo}";
}

public class EpisodioCompletoVM
{
    public int Id { get; set; }
    public int TemporadaId { get; set; }
    public int Numero { get; set; }
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public int? Duracao { get; set; }
    public DateTime? DataExibicao { get; set; }
    public decimal? ImdbRating { get; set; }
    public int MidiaId { get; set; }
    public int NumeroTemporada { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public StatusEpisodio? StatusUsuario { get; set; }
    public bool Assistido { get; set; }
    public AssistindoCom? AssistindoCom { get; set; }
    public DateTime? DataAssistido { get; set; }
    public string? ComentarioUsuario { get; set; }

    public string TituloExibicao => string.IsNullOrEmpty(Titulo)
        ? Codigo
        : $"{Codigo} - {Titulo}";
}

public class AssociacaoPendenteVM
{
    public int Id { get; set; }
    public string TituloCapturado { get; set; } = string.Empty;
    public string? TituloBrutoCapturado { get; set; }
    public int? AnoCapturado { get; set; }
    public int? TemporadaCapturada { get; set; }
    public int? EpisodioCapturado { get; set; }
    public string? LinkSceneSource { get; set; }
    public string? CategoriaCapturada { get; set; }
    public string? QualidadeCapturada { get; set; }
    public string? FonteReleaseCapturada { get; set; }
    public string? CodecCapturado { get; set; }
    public string? ProviderCapturado { get; set; }
    public string? GrupoReleaseCapturado { get; set; }
    public string? ChaveAgrupamento { get; set; }
    public int ReleaseScore { get; set; }
    public string? ImdbIdCapturado { get; set; }
    public decimal Confianca { get; set; }
    public DateTime DataCaptura { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public StatusAssociacao Status { get; set; }
    public int? MidiaId { get; set; }
    public string? MidiaTitulo { get; set; }
    public string? MidiaTituloOriginal { get; set; }
    public int? MidiaAno { get; set; }
    public TipoMidia? MidiaTipo { get; set; }
    public string? MidiaImagem { get; set; }
    public int? EpisodioId { get; set; }
    public int? EpisodioNumero { get; set; }
    public string? EpisodioTitulo { get; set; }
    public int? TemporadaNumero { get; set; }

    public string GrupoFallback => string.IsNullOrWhiteSpace(ChaveAgrupamento)
        ? $"{TituloCapturado}|{AnoCapturado}|{TemporadaCapturada}|{EpisodioCapturado}"
        : ChaveAgrupamento;

    public bool FoiSelecionada => Status == StatusAssociacao.Confirmado;
}

public class EstatisticasVM
{
    public int TotalFilmes { get; set; }
    public int TotalSeries { get; set; }
    public int TotalEpisodios { get; set; }
    public int FilmesAssistidos { get; set; }
    public int SeriesCompletas { get; set; }
    public int SeriesEmAndamento { get; set; }
    public int EpisodiosAssistidos { get; set; }
    public int HorasAssistidas { get; set; }
    public int AssociacoesPendentes { get; set; }
    public DateTime? UltimaCaptura { get; set; }
}

public class GeneroStatVM
{
    public string Nome { get; set; } = string.Empty;
    public int Contagem { get; set; }
    public decimal Pct { get; set; }
}

public class AtividadeMensalVM
{
    public string M { get; set; } = string.Empty;
    public int V { get; set; }
}

public class ScrapingResult
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int NovosItens { get; set; }
    public int ItensAtualizados { get; set; }
    public int Erros { get; set; }
    public List<SourceSyncResult> Fontes { get; set; } = [];
}

public class SourceSyncResult
{
    public string Fonte { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public int ItensProcessados { get; set; }
    public int NovosItens { get; set; }
    public int ItensAtualizados { get; set; }
    public int Erros { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fim { get; set; }
}

public class FonteSyncStatus
{
    public string Fonte { get; set; } = string.Empty;
    public bool Habilitada { get; set; }
    public bool Configurada { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Mensagem { get; set; }
    public DateTime? UltimaSync { get; set; }
    public int? ItensPendentes { get; set; }
    public int? ErrosRecentes { get; set; }
}

public class RottenTomatoesRatingsResult
{
    public bool Sucesso { get; set; }
    public string Provider { get; set; } = "Disabled";
    public string Mensagem { get; set; } = string.Empty;
    public int ItensProcessados { get; set; }
    public int ItensAtualizados { get; set; }
    public int ItensIgnorados { get; set; }
    public int Erros { get; set; }
}

public class ImdbDatasetImportResult
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int TitulosProcessados { get; set; }
    public int MidiasInseridas { get; set; }
    public int MidiasAtualizadas { get; set; }
    public int TemporadasInseridas { get; set; }
    public int TemporadasAtualizadas { get; set; }
    public int EpisodiosInseridos { get; set; }
    public int EpisodiosAtualizados { get; set; }
    public int RatingsAplicados { get; set; }
    public int Erros { get; set; }
}

public class PosterBackfillResult
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int ItensProcessados { get; set; }
    public int ItensAtualizados { get; set; }
    public int ItensSemPoster { get; set; }
    public int Erros { get; set; }
}

public class BackfillSimulationResult
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int CapturasEncontradas { get; set; }
    public int GruposUnicosEncontrados { get; set; }
    public int GruposSimulados { get; set; }
    public int GruposComMatchImdb { get; set; }
    public int GruposSemMatchImdb { get; set; }
    public int FilmesEstimados { get; set; }
    public int SeriesEstimadas { get; set; }
    public int SeriesExpandidasCompletas { get; set; }
    public int TemporadasEstimadas { get; set; }
    public int EpisodiosEstimados { get; set; }
    public long CaracteresDescricoesMidia { get; set; }
    public long CaracteresDescricoesEpisodio { get; set; }
    public long CaracteresEstimadosFallback { get; set; }
    public long CaracteresTotaisTraducao { get; set; }
    public int MidiasSemDescricaoEstimadas { get; set; }
    public int EpisodiosSemDescricaoEstimados { get; set; }
    public int FranquiaGratisCaracteres { get; set; }
    public long CaracteresCobrados { get; set; }
    public decimal PrecoUsdPorMilhao { get; set; }
    public string TranslationProvider { get; set; } = "LibreTranslate";
    public int EstimativaCaracteresMidiaSemTexto { get; set; }
    public int EstimativaCaracteresEpisodioSemTexto { get; set; }
    public bool ExpandirSeriesCompleta { get; set; }
    public int MaxTemporadasPorSerie { get; set; }
    public int EstimativaTemporadasPorSerieSemLista { get; set; }
    public int EstimativaEpisodiosPorTemporadaSemLista { get; set; }
    public decimal CustoUsdBruto { get; set; }
    public decimal CustoUsdExcedente { get; set; }
    public int Erros { get; set; }
}
