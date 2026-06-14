using CineTrack.Mobile.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CineTrack.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        using (var appSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult())
        {
            builder.Configuration.AddJsonStream(appSettingsStream);
        }

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Registrar serviços
        builder.Services.Configure<CineTrackApiOptions>(builder.Configuration.GetSection(CineTrackApiOptions.SectionName));
        builder.Services.AddScoped<HttpClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CineTrackApiOptions>>().Value;
            var baseUrl = ResolveBaseUrl(options);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 30)
            };

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
            }

            return httpClient;
        });
        builder.Services.AddScoped<CineTrackApiClient>();
        builder.Services.AddScoped<IconService>();

        return builder.Build();
    }

    private static string ResolveBaseUrl(CineTrackApiOptions options)
    {
        if (HasConfiguredValue(options.BaseUrl))
        {
            return EnsureTrailingSlash(options.BaseUrl);
        }

#if ANDROID
        return "http://10.0.2.2:5050/";
#else
        return "http://localhost:5050/";
#endif
    }

    private static bool HasConfiguredValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.Trim().StartsWith("__CONFIGURE_", StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith('/') ? value : $"{value}/";
}
