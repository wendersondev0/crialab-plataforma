using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Identidade;

public sealed class SessaoRefresh(UserManager<UsuarioAplicacao> usuarios, ContextoAplicacao contexto) : ISessaoRefresh
{
    private const string Provedor = "CriaLab";
    private const string Nome = "RefreshToken";

    public Task GuardarAsync(UsuarioAplicacao usuario, string refreshToken) =>
        usuarios.SetAuthenticationTokenAsync(usuario, Provedor, Nome, refreshToken);

    public async Task<UsuarioAplicacao?> BuscarPorRefreshAsync(string refreshToken)
    {
        var token = await contexto.Set<IdentityUserToken<string>>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LoginProvider == Provedor && x.Name == Nome && x.Value == refreshToken);
        if (token is null)
        {
            return null;
        }

        return await usuarios.FindByIdAsync(token.UserId);
    }

    public Task RemoverAsync(UsuarioAplicacao usuario) =>
        usuarios.RemoveAuthenticationTokenAsync(usuario, Provedor, Nome);
}
