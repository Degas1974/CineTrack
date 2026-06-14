using CineTrack.Shared.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CineTrack.Shared.Data;

public interface ISyncRepository
{
    Task<IEnumerable<AssociacaoPendenteVM>> GetAssociacoesPendentesAsync();
    Task<IEnumerable<AssociacaoPendenteVM>> GetAssociacoesResolvidasAsync(int quantidade = 100);
    Task ConfirmarAssociacaoAsync(int id);
    Task SelecionarAssociacaoAsync(int id);
    Task RejeitarAssociacaoAsync(int id);
    Task<bool> AssociacaoJaRegistradaAsync(AssociacaoSceneSource associacao);
    Task<decimal> GetConfiancaMinimaAutoAssociacaoAsync();
    Task<int> InsertAssociacaoAsync(AssociacaoSceneSource associacao);
    Task<IEnumerable<LogCaptura>> GetLogsAsync(int quantidade = 50);
    Task InsertLogAsync(LogCaptura log);
    Task<DateTime?> GetUltimaSyncAsync();
    Task<DateTime?> GetUltimaSyncFonteAsync(string fonte);
    Task SetUltimaSyncAsync(DateTime data);
    Task SetUltimaSyncFonteAsync(string fonte, DateTime data);
    Task<EstatisticasVM> GetEstatisticasAsync();
    Task<IEnumerable<GeneroStatVM>> GetGenerosAsync();
    Task<IEnumerable<AtividadeMensalVM>> GetAtividadeMensalAsync();
    Task<IEnumerable<FonteSyncStatus>> GetFontesStatusAsync();
}

public class SyncRepository : ISyncRepository
{
    private readonly string _connectionString;

    public SyncRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<AssociacaoPendenteVM>> GetAssociacoesPendentesAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<AssociacaoPendenteVM>(
            "SELECT * FROM ViewAssociacoesPendentes ORDER BY DataCaptura DESC");
    }

    public async Task<IEnumerable<AssociacaoPendenteVM>> GetAssociacoesResolvidasAsync(int quantidade = 100)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<AssociacaoPendenteVM>(
            @"SELECT TOP (@Quantidade) *
              FROM ViewAssociacoesResolvidas
              ORDER BY ISNULL(DataConfirmacao, DataCaptura) DESC, ReleaseScore DESC, DataCaptura DESC",
            new { Quantidade = quantidade });
    }

    public async Task ConfirmarAssociacaoAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("EXEC sp_ConfirmarAssociacao @AssociacaoId", new { AssociacaoId = id });
    }

    public async Task SelecionarAssociacaoAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("EXEC sp_SelecionarAssociacao @AssociacaoId", new { AssociacaoId = id });
    }

    public async Task RejeitarAssociacaoAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("EXEC sp_RejeitarAssociacao @AssociacaoId", new { AssociacaoId = id });
    }

    public async Task<bool> AssociacaoJaRegistradaAsync(AssociacaoSceneSource associacao)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<bool>(
            @"SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM AssociacaoSceneSource
                    WHERE Status IN (@StatusPendente, @StatusConfirmado)
                      AND (
                          (LinkSceneSource IS NOT NULL AND LinkSceneSource = @LinkSceneSource)
                          OR (
                              @TituloBrutoCapturado IS NOT NULL
                              AND TituloBrutoCapturado = @TituloBrutoCapturado
                          )
                          OR (
                              @TituloBrutoCapturado IS NULL
                              AND
                              TituloCapturado = @TituloCapturado
                              AND ISNULL(AnoCapturado, -1) = ISNULL(@AnoCapturado, -1)
                              AND ISNULL(TemporadaCapturada, -1) = ISNULL(@TemporadaCapturada, -1)
                              AND ISNULL(EpisodioCapturado, -1) = ISNULL(@EpisodioCapturado, -1)
                          )
                      )
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
            new
            {
                associacao.LinkSceneSource,
                associacao.TituloBrutoCapturado,
                associacao.TituloCapturado,
                associacao.AnoCapturado,
                associacao.TemporadaCapturada,
                associacao.EpisodioCapturado,
                StatusPendente = (int)StatusAssociacao.Pendente,
                StatusConfirmado = (int)StatusAssociacao.Confirmado
            });
    }

    public async Task<decimal> GetConfiancaMinimaAutoAssociacaoAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var valor = await connection.ExecuteScalarAsync<string?>(
            "SELECT Valor FROM Configuracao WHERE Chave = 'ConfiancaMinimaAutoAssociacao'");

        return decimal.TryParse(valor, out var confiancaMinima)
            ? confiancaMinima
            : 85m;
    }

    public async Task<int> InsertAssociacaoAsync(AssociacaoSceneSource associacao)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleAsync<int>(
                        @"INSERT INTO AssociacaoSceneSource (TituloCapturado, TituloBrutoCapturado, AnoCapturado, TemporadaCapturada, 
                                EpisodioCapturado, LinkSceneSource, CategoriaCapturada, QualidadeCapturada, FonteReleaseCapturada,
                                CodecCapturado, ProviderCapturado, GrupoReleaseCapturado, ChaveAgrupamento, ReleaseScore,
                                ImdbIdCapturado, MidiaId, EpisodioId, Confianca, Status, DataCaptura)
                            VALUES (@TituloCapturado, @TituloBrutoCapturado, @AnoCapturado, @TemporadaCapturada, @EpisodioCapturado, 
                                @LinkSceneSource, @CategoriaCapturada, @QualidadeCapturada, @FonteReleaseCapturada,
                                @CodecCapturado, @ProviderCapturado, @GrupoReleaseCapturado, @ChaveAgrupamento, @ReleaseScore,
                                @ImdbIdCapturado, @MidiaId, @EpisodioId, @Confianca, @Status, @DataCaptura);
              SELECT CAST(SCOPE_IDENTITY() as int)", associacao);
    }

    public async Task<IEnumerable<LogCaptura>> GetLogsAsync(int quantidade = 50)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<LogCaptura>(
            "SELECT TOP (@Quantidade) * FROM LogCaptura ORDER BY DataLog DESC",
            new { Quantidade = quantidade });
    }

    public async Task InsertLogAsync(LogCaptura log)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"INSERT INTO LogCaptura (Fonte, Tipo, Mensagem, Detalhes, DataLog)
              VALUES (@Fonte, @Tipo, @Mensagem, @Detalhes, @DataLog)", log);
    }

    public async Task<DateTime?> GetUltimaSyncAsync()
    {
        return await GetUltimaSyncFonteAsync("SceneSource");
    }

    public async Task<DateTime?> GetUltimaSyncFonteAsync(string fonte)
    {
        using var connection = new SqlConnection(_connectionString);
        var chave = $"UltimaSync{fonte}";
        var valor = await connection.ExecuteScalarAsync<string?>(
            "SELECT Valor FROM Configuracao WHERE Chave = @Chave",
            new { Chave = chave });

        return string.IsNullOrWhiteSpace(valor) ? null : DateTime.Parse(valor);
    }

    public async Task SetUltimaSyncAsync(DateTime data)
    {
        await SetUltimaSyncFonteAsync("SceneSource", data);
    }

    public async Task SetUltimaSyncFonteAsync(string fonte, DateTime data)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(
            @"IF EXISTS (SELECT 1 FROM Configuracao WHERE Chave = @Chave)
                  UPDATE Configuracao SET Valor = @Valor WHERE Chave = @Chave
              ELSE
                  INSERT INTO Configuracao (Chave, Valor, Descricao)
                  VALUES (@Chave, @Valor, @Descricao)",
            new
            {
                Chave = $"UltimaSync{fonte}",
                Valor = data.ToString("yyyy-MM-dd HH:mm:ss"),
                Descricao = $"Última sincronização da fonte {fonte}"
            });
    }

    public async Task<EstatisticasVM> GetEstatisticasAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<EstatisticasVM>(
            "SELECT * FROM ViewEstatisticas") ?? new EstatisticasVM();
    }

    public async Task<IEnumerable<GeneroStatVM>> GetGenerosAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var rows = await connection.QueryAsync<(string Nome, int Contagem)>(
            @"SELECT TRIM(g.value) AS Nome, COUNT(*) AS Contagem
              FROM Midia m
              INNER JOIN UsuarioMidia um ON m.Id = um.MidiaId
              CROSS APPLY STRING_SPLIT(m.Generos, ',') g
              WHERE m.Ativo = 1
                AND um.Status IN (1, 2)
                AND LEN(TRIM(ISNULL(m.Generos, ''))) > 0
                AND LEN(TRIM(g.value)) > 0
              GROUP BY TRIM(g.value)
              ORDER BY Contagem DESC");

        var list = rows.ToList();
        var total = list.Sum(x => x.Contagem);
        return list.Select(x => new GeneroStatVM
        {
            Nome = x.Nome,
            Contagem = x.Contagem,
            Pct = total > 0 ? Math.Round((decimal)x.Contagem / total * 100, 1) : 0
        });
    }

    private static readonly string[] _mesesPt = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"];

    public async Task<IEnumerable<AtividadeMensalVM>> GetAtividadeMensalAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        var inicioJanela = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11);

        var rows = await connection.QueryAsync<(int Ano, int Mes, int Contagem)>(
            @"SELECT Ano, Mes, SUM(Contagem) AS Contagem FROM (
                SELECT YEAR(ue.DataAssistido) AS Ano, MONTH(ue.DataAssistido) AS Mes, COUNT(*) AS Contagem
                FROM UsuarioEpisodio ue
                WHERE ue.Status = 1 AND ue.DataAssistido >= @Inicio
                GROUP BY YEAR(ue.DataAssistido), MONTH(ue.DataAssistido)
                UNION ALL
                SELECT YEAR(um.DataFim) AS Ano, MONTH(um.DataFim) AS Mes, COUNT(*) AS Contagem
                FROM UsuarioMidia um
                INNER JOIN Midia m ON um.MidiaId = m.Id
                WHERE um.Status = 2 AND m.Tipo = 0 AND um.DataFim >= @Inicio
                GROUP BY YEAR(um.DataFim), MONTH(um.DataFim)
              ) t GROUP BY Ano, Mes ORDER BY Ano, Mes",
            new { Inicio = inicioJanela });

        var porMes = rows.ToDictionary(x => (x.Ano, x.Mes), x => x.Contagem);

        var result = new List<AtividadeMensalVM>(12);
        for (int i = 11; i >= 0; i--)
        {
            var d = DateTime.Today.AddMonths(-i);
            d = new DateTime(d.Year, d.Month, 1);
            porMes.TryGetValue((d.Year, d.Month), out var cnt);
            result.Add(new AtividadeMensalVM { M = _mesesPt[d.Month - 1], V = cnt });
        }
        return result;
    }

    public async Task<IEnumerable<FonteSyncStatus>> GetFontesStatusAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var configuracoes = (await connection.QueryAsync<Configuracao>(
            "SELECT * FROM Configuracao WHERE Chave LIKE 'UltimaSync%'"))
            .ToDictionary(x => x.Chave, x => x.Valor, StringComparer.OrdinalIgnoreCase);

        var pendentes = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM AssociacaoSceneSource WHERE Status = @Status",
            new { Status = (int)StatusAssociacao.Pendente });

        var errosRecentes = await connection.QueryAsync<FonteErroCount>(
            @"SELECT Fonte, COUNT(*) AS Total
              FROM LogCaptura
              WHERE Tipo = @TipoErro AND DataLog >= DATEADD(day, -7, GETDATE())
              GROUP BY Fonte",
            new { TipoErro = (int)TipoLog.Erro });

        var errosPorFonte = errosRecentes.ToDictionary(x => (FonteCaptura)x.Fonte, x => x.Total);

        return
        [
            BuildStatus("SceneSource", enabled: true, configured: true, configuracoes, pendentes, errosPorFonte.GetValueOrDefault(FonteCaptura.SceneSource)),
            BuildStatus("IMDb", enabled: true, configured: true, configuracoes, null, errosPorFonte.GetValueOrDefault(FonteCaptura.IMDb)),
            BuildStatus("RottenTomatoes", enabled: false, configured: false, configuracoes, null, errosPorFonte.GetValueOrDefault(FonteCaptura.RottenTomatoes),
                "Provider desabilitado/manual até configurar uma API licenciada.")
        ];
    }

    private sealed class FonteErroCount
    {
        public int Fonte { get; set; }
        public int Total { get; set; }
    }

    private static FonteSyncStatus BuildStatus(
        string fonte,
        bool enabled,
        bool configured,
        IDictionary<string, string?> configuracoes,
        int? pendentes,
        int errosRecentes,
        string? mensagem = null)
    {
        var ultima = configuracoes.TryGetValue($"UltimaSync{fonte}", out var valor)
            && DateTime.TryParse(valor, out var parsed)
                ? parsed
                : (DateTime?)null;

        return new FonteSyncStatus
        {
            Fonte = fonte,
            Habilitada = enabled,
            Configurada = configured,
            Status = enabled && configured ? "OK" : "Desabilitada",
            Mensagem = mensagem,
            UltimaSync = ultima,
            ItensPendentes = pendentes,
            ErrosRecentes = errosRecentes
        };
    }
}
