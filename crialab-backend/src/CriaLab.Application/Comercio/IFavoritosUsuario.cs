namespace CriaLab.Application.Comercio;

public interface IFavoritosUsuario
{
    Task<bool> ProdutoExisteAsync(string produtoId, CancellationToken cancelamento = default);

    Task<bool> EstaAsync(string usuarioId, string produtoId, CancellationToken cancelamento = default);

    Task<bool> AlternarAsync(string usuarioId, string produtoId, CancellationToken cancelamento = default);
}
