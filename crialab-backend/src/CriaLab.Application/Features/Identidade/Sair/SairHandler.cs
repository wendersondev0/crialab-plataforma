using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using Microsoft.AspNetCore.Identity;

namespace CriaLab.Application.Features.Identidade.Sair;

public sealed class SairHandler(UserManager<UsuarioAplicacao> usuarios, ISessaoRefresh sessoes)
{
    public async Task ExecutarAsync(string? userId, string? refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var usuario = await usuarios.FindByIdAsync(userId);
            if (usuario is not null)
            {
                await sessoes.RemoverAsync(usuario);
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var usuario = await sessoes.BuscarPorRefreshAsync(refreshToken);
            if (usuario is not null)
            {
                await sessoes.RemoverAsync(usuario);
            }
        }
    }
}
