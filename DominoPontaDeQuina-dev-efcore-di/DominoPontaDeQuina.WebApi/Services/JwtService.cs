using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DominoPontaDeQuina.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DominoPontaDeQuina.WebApi.Services;

public sealed class JwtService(IConfiguration configuration)
{
    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var section = configuration.GetSection("Jwt");
        var key = section["Key"] ?? throw new InvalidOperationException("Jwt:Key não foi configurada.");
        var issuer = section["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer não foi configurado.");
        var audience = section["Audience"] ?? throw new InvalidOperationException("Jwt:Audience não foi configurado.");
        var minutes = section.GetValue<int?>("ExpiresInMinutes") ?? 60;

        var expiraEm = DateTime.UtcNow.AddMinutes(minutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
