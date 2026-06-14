using CineTrack.API.Middleware;
using CineTrack.Shared.Data;
using CineTrack.Shared.Services;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "CineTrackCors";

// Configuração
var connectionString = ResolveConnectionString(builder.Configuration, builder.Environment);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada. Defina a connection string ou use a variável de ambiente CINETRACK_SQL_CONNECTION.");
}

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var swaggerEnabled = builder.Configuration.GetValue("Swagger:Enabled", builder.Environment.IsDevelopment());

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SceneSourceScraperOptions>(builder.Configuration.GetSection("Scraping:SceneSource"));
builder.Services.Configure<ImdbScraperOptions>(builder.Configuration.GetSection("ImdbDatasets"));

// Repositories
builder.Services.AddScoped<IMidiaRepository>(_ => new MidiaRepository(connectionString));
builder.Services.AddScoped<ITemporadaRepository>(_ => new TemporadaRepository(connectionString));
builder.Services.AddScoped<IEpisodioRepository>(_ => new EpisodioRepository(connectionString));
builder.Services.AddScoped<IUsuarioRepository>(_ => new UsuarioRepository(connectionString));
builder.Services.AddScoped<ISyncRepository>(_ => new SyncRepository(connectionString));
builder.Services.AddScoped<ISyncOrchestrator, SyncOrchestrator>();
builder.Services.AddScoped<IImdbDatasetImporter, ImdbDatasetImporter>();
builder.Services.AddScoped<IRottenTomatoesProvider, DisabledRottenTomatoesProvider>();

// Scrapers
builder.Services.AddHttpClient<ISceneSourceScraper, SceneSourceScraper>();
builder.Services.AddHttpClient<IImdbScraper, ImdbScraper>();
builder.Services.AddRequiredTextTranslation(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    {
                        return false;
                    }

                    return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                        || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
                })
                .AllowAnyMethod()
                .AllowAnyHeader();

            return;
        }

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    app.Logger.LogWarning("Nenhuma origem CORS foi configurada para ambiente não-desenvolvimento. Requisições cross-origin ficarão bloqueadas.");
}

app.UseCors(CorsPolicyName);
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();
app.MapControllers();

// Health check
app.MapGet("/health", () => "OK");

app.Run();

static string? ResolveConnectionString(IConfiguration configuration, IHostEnvironment environment)
{
    var configuredConnection = configuration.GetConnectionString("DefaultConnection");
    if (HasConfiguredValue(configuredConnection))
    {
        return configuredConnection;
    }

    var environmentConnection = configuration["CINETRACK_SQL_CONNECTION"];
    if (HasConfiguredValue(environmentConnection))
    {
        return environmentConnection;
    }

    if (environment.IsDevelopment())
    {
        return "Server=(localdb)\\MSSQLLocalDB;Database=CineTrackDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    }

    return null;
}

static bool HasConfiguredValue(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    return !value.Trim().StartsWith("__CONFIGURE_", StringComparison.OrdinalIgnoreCase);
}
