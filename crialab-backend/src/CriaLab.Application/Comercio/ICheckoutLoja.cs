using CriaLab.Domain.Calculos;
using CriaLab.Domain.Entidades;

namespace CriaLab.Application.Comercio;

public sealed record PecaCheckout(string Id, string Titulo, decimal Preco);

public sealed record LojaCheckout(
    decimal FreteGratisAPartir,
    decimal DescontoAVista,
    IReadOnlyList<FaixaCep> Faixas,
    IReadOnlyList<Cupom> Cupons);

public interface ICheckoutLoja
{
    Task<IReadOnlyList<PecaCheckout>> PecasAsync(IReadOnlyCollection<string> ids, CancellationToken cancelamento = default);

    Task<LojaCheckout> ContextoAsync(CancellationToken cancelamento = default);

    Task GravarPedidoAsync(PedidoCheckout.Gravado pedido, CancellationToken cancelamento = default);
}
