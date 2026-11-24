using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.ObterContato;

public sealed record ObterContatoResposta(
    string? Email,
    string? EmailHref,
    string? Telefone,
    string? TelefoneHref,
    string? WhatsappHref,
    string? Endereco,
    string? Horario,
    string? NomeConta,
    string? EmailConta);

public sealed class ObterContatoHandler(IContatos contatos)
{
    public async Task<ObterContatoResposta> ExecutarAsync(string? usuarioId, CancellationToken cancelamento = default)
    {
        var canal = await contatos.CanalAsync(cancelamento);
        var email = ContatoLoja.Visivel(canal?.Email);
        ContaContato? conta = null;
        if (!string.IsNullOrWhiteSpace(usuarioId))
            conta = await contatos.ContaAsync(usuarioId, cancelamento);

        return new ObterContatoResposta(
            email,
            email is null ? null : "mailto:" + email,
            ContatoLoja.Visivel(canal?.Telefone),
            ContatoLoja.Ligacao(canal?.Telefone),
            ContatoLoja.Whatsapp(canal?.Whatsapp),
            ContatoLoja.Visivel(canal?.Endereco),
            ContatoLoja.Visivel(canal?.Horario),
            string.IsNullOrWhiteSpace(conta?.Nome) ? null : conta.Nome,
            string.IsNullOrWhiteSpace(conta?.Email) ? null : conta.Email);
    }
}
