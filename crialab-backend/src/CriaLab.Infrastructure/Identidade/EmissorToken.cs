using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CriaLab.Infrastructure.Identidade;

public sealed class EmissorToken(IOptions<OpcoesJwt> opcoes) : IEmissorToken
{
    public string CriarAccessToken(UsuarioAplicacao usuario, IEnumerable<string> papeis)
    {
        var cfg = opcoes.Value;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Name, usuario.NomeCompleto),
            new(ClaimTypes.Email, usuario.Email ?? string.Empty)
        };
        claims.AddRange(papeis.Select(p => new Claim(ClaimTypes.Role, p)));

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.Chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: cfg.Emissor,
            audience: cfg.Publico,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(cfg.AccessTokenMinutos),
            signingCredentials: credenciais);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CriarRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
