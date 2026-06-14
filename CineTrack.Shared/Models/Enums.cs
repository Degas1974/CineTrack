namespace CineTrack.Shared.Models;

public enum TipoMidia
{
    Filme = 0,
    Serie = 1
}

public enum StatusMidia
{
    Pendente = 0,
    Assistindo = 1,
    Assistido = 2,
    Abandonado = 3
}

public enum StatusEpisodio
{
    NaoAssistido = 0,
    Assistido = 1,
    Pulado = 2
}

public enum AssistindoCom
{
    Sozinho = 0,
    Familia = 1,
    Amigos = 2,
    Namorada = 3
}

public enum StatusAssociacao
{
    Pendente = 0,
    Confirmado = 1,
    Rejeitado = 2
}

public enum FonteCaptura
{
    SceneSource = 0,
    IMDb = 1,
    RottenTomatoes = 2
}

public enum TipoLog
{
    Info = 0,
    Sucesso = 1,
    Erro = 2,
    Aviso = 3
}
