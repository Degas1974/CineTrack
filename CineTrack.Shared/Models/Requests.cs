using System.ComponentModel.DataAnnotations;

namespace CineTrack.Shared.Models;

public class UpdateStatusRequest
{
    [Range(0, 3)]
    public int Status { get; set; }
}

public class UpdateNotaRequest
{
    [Range(1, 5)]
    public int Estrelas { get; set; }

    [Range(typeof(decimal), "0", "10")]
    public decimal? Nota { get; set; }

    [MaxLength(1000)]
    public string? Comentario { get; set; }
}

public class MarcarAssistidoRequest
{
    [Range(0, 3)]
    public int? AssistindoCom { get; set; }

    [MaxLength(1000)]
    public string? Comentario { get; set; }
}

public class ImdbDatasetImportRequest
{
    [MaxLength(1000)]
    public string? DatasetDirectory { get; set; }

    public int MaxTitles { get; set; } = 0;
    public bool ImportEpisodes { get; set; } = true;
    public bool ImportRatings { get; set; } = true;
}

public class RatingsReprocessRequest
{
    public int Quantidade { get; set; } = 100;
    public bool Forcar { get; set; }
}
