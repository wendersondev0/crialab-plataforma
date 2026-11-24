using CriaLab.Application.Comercio;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class Recibos(ContextoAplicacao db) : IRecibos
{
    public async Task<ReciboLido?> ObterAsync(string id, CancellationToken cancelamento = default)
    {
        var pedido = await db.Pedidos.AsNoTracking()
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id, cancelamento);
        if (pedido is null)
            return null;

        var identidade = await db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync(cancelamento);
        return new ReciboLido(
            pedido.Id,
            pedido.UsuarioId,
            pedido.CriadoEm,
            pedido.Endereco,
            pedido.Cep,
            pedido.Forma,
            pedido.FreteNome,
            pedido.DescontoCupom,
            pedido.DescontoAVista,
            pedido.FreteCobrado,
            pedido.Total,
            identidade?.Nome,
            identidade?.Cnpj,
            pedido.Itens.Select(item => new ReciboItem(
                item.Titulo,
                item.CorEscolhida,
                item.TamanhoEscolhido,
                item.Quantidade,
                item.PrecoUnitario)).ToList());
    }
}
