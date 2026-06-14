namespace CineTrack.Shared.Models;

public class UsuarioMidia
{
    public int Id { get; set; }
    public int MidiaId { get; set; }
    public StatusMidia Status { get; set; }
    public int? Estrelas { get; set; }
    public decimal? Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}

public class UsuarioEpisodio
{
    public int Id { get; set; }
    public int EpisodioId { get; set; }
    public StatusEpisodio Status { get; set; }
    public AssistindoCom? AssistindoCom { get; set; }
    public DateTime? DataAssistido { get; set; }
    public string? Comentario { get; set; }
}
