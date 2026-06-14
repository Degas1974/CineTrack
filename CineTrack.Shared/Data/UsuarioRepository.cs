using CineTrack.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CineTrack.Shared.Data;

public interface IUsuarioRepository
{
    Task UpdateStatusMidiaAsync(int midiaId, StatusMidia status);
    Task UpdateNotaMidiaAsync(int midiaId, int estrelas, decimal? nota, string? comentario);
    Task MarcarEpisodioAssistidoAsync(int episodioId, AssistindoCom? assistindoCom, string? comentario);
    Task DesmarcarEpisodioAsync(int episodioId);
    Task MarcarTemporadaAssistidaAsync(int temporadaId, AssistindoCom? assistindoCom);
    Task DesmarcarTemporadaAsync(int temporadaId);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task UpdateStatusMidiaAsync(int midiaId, StatusMidia status)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var exists = await connection.ExecuteScalarAsync<bool>(
            "SELECT COUNT(1) FROM UsuarioMidia WHERE MidiaId = @MidiaId", new { MidiaId = midiaId });

        if (exists)
        {
            await connection.ExecuteAsync(
                @"UPDATE UsuarioMidia SET Status = @Status, DataAtualizacao = GETDATE(),
                  DataInicio = CASE WHEN @Status = 1 AND DataInicio IS NULL THEN GETDATE() ELSE DataInicio END,
                  DataFim = CASE WHEN @Status = 2 THEN GETDATE() ELSE DataFim END
                  WHERE MidiaId = @MidiaId",
                new { MidiaId = midiaId, Status = (int)status });
        }
        else
        {
            await connection.ExecuteAsync(
                @"INSERT INTO UsuarioMidia (MidiaId, Status, DataInicio, DataAtualizacao)
                  VALUES (@MidiaId, @Status, CASE WHEN @Status = 1 THEN GETDATE() ELSE NULL END, GETDATE())",
                new { MidiaId = midiaId, Status = (int)status });
        }
    }

    public async Task UpdateNotaMidiaAsync(int midiaId, int estrelas, decimal? nota, string? comentario)
    {
        using var connection = new SqlConnection(_connectionString);
        
        var exists = await connection.ExecuteScalarAsync<bool>(
            "SELECT COUNT(1) FROM UsuarioMidia WHERE MidiaId = @MidiaId", new { MidiaId = midiaId });

        if (exists)
        {
            await connection.ExecuteAsync(
                @"UPDATE UsuarioMidia SET Estrelas = @Estrelas, Nota = @Nota, Comentario = @Comentario, 
                  DataAtualizacao = GETDATE() WHERE MidiaId = @MidiaId",
                new { MidiaId = midiaId, Estrelas = estrelas, Nota = nota, Comentario = comentario });
        }
        else
        {
            await connection.ExecuteAsync(
                @"INSERT INTO UsuarioMidia (MidiaId, Status, Estrelas, Nota, Comentario, DataAtualizacao)
                  VALUES (@MidiaId, 0, @Estrelas, @Nota, @Comentario, GETDATE())",
                new { MidiaId = midiaId, Estrelas = estrelas, Nota = nota, Comentario = comentario });
        }
    }

    public async Task MarcarEpisodioAssistidoAsync(int episodioId, AssistindoCom? assistindoCom, string? comentario)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("EXEC sp_MarcarEpisodioAssistido @EpisodioId, @AssistindoCom, @Comentario",
            new { EpisodioId = episodioId, AssistindoCom = (int?)assistindoCom, Comentario = comentario });
    }

    public async Task DesmarcarEpisodioAsync(int episodioId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            "DELETE FROM UsuarioEpisodio WHERE EpisodioId = @EpisodioId", new { EpisodioId = episodioId });
    }

    public async Task MarcarTemporadaAssistidaAsync(int temporadaId, AssistindoCom? assistindoCom)
    {
        using var connection = new SqlConnection(_connectionString);
        var episodios = await connection.QueryAsync<int>(
            "SELECT Id FROM Episodio WHERE TemporadaId = @TemporadaId", new { TemporadaId = temporadaId });

        foreach (var episodioId in episodios)
        {
            await MarcarEpisodioAssistidoAsync(episodioId, assistindoCom, null);
        }
    }

    public async Task DesmarcarTemporadaAsync(int temporadaId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"DELETE ue FROM UsuarioEpisodio ue
              INNER JOIN Episodio e ON ue.EpisodioId = e.Id
              WHERE e.TemporadaId = @TemporadaId", new { TemporadaId = temporadaId });
    }
}
