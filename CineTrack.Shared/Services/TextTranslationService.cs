using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CineTrack.Shared.Services;

public sealed class TranslationOptions
{
    public string Provider { get; set; } = "LibreTranslate";
    public string? BaseUrl { get; set; }
    public string SourceLanguage { get; set; } = "en";
    public string TargetLanguage { get; set; } = "pt";
    public string? ApiKey { get; set; }
}

public sealed class TextTranslationException : Exception
{
    public TextTranslationException(string message)
        : base(message)
    {
    }

    public TextTranslationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public interface ITextTranslationService
{
    Task<string?> TranslateAsync(string? text, CancellationToken cancellationToken = default);
}

public static class TextTranslationServiceCollectionExtensions
{
    public static IServiceCollection AddRequiredTextTranslation(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Translation");
        services.Configure<TranslationOptions>(section);

        var options = section.Get<TranslationOptions>() ?? new TranslationOptions();
        ValidateConfiguration(options);

        if (string.Equals(options.Provider, "Disabled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(options.Provider, "None", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<ITextTranslationService, DisabledTextTranslationService>();
            return services;
        }

        var normalizedBaseUrl = options.BaseUrl!.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";

        services.AddHttpClient<ITextTranslationService, LibreTranslateTextTranslationService>(client =>
        {
            client.BaseAddress = new Uri(normalizedBaseUrl);
        });

        return services;
    }

    private static void ValidateConfiguration(TranslationOptions options)
    {
        if (!string.Equals(options.Provider, "LibreTranslate", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Provider, "Disabled", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Provider, "None", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Translation:Provider '{options.Provider}' não é suportado. Configure 'LibreTranslate' ou 'Disabled'.");
        }

        if (string.Equals(options.Provider, "Disabled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(options.Provider, "None", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!HasConfiguredValue(options.BaseUrl))
        {
            throw new InvalidOperationException("Translation:BaseUrl é obrigatório. Configure um endpoint válido do provider de tradução para subir a API e as Functions.");
        }

        if (!HasConfiguredValue(options.SourceLanguage))
        {
            throw new InvalidOperationException("Translation:SourceLanguage é obrigatório.");
        }

        if (!HasConfiguredValue(options.TargetLanguage))
        {
            throw new InvalidOperationException("Translation:TargetLanguage é obrigatório.");
        }
    }

    private static bool HasConfiguredValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.Trim().StartsWith("__CONFIGURE_", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class DisabledTextTranslationService : ITextTranslationService
{
    public Task<string?> TranslateAsync(string? text, CancellationToken cancellationToken = default) =>
        Task.FromResult(text);
}

public sealed class LibreTranslateTextTranslationService : ITextTranslationService
{
    private readonly HttpClient _httpClient;
    private readonly TranslationOptions _options;
    private readonly ILogger<LibreTranslateTextTranslationService> _logger;

    public LibreTranslateTextTranslationService(
        HttpClient httpClient,
        IOptions<TranslationOptions> options,
        ILogger<LibreTranslateTextTranslationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> TranslateAsync(string? text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        if (!string.Equals(_options.Provider, "LibreTranslate", StringComparison.OrdinalIgnoreCase))
        {
            throw new TextTranslationException($"Provider de tradução '{_options.Provider}' não é suportado.");
        }

        try
        {
            var request = new Dictionary<string, string?>
            {
                ["q"] = text,
                ["source"] = _options.SourceLanguage,
                ["target"] = _options.TargetLanguage,
                ["format"] = "text"
            };

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                request["api_key"] = _options.ApiKey;
            }

            using var response = await _httpClient.PostAsJsonAsync("translate", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new TextTranslationException($"Falha ao traduzir texto via {_options.Provider}. StatusCode={(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (document.RootElement.TryGetProperty("translatedText", out var translatedText))
            {
                var translated = translatedText.GetString();
                if (!string.IsNullOrWhiteSpace(translated))
                {
                    return translated.Trim();
                }
            }

            throw new TextTranslationException($"O provider {_options.Provider} respondeu sem 'translatedText'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao traduzir texto via {Provider}", _options.Provider);
            throw ex as TextTranslationException ?? new TextTranslationException("Falha ao traduzir texto para PT-BR.", ex);
        }
    }
}
