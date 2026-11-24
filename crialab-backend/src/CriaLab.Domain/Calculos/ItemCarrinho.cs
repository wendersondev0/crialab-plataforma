namespace CriaLab.Domain.Calculos;

public static class ItemCarrinho
{
    public sealed record Normalizado(int Quantidade, string? Cor, string? Tamanho, string? Detalhes, string Destino);

    public static Normalizado Preparar(
        string? modalidadeId,
        int estoque,
        int quantidade,
        string? cor,
        string? tamanho,
        string? detalhes,
        string? destino)
    {
        var qtd = Math.Max(1, quantidade);
        if (string.Equals(modalidadeId, "pronta-entrega", StringComparison.OrdinalIgnoreCase) && estoque > 0 && qtd > estoque)
            qtd = estoque;

        var destinoNorm = string.Equals(destino, "checkout", StringComparison.OrdinalIgnoreCase) ? "checkout" : "toast";
        return new Normalizado(qtd, Limpar(cor), Limpar(tamanho), Limpar(detalhes), destinoNorm);
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
