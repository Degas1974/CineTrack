namespace CineTrack.Mobile.Services;

public class CineTrackApiOptions
{
    public const string SectionName = "CineTrackApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
