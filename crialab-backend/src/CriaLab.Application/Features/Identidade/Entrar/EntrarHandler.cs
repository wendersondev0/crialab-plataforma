using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace CriaLab.Application.Features.Identidade.Entrar;

public sealed class EntrarHandler(
    UserManager<UsuarioAplicacao> usuarios,
    IEmissorToken emissor,
    ISessaoRefresh sessoes,
    IValidator<EntrarPedido> validador)
{
    public async Task<ResultadoIdentidade<SessaoResposta>> ExecutarAsync(EntrarPedido pedido)
    {
        var validacao = await validador.ValidateAsync(pedido);
        if (!validacao.IsValid)
        {
            return ResultadoIdentidade<SessaoResposta>.Falha(400, validacao.Errors.Select(e => e.ErrorMessage).ToArray());
        }

        var usuario = await usuarios.FindByEmailAsync(pedido.Email.Trim());
        if (usuario is null || !await usuarios.CheckPasswordAsync(usuario, pedido.Senha))
        {
            return ResultadoIdentidade<SessaoResposta>.Falha(401, "E-mail ou senha inválidos.");
        }

        var papeis = await usuarios.GetRolesAsync(usuario);
        var refresh = emissor.CriarRefreshToken();
        await sessoes.GuardarAsync(usuario, refresh);
        var access = emissor.CriarAccessToken(usuario, papeis);
        return ResultadoIdentidade<SessaoResposta>.Ok(new SessaoResposta(
            access,
            new UsuarioResposta(usuario.Id, usuario.Email!, usuario.NomeCompleto, usuario.TelefoneContato, papeis.ToArray()),
            refresh));
    }
}
