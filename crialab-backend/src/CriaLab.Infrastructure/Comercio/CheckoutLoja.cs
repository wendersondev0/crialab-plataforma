using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;
using CriaLab.Domain.Entidades;
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

    public async Task GravarPedidoAsync(PedidoCheckout.Gravado pedido, CancellationToken cancelamento = default)
    {
        var entidade = new Pedido
        {
            Id = pedido.Id,
            UsuarioId = pedido.UsuarioId,
            Status = pedido.Status,
            CriadoEm = pedido.CriadoEm,
            Subtotal = pedido.Subtotal,
            DescontoCupom = pedido.DescontoCupom,
            DescontoAVista = pedido.DescontoAVista,
            FreteCobrado = pedido.FreteCobrado,
            Total = pedido.Total,
            Forma = pedido.Forma,
            Endereco = pedido.Endereco,
            Cep = pedido.Cep,
            FreteNome = pedido.FreteNome,
            Cupom = pedido.Cupom,
            RepassarFrete = false,
            Nota = ""
        };
        foreach (var item in pedido.Itens)
        {
            entidade.Itens.Add(new PedidoItem
            {
                Id = item.Id,
                PedidoId = pedido.Id,
                ProdutoId = item.ProdutoId,
                Titulo = item.Titulo,
                Tipo = item.Tipo,
                Fisico = item.Fisico,
                Arquivo = false,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                CorEscolhida = item.CorEscolhida,
                TamanhoEscolhido = item.TamanhoEscolhido
            });
        }

        db.Pedidos.Add(entidade);
        db.PedidoOrdensServico.Add(new PedidoOs
        {
            PedidoId = pedido.Id,
            Status = pedido.Os.Status,
            InsumoBaixado = pedido.Os.InsumoBaixado
        });
        await db.SaveChangesAsync(cancelamento);
    }
}
