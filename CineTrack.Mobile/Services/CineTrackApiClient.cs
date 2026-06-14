using CineTrack.Shared.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CineTrack.Mobile.Services;

public class CineTrackApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public CineTrackApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // ==================== MÍDIAS ====================

    public async Task<IEnumerable<MidiaCompletaVM>> GetMidiasAsync(StatusMidia? status = null, TipoMidia? tipo = null)
    {
        var url = "api/midias";
        var query = new List<string>();
        if (status.HasValue) query.Add($"status={(int)status}");
        if (tipo.HasValue) query.Add($"tipo={(int)tipo}");
        if (query.Any()) url += "?" + string.Join("&", query);
        return await GetAsync<IEnumerable<MidiaCompletaVM>>(url) ?? [];
    }

    public async Task<MidiaCompletaVM?> GetMidiaByIdAsync(int id) =>
        await GetAsync<MidiaCompletaVM>($"api/midias/{id}");

    public async Task<IEnumerable<MidiaCompletaVM>> SearchMidiasAsync(string termo, StatusMidia? status = null, TipoMidia? tipo = null)
    {
        var query = new List<string> { $"termo={Uri.EscapeDataString(termo)}" };
        if (status.HasValue) query.Add($"status={(int)status.Value}");
        if (tipo.HasValue) query.Add($"tipo={(int)tipo.Value}");

        return await GetAsync<IEnumerable<MidiaCompletaVM>>($"api/midias/search?{string.Join("&", query)}") ?? [];
    }

    public async Task<IEnumerable<MidiaCompletaVM>> GetSugestoesAsync(int quantidade = 10) =>
        await GetAsync<IEnumerable<MidiaCompletaVM>>($"api/midias/sugestoes?quantidade={quantidade}") ?? [];

    public async Task<IEnumerable<MidiaCompletaVM>> GetEmAndamentoAsync() =>
        await GetAsync<IEnumerable<MidiaCompletaVM>>("api/midias/em-andamento") ?? [];

    public async Task<IEnumerable<MidiaCompletaVM>> GetRecentesAsync(int quantidade = 10) =>
        await GetAsync<IEnumerable<MidiaCompletaVM>>($"api/midias/recentes?quantidade={quantidade}") ?? [];

    public async Task<IEnumerable<TemporadaCompletaVM>> GetTemporadasAsync(int midiaId) =>
        await GetAsync<IEnumerable<TemporadaCompletaVM>>($"api/midias/{midiaId}/temporadas") ?? [];

    public async Task<IEnumerable<EpisodioCompletoVM>> GetEpisodiosAsync(int midiaId) =>
        await GetAsync<IEnumerable<EpisodioCompletoVM>>($"api/midias/{midiaId}/episodios") ?? [];

    // ==================== USUÁRIO ====================

    public async Task UpdateStatusAsync(int midiaId, StatusMidia status) =>
        await PutAsync($"api/usuario/midia/{midiaId}/status", new UpdateStatusRequest { Status = (int)status });

    public async Task UpdateNotaAsync(int midiaId, int estrelas, decimal? nota = null, string? comentario = null) =>
        await PutAsync($"api/usuario/midia/{midiaId}/nota", new UpdateNotaRequest { Estrelas = estrelas, Nota = nota, Comentario = comentario });

    public async Task MarcarEpisodioAssistidoAsync(int episodioId, AssistindoCom? assistindoCom = null, string? comentario = null) =>
        await PostAsync($"api/usuario/episodio/{episodioId}/assistido", new MarcarAssistidoRequest { AssistindoCom = (int?)assistindoCom, Comentario = comentario });

    public async Task DesmarcarEpisodioAsync(int episodioId) =>
        await DeleteAsync($"api/usuario/episodio/{episodioId}");

    public async Task MarcarTemporadaAssistidaAsync(int temporadaId, AssistindoCom? assistindoCom = null) =>
        await PostAsync($"api/usuario/temporada/{temporadaId}/assistido", new MarcarAssistidoRequest { AssistindoCom = (int?)assistindoCom });

    public async Task DesmarcarTemporadaAsync(int temporadaId) =>
        await DeleteAsync($"api/usuario/temporada/{temporadaId}");

    // ==================== ESTATÍSTICAS ====================

    public async Task<EstatisticasVM> GetEstatisticasAsync() =>
        await GetAsync<EstatisticasVM>("api/estatisticas") ?? new EstatisticasVM();

    public async Task<IEnumerable<GeneroStatVM>> GetGenerosAsync() =>
        await GetAsync<IEnumerable<GeneroStatVM>>("api/estatisticas/generos") ?? [];

    public async Task<IEnumerable<AtividadeMensalVM>> GetAtividadeMensalAsync() =>
        await GetAsync<IEnumerable<AtividadeMensalVM>>("api/estatisticas/atividade") ?? [];

    // ==================== SYNC ====================

    public async Task<IEnumerable<AssociacaoPendenteVM>> GetAssociacoesPendentesAsync() =>
        await GetAsync<IEnumerable<AssociacaoPendenteVM>>("api/sync/associacoes/pendentes") ?? [];

    public async Task<IEnumerable<FonteSyncStatus>> GetFontesSyncAsync() =>
        await GetAsync<IEnumerable<FonteSyncStatus>>("api/sync/fontes") ?? [];

    public async Task<IEnumerable<AssociacaoPendenteVM>> GetAssociacoesResolvidasAsync(int quantidade = 100) =>
        await GetAsync<IEnumerable<AssociacaoPendenteVM>>($"api/sync/associacoes/resolvidas?quantidade={quantidade}") ?? [];

    public async Task ConfirmarAssociacaoAsync(int id) =>
        await PostAsync($"api/sync/associacoes/{id}/confirmar", null);

    public async Task SelecionarAssociacaoAsync(int id) =>
        await PostAsync($"api/sync/associacoes/{id}/selecionar", null);

    public async Task RejeitarAssociacaoAsync(int id) =>
        await PostAsync($"api/sync/associacoes/{id}/rejeitar", null);

    public async Task<IEnumerable<LogCaptura>> GetLogsAsync(int quantidade = 50) =>
        await GetAsync<IEnumerable<LogCaptura>>($"api/sync/logs?quantidade={quantidade}") ?? [];

    public async Task<IEnumerable<DiagnosticoParserVM>> GetLogsDiagnosticoAsync(int quantidade = 20) =>
        await GetAsync<IEnumerable<DiagnosticoParserVM>>($"api/sync/diagnostico?quantidade={quantidade}") ?? [];

    public async Task<DateTime?> GetUltimaSyncAsync()
    {
        var result = await GetAsync<UltimaSyncResponse>("api/sync/ultima-sync");
        return result?.UltimaSync;
    }

    public async Task<ScrapingResult?> ExecutarScrapingAsync() =>
        await PostAsync<ScrapingResult>("api/sync/scraping/executar", null);

    public async Task<RottenTomatoesRatingsResult?> ReprocessarRatingsAsync(int quantidade = 100, bool forcar = false) =>
        await PostAsync<RottenTomatoesRatingsResult>("api/sync/ratings/reprocessar", new RatingsReprocessRequest { Quantidade = quantidade, Forcar = forcar });

    // ==================== HEALTH ====================

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("health");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ==================== HTTP HELPERS ====================

    private async Task<T?> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    private async Task PostAsync(string url, object? data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions);
        await EnsureSuccessAsync(response);
    }

    private async Task<T?> PostAsync<T>(string url, object? data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    private async Task PutAsync(string url, object data)
    {
        var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions);
        await EnsureSuccessAsync(response);
    }

    private async Task DeleteAsync(string url)
    {
        var response = await _httpClient.DeleteAsync(url);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(body)
            ? $"Erro HTTP {(int)response.StatusCode} ({response.StatusCode})."
            : body;

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException("Recurso não encontrado.");
        }

        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private class UltimaSyncResponse { public DateTime? UltimaSync { get; set; } }
}
