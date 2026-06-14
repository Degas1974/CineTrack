namespace CineTrack.Shared.Models;

public class AssociacaoSceneSource
{
    public int Id { get; set; }
    public string TituloCapturado { get; set; } = string.Empty;
    public string? TituloBrutoCapturado { get; set; }
    public string? TituloEpisodioCapturado { get; set; }
    public string? ModoParsingCapturado { get; set; }
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
    public int? MidiaId { get; set; }
    public int? EpisodioId { get; set; }
    public decimal Confianca { get; set; }
    public StatusAssociacao Status { get; set; }
    public DateTime DataCaptura { get; set; } = DateTime.Now;
    public DateTime? DataConfirmacao { get; set; }
}

public class LogCaptura
{
    public int Id { get; set; }
    public FonteCaptura Fonte { get; set; }
    public TipoLog Tipo { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? Detalhes { get; set; }
    public DateTime DataLog { get; set; } = DateTime.Now;
}

public class Configuracao
{
    public int Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string? Valor { get; set; }
    public string? Descricao { get; set; }
}
