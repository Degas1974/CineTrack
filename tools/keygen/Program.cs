using System.Security.Cryptography;

var bytes = RandomNumberGenerator.GetBytes(32);
var token = Convert.ToBase64String(bytes)
    .Replace("+", "-", StringComparison.Ordinal)
    .Replace("/", "_", StringComparison.Ordinal)
    .TrimEnd('=');

Console.WriteLine("========================================");
Console.WriteLine("  TrackList - API KEY");
Console.WriteLine("========================================");
Console.WriteLine();
Console.WriteLine("Configure este valor em Security:ApiKey na API e CineTrackApi:ApiKey no mobile:");
Console.WriteLine(token);
Console.WriteLine();
Console.WriteLine("========================================");
