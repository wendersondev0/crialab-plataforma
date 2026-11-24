namespace CriaLab.Domain.Calculos;

public static class CarrinhoLista
{
    public readonly record struct Linha(decimal PrecoUnitario, int Quantidade);

    public static bool SaiNaRemocao(string produtoId, string? cor, string? tamanho, string itemProdutoId, string? itemCor, string? itemTamanho) =>
        itemProdutoId == produtoId && itemCor == cor && itemTamanho == tamanho;

    public static decimal Subtotal(IEnumerable<Linha> itens) =>
        itens.Sum(item => item.PrecoUnitario * item.Quantidade);
}
