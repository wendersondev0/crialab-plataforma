using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace CriaLab.Application.Features.Identidade.Cadastrar;

public sealed class CadastrarHandler(
    UserManager<UsuarioAplicacao> usuarios,
    IEmissorToken emissor,
    ISessaoRefresh sessoes,
    IValidator<CadastrarPedido> validador)
{
    public async Task<ResultadoIdentidade<SessaoResposta>> ExecutarAsync(CadastrarPedido pedido)
    {
        var validacao = await validador.ValidateAsync(pedido);
        if (!validacao.IsValid)
        {
            return ResultadoIdentidade<SessaoResposta>.Falha(400, validacao.Errors.Select(e => e.ErrorMessage).ToArray());
        }

        var usuario = new UsuarioAplicacao
        {
            UserName = pedido.Email.Trim(),
            Email = pedido.Email.Trim(),
            EmailConfirmed = true,
            NomeCompleto = pedido.NomeCompleto.Trim(),
            TelefoneContato = string.IsNullOrWhiteSpace(pedido.Telefone) ? null : pedido.Telefone.Trim(),
            CriadoEm = DateTime.UtcNow
        };

        var criacao = await usuarios.CreateAsync(usuario, pedido.Senha);
        if (!criacao.Succeeded)
        {
            return ResultadoIdentidade<SessaoResposta>.Falha(400, criacao.Errors.Select(e => e.Description).ToArray());
        }

        await usuarios.AddToRoleAsync(usuario, Papeis.Usuario);
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
