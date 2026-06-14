using CineTrack.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CineTrack.Shared.Data;

public interface ITemporadaRepository
{
    Task<IEnumerable<TemporadaCompletaVM>> GetByMidiaIdAsync(int midiaId);
    Task<TemporadaCompletaVM?> GetByIdAsync(int id);
    Task<Temporada?> GetByMidiaAndNumeroAsync(int midiaId, int numero);
    Task<int> InsertAsync(Temporada temporada);
    Task UpdateAsync(Temporada temporada);
}

public class TemporadaRepository : ITemporadaRepository
{
    private readonly string _connectionString;

    public TemporadaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<TemporadaCompletaVM>> GetByMidiaIdAsync(int midiaId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<TemporadaCompletaVM>(
            "SELECT * FROM ViewTemporadaCompleta WHERE MidiaId = @MidiaId ORDER BY Numero",
            new { MidiaId = midiaId });
    }

    public async Task<TemporadaCompletaVM?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<TemporadaCompletaVM>(
            "SELECT * FROM ViewTemporadaCompleta WHERE Id = @Id", new { Id = id });
    }

    public async Task<Temporada?> GetByMidiaAndNumeroAsync(int midiaId, int numero)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Temporada>(
            "SELECT * FROM Temporada WHERE MidiaId = @MidiaId AND Numero = @Numero",
            new { MidiaId = midiaId, Numero = numero });
    }

    public async Task<int> InsertAsync(Temporada temporada)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(
            @"INSERT INTO Temporada (MidiaId, Numero, Titulo, Ano, TotalEpisodios)
              VALUES (@MidiaId, @Numero, @Titulo, @Ano, @TotalEpisodios);
              SELECT CAST(SCOPE_IDENTITY() as int)", temporada);
    }

    public async Task UpdateAsync(Temporada temporada)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE Temporada SET Titulo = @Titulo, Ano = @Ano, TotalEpisodios = @TotalEpisodios
              WHERE Id = @Id", temporada);
    }
}
