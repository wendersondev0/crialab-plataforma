namespace CriaLab.Application.Comercio;

public sealed record PerguntaProdutoNova(
    string Id,
    string ProdutoId,
    string Autor,
    string Texto,
    DateTime CriadoEm);

public interface IPerguntasProduto
{
    Task<bool> ExisteProdutoAsync(string produtoId, CancellationToken cancelamento = default);

    Task<string?> NomeUsuarioAsync(string usuarioId, CancellationToken cancelamento = default);

    Task CriarAsync(PerguntaProdutoNova pergunta, CancellationToken cancelamento = default);
}
