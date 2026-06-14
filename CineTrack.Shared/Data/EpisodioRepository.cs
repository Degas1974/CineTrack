using CineTrack.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CineTrack.Shared.Data;

public interface IEpisodioRepository
{
    Task<IEnumerable<EpisodioCompletoVM>> GetByMidiaIdAsync(int midiaId);
    Task<IEnumerable<EpisodioCompletoVM>> GetByTemporadaIdAsync(int temporadaId);
    Task<EpisodioCompletoVM?> GetByIdAsync(int id);
    Task<Episodio?> GetByTemporadaAndNumeroAsync(int temporadaId, int numero);
    Task<int> InsertAsync(Episodio episodio);
    Task UpdateAsync(Episodio episodio);
}

public class EpisodioRepository : IEpisodioRepository
{
    private readonly string _connectionString;

    public EpisodioRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<EpisodioCompletoVM>> GetByMidiaIdAsync(int midiaId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<EpisodioCompletoVM>(
            "SELECT * FROM ViewEpisodioCompleto WHERE MidiaId = @MidiaId ORDER BY NumeroTemporada, Numero",
            new { MidiaId = midiaId });
    }

    public async Task<IEnumerable<EpisodioCompletoVM>> GetByTemporadaIdAsync(int temporadaId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<EpisodioCompletoVM>(
            "SELECT * FROM ViewEpisodioCompleto WHERE TemporadaId = @TemporadaId ORDER BY Numero",
            new { TemporadaId = temporadaId });
    }

    public async Task<EpisodioCompletoVM?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<EpisodioCompletoVM>(
            "SELECT * FROM ViewEpisodioCompleto WHERE Id = @Id", new { Id = id });
    }

    public async Task<Episodio?> GetByTemporadaAndNumeroAsync(int temporadaId, int numero)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Episodio>(
            "SELECT * FROM Episodio WHERE TemporadaId = @TemporadaId AND Numero = @Numero",
            new { TemporadaId = temporadaId, Numero = numero });
    }

    public async Task<int> InsertAsync(Episodio episodio)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(
            @"INSERT INTO Episodio (TemporadaId, Numero, Titulo, Descricao, Duracao, DataExibicao, ImdbRating)
              VALUES (@TemporadaId, @Numero, @Titulo, @Descricao, @Duracao, @DataExibicao, @ImdbRating);
              SELECT CAST(SCOPE_IDENTITY() as int)", episodio);
    }

    public async Task UpdateAsync(Episodio episodio)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE Episodio SET Titulo = @Titulo, Descricao = @Descricao, Duracao = @Duracao, 
              DataExibicao = @DataExibicao, ImdbRating = @ImdbRating WHERE Id = @Id", episodio);
    }
}
