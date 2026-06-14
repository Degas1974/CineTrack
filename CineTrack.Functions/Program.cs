using CineTrack.Shared.Data;
using CineTrack.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables();

var connectionString = ResolveConnectionString(builder.Configuration)
    ?? throw new InvalidOperationException("SqlConnectionString ou ConnectionStrings:DefaultConnection não foi configurada.");

builder.Services.AddScoped<IMidiaRepository>(_ => new MidiaRepository(connectionString));
builder.Services.AddScoped<ITemporadaRepository>(_ => new TemporadaRepository(connectionString));
builder.Services.AddScoped<IEpisodioRepository>(_ => new EpisodioRepository(connectionString));
builder.Services.AddScoped<ISyncRepository>(_ => new SyncRepository(connectionString));
builder.Services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();
builder.Services.AddScoped<IImdbDatasetImporter, ImdbDatasetImporter>();
builder.Services.Configure<SceneSourceScraperOptions>(builder.Configuration.GetSection("Scraping:SceneSource"));
builder.Services.Configure<ImdbScraperOptions>(builder.Configuration.GetSection("ImdbDatasets"));
builder.Services.AddHttpClient<ISceneSourceScraper, SceneSourceScraper>();
builder.Services.AddHttpClient<IImdbScraper, ImdbScraper>();
builder.Services.AddRequiredTextTranslation(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TrackList.LocalWorker");
var orchestrator = scope.ServiceProvider.GetRequiredService<ISyncOrchestrator>();

logger.LogInformation("TrackList local worker iniciando sync manual.");
var result = await orchestrator.ExecutarScrapingSceneSourceAsync("LocalWorker");

logger.LogInformation(
    "TrackList local worker finalizado. Sucesso={Sucesso}; Novos={Novos}; Atualizados={Atualizados}; Erros={Erros}; Mensagem={Mensagem}",
    result.Sucesso,
    result.NovosItens,
    result.ItensAtualizados,
    result.Erros,
    result.Mensagem);

return result.Sucesso ? 0 : 1;

static string? ResolveConnectionString(IConfiguration configuration)
{
    var configured = configuration.GetConnectionString("DefaultConnection");
    if (HasConfiguredValue(configured))
    {
        return configured;
    }

    var direct = configuration["SqlConnectionString"];
    if (HasConfiguredValue(direct))
    {
        return direct;
    }

    return configuration["CINETRACK_SQL_CONNECTION"];
}

static bool HasConfiguredValue(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    return !value.Trim().StartsWith("__CONFIGURE_", StringComparison.OrdinalIgnoreCase);
}
