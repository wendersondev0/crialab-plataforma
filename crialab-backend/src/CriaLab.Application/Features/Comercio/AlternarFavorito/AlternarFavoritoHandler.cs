using CriaLab.Application.Comercio;

namespace CriaLab.Application.Features.Comercio.AlternarFavorito;

public sealed class AlternarFavoritoHandler(IFavoritosUsuario favoritos)
{
    public async Task<bool?> ExecutarAsync(string? usuarioId, string? produtoId, CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(produtoId))
            return null;

        var id = produtoId.Trim();
        if (await favoritos.EstaAsync(usuarioId, id, cancelamento))
            return await favoritos.AlternarAsync(usuarioId, id, cancelamento);

        if (!await favoritos.ProdutoExisteAsync(id, cancelamento))
            return null;

        return await favoritos.AlternarAsync(usuarioId, id, cancelamento);
    }
}
