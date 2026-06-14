namespace CineTrack.Shared.Models;

public class Midia
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? TituloOriginal { get; set; }
    public TipoMidia Tipo { get; set; }
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
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? DataAtualizacao { get; set; }
}
