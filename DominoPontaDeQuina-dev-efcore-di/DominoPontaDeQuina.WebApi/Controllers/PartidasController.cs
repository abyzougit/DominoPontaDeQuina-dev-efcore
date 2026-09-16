using DominoPontaDeQuina.Application.Services;
using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DominoPontaDeQuina.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PartidasController(IPartidaService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Partida), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Partida>> IniciarPartida(
        [FromBody] IniciarPartidaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var partida = await service.IniciarPartidaAsync(request.PontuacaoAlvo, cancellationToken);
            return CreatedAtAction(nameof(VerificarStatus), new { id = partida.Id }, partida);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { mensagem = "A pontuação alvo deve ser maior que zero.", detalhe = ex.Message });
        }
    }

    [HttpPost("{id:guid}/jogadores")]
    [ProducesResponseType(typeof(Jogador), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Jogador>> RegistrarJogador(
        Guid id,
        [FromBody] RegistrarJogadorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var jogador = await service.RegistrarJogadorAsync(
                id, request.Nome, request.UsuarioId, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, jogador);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPost("{id:guid}/lances")]
    [ProducesResponseType(typeof(Lance), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Lance>> RegistrarLance(
        Guid id,
        [FromBody] RegistrarLanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var lance = await service.RegistrarLanceAsync(id, request.JogadorId, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, lance);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Partida), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Partida>> VerificarStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.VerificarStatusAsync(id, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpGet("historico")]
    [ProducesResponseType(typeof(IReadOnlyList<Partida>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Partida>>> ConsultarHistorico(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ConsultarHistoricoAsync(cancellationToken));
    }

    [HttpGet("ranking")]
    [ProducesResponseType(typeof(IReadOnlyList<Ranking>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Ranking>>> ConsultarRanking(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ConsultarRankingAsync(cancellationToken));
    }
}
