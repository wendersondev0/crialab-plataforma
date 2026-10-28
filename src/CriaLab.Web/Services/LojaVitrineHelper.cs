using CriaLab.Web.Data.Entities;

namespace CriaLab.Web.Services;

public static class LojaVitrineHelper
{
    public static string TextoEntrega(Produto produto, decimal freteGratisAPartir, int pacDias = 5)
    {
        var hoje = DateTime.Today;
        var prazo = produto.PrazoDias > 0 ? produto.PrazoDias : 5;
        var mod = produto.ModalidadeId;
        if (string.Equals(mod, "personalizado", StringComparison.OrdinalIgnoreCase))
            return $"Prazo após orçamento · cerca de <strong class=\"font-extrabold\">{prazo} dias</strong>";

        DateTime ini;
        DateTime fim;
        if (string.Equals(mod, "sob-demanda", StringComparison.OrdinalIgnoreCase))
        {
            ini = hoje.AddDays(prazo);
            fim = hoje.AddDays(prazo + pacDias);
        }
        else
        {
            ini = hoje.AddDays(Math.Max(2, pacDias - 1));
            fim = hoje.AddDays(pacDias + 3);
        }

        var gratis = produto.Preco >= freteGratisAPartir;
        var prefixo = gratis ? "Entrega GRÁTIS: " : "Entrega: ";
        return prefixo + $"<strong class=\"font-extrabold\">{FaixaDias(ini, fim)}</strong>";
    }

    public static string ClasseTag(string? modalidadeId) =>
        string.Equals(modalidadeId, "personalizado", StringComparison.OrdinalIgnoreCase) ? "tag-pers" :
        string.Equals(modalidadeId, "sob-demanda", StringComparison.OrdinalIgnoreCase) ? "tag-demanda" : "tag-pronta";

    private static string FaixaDias(DateTime ini, DateTime fim)
    {
        if (ini.Month == fim.Month && ini.Year == fim.Year)
            return $"{ini.Day} a {fim.Day} de {Mes(ini)}";
        return $"{ini.Day} de {Mes(ini)} a {fim.Day} de {Mes(fim)}";
    }

    private static string Mes(DateTime d) => d.ToString("MMM", new System.Globalization.CultureInfo("pt-BR"));
}
