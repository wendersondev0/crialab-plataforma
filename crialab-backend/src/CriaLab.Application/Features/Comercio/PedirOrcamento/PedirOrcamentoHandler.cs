using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;
using CriaLab.Domain.Status;

namespace CriaLab.Application.Features.Comercio.PedirOrcamento;

public sealed record PedirOrcamentoPedido(string? Detalhes, string? ArquivoNome);

public sealed record PedirOrcamentoResposta(
    string Id,
    string ProdutoId,
    string Titulo,
    string Status,
    string Detalhes,
    string ArquivoNome);

public sealed class PedirOrcamentoHandler(IOrcamentosCliente orcamentos)
{
    public async Task<PedirOrcamentoResposta?> ExecutarAsync(
        string? usuarioId,
        string? produtoId,
        PedirOrcamentoPedido? pedido,
        CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(produtoId))
            return null;

        var idProduto = produtoId.Trim();
        var produto = await orcamentos.ProdutoAsync(idProduto, cancelamento);
        if (produto is null)
            return null;

        var normal = PedidoOrcamento.Preparar(pedido?.Detalhes, pedido?.ArquivoNome);
        var id = "orc-" + Guid.NewGuid().ToString("N")[..8];
        await orcamentos.CriarAsync(new OrcamentoClienteNovo(
            id,
            usuarioId,
            produto.Id,
            produto.Titulo,
            StatusOrcamento.Aguardando,
            normal.Detalhes,
            normal.ArquivoNome,
            DateTime.UtcNow), cancelamento);

        return new PedirOrcamentoResposta(
            id,
            produto.Id,
            produto.Titulo,
            StatusOrcamento.Aguardando,
            normal.Detalhes,
            normal.ArquivoNome);
    }
}
