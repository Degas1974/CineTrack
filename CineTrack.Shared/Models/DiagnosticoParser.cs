using System.Text.Json;

namespace CineTrack.Shared.Models;

public class DiagnosticoParserVM
{
    public DateTime DataLog { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string ModoParsing { get; set; } = "Outro";
    public string Titulo { get; set; } = string.Empty;
    public string? TituloEpisodio { get; set; }
    public int? Ano { get; set; }
    public int? Temporada { get; set; }
    public int? Episodio { get; set; }
    public string? LinkSceneSource { get; set; }

    public string? CodigoEpisodio => Temporada.HasValue && Episodio.HasValue
        ? $"S{Temporada.Value:00}E{Episodio.Value:00}"
        : Temporada.HasValue
            ? $"Temporada {Temporada.Value}"
            : null;

    public bool TemTituloEpisodio => !string.IsNullOrWhiteSpace(TituloEpisodio);

    public bool IsAmbiguo => ModoParsing == "Outro";

    public int ClarezaScore => ModoParsing switch
    {
        "SxxEyy" => TemTituloEpisodio ? 120 : 110,
        "Data" => TemTituloEpisodio ? 105 : 95,
        "Temporada" => TemTituloEpisodio ? 90 : 80,
        "Filme" => Ano.HasValue ? 70 : 60,
        _ => TemTituloEpisodio ? 40 : 20
    };

    public string ClarezaDescricao => ClarezaScore switch
    {
        >= 110 => "Muito claro",
        >= 90 => "Claro",
        >= 60 => "Moderado",
        _ => "Ambíguo"
    };

    public string ResumoHumano
    {
        get
        {
            var resumoBase = ModoParsing switch
            {
                "SxxEyy" when !string.IsNullOrWhiteSpace(CodigoEpisodio) => $"Série por {CodigoEpisodio}",
                "Temporada" when Temporada.HasValue => $"Série por temporada {Temporada.Value}",
                "Data" => "Série por data de exibição",
                "Filme" => "Filme por título",
                _ when !string.IsNullOrWhiteSpace(CodigoEpisodio) => $"Captura com marcador {CodigoEpisodio}",
                _ => "Captura por heurística genérica"
            };

            if (!string.IsNullOrWhiteSpace(TituloEpisodio))
            {
                return resumoBase + " com título de episódio capturado";
            }

            if (Ano.HasValue && (ModoParsing == "Filme" || ModoParsing == "Outro"))
            {
                return resumoBase + " e ano capturado";
            }

            return IsAmbiguo ? resumoBase + ", revisar manualmente" : resumoBase;
        }
    }
}

public class DiagnosticoParserPayload
{
    public string? ModoParsing { get; set; }
    public string? Titulo { get; set; }
    public string? TituloEpisodio { get; set; }
    public int? Ano { get; set; }
    public int? Temporada { get; set; }
    public int? Episodio { get; set; }
    public string? LinkSceneSource { get; set; }
}

public static class DiagnosticoParserMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string Serialize(AssociacaoSceneSource captura)
    {
        var payload = new DiagnosticoParserPayload
        {
            ModoParsing = NormalizarModo(captura.ModoParsingCapturado),
            Titulo = captura.TituloCapturado,
            TituloEpisodio = captura.TituloEpisodioCapturado,
            Ano = captura.AnoCapturado,
            Temporada = captura.TemporadaCapturada,
            Episodio = captura.EpisodioCapturado,
            LinkSceneSource = captura.LinkSceneSource
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    public static DiagnosticoParserVM FromLog(LogCaptura log)
    {
        var payload = TryParsePayload(log.Detalhes) ?? new DiagnosticoParserPayload();

        return new DiagnosticoParserVM
        {
            DataLog = log.DataLog,
            Mensagem = log.Mensagem,
            ModoParsing = NormalizarModo(payload.ModoParsing),
            Titulo = payload.Titulo ?? string.Empty,
            TituloEpisodio = payload.TituloEpisodio,
            Ano = payload.Ano,
            Temporada = payload.Temporada,
            Episodio = payload.Episodio,
            LinkSceneSource = payload.LinkSceneSource
        };
    }

    private static DiagnosticoParserPayload? TryParsePayload(string? detalhes)
    {
        if (string.IsNullOrWhiteSpace(detalhes))
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<DiagnosticoParserPayload>(detalhes, JsonOptions);
            if (payload != null && TemConteudo(payload))
            {
                return payload;
            }
        }
        catch (JsonException)
        {
        }

        var legado = ParseLegacyPayload(detalhes);
        return TemConteudo(legado) ? legado : null;
    }

    private static DiagnosticoParserPayload ParseLegacyPayload(string detalhes)
    {
        return new DiagnosticoParserPayload
        {
            ModoParsing = ExtractQuotedValue(detalhes, "ModoParsing"),
            Titulo = ExtractQuotedValue(detalhes, "Titulo"),
            TituloEpisodio = ExtractQuotedValue(detalhes, "TituloEpisodio"),
            Ano = ExtractNullableInt(detalhes, "Ano"),
            Temporada = ExtractNullableInt(detalhes, "Temporada"),
            Episodio = ExtractNullableInt(detalhes, "Episodio"),
            LinkSceneSource = ExtractQuotedValue(detalhes, "Link")
        };
    }

    private static bool TemConteudo(DiagnosticoParserPayload payload)
    {
        return !string.IsNullOrWhiteSpace(payload.ModoParsing)
            || !string.IsNullOrWhiteSpace(payload.Titulo)
            || !string.IsNullOrWhiteSpace(payload.TituloEpisodio)
            || payload.Ano.HasValue
            || payload.Temporada.HasValue
            || payload.Episodio.HasValue
            || !string.IsNullOrWhiteSpace(payload.LinkSceneSource);
    }

    private static string NormalizarModo(string? modoParsing)
    {
        return string.IsNullOrWhiteSpace(modoParsing)
            ? "Outro"
            : modoParsing.Trim();
    }

    private static string? ExtractQuotedValue(string detalhes, string key)
    {
        var prefix = key + "='";
        var start = detalhes.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        start += prefix.Length;
        var end = detalhes.IndexOf('\'', start);
        if (end <= start)
        {
            return null;
        }

        var value = detalhes[start..end].Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int? ExtractNullableInt(string detalhes, string key)
    {
        var prefix = key + "=";
        var start = detalhes.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        start += prefix.Length;
        var end = detalhes.IndexOf(';', start);
        var value = (end >= start ? detalhes[start..end] : detalhes[start..]).Trim();

        if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}