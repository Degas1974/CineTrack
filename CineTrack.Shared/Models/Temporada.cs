namespace CineTrack.Shared.Models;

public class Temporada
{
    public int Id { get; set; }
    public int MidiaId { get; set; }
    public int Numero { get; set; }
    public string? Titulo { get; set; }
    public int? Ano { get; set; }
    public int TotalEpisodios { get; set; }
}
