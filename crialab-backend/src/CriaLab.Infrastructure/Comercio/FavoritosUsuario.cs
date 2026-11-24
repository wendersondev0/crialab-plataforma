using CriaLab.Application.Comercio;
using CriaLab.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class FavoritosUsuario(ContextoAplicacao db) : IFavoritosUsuario
{
    public Task<bool> ProdutoExisteAsync(string produtoId, CancellationToken cancelamento = default) =>
        db.Produtos.AsNoTracking().AnyAsync(x => x.Id == produtoId, cancelamento);

    public Task<bool> EstaAsync(string usuarioId, string produtoId, CancellationToken cancelamento = default) =>
        db.Favoritos.AsNoTracking().AnyAsync(x => x.UsuarioId == usuarioId && x.ProdutoId == produtoId, cancelamento);

    public async Task<bool> AlternarAsync(string usuarioId, string produtoId, CancellationToken cancelamento = default)
    {
        var favorito = await db.Favoritos.FirstOrDefaultAsync(
            x => x.UsuarioId == usuarioId && x.ProdutoId == produtoId,
            cancelamento);
        if (favorito != null)
        {
            db.Favoritos.Remove(favorito);
            await db.SaveChangesAsync(cancelamento);
            return false;
        }

        db.Favoritos.Add(new Favorito
        {
            UsuarioId = usuarioId,
            ProdutoId = produtoId,
            CriadoEm = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancelamento);
        return true;
    }
}
