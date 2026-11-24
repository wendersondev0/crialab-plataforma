using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using Microsoft.AspNetCore.Identity;

namespace CriaLab.Application.Features.Identidade.Renovar;

public sealed class RenovarHandler(
    UserManager<UsuarioAplicacao> usuarios,
    IEmissorToken emissor,
    ISessaoRefresh sessoes)
{
    public async Task<ResultadoIdentidade<SessaoResposta>> ExecutarAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ResultadoIdentidade<SessaoResposta>.Falha(401, "Sessão expirada. Entre novamente.");
        }

        var usuario = await sessoes.BuscarPorRefreshAsync(refreshToken);
        if (usuario is null)
        {
            return ResultadoIdentidade<SessaoResposta>.Falha(401, "Sessão expirada. Entre novamente.");
        }

        var papeis = await usuarios.GetRolesAsync(usuario);
        var novoRefresh = emissor.CriarRefreshToken();
        await sessoes.GuardarAsync(usuario, novoRefresh);
        var access = emissor.CriarAccessToken(usuario, papeis);
        return ResultadoIdentidade<SessaoResposta>.Ok(new SessaoResposta(
            access,
            new UsuarioResposta(usuario.Id, usuario.Email!, usuario.NomeCompleto, usuario.TelefoneContato, papeis.ToArray()),
            novoRefresh));
    }
}
