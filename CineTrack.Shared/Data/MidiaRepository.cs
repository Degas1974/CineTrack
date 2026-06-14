using CineTrack.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CineTrack.Shared.Data;

public interface IMidiaRepository
{
    Task<IEnumerable<MidiaCompletaVM>> GetAllAsync(StatusMidia? status = null, TipoMidia? tipo = null);
    Task<MidiaCompletaVM?> GetByIdAsync(int id);
    Task<IEnumerable<MidiaCompletaVM>> SearchAsync(string termo, StatusMidia? status = null, TipoMidia? tipo = null);
    Task<IEnumerable<MidiaCompletaVM>> GetSugestoesAsync(int quantidade = 10);
    Task<IEnumerable<MidiaCompletaVM>> GetEmAndamentoAsync();
    Task<IEnumerable<MidiaCompletaVM>> GetRecentesAsync(int quantidade = 10);
    Task<Midia?> GetByImdbIdAsync(string imdbId);
    Task<IEnumerable<Midia>> FindByTitleAsync(string titulo, int? ano = null, int quantidade = 10);
    Task<IEnumerable<Midia>> GetMidiasSemPosterAsync(int quantidade = 100);
    Task<int> InsertAsync(Midia midia);
    Task UpdateAsync(Midia midia);
}

public class MidiaRepository : IMidiaRepository
{
    private readonly string _connectionString;

    public MidiaRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<MidiaCompletaVM>> GetAllAsync(StatusMidia? status = null, TipoMidia? tipo = null)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = "SELECT * FROM ViewMidiaCompleta WHERE 1=1";
        var parameters = new DynamicParameters();

        if (status.HasValue)
        {
            sql += " AND StatusUsuario = @Status";
            parameters.Add("Status", (int)status.Value);
        }

        if (tipo.HasValue)
        {
            sql += " AND Tipo = @Tipo";
            parameters.Add("Tipo", (int)tipo.Value);
        }
        
        sql += " ORDER BY Titulo";
        
        return await connection.QueryAsync<MidiaCompletaVM>(sql, parameters);
    }

    public async Task<MidiaCompletaVM?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<MidiaCompletaVM>(
            "SELECT * FROM ViewMidiaCompleta WHERE Id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<MidiaCompletaVM>> SearchAsync(string termo, StatusMidia? status = null, TipoMidia? tipo = null)
    {
        using var connection = new SqlConnection(_connectionString);
        var sql = @"SELECT * FROM ViewMidiaCompleta 
              WHERE (Titulo LIKE @Termo OR TituloOriginal LIKE @Termo)";
        var parameters = new DynamicParameters();
        parameters.Add("Termo", $"%{termo}%");

        if (status.HasValue)
        {
            sql += " AND StatusUsuario = @Status";
            parameters.Add("Status", (int)status.Value);
        }

        if (tipo.HasValue)
        {
            sql += " AND Tipo = @Tipo";
            parameters.Add("Tipo", (int)tipo.Value);
        }

        sql += " ORDER BY Titulo";

        return await connection.QueryAsync<MidiaCompletaVM>(
            sql,
            parameters);
    }

    public async Task<IEnumerable<MidiaCompletaVM>> GetSugestoesAsync(int quantidade = 10)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<MidiaCompletaVM>(
            @"SELECT TOP (@Quantidade) * FROM ViewMidiaCompleta 
              WHERE StatusUsuario = 0 AND ImdbRating >= 7.0 
              ORDER BY ImdbRating DESC, ImdbVotes DESC",
            new { Quantidade = quantidade });
    }

    public async Task<IEnumerable<MidiaCompletaVM>> GetEmAndamentoAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<MidiaCompletaVM>(
            "SELECT * FROM ViewMidiaCompleta WHERE StatusUsuario = 1 ORDER BY DataAtualizacao DESC");
    }

    public async Task<IEnumerable<MidiaCompletaVM>> GetRecentesAsync(int quantidade = 10)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<MidiaCompletaVM>(
                        @"SELECT TOP (@Quantidade) * FROM ViewMidiaCompleta
                            WHERE StatusUsuario = @StatusAssistido
                            ORDER BY COALESCE(DataFim, DataAtualizacao, DataCriacao) DESC",
                        new { Quantidade = quantidade, StatusAssistido = (int)StatusMidia.Assistido });
    }

    public async Task<Midia?> GetByImdbIdAsync(string imdbId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<Midia>(
            "SELECT * FROM Midia WHERE ImdbId = @ImdbId", new { ImdbId = imdbId });
    }

    public async Task<IEnumerable<Midia>> FindByTitleAsync(string titulo, int? ano = null, int quantidade = 10)
    {
        using var connection = new SqlConnection(_connectionString);
        var parameters = new DynamicParameters();
        parameters.Add("Termo", $"%{titulo}%");
        parameters.Add("Quantidade", Math.Max(1, quantidade));
        parameters.Add("Ano", ano);

        var sql = @"SELECT TOP (@Quantidade) *
                    FROM Midia
                    WHERE Ativo = 1
                      AND (Titulo LIKE @Termo OR TituloOriginal LIKE @Termo)
                    ORDER BY
                        CASE WHEN Ano = @Ano THEN 0 ELSE 1 END,
                        ImdbRating DESC,
                        ImdbVotes DESC";

        return await connection.QueryAsync<Midia>(sql, parameters);
    }

        public async Task<IEnumerable<Midia>> GetMidiasSemPosterAsync(int quantidade = 100)
        {
                using var connection = new SqlConnection(_connectionString);
                return await connection.QueryAsync<Midia>(
                        @"SELECT TOP (@Quantidade) *
                            FROM Midia
                            WHERE ImdbId IS NOT NULL
                                AND LTRIM(RTRIM(ImdbId)) <> ''
                                AND (ImagemUrl IS NULL OR LTRIM(RTRIM(ImagemUrl)) = '')
                            ORDER BY DataAtualizacao DESC, DataCriacao DESC, Id DESC",
                        new { Quantidade = quantidade });
        }

    public async Task<int> InsertAsync(Midia midia)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(
            @"INSERT INTO Midia (Titulo, TituloOriginal, Tipo, Ano, Descricao, ImagemUrl, ImdbId, 
                ImdbRating, ImdbVotes, Tomatometer, Popcornmeter, RottenTomatoesUrl, Generos, 
                Duracao, Diretor, Elenco, Ativo, DataCriacao)
              VALUES (@Titulo, @TituloOriginal, @Tipo, @Ano, @Descricao, @ImagemUrl, @ImdbId, 
                @ImdbRating, @ImdbVotes, @Tomatometer, @Popcornmeter, @RottenTomatoesUrl, @Generos, 
                @Duracao, @Diretor, @Elenco, @Ativo, @DataCriacao);
              SELECT CAST(SCOPE_IDENTITY() as int)", midia);
    }

    public async Task UpdateAsync(Midia midia)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"UPDATE Midia SET Titulo = @Titulo, TituloOriginal = @TituloOriginal, Tipo = @Tipo, 
                Ano = @Ano, Descricao = @Descricao, ImagemUrl = @ImagemUrl, ImdbId = @ImdbId, 
                ImdbRating = @ImdbRating, ImdbVotes = @ImdbVotes, Tomatometer = @Tomatometer, 
                Popcornmeter = @Popcornmeter, RottenTomatoesUrl = @RottenTomatoesUrl, Generos = @Generos, 
                Duracao = @Duracao, Diretor = @Diretor, Elenco = @Elenco, DataAtualizacao = GETDATE()
              WHERE Id = @Id", midia);
    }
}
