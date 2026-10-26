using CriaLab.Web.Data.Entities;
using System.Globalization;

namespace CriaLab.Web.Services;

public record MesResumo(
    string Competencia,
    decimal Receita,
    decimal GastosFixos,
    decimal Filamentos,
    decimal Outros,
    decimal Energia,
    decimal Investimento,
    decimal DespesaOperacional,
    decimal ResultadoOperacional,
    decimal ResultadoCaixa,
    decimal Lucro,
    decimal Quantidade,
    decimal Kwh)
{
    public decimal GastoFixo => GastosFixos;
    public decimal Filamento => Filamentos;
    public decimal OutroInsumo => Outros;
    public decimal Resultado => ResultadoOperacional;
}

public static class FinanceiroResumo
{
    public static readonly string[] Categorias =
        ["gastoFixo", "energia", "filamento", "outroInsumo", "receita", "investimento"];

    public static readonly (string Id, string Titulo)[] BlocosCaderno =
    [
        ("gastoFixo", "Gastos fixos"),
        ("energia", "Energia"),
        ("filamento", "Filamentos"),
        ("outroInsumo", "Outros insumos"),
        ("receita", "Vendas"),
        ("investimento", "Investimento em patrimônio")
    ];

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

    public static string CompetenciaBr(string? competencia)
    {
        if (string.IsNullOrWhiteSpace(competencia)) return "—";
        if (competencia.Equals("geral", StringComparison.OrdinalIgnoreCase)) return "Geral";
        if (competencia.Length == 4 && int.TryParse(competencia, out _)) return "Ano " + competencia;
        if (competencia.Length >= 7 && DateOnly.TryParse(competencia[..7] + "-01", out var d))
            return d.ToString("MMM/yyyy", new CultureInfo("pt-BR"));
        return competencia;
    }

    public static MesResumo Resumir(IEnumerable<LancamentoFinanceiro> lancamentos, string? competencia)
    {
        var lista = Filtrar(lancamentos, competencia).ToList();
        return Montar(string.IsNullOrWhiteSpace(competencia) ? "geral" : competencia.Trim(), lista);
    }

    public static MesResumo ResumirLista(IEnumerable<LancamentoFinanceiro> lancamentos, string rotulo)
    {
        var lista = lancamentos.Where(x => !x.Estornado).ToList();
        return Montar(rotulo, lista);
    }

    public static IEnumerable<LancamentoFinanceiro> Filtrar(IEnumerable<LancamentoFinanceiro> lancamentos, string? competencia)
    {
        var baseQ = lancamentos.Where(x => !x.Estornado);
        if (string.IsNullOrWhiteSpace(competencia) || competencia.Equals("geral", StringComparison.OrdinalIgnoreCase))
            return baseQ;
        var c = competencia.Trim();
        if (c.Length == 4)
            return baseQ.Where(x => x.Competencia.StartsWith(c, StringComparison.Ordinal));
        return baseQ.Where(x => x.Competencia == c);
    }

    public static (decimal Quantidade, decimal Valor, decimal Media) MediaFilamento(
        IEnumerable<LancamentoFinanceiro> lancamentos, string? competencia, string tipo)
    {
        var lista = Filtrar(lancamentos, competencia)
            .Where(x => x.Categoria == "filamento" &&
                        string.Equals(x.TipoInsumo, tipo, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var qtd = lista.Sum(x => x.Quantidade);
        var valor = lista.Sum(x => x.Valor);
        return (qtd, valor, qtd > 0 ? valor / qtd : 0);
    }

    public static decimal MedianaKg(IEnumerable<LancamentoFinanceiro> lancamentos, string tipoInsumo, IEnumerable<string> meses)
    {
        var set = meses.ToHashSet();
        var precos = lancamentos
            .Where(x => !x.Estornado && x.Categoria == "filamento" &&
                        string.Equals(x.TipoInsumo, tipoInsumo, StringComparison.OrdinalIgnoreCase) &&
                        set.Contains(x.Competencia) && x.Quantidade > 0)
            .Select(x => x.Valor / x.Quantidade)
            .OrderBy(x => x)
            .ToList();
        if (precos.Count == 0) return 0;
        var mid = precos.Count / 2;
        return precos.Count % 2 == 0 ? (precos[mid - 1] + precos[mid]) / 2 : precos[mid];
    }

    private static MesResumo Montar(string rotulo, List<LancamentoFinanceiro> lista)
    {
        decimal Soma(string cat) => lista.Where(x => x.Categoria == cat).Sum(x => x.Valor);
        var receita = Soma("receita");
        var gastosFixos = Soma("gastoFixo");
        var filamentos = Soma("filamento");
        var outros = Soma("outroInsumo");
        var energia = Soma("energia");
        var investimento = Soma("investimento");
        var despesaOp = energia + filamentos + outros;
        var qtd = lista.Where(x => x.Categoria == "receita").Sum(x => x.Quantidade);
        var kwh = lista.Where(x => x.Categoria == "energia" && x.Kwh.HasValue).Sum(x => x.Kwh ?? 0);
        return new MesResumo(
            rotulo,
            receita,
            gastosFixos,
            filamentos,
            outros,
            energia,
            investimento,
            despesaOp,
            receita - despesaOp,
            receita - (despesaOp + gastosFixos + investimento),
            receita - despesaOp - gastosFixos,
            qtd,
            kwh);
    }
}
