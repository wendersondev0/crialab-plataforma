using CriaLab.Application.Comercio;
using CriaLab.Application.Features.Comercio.CotarCheckout;
using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.ConfirmarCheckout;

public sealed record ConfirmarCheckoutResposta(
    decimal Subtotal,
    decimal DescontoCupom,
    decimal DescontoAVista,
    decimal FreteCobrado,
    string FreteNome,
    string FreteId,
    decimal FreteGratisAPartir,
    decimal Total,
    string? ErroFrete,
    IReadOnlyList<string> Erros,
    IReadOnlyList<CotarCheckoutItem> Itens,
    IReadOnlyList<CotarCheckoutOpcao> Opcoes,
    string? PedidoId);

public sealed class ConfirmarCheckoutHandler(CotarCheckoutHandler cotar, ICheckoutLoja loja)
{
    public async Task<ConfirmarCheckoutResposta?> ExecutarAsync(
        string? usuarioId,
        CotarCheckoutPedido? pedido,
        CancellationToken cancelamento = default)
    {
        var enviado = pedido ?? new CotarCheckoutPedido(null, null, null, null, null, null, null, null);
        var cotacao = await cotar.ExecutarAsync(enviado with { Acao = "finalizar" }, cancelamento);
        if (cotacao is null)
            return null;

        string? pedidoId = null;
        if (!string.IsNullOrWhiteSpace(usuarioId))
        {
            var ids = cotacao.Itens.Select(_ => Guid.NewGuid().ToString("N")).ToList();
            var montado = PedidoCheckout.Montar(
                "ped-" + Guid.NewGuid().ToString("N")[..8],
                usuarioId,
                DateTime.UtcNow,
                ids,
                ParaResultado(cotacao),
                enviado.Cep,
                enviado.Cupom,
                enviado.Forma,
                enviado.Endereco);
            if (montado is not null)
            {
                await loja.GravarPedidoAsync(montado, cancelamento);
                pedidoId = montado.Id;
            }
        }

        return new ConfirmarCheckoutResposta(
            cotacao.Subtotal,
            cotacao.DescontoCupom,
            cotacao.DescontoAVista,
            cotacao.FreteCobrado,
            cotacao.FreteNome,
            cotacao.FreteId,
            cotacao.FreteGratisAPartir,
            cotacao.Total,
            cotacao.ErroFrete,
            cotacao.Erros,
            cotacao.Itens,
            cotacao.Opcoes,
            pedidoId);
    }

    private static CheckoutCalculo.Resultado ParaResultado(CotarCheckoutResposta cotacao) =>
        new(
            cotacao.Subtotal,
            cotacao.DescontoCupom,
            cotacao.DescontoAVista,
            cotacao.FreteCobrado,
            cotacao.FreteNome,
            cotacao.FreteId,
            cotacao.FreteGratisAPartir,
            cotacao.Total,
            cotacao.ErroFrete,
            cotacao.Erros,
            cotacao.Itens.Select(item => new CheckoutCalculo.Item(
                item.ProdutoId,
                item.Titulo,
                item.PrecoUnitario,
                item.Quantidade,
                item.Cor,
                item.Tamanho,
                item.Detalhes)).ToList(),
            cotacao.Opcoes.Select(opcao => new FreteOpcao(opcao.Id, opcao.Nome, opcao.Preco, opcao.Dias, opcao.Descricao)).ToList());
}
