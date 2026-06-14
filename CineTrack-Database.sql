-- =====================================================
-- CineTrack Database - Script Completo v2
-- =====================================================

USE CineTrackDb;
GO

-- =====================================================
-- TABELAS PRINCIPAIS
-- =====================================================

-- Tabela de Mídias (Filmes e Séries)
CREATE TABLE Midia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(300) NOT NULL,
    TituloOriginal NVARCHAR(300) NULL,
    Tipo INT NOT NULL DEFAULT 0, -- 0=Filme, 1=Serie
    Ano INT NULL,
    Descricao NVARCHAR(MAX) NULL,
    ImagemUrl NVARCHAR(500) NULL,
    ImdbId NVARCHAR(20) NULL,
    ImdbRating DECIMAL(3,1) NULL,
    ImdbVotes INT NULL,
    Tomatometer INT NULL,
    Popcornmeter INT NULL,
    RottenTomatoesUrl NVARCHAR(500) NULL,
    Generos NVARCHAR(200) NULL,
    Duracao INT NULL,
    Diretor NVARCHAR(200) NULL,
    Elenco NVARCHAR(500) NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
    DataAtualizacao DATETIME NULL
);
GO

CREATE INDEX IX_Midia_ImdbId ON Midia(ImdbId);
CREATE INDEX IX_Midia_Tipo ON Midia(Tipo);
CREATE INDEX IX_Midia_Titulo ON Midia(Titulo);
GO

-- Tabela de Temporadas
CREATE TABLE Temporada (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MidiaId INT NOT NULL,
    Numero INT NOT NULL,
    Titulo NVARCHAR(200) NULL,
    Ano INT NULL,
    TotalEpisodios INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Temporada_Midia FOREIGN KEY (MidiaId) 
        REFERENCES Midia(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Temporada_MidiaId ON Temporada(MidiaId);
GO

-- Tabela de Episódios
CREATE TABLE Episodio (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TemporadaId INT NOT NULL,
    Numero INT NOT NULL,
    Titulo NVARCHAR(300) NULL,
    Descricao NVARCHAR(MAX) NULL,
    Duracao INT NULL,
    DataExibicao DATE NULL,
    ImdbRating DECIMAL(3,1) NULL,
    CONSTRAINT FK_Episodio_Temporada FOREIGN KEY (TemporadaId) 
        REFERENCES Temporada(Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Episodio_TemporadaId ON Episodio(TemporadaId);
GO

-- =====================================================
-- TABELAS DE USUÁRIO
-- =====================================================

-- Status do usuário para cada mídia
CREATE TABLE UsuarioMidia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MidiaId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=Pendente, 1=Assistindo, 2=Assistido, 3=Abandonado
    Estrelas INT NULL, -- 1-5
    Nota DECIMAL(3,1) NULL,
    Comentario NVARCHAR(MAX) NULL,
    DataInicio DATETIME NULL,
    DataFim DATETIME NULL,
    DataAtualizacao DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UsuarioMidia_Midia FOREIGN KEY (MidiaId) 
        REFERENCES Midia(Id) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX IX_UsuarioMidia_MidiaId ON UsuarioMidia(MidiaId);
GO

-- Status do usuário para cada episódio
CREATE TABLE UsuarioEpisodio (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EpisodioId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- 0=NaoAssistido, 1=Assistido, 2=Pulado
    AssistindoCom INT NULL, -- 0=Sozinho, 1=Familia, 2=Amigos, 3=Namorada
    DataAssistido DATETIME NULL,
    Comentario NVARCHAR(500) NULL,
    CONSTRAINT FK_UsuarioEpisodio_Episodio FOREIGN KEY (EpisodioId) 
        REFERENCES Episodio(Id) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX IX_UsuarioEpisodio_EpisodioId ON UsuarioEpisodio(EpisodioId);
GO

-- =====================================================
-- TABELAS DE SCRAPING
-- =====================================================

-- Associações do SceneSource (SEM CASCADE para evitar ciclos)
CREATE TABLE AssociacaoSceneSource (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TituloCapturado NVARCHAR(500) NOT NULL,
    TituloBrutoCapturado NVARCHAR(500) NULL,
    AnoCapturado INT NULL,
    TemporadaCapturada INT NULL,
    EpisodioCapturado INT NULL,
    LinkSceneSource NVARCHAR(1000) NULL,
    CategoriaCapturada NVARCHAR(200) NULL,
    QualidadeCapturada NVARCHAR(50) NULL,
    FonteReleaseCapturada NVARCHAR(50) NULL,
    CodecCapturado NVARCHAR(50) NULL,
    ProviderCapturado NVARCHAR(50) NULL,
    GrupoReleaseCapturado NVARCHAR(100) NULL,
    ChaveAgrupamento NVARCHAR(500) NULL,
    ReleaseScore INT NOT NULL DEFAULT 0,
    ImdbIdCapturado NVARCHAR(20) NULL,
    MidiaId INT NULL,
    EpisodioId INT NULL,
    Confianca DECIMAL(5,2) NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0, -- 0=Pendente, 1=Confirmado, 2=Rejeitado
    DataCaptura DATETIME NOT NULL DEFAULT GETDATE(),
    DataConfirmacao DATETIME NULL,
    CONSTRAINT FK_AssociacaoSceneSource_Midia FOREIGN KEY (MidiaId) 
        REFERENCES Midia(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_AssociacaoSceneSource_Episodio FOREIGN KEY (EpisodioId) 
        REFERENCES Episodio(Id) ON DELETE NO ACTION
);
GO

IF COL_LENGTH('AssociacaoSceneSource', 'TituloBrutoCapturado') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD TituloBrutoCapturado NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'CategoriaCapturada') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD CategoriaCapturada NVARCHAR(200) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'QualidadeCapturada') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD QualidadeCapturada NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'FonteReleaseCapturada') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD FonteReleaseCapturada NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'CodecCapturado') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD CodecCapturado NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'ProviderCapturado') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD ProviderCapturado NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'GrupoReleaseCapturado') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD GrupoReleaseCapturado NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'ChaveAgrupamento') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD ChaveAgrupamento NVARCHAR(500) NULL;
END;
GO

IF COL_LENGTH('AssociacaoSceneSource', 'ReleaseScore') IS NULL
BEGIN
    ALTER TABLE AssociacaoSceneSource ADD ReleaseScore INT NOT NULL CONSTRAINT DF_AssociacaoSceneSource_ReleaseScore DEFAULT 0;
END;
GO

CREATE INDEX IX_AssociacaoSceneSource_Status ON AssociacaoSceneSource(Status);
CREATE INDEX IX_AssociacaoSceneSource_MidiaId ON AssociacaoSceneSource(MidiaId);
GO

-- Log de capturas
CREATE TABLE LogCaptura (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Fonte INT NOT NULL, -- 0=SceneSource, 1=IMDb, 2=RottenTomatoes
    Tipo INT NOT NULL, -- 0=Info, 1=Sucesso, 2=Erro, 3=Aviso
    Mensagem NVARCHAR(MAX) NOT NULL,
    Detalhes NVARCHAR(MAX) NULL,
    DataLog DATETIME NOT NULL DEFAULT GETDATE()
);
GO

CREATE INDEX IX_LogCaptura_DataLog ON LogCaptura(DataLog DESC);
CREATE INDEX IX_LogCaptura_Fonte ON LogCaptura(Fonte);
GO

-- Configurações
CREATE TABLE Configuracao (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Chave NVARCHAR(100) NOT NULL UNIQUE,
    Valor NVARCHAR(500) NULL,
    Descricao NVARCHAR(200) NULL
);
GO

-- =====================================================
-- VIEWS
-- =====================================================

-- View de Mídia Completa
CREATE OR ALTER VIEW ViewMidiaCompleta AS
SELECT 
    m.Id,
    m.Titulo,
    m.TituloOriginal,
    m.Tipo,
    CASE m.Tipo WHEN 0 THEN 'Filme' ELSE 'Série' END AS TipoDescricao,
    m.Ano,
    m.Descricao,
    m.ImagemUrl,
    m.ImdbId,
    m.ImdbRating,
    m.ImdbVotes,
    m.Tomatometer,
    m.Popcornmeter,
    m.RottenTomatoesUrl,
    m.Generos,
    m.Duracao,
    m.Diretor,
    m.Elenco,
    m.Ativo,
    m.DataCriacao,
    m.DataAtualizacao,
    ISNULL(um.Status, 0) AS StatusUsuario,
    CASE ISNULL(um.Status, 0)
        WHEN 0 THEN 'Pendente'
        WHEN 1 THEN 'Assistindo'
        WHEN 2 THEN 'Assistido'
        WHEN 3 THEN 'Abandonado'
    END AS StatusDescricao,
    um.Estrelas AS EstrelaUsuario,
    um.Nota AS NotaUsuario,
    um.Comentario AS ComentarioUsuario,
    um.DataInicio,
    um.DataFim,
    CASE WHEN m.Tipo = 1 THEN (SELECT COUNT(*) FROM Temporada WHERE MidiaId = m.Id) ELSE NULL END AS TotalTemporadas,
    CASE WHEN m.Tipo = 1 THEN (
        SELECT COUNT(*) FROM Episodio e 
        INNER JOIN Temporada t ON e.TemporadaId = t.Id 
        WHERE t.MidiaId = m.Id
    ) ELSE NULL END AS TotalEpisodios,
    CASE WHEN m.Tipo = 1 THEN (
        SELECT COUNT(*) FROM UsuarioEpisodio ue
        INNER JOIN Episodio e ON ue.EpisodioId = e.Id
        INNER JOIN Temporada t ON e.TemporadaId = t.Id
        WHERE t.MidiaId = m.Id AND ue.Status = 1
    ) ELSE NULL END AS EpisodiosAssistidos,
    COALESCE(m.ImagemUrl, 'https://via.placeholder.com/300x450?text=' + REPLACE(m.Titulo, ' ', '+')) AS ImagemExibicao
FROM Midia m
LEFT JOIN UsuarioMidia um ON m.Id = um.MidiaId
WHERE m.Ativo = 1;
GO

-- View de Temporada Completa
CREATE OR ALTER VIEW ViewTemporadaCompleta AS
SELECT 
    t.Id,
    t.MidiaId,
    t.Numero,
    t.Titulo,
    t.Ano,
    t.TotalEpisodios,
    (SELECT COUNT(*) FROM Episodio WHERE TemporadaId = t.Id) AS EpisodiosDisponiveis,
    (SELECT COUNT(*) FROM UsuarioEpisodio ue 
     INNER JOIN Episodio e ON ue.EpisodioId = e.Id 
     WHERE e.TemporadaId = t.Id AND ue.Status = 1) AS EpisodiosAssistidos,
    CASE 
        WHEN t.TotalEpisodios > 0 THEN 
            CAST((SELECT COUNT(*) FROM UsuarioEpisodio ue 
                  INNER JOIN Episodio e ON ue.EpisodioId = e.Id 
                  WHERE e.TemporadaId = t.Id AND ue.Status = 1) AS DECIMAL) / t.TotalEpisodios * 100
        ELSE 0 
    END AS PercentualCompleto,
    CASE 
        WHEN (SELECT COUNT(*) FROM Episodio WHERE TemporadaId = t.Id) = 
             (SELECT COUNT(*) FROM UsuarioEpisodio ue 
              INNER JOIN Episodio e ON ue.EpisodioId = e.Id 
              WHERE e.TemporadaId = t.Id AND ue.Status = 1)
        AND (SELECT COUNT(*) FROM Episodio WHERE TemporadaId = t.Id) > 0
        THEN 1 ELSE 0 
    END AS Completa
FROM Temporada t;
GO

-- View de Episódio Completo
CREATE OR ALTER VIEW ViewEpisodioCompleto AS
SELECT 
    e.Id,
    e.TemporadaId,
    e.Numero,
    e.Titulo,
    e.Descricao,
    e.Duracao,
    e.DataExibicao,
    e.ImdbRating,
    t.MidiaId,
    t.Numero AS NumeroTemporada,
    'S' + RIGHT('00' + CAST(t.Numero AS VARCHAR), 2) + 'E' + RIGHT('00' + CAST(e.Numero AS VARCHAR), 2) AS Codigo,
    ue.Status AS StatusUsuario,
    CASE WHEN ue.Status = 1 THEN 1 ELSE 0 END AS Assistido,
    ue.AssistindoCom,
    ue.DataAssistido,
    ue.Comentario AS ComentarioUsuario
FROM Episodio e
INNER JOIN Temporada t ON e.TemporadaId = t.Id
LEFT JOIN UsuarioEpisodio ue ON e.Id = ue.EpisodioId;
GO

-- View de Associações Pendentes
CREATE OR ALTER VIEW ViewAssociacoesPendentes AS
SELECT 
    a.Id,
    a.TituloCapturado,
    a.TituloBrutoCapturado,
    a.AnoCapturado,
    a.TemporadaCapturada,
    a.EpisodioCapturado,
    a.LinkSceneSource,
    a.CategoriaCapturada,
    a.QualidadeCapturada,
    a.FonteReleaseCapturada,
    a.CodecCapturado,
    a.ProviderCapturado,
    a.GrupoReleaseCapturado,
    a.ChaveAgrupamento,
    a.ReleaseScore,
    a.ImdbIdCapturado,
    a.Confianca,
    a.DataCaptura,
    a.DataConfirmacao,
    a.Status,
    m.Id AS MidiaId,
    m.Titulo AS MidiaTitulo,
    m.TituloOriginal AS MidiaTituloOriginal,
    m.Ano AS MidiaAno,
    m.Tipo AS MidiaTipo,
    m.ImagemUrl AS MidiaImagem,
    e.Id AS EpisodioId,
    e.Numero AS EpisodioNumero,
    e.Titulo AS EpisodioTitulo,
    t.Numero AS TemporadaNumero
FROM AssociacaoSceneSource a
LEFT JOIN Midia m ON a.MidiaId = m.Id
LEFT JOIN Episodio e ON a.EpisodioId = e.Id
LEFT JOIN Temporada t ON e.TemporadaId = t.Id
WHERE a.Status = 0;
GO

CREATE OR ALTER VIEW ViewAssociacoesResolvidas AS
SELECT 
    a.Id,
    a.TituloCapturado,
    a.TituloBrutoCapturado,
    a.AnoCapturado,
    a.TemporadaCapturada,
    a.EpisodioCapturado,
    a.LinkSceneSource,
    a.CategoriaCapturada,
    a.QualidadeCapturada,
    a.FonteReleaseCapturada,
    a.CodecCapturado,
    a.ProviderCapturado,
    a.GrupoReleaseCapturado,
    a.ChaveAgrupamento,
    a.ReleaseScore,
    a.ImdbIdCapturado,
    a.Confianca,
    a.DataCaptura,
    a.DataConfirmacao,
    a.Status,
    m.Id AS MidiaId,
    m.Titulo AS MidiaTitulo,
    m.TituloOriginal AS MidiaTituloOriginal,
    m.Ano AS MidiaAno,
    m.Tipo AS MidiaTipo,
    m.ImagemUrl AS MidiaImagem,
    e.Id AS EpisodioId,
    e.Numero AS EpisodioNumero,
    e.Titulo AS EpisodioTitulo,
    t.Numero AS TemporadaNumero
FROM AssociacaoSceneSource a
LEFT JOIN Midia m ON a.MidiaId = m.Id
LEFT JOIN Episodio e ON a.EpisodioId = e.Id
LEFT JOIN Temporada t ON e.TemporadaId = t.Id
WHERE a.Status IN (1, 2);
GO

-- View de Estatísticas
CREATE OR ALTER VIEW ViewEstatisticas AS
SELECT
    (SELECT COUNT(*) FROM Midia WHERE Tipo = 0 AND Ativo = 1) AS TotalFilmes,
    (SELECT COUNT(*) FROM Midia WHERE Tipo = 1 AND Ativo = 1) AS TotalSeries,
    (SELECT COUNT(*) FROM Episodio e INNER JOIN Temporada t ON e.TemporadaId = t.Id 
     INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.Ativo = 1) AS TotalEpisodios,
    (SELECT COUNT(*) FROM UsuarioMidia um INNER JOIN Midia m ON um.MidiaId = m.Id 
     WHERE m.Tipo = 0 AND um.Status = 2) AS FilmesAssistidos,
    (SELECT COUNT(*) FROM UsuarioMidia um INNER JOIN Midia m ON um.MidiaId = m.Id 
     WHERE m.Tipo = 1 AND um.Status = 2) AS SeriesCompletas,
    (SELECT COUNT(*) FROM UsuarioMidia um INNER JOIN Midia m ON um.MidiaId = m.Id 
     WHERE m.Tipo = 1 AND um.Status = 1) AS SeriesEmAndamento,
    (SELECT COUNT(*) FROM UsuarioEpisodio WHERE Status = 1) AS EpisodiosAssistidos,
    (SELECT ISNULL(SUM(
        CASE WHEN m.Tipo = 0 THEN m.Duracao 
             ELSE e.Duracao END
    ), 0) / 60
    FROM UsuarioEpisodio ue
    INNER JOIN Episodio e ON ue.EpisodioId = e.Id
    INNER JOIN Temporada t ON e.TemporadaId = t.Id
    INNER JOIN Midia m ON t.MidiaId = m.Id
    WHERE ue.Status = 1) AS HorasAssistidas,
    (SELECT COUNT(*) FROM AssociacaoSceneSource WHERE Status = 0) AS AssociacoesPendentes,
    (SELECT MAX(DataCaptura) FROM AssociacaoSceneSource) AS UltimaCaptura;
GO

-- =====================================================
-- STORED PROCEDURES
-- =====================================================

-- Obter sugestões de mídias
CREATE OR ALTER PROCEDURE sp_ObterSugestoes
    @Quantidade INT = 10
AS
BEGIN
    SELECT TOP (@Quantidade) *
    FROM ViewMidiaCompleta
    WHERE StatusUsuario = 0
    AND ImdbRating >= 7.0
    ORDER BY ImdbRating DESC, ImdbVotes DESC;
END;
GO

-- Marcar episódio como assistido
CREATE OR ALTER PROCEDURE sp_MarcarEpisodioAssistido
    @EpisodioId INT,
    @AssistindoCom INT = NULL,
    @Comentario NVARCHAR(500) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM UsuarioEpisodio WHERE EpisodioId = @EpisodioId)
    BEGIN
        UPDATE UsuarioEpisodio 
        SET Status = 1, 
            AssistindoCom = @AssistindoCom,
            DataAssistido = GETDATE(),
            Comentario = @Comentario
        WHERE EpisodioId = @EpisodioId;
    END
    ELSE
    BEGIN
        INSERT INTO UsuarioEpisodio (EpisodioId, Status, AssistindoCom, DataAssistido, Comentario)
        VALUES (@EpisodioId, 1, @AssistindoCom, GETDATE(), @Comentario);
    END

    -- Atualiza status da série para "Assistindo" se ainda não estiver
    DECLARE @MidiaId INT;
    SELECT @MidiaId = t.MidiaId 
    FROM Episodio e 
    INNER JOIN Temporada t ON e.TemporadaId = t.Id 
    WHERE e.Id = @EpisodioId;

    IF NOT EXISTS (SELECT 1 FROM UsuarioMidia WHERE MidiaId = @MidiaId)
    BEGIN
        INSERT INTO UsuarioMidia (MidiaId, Status, DataInicio)
        VALUES (@MidiaId, 1, GETDATE());
    END
    ELSE IF EXISTS (SELECT 1 FROM UsuarioMidia WHERE MidiaId = @MidiaId AND Status = 0)
    BEGIN
        UPDATE UsuarioMidia SET Status = 1, DataInicio = GETDATE() WHERE MidiaId = @MidiaId;
    END
END;
GO

-- Confirmar associação
CREATE OR ALTER PROCEDURE sp_ConfirmarAssociacao
    @AssociacaoId INT
AS
BEGIN
    DECLARE @EpisodioId INT;
    DECLARE @ChaveAgrupamento NVARCHAR(500);
    DECLARE @TituloCapturado NVARCHAR(500);
    DECLARE @AnoCapturado INT;
    DECLARE @TemporadaCapturada INT;
    DECLARE @EpisodioCapturado INT;
    
    SELECT
        @EpisodioId = EpisodioId,
        @ChaveAgrupamento = ChaveAgrupamento,
        @TituloCapturado = TituloCapturado,
        @AnoCapturado = AnoCapturado,
        @TemporadaCapturada = TemporadaCapturada,
        @EpisodioCapturado = EpisodioCapturado
    FROM AssociacaoSceneSource
    WHERE Id = @AssociacaoId;
    
    UPDATE AssociacaoSceneSource 
    SET Status = 1, DataConfirmacao = GETDATE() 
    WHERE Id = @AssociacaoId;

        UPDATE AssociacaoSceneSource
        SET Status = 2, DataConfirmacao = GETDATE()
        WHERE Id <> @AssociacaoId
            AND Status IN (0, 1)
            AND (
                    (@ChaveAgrupamento IS NOT NULL AND ChaveAgrupamento = @ChaveAgrupamento)
                    OR (
                            TituloCapturado = @TituloCapturado
                            AND ISNULL(AnoCapturado, -1) = ISNULL(@AnoCapturado, -1)
                            AND ISNULL(TemporadaCapturada, -1) = ISNULL(@TemporadaCapturada, -1)
                            AND ISNULL(EpisodioCapturado, -1) = ISNULL(@EpisodioCapturado, -1)
                    )
            );

    -- Se tem episódio associado, marca como assistido
    IF @EpisodioId IS NOT NULL
    BEGIN
        EXEC sp_MarcarEpisodioAssistido @EpisodioId;
    END
END;
GO

CREATE OR ALTER PROCEDURE sp_SelecionarAssociacao
    @AssociacaoId INT
AS
BEGIN
    DECLARE @EpisodioId INT;
    DECLARE @ChaveAgrupamento NVARCHAR(500);
    DECLARE @TituloCapturado NVARCHAR(500);
    DECLARE @AnoCapturado INT;
    DECLARE @TemporadaCapturada INT;
    DECLARE @EpisodioCapturado INT;

    SELECT
        @EpisodioId = EpisodioId,
        @ChaveAgrupamento = ChaveAgrupamento,
        @TituloCapturado = TituloCapturado,
        @AnoCapturado = AnoCapturado,
        @TemporadaCapturada = TemporadaCapturada,
        @EpisodioCapturado = EpisodioCapturado
    FROM AssociacaoSceneSource
    WHERE Id = @AssociacaoId;

    UPDATE AssociacaoSceneSource
    SET Status = 2, DataConfirmacao = GETDATE()
    WHERE Id <> @AssociacaoId
        AND Status IN (0, 1)
        AND (
                (@ChaveAgrupamento IS NOT NULL AND ChaveAgrupamento = @ChaveAgrupamento)
                OR (
                        TituloCapturado = @TituloCapturado
                        AND ISNULL(AnoCapturado, -1) = ISNULL(@AnoCapturado, -1)
                        AND ISNULL(TemporadaCapturada, -1) = ISNULL(@TemporadaCapturada, -1)
                        AND ISNULL(EpisodioCapturado, -1) = ISNULL(@EpisodioCapturado, -1)
                )
        );

    UPDATE AssociacaoSceneSource
    SET Status = 1, DataConfirmacao = GETDATE()
    WHERE Id = @AssociacaoId;

    IF @EpisodioId IS NOT NULL
    BEGIN
        EXEC sp_MarcarEpisodioAssistido @EpisodioId;
    END
END;
GO

CREATE OR ALTER PROCEDURE sp_NormalizarPreferencia1080p
AS
BEGIN
    ;WITH Base AS
    (
        SELECT
            a.Id,
            GroupKey = COALESCE(NULLIF(a.ChaveAgrupamento, ''), CONCAT(a.TituloCapturado, '|', ISNULL(CAST(a.AnoCapturado AS VARCHAR(10)), ''), '|', ISNULL(CAST(a.TemporadaCapturada AS VARCHAR(10)), ''), '|', ISNULL(CAST(a.EpisodioCapturado AS VARCHAR(10)), ''))),
            SourceText = COALESCE(a.TituloBrutoCapturado, a.CategoriaCapturada, a.LinkSceneSource, ''),
            a.ReleaseScore,
            a.Status
        FROM AssociacaoSceneSource a
        WHERE a.Status IN (1, 2)
    ),
    Ranked AS
    (
        SELECT
            b.Id,
            b.GroupKey,
            Has1080 = MAX(CASE WHEN b.SourceText LIKE '%1080p%' THEN 1 ELSE 0 END) OVER (PARTITION BY b.GroupKey),
            PreferredOrder = ROW_NUMBER() OVER (
                PARTITION BY b.GroupKey
                ORDER BY
                    CASE
                        WHEN b.SourceText LIKE '%1080p%' THEN 3
                        WHEN b.SourceText LIKE '%2160p%' OR b.SourceText LIKE '%4K%' THEN 2
                        WHEN b.SourceText LIKE '%720p%' THEN 1
                        ELSE 0
                    END DESC,
                    b.ReleaseScore DESC,
                    CASE WHEN b.Status = 1 THEN 1 ELSE 0 END DESC,
                    b.Id ASC)
        FROM Base b
    )
    UPDATE a
    SET
        Status = CASE WHEN r.Has1080 = 1 AND r.PreferredOrder = 1 THEN 1 ELSE 2 END,
        DataConfirmacao = CASE
            WHEN a.Status <> CASE WHEN r.Has1080 = 1 AND r.PreferredOrder = 1 THEN 1 ELSE 2 END THEN GETDATE()
            ELSE a.DataConfirmacao
        END
    FROM AssociacaoSceneSource a
    INNER JOIN Ranked r ON r.Id = a.Id
    WHERE r.Has1080 = 1;
END;
GO

-- Rejeitar associação
CREATE OR ALTER PROCEDURE sp_RejeitarAssociacao
    @AssociacaoId INT
AS
BEGIN
    UPDATE AssociacaoSceneSource 
    SET Status = 2, DataConfirmacao = GETDATE() 
    WHERE Id = @AssociacaoId;
END;
GO

-- =====================================================
-- CONFIGURAÇÕES INICIAIS
-- =====================================================

INSERT INTO Configuracao (Chave, Valor, Descricao) VALUES 
('UltimaSyncSceneSource', NULL, 'Data/hora da última sincronização com SceneSource'),
('UltimaSyncIMDb', NULL, 'Data/hora da última sincronização/importação IMDb'),
('UltimaSyncRottenTomatoes', NULL, 'Data/hora da última sincronização Rotten Tomatoes'),
('IntervaloSyncMinutos', '30', 'Intervalo em minutos entre sincronizações automáticas'),
('ConfiancaMinimaAutoAssociacao', '85', 'Percentual mínimo de confiança para auto-associar'),
('LocalWorkerEnabled', 'true', 'Indica que o worker/agendamento local substitui automações em nuvem'),
('TranslationProvider', 'LibreTranslate', 'Provider de tradução configurado para o TrackList');
GO

-- =====================================================
-- DADOS DE EXEMPLO
-- =====================================================

-- Filmes
INSERT INTO Midia (Titulo, TituloOriginal, Tipo, Ano, Descricao, ImdbId, ImdbRating, ImdbVotes, Tomatometer, Popcornmeter, Generos, Duracao, Diretor, Elenco) VALUES
('A Origem', 'Inception', 0, 2010, 'Um ladrão que rouba segredos corporativos através do uso de tecnologia de compartilhamento de sonhos.', 'tt1375666', 8.8, 2400000, 87, 91, 'Ação, Ficção Científica, Thriller', 148, 'Christopher Nolan', 'Leonardo DiCaprio, Joseph Gordon-Levitt, Elliot Page'),
('Interestelar', 'Interstellar', 0, 2014, 'Uma equipe de exploradores viaja através de um buraco de minhoca no espaço.', 'tt0816692', 8.7, 1900000, 73, 86, 'Aventura, Drama, Ficção Científica', 169, 'Christopher Nolan', 'Matthew McConaughey, Anne Hathaway, Jessica Chastain'),
('O Poderoso Chefão', 'The Godfather', 0, 1972, 'A saga da família Corleone sob a liderança de Don Vito Corleone.', 'tt0068646', 9.2, 1900000, 97, 98, 'Crime, Drama', 175, 'Francis Ford Coppola', 'Marlon Brando, Al Pacino, James Caan'),
('Matrix', 'The Matrix', 0, 1999, 'Um hacker descobre a verdadeira natureza de sua realidade.', 'tt0133093', 8.7, 1950000, 83, 85, 'Ação, Ficção Científica', 136, 'Lana Wachowski, Lilly Wachowski', 'Keanu Reeves, Laurence Fishburne, Carrie-Anne Moss'),
('Duna', 'Dune', 0, 2021, 'Paul Atreides se une aos Fremen enquanto busca vingança contra aqueles que destruíram sua família.', 'tt1160419', 8.0, 800000, 83, 90, 'Ação, Aventura, Drama', 155, 'Denis Villeneuve', 'Timothée Chalamet, Rebecca Ferguson, Zendaya');
GO

-- Séries
INSERT INTO Midia (Titulo, TituloOriginal, Tipo, Ano, Descricao, ImdbId, ImdbRating, ImdbVotes, Tomatometer, Popcornmeter, Generos, Duracao, Diretor, Elenco) VALUES
('Breaking Bad', 'Breaking Bad', 1, 2008, 'Um professor de química do ensino médio se torna um fabricante de metanfetamina.', 'tt0903747', 9.5, 2000000, 96, 98, 'Crime, Drama, Thriller', 49, 'Vince Gilligan', 'Bryan Cranston, Aaron Paul, Anna Gunn'),
('Stranger Things', 'Stranger Things', 1, 2016, 'Quando um garoto desaparece, uma pequena cidade descobre um mistério envolvendo experimentos secretos.', 'tt4574334', 8.7, 1200000, 91, 90, 'Drama, Fantasia, Terror', 51, 'The Duffer Brothers', 'Millie Bobby Brown, Finn Wolfhard, Winona Ryder'),
('The Last of Us', 'The Last of Us', 1, 2023, 'Joel e Ellie, um par conectado pela dureza do mundo em que vivem, são forçados a enfrentar criaturas e outros humanos.', 'tt3581920', 8.8, 500000, 96, 90, 'Ação, Aventura, Drama', 52, 'Craig Mazin, Neil Druckmann', 'Pedro Pascal, Bella Ramsey, Anna Torv');
GO

-- Temporadas
-- Breaking Bad
INSERT INTO Temporada (MidiaId, Numero, Ano, TotalEpisodios) VALUES
((SELECT Id FROM Midia WHERE ImdbId = 'tt0903747'), 1, 2008, 7),
((SELECT Id FROM Midia WHERE ImdbId = 'tt0903747'), 2, 2009, 13),
((SELECT Id FROM Midia WHERE ImdbId = 'tt0903747'), 3, 2010, 13),
((SELECT Id FROM Midia WHERE ImdbId = 'tt0903747'), 4, 2011, 13),
((SELECT Id FROM Midia WHERE ImdbId = 'tt0903747'), 5, 2012, 16);
GO

-- Stranger Things
INSERT INTO Temporada (MidiaId, Numero, Ano, TotalEpisodios) VALUES
((SELECT Id FROM Midia WHERE ImdbId = 'tt4574334'), 1, 2016, 8),
((SELECT Id FROM Midia WHERE ImdbId = 'tt4574334'), 2, 2017, 9),
((SELECT Id FROM Midia WHERE ImdbId = 'tt4574334'), 3, 2019, 8),
((SELECT Id FROM Midia WHERE ImdbId = 'tt4574334'), 4, 2022, 9);
GO

-- The Last of Us
INSERT INTO Temporada (MidiaId, Numero, Ano, TotalEpisodios) VALUES
((SELECT Id FROM Midia WHERE ImdbId = 'tt3581920'), 1, 2023, 9);
GO

-- Episódios Breaking Bad T1
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 1, 'Pilot', 58 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 2, 'Cat''s in the Bag...', 48 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 3, '...And the Bag''s in the River', 48 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 4, 'Cancer Man', 48 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 5, 'Gray Matter', 48 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 6, 'Crazy Handful of Nothin''', 48 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 7, 'A No-Rough-Stuff-Type Deal', 48 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
GO

-- Episódios Stranger Things T1
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 1, 'Chapter One: The Vanishing of Will Byers', 49 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 2, 'Chapter Two: The Weirdo on Maple Street', 56 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 3, 'Chapter Three: Holly, Jolly', 52 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 4, 'Chapter Four: The Body', 51 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 5, 'Chapter Five: The Flea and the Acrobat', 52 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 6, 'Chapter Six: The Monster', 46 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 7, 'Chapter Seven: The Bathtub', 41 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 8, 'Chapter Eight: The Upside Down', 55 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt4574334' AND t.Numero = 1;
GO

-- Episódios The Last of Us T1
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 1, 'When You''re Lost in the Darkness', 81 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 2, 'Infected', 55 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 3, 'Long, Long Time', 76 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 4, 'Please Hold to My Hand', 45 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 5, 'Endure and Survive', 60 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 6, 'Kin', 58 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 7, 'Left Behind', 56 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 8, 'When We Are in Need', 52 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
INSERT INTO Episodio (TemporadaId, Numero, Titulo, Duracao) 
SELECT t.Id, 9, 'Look for the Light', 43 FROM Temporada t INNER JOIN Midia m ON t.MidiaId = m.Id WHERE m.ImdbId = 'tt3581920' AND t.Numero = 1;
GO

-- Alguns status de usuário (já assistidos)
INSERT INTO UsuarioMidia (MidiaId, Status, Estrelas, DataFim) VALUES
((SELECT Id FROM Midia WHERE ImdbId = 'tt1375666'), 2, 5, '2023-05-15'),
((SELECT Id FROM Midia WHERE ImdbId = 'tt0068646'), 2, 5, '2022-12-01'),
((SELECT Id FROM Midia WHERE ImdbId = 'tt0903747'), 2, 5, '2023-08-20');
GO

-- Marcar alguns episódios como assistidos
INSERT INTO UsuarioEpisodio (EpisodioId, Status, DataAssistido)
SELECT e.Id, 1, DATEADD(DAY, -e.Numero, GETDATE())
FROM Episodio e
INNER JOIN Temporada t ON e.TemporadaId = t.Id
INNER JOIN Midia m ON t.MidiaId = m.Id
WHERE m.ImdbId = 'tt0903747' AND t.Numero = 1;
GO

-- =====================================================
PRINT 'CineTrack Database criado com sucesso!';
GO
