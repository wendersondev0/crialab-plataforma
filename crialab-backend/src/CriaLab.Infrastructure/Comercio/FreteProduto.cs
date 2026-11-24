using CriaLab.Application.Comercio;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class FreteProduto(ContextoAplicacao db) : IFreteProduto
{
    public Task<decimal?> PrecoAsync(string produtoId, CancellationToken cancelamento = default) =>
        db.Produtos.AsNoTracking()
            .Where(x => x.Id == produtoId)
            .Select(x => (decimal?)x.Preco)
            .FirstOrDefaultAsync(cancelamento);

    public async Task<FreteContexto> ContextoAsync(CancellationToken cancelamento = default)
    {
        var gratis = await db.ConfiguracoesLoja.AsNoTracking()
            .Select(x => (decimal?)x.FreteGratisAPartir)
            .FirstOrDefaultAsync(cancelamento) ?? 200m;
        var faixas = await db.FaixasCep.AsNoTracking().ToListAsync(cancelamento);
        return new FreteContexto(gratis, faixas);
    }
}
