using CriaLab.Application.Comercio;
using CriaLab.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class OrcamentosCliente(ContextoAplicacao db) : IOrcamentosCliente
{
    public async Task<ProdutoOrcamento?> ProdutoAsync(string produtoId, CancellationToken cancelamento = default)
    {
        var produto = await db.Produtos.AsNoTracking()
            .Where(x => x.Id == produtoId)
            .Select(x => new ProdutoOrcamento(x.Id, x.Titulo))
            .FirstOrDefaultAsync(cancelamento);
        return produto;
    }

    public async Task CriarAsync(OrcamentoClienteNovo orcamento, CancellationToken cancelamento = default)
    {
        db.OrcamentosCliente.Add(new OrcamentoCliente
        {
            Id = orcamento.Id,
            UsuarioId = orcamento.UsuarioId,
            ProdutoId = orcamento.ProdutoId,
            Titulo = orcamento.Titulo,
            Status = orcamento.Status,
            Detalhes = orcamento.Detalhes,
            ArquivoNome = orcamento.ArquivoNome,
            CriadoEm = orcamento.CriadoEm
        });
        await db.SaveChangesAsync(cancelamento);
    }
}
