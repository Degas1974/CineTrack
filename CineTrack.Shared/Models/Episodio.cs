namespace CineTrack.Shared.Models;

public class Episodio
{
    public int Id { get; set; }
    public int TemporadaId { get; set; }
    public int Numero { get; set; }
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public int? Duracao { get; set; }
    public DateTime? DataExibicao { get; set; }
    public decimal? ImdbRating { get; set; }
}
