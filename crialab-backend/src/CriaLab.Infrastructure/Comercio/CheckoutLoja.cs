using CriaLab.Application.Comercio;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class CheckoutLoja(ContextoAplicacao db) : ICheckoutLoja
{
    public async Task<IReadOnlyList<PecaCheckout>> PecasAsync(IReadOnlyCollection<string> ids, CancellationToken cancelamento = default)
    {
        if (ids.Count == 0)
            return [];

        return await db.Produtos.AsNoTracking()
            .Where(produto => ids.Contains(produto.Id))
            .Select(produto => new PecaCheckout(produto.Id, produto.Titulo, produto.Preco))
            .ToListAsync(cancelamento);
    }

    public async Task<LojaCheckout> ContextoAsync(CancellationToken cancelamento = default)
    {
        var configuracao = await db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync(cancelamento);
        var faixas = await db.FaixasCep.AsNoTracking().ToListAsync(cancelamento);
        var cupons = await db.Cupons.AsNoTracking().ToListAsync(cancelamento);
        return new LojaCheckout(
            configuracao?.FreteGratisAPartir ?? 200m,
            configuracao?.DescontoAVista ?? 0.05m,
            faixas,
            cupons);
    }
}
