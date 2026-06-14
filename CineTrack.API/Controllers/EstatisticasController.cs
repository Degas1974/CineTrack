using CineTrack.Shared.Data;
using CineTrack.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstatisticasController : ControllerBase
{
    private readonly ISyncRepository _syncRepository;

    public EstatisticasController(ISyncRepository syncRepository)
    {
        _syncRepository = syncRepository;
    }

    [HttpGet]
    public async Task<ActionResult<EstatisticasVM>> Get()
    {
        var estatisticas = await _syncRepository.GetEstatisticasAsync();
        return Ok(estatisticas);
    }

    [HttpGet("generos")]
    public async Task<ActionResult<IEnumerable<GeneroStatVM>>> GetGeneros()
    {
        var generos = await _syncRepository.GetGenerosAsync();
        return Ok(generos);
    }

    [HttpGet("atividade")]
    public async Task<ActionResult<IEnumerable<AtividadeMensalVM>>> GetAtividade()
    {
        var atividade = await _syncRepository.GetAtividadeMensalAsync();
        return Ok(atividade);
    }
}
