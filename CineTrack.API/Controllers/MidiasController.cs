using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MidiasController : ControllerBase
{
    private readonly IMidiaRepository _midiaRepository;
    private readonly ITemporadaRepository _temporadaRepository;
    private readonly IEpisodioRepository _episodioRepository;

    public MidiasController(
        IMidiaRepository midiaRepository,
        ITemporadaRepository temporadaRepository,
        IEpisodioRepository episodioRepository)
    {
        _midiaRepository = midiaRepository;
        _temporadaRepository = temporadaRepository;
        _episodioRepository = episodioRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MidiaCompletaVM>>> GetAll(
        [FromQuery] StatusMidia? status = null,
        [FromQuery] TipoMidia? tipo = null)
    {
        var midias = await _midiaRepository.GetAllAsync(status, tipo);
        return Ok(midias);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MidiaCompletaVM>> GetById(int id)
    {
        var midia = await _midiaRepository.GetByIdAsync(id);
        if (midia == null) return NotFound();
        return Ok(midia);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MidiaCompletaVM>>> Search(
        [FromQuery] string termo,
        [FromQuery] StatusMidia? status = null,
        [FromQuery] TipoMidia? tipo = null)
    {
        if (string.IsNullOrWhiteSpace(termo)) return Ok(Array.Empty<MidiaCompletaVM>());
        var midias = await _midiaRepository.SearchAsync(termo, status, tipo);
        return Ok(midias);
    }

    [HttpGet("sugestoes")]
    public async Task<ActionResult<IEnumerable<MidiaCompletaVM>>> GetSugestoes([FromQuery] int quantidade = 10)
    {
        var midias = await _midiaRepository.GetSugestoesAsync(quantidade);
        return Ok(midias);
    }

    [HttpGet("em-andamento")]
    public async Task<ActionResult<IEnumerable<MidiaCompletaVM>>> GetEmAndamento()
    {
        var midias = await _midiaRepository.GetEmAndamentoAsync();
        return Ok(midias);
    }

    [HttpGet("recentes")]
    public async Task<ActionResult<IEnumerable<MidiaCompletaVM>>> GetRecentes([FromQuery] int quantidade = 10)
    {
        var midias = await _midiaRepository.GetRecentesAsync(quantidade);
        return Ok(midias);
    }

    [HttpGet("{id}/temporadas")]
    public async Task<ActionResult<IEnumerable<TemporadaCompletaVM>>> GetTemporadas(int id)
    {
        var temporadas = await _temporadaRepository.GetByMidiaIdAsync(id);
        return Ok(temporadas);
    }

    [HttpGet("{id}/episodios")]
    public async Task<ActionResult<IEnumerable<EpisodioCompletoVM>>> GetEpisodios(int id)
    {
        var episodios = await _episodioRepository.GetByMidiaIdAsync(id);
        return Ok(episodios);
    }
}
