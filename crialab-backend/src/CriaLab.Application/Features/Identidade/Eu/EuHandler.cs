using System.Security.Claims;
using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using Microsoft.AspNetCore.Identity;

namespace CriaLab.Application.Features.Identidade.Eu;

public sealed class EuHandler(UserManager<UsuarioAplicacao> usuarios)
{
    public async Task<ResultadoIdentidade<UsuarioResposta>> ExecutarAsync(ClaimsPrincipal principal)
    {
        var id = usuarios.GetUserId(principal);
        if (string.IsNullOrWhiteSpace(id))
        {
            return ResultadoIdentidade<UsuarioResposta>.Falha(401, "Não autenticado.");
        }

        var usuario = await usuarios.FindByIdAsync(id);
        if (usuario is null)
        {
            return ResultadoIdentidade<UsuarioResposta>.Falha(401, "Não autenticado.");
        }

        var papeis = await usuarios.GetRolesAsync(usuario);
        return ResultadoIdentidade<UsuarioResposta>.Ok(
            new UsuarioResposta(usuario.Id, usuario.Email!, usuario.NomeCompleto, usuario.TelefoneContato, papeis.ToArray()));
    }
}
