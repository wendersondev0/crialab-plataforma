using CriaLab.Domain.Entidades;

namespace CriaLab.Domain.Calculos;

public static class PrazoVitrine
{
    public const string AposOrcamento = "apos-orcamento";
    public const string Entrega = "entrega";

    public sealed record Dados(
        string Tipo,
        string Modalidade,
        bool FreteGratis,
        int PrazoDias,
        DateOnly? Inicio,
        DateOnly? Fim);

    public static Dados Calcular(
        Produto produto,
        decimal freteGratisAPartir,
        int pacDias = 5,
        DateOnly? hoje = null) =>
        Calcular(produto.ModalidadeId, produto.PrazoDias, produto.Preco, freteGratisAPartir, pacDias, hoje);

    public static Dados Calcular(
        string? modalidadeId,
        int prazoDias,
        decimal preco,
        decimal freteGratisAPartir,
        int pacDias = 5,
        DateOnly? hoje = null)
    {
        var dia = hoje ?? DateOnly.FromDateTime(DateTime.Today);
        var prazo = prazoDias > 0 ? prazoDias : 5;
        var gratis = preco >= freteGratisAPartir;

        if (string.Equals(modalidadeId, "personalizado", StringComparison.OrdinalIgnoreCase))
            return new Dados(AposOrcamento, "personalizado", gratis, prazo, null, null);

        if (string.Equals(modalidadeId, "sob-demanda", StringComparison.OrdinalIgnoreCase))
        {
            return new Dados(
                Entrega,
                "sob-demanda",
                gratis,
                prazo,
                dia.AddDays(prazo),
                dia.AddDays(prazo + pacDias));
        }

        return new Dados(
            Entrega,
            "pronta-entrega",
            gratis,
            prazo,
            dia.AddDays(Math.Max(2, pacDias - 1)),
            dia.AddDays(pacDias + 3));
    }
}
