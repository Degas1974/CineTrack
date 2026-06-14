using CineTrack.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace CineTrack.Shared.Services;

public interface IRottenTomatoesProvider
{
    Task<RottenTomatoesRatingsResult> ReprocessarRatingsAsync(int quantidade = 100, bool forcar = false, CancellationToken cancellationToken = default);
}

public sealed class RottenTomatoesProviderOptions
{
    public string Provider { get; set; } = "Disabled";
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
}

public sealed class DisabledRottenTomatoesProvider : IRottenTomatoesProvider
{
    private readonly RottenTomatoesProviderOptions _options;

    public DisabledRottenTomatoesProvider(IConfiguration configuration)
    {
        _options = configuration.GetSection("RottenTomatoes").Get<RottenTomatoesProviderOptions>()
            ?? new RottenTomatoesProviderOptions();
    }

    public Task<RottenTomatoesRatingsResult> ReprocessarRatingsAsync(int quantidade = 100, bool forcar = false, CancellationToken cancellationToken = default)
    {
        var provider = string.IsNullOrWhiteSpace(_options.Provider) ? "Disabled" : _options.Provider;
        return Task.FromResult(new RottenTomatoesRatingsResult
        {
            Sucesso = true,
            Provider = provider,
            Mensagem = "Rotten Tomatoes está em modo Disabled/Manual. Configure um provider licenciado antes de reprocessar ratings automaticamente.",
            ItensIgnorados = Math.Max(0, quantidade)
        });
    }
}
