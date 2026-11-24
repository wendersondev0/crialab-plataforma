namespace CriaLab.Application.Comercio;

public sealed record ProdutoOrcamento(string Id, string Titulo);

public sealed record OrcamentoClienteNovo(
    string Id,
    string UsuarioId,
    string ProdutoId,
    string Titulo,
    string Status,
    string Detalhes,
    string ArquivoNome,
    DateTime CriadoEm);

public interface IOrcamentosCliente
{
    Task<ProdutoOrcamento?> ProdutoAsync(string produtoId, CancellationToken cancelamento = default);

    Task CriarAsync(OrcamentoClienteNovo orcamento, CancellationToken cancelamento = default);
}
