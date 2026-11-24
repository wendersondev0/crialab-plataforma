using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.ResumirCarrinho;

public sealed record ResumirCarrinhoItem(
    string? ProdutoId,
    string? Titulo,
    decimal PrecoUnitario,
    int Quantidade,
    string? Cor,
    string? Tamanho,
    string? Detalhes);

public sealed record ResumirCarrinhoPedido(IReadOnlyList<ResumirCarrinhoItem>? Itens, decimal? Total);

public sealed record ResumirCarrinhoLinha(
    string ProdutoId,
    string Titulo,
    decimal PrecoUnitario,
    int Quantidade,
    string? Cor,
    string? Tamanho,
    string? Detalhes,
    decimal TotalLinha);

public sealed record ResumirCarrinhoResposta(decimal Subtotal, IReadOnlyList<ResumirCarrinhoLinha> Itens);

public sealed class ResumirCarrinhoHandler
{
    public ResumirCarrinhoResposta Executar(ResumirCarrinhoPedido pedido)
    {
        var linhas = (pedido.Itens ?? []).Select(item => new ResumirCarrinhoLinha(
            item.ProdutoId ?? "",
            item.Titulo ?? "",
            item.PrecoUnitario,
            item.Quantidade,
            item.Cor,
            item.Tamanho,
            item.Detalhes,
            item.PrecoUnitario * item.Quantidade)).ToList();

        var subtotal = CarrinhoLista.Subtotal(linhas.Select(linha => new CarrinhoLista.Linha(linha.PrecoUnitario, linha.Quantidade)));
        return new ResumirCarrinhoResposta(subtotal, linhas);
    }
}
