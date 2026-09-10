using DominoPontaDeQuina.Domain.Entities;
using DominoPontaDeQuina.Infrastructure.Persistence;
using DominoPontaDeQuina.WebApi.Models;
using DominoPontaDeQuina.WebApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DominoPontaDeQuina.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(DominoDbContext db, JwtService jwtService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return BadRequest(new { mensagem = "Email e senha são obrigatórios." });

        var email = request.Email.Trim().ToLowerInvariant();
        var existe = await db.Usuarios.AnyAsync(u => u.Email == email, cancellationToken);
        if (existe)
            return BadRequest(new { mensagem = "Já existe um usuário com este email." });

        var usuario = new Usuario { Email = email };
        var hasher = new PasswordHasher<Usuario>();
        usuario.SenhaHash = hasher.HashPassword(usuario, request.Senha);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            usuario.Id,
            usuario.Email,
            usuario.CriadoEm
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return Unauthorized(new { mensagem = "Email ou senha inválidos." });

        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (usuario is null)
            return Unauthorized(new { mensagem = "Email ou senha inválidos." });

        var hasher = new PasswordHasher<Usuario>();
        var resultado = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            return Unauthorized(new { mensagem = "Email ou senha inválidos." });

        var (token, expiraEm) = jwtService.GerarToken(usuario);
        return Ok(new LoginResponse(token, expiraEm));
    }
}
