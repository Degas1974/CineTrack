namespace CineTrack.API.Middleware;

public sealed class ApiKeyMiddleware
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly string? _apiKey;
    private readonly IHostEnvironment _environment;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
        _apiKey = configuration["Security:ApiKey"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HasConfiguredValue(_apiKey) || IsPublicRequest(context))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var receivedKey)
            || !string.Equals(receivedKey.ToString(), _apiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "API key ausente ou inválida." });
            return;
        }

        await _next(context);
    }

    private bool IsPublicRequest(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            return true;
        }

        return _environment.IsDevelopment()
            && (context.Request.Path.StartsWithSegments("/swagger")
                || context.Request.Path.StartsWithSegments("/openapi"));
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
