using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;
using CriaLab.Domain.Status;

namespace CriaLab.Application.Features.Comercio.EnviarContato;

public sealed record EnviarContatoPedido(string? Nome, string? Email, string? Tipo, string? Texto);

public sealed record EnviarContatoResposta(string Id, string Status, string Tipo);

public sealed record EnviarContatoResultado(bool Gravada, EnviarContatoResposta? Mensagem, IReadOnlyList<string> Erros);

public sealed class EnviarContatoHandler(IContatos contatos)
{
    public async Task<EnviarContatoResultado> ExecutarAsync(EnviarContatoPedido? pedido, CancellationToken cancelamento = default)
    {
        var preparada = ContatoLoja.Preparar(pedido?.Nome, pedido?.Email, pedido?.Tipo, pedido?.Texto);
        if (preparada.Erros.Count > 0)
            return new EnviarContatoResultado(false, null, preparada.Erros);

        var id = "msg-" + Guid.NewGuid().ToString("N");
        await contatos.GravarAsync(new MensagemContatoNova(
            id,
            preparada.Nome,
            preparada.Email,
            preparada.Tipo,
            preparada.Texto,
            StatusMensagem.Aberto,
            DateTime.UtcNow), cancelamento);

        return new EnviarContatoResultado(true, new EnviarContatoResposta(id, StatusMensagem.Aberto, preparada.Tipo), []);
    }
}
