using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.EnviarPergunta;

public sealed record EnviarPerguntaPedido(string? Texto);

public sealed record EnviarPerguntaResposta(
    string Id,
    string Autor,
    string Texto,
    string Resposta,
    bool Respondida);

public enum EnviarPerguntaEstado
{
    Criada,
    ProdutoAusente,
    TextoVazio
}

public sealed record EnviarPerguntaResultado(EnviarPerguntaEstado Estado, EnviarPerguntaResposta? Pergunta);

public sealed class EnviarPerguntaHandler(IPerguntasProduto perguntas)
{
    public async Task<EnviarPerguntaResultado> ExecutarAsync(
        string? usuarioId,
        string? produtoId,
        EnviarPerguntaPedido? pedido,
        CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(produtoId))
            return new EnviarPerguntaResultado(EnviarPerguntaEstado.ProdutoAusente, null);

        var texto = PerguntaFicha.Texto(pedido?.Texto);
        if (texto is null)
            return new EnviarPerguntaResultado(EnviarPerguntaEstado.TextoVazio, null);

        var idProduto = produtoId.Trim();
        if (!await perguntas.ExisteProdutoAsync(idProduto, cancelamento))
            return new EnviarPerguntaResultado(EnviarPerguntaEstado.ProdutoAusente, null);

        var autenticado = !string.IsNullOrWhiteSpace(usuarioId);
        var nome = autenticado
            ? await perguntas.NomeUsuarioAsync(usuarioId!, cancelamento)
            : null;
        var autor = PerguntaFicha.Autor(autenticado, nome);
        var id = "per-" + Guid.NewGuid().ToString("N")[..8];
        await perguntas.CriarAsync(new PerguntaProdutoNova(id, idProduto, autor, texto, DateTime.UtcNow), cancelamento);
        return new EnviarPerguntaResultado(
            EnviarPerguntaEstado.Criada,
            new EnviarPerguntaResposta(id, autor, texto, "", false));
    }
}
