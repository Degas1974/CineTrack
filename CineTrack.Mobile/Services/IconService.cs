using Microsoft.JSInterop;

namespace CineTrack.Mobile.Services;

public sealed class IconService
{
    private readonly IJSRuntime _js;
    private readonly Dictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

    public IconService(IJSRuntime js) => _js = js;

    public async ValueTask<string> GetAsync(string name)
    {
        if (_cache.TryGetValue(name, out var hit))
        {
            return hit;
        }

        var svg = await _js.InvokeAsync<string>("cineTrackIcons.get", name);
        _cache[name] = svg ?? string.Empty;
        return _cache[name];
    }
}
