using CriaLab.Web.Data.Entities;

namespace CriaLab.Web.Services;

public record MesResumo(
    string Competencia,
    decimal Receita,
    decimal GastoFixo,
    decimal Filamento,
    decimal OutroInsumo,
    decimal Energia,
    decimal Investimento,
    decimal DespesaOperacional,
    decimal Resultado,
    decimal Quantidade);

public static class FinanceiroResumo
{
    public static readonly string[] Categorias =
        ["gastoFixo", "energia", "filamento", "outroInsumo", "receita", "investimento"];

    public static string NomeCategoria(string? categoria) => categoria switch
    {
        "gastoFixo" => "Gasto fixo",
        "filamento" => "Filamento",
        "outroInsumo" => "Outro insumo",
        "receita" => "Receita",
        "investimento" => "Investimento",
        "energia" => "Energia",
        _ => categoria ?? ""
    };

    public static MesResumo Resumir(IEnumerable<LancamentoFinanceiro> lancamentos, string competencia)
    {
        var lista = lancamentos.Where(x => x.Competencia == competencia && !x.Estornado).ToList();
        decimal Soma(string cat) => lista.Where(x => x.Categoria == cat).Sum(x => x.Valor);
        var receita = Soma("receita");
        var gastoFixo = Soma("gastoFixo");
        var filamento = Soma("filamento");
        var outro = Soma("outroInsumo");
        var energia = Soma("energia");
        var investimento = Soma("investimento");
        var despesaOp = gastoFixo + filamento + outro + energia;
        var qtd = lista.Where(x => x.Categoria == "receita").Sum(x => x.Quantidade);
        return new MesResumo(competencia, receita, gastoFixo, filamento, outro, energia, investimento, despesaOp, receita - despesaOp, qtd);
    }

    public static decimal MedianaKg(IEnumerable<LancamentoFinanceiro> lancamentos, string tipoInsumo, IEnumerable<string> meses)
    {
        var set = meses.ToHashSet();
        var precos = lancamentos
            .Where(x => !x.Estornado && x.Categoria == "filamento" && x.TipoInsumo == tipoInsumo && set.Contains(x.Competencia) && x.Quantidade > 0)
            .Select(x => x.Valor / x.Quantidade)
            .OrderBy(x => x)
            .ToList();
        if (precos.Count == 0) return 0;
        var mid = precos.Count / 2;
        return precos.Count % 2 == 0 ? (precos[mid - 1] + precos[mid]) / 2 : precos[mid];
    }
}
