using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.CotarCheckout;

public sealed record CotarCheckoutItemPedido(
    string? ProdutoId,
    string? Titulo,
    decimal PrecoUnitario,
    int Quantidade,
    string? Cor,
    string? Tamanho,
    string? Detalhes);

public sealed record CotarCheckoutPedido(
    IReadOnlyList<CotarCheckoutItemPedido>? Itens,
    string? Cep,
    string? Cupom,
    string? FreteId,
    string? Endereco,
    string? Forma,
    string? Acao,
    decimal? Total);

public sealed record CotarCheckoutItem(
    string ProdutoId,
    string Titulo,
    decimal PrecoUnitario,
    int Quantidade,
    string? Cor,
    string? Tamanho,
    string? Detalhes,
    decimal TotalLinha);

public sealed record CotarCheckoutOpcao(string Id, string Nome, decimal Preco, int Dias, string? Descricao);

public sealed record CotarCheckoutResposta(
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
    IReadOnlyList<CotarCheckoutOpcao> Opcoes);

public sealed class CotarCheckoutHandler(ICheckoutLoja loja)
{
    public async Task<CotarCheckoutResposta?> ExecutarAsync(CotarCheckoutPedido pedido, CancellationToken cancelamento = default)
    {
        var enviados = pedido.Itens ?? [];
        var ids = enviados
            .Select(item => item.ProdutoId?.Trim() ?? "")
            .Where(id => id.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var pecas = await loja.PecasAsync(ids, cancelamento);
        var porId = pecas.ToDictionary(peca => peca.Id, StringComparer.Ordinal);
        if (ids.Any(id => !porId.ContainsKey(id)))
            return null;

        var itens = enviados
            .Where(item => !string.IsNullOrWhiteSpace(item.ProdutoId))
            .Select(item =>
            {
                var peca = porId[item.ProdutoId!.Trim()];
                return new CheckoutCalculo.Item(peca.Id, peca.Titulo, peca.Preco, item.Quantidade, item.Cor, item.Tamanho, item.Detalhes);
            })
            .ToList();

        var contexto = await loja.ContextoAsync(cancelamento);
        var resultado = CheckoutCalculo.Calcular(
            itens,
            pedido.Cep,
            pedido.Cupom,
            string.IsNullOrWhiteSpace(pedido.FreteId) ? "pac" : pedido.FreteId,
            pedido.Endereco,
            pedido.Forma,
            string.IsNullOrWhiteSpace(pedido.Acao) ? "cotar" : pedido.Acao,
            contexto.Cupons,
            contexto.Faixas,
            contexto.FreteGratisAPartir,
            contexto.DescontoAVista);

        return new CotarCheckoutResposta(
            resultado.Subtotal,
            resultado.DescontoCupom,
            resultado.DescontoAVista,
            resultado.FreteCobrado,
            resultado.FreteNome,
            resultado.FreteId,
            resultado.FreteGratisAPartir,
            resultado.Total,
            resultado.ErroFrete,
            resultado.Erros,
            resultado.Itens.Select(item => new CotarCheckoutItem(
                item.ProdutoId,
                item.Titulo,
                item.PrecoUnitario,
                item.Quantidade,
                item.Cor,
                item.Tamanho,
                item.Detalhes,
                item.PrecoUnitario * item.Quantidade)).ToList(),
            resultado.Opcoes.Select(opcao => new CotarCheckoutOpcao(opcao.Id, opcao.Nome, opcao.Preco, opcao.Dias, opcao.Descricao)).ToList());
    }
}
