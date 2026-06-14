namespace CineTrackTokenGen;

public class TokenGenOptions
{
    public const string SectionName = "TokenGen";

    public string KeyName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Uri { get; set; } = string.Empty;
    public int ExpiryYears { get; set; } = 1;
}