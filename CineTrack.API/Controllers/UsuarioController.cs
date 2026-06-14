using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMidiaRepository _midiaRepository;
    private readonly IEpisodioRepository _episodioRepository;
    private readonly ITemporadaRepository _temporadaRepository;

    public UsuarioController(
        IUsuarioRepository usuarioRepository,
        IMidiaRepository midiaRepository,
        IEpisodioRepository episodioRepository,
        ITemporadaRepository temporadaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _midiaRepository = midiaRepository;
        _episodioRepository = episodioRepository;
        _temporadaRepository = temporadaRepository;
    }

    [HttpPut("midia/{midiaId}/status")]
    public async Task<IActionResult> UpdateStatus(int midiaId, [FromBody] UpdateStatusRequest request)
    {
        if (!Enum.IsDefined(typeof(StatusMidia), request.Status))
        {
            return BadRequest("Status de mídia inválido.");
        }

        var midia = await _midiaRepository.GetByIdAsync(midiaId);
        if (midia == null)
        {
            return NotFound();
        }

        await _usuarioRepository.UpdateStatusMidiaAsync(midiaId, (StatusMidia)request.Status);
        return Ok();
    }

    [HttpPut("midia/{midiaId}/nota")]
    public async Task<IActionResult> UpdateNota(int midiaId, [FromBody] UpdateNotaRequest request)
    {
        var midia = await _midiaRepository.GetByIdAsync(midiaId);
        if (midia == null)
        {
            return NotFound();
        }

        await _usuarioRepository.UpdateNotaMidiaAsync(midiaId, request.Estrelas, request.Nota, request.Comentario);
        return Ok();
    }

    [HttpPost("episodio/{episodioId}/assistido")]
    public async Task<IActionResult> MarcarEpisodioAssistido(int episodioId, [FromBody] MarcarAssistidoRequest? request = null)
    {
        var episodio = await _episodioRepository.GetByIdAsync(episodioId);
        if (episodio == null)
        {
            return NotFound();
        }

        if (request?.AssistindoCom != null && !Enum.IsDefined(typeof(AssistindoCom), request.AssistindoCom.Value))
        {
            return BadRequest("Valor de AssistindoCom inválido.");
        }

        await _usuarioRepository.MarcarEpisodioAssistidoAsync(
            episodioId,
            request?.AssistindoCom != null ? (AssistindoCom)request.AssistindoCom : null,
            request?.Comentario);
        return Ok();
    }

    [HttpDelete("episodio/{episodioId}")]
    public async Task<IActionResult> DesmarcarEpisodio(int episodioId)
    {
        var episodio = await _episodioRepository.GetByIdAsync(episodioId);
        if (episodio == null)
        {
            return NotFound();
        }

        await _usuarioRepository.DesmarcarEpisodioAsync(episodioId);
        return Ok();
    }

    [HttpPost("temporada/{temporadaId}/assistido")]
    public async Task<IActionResult> MarcarTemporadaAssistida(int temporadaId, [FromBody] MarcarAssistidoRequest? request = null)
    {
        var temporada = await _temporadaRepository.GetByIdAsync(temporadaId);
        if (temporada == null)
        {
            return NotFound();
        }

        if (request?.AssistindoCom != null && !Enum.IsDefined(typeof(AssistindoCom), request.AssistindoCom.Value))
        {
            return BadRequest("Valor de AssistindoCom inválido.");
        }

        await _usuarioRepository.MarcarTemporadaAssistidaAsync(
            temporadaId,
            request?.AssistindoCom != null ? (AssistindoCom)request.AssistindoCom : null);
        return Ok();
    }

    [HttpDelete("temporada/{temporadaId}")]
    public async Task<IActionResult> DesmarcarTemporada(int temporadaId)
    {
        var temporada = await _temporadaRepository.GetByIdAsync(temporadaId);
        if (temporada == null)
        {
            return NotFound();
        }

        await _usuarioRepository.DesmarcarTemporadaAsync(temporadaId);
        return Ok();
    }
}
