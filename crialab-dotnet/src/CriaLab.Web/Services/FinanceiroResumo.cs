using System.Globalization;
using CriaLab.Web.Data.Entities;

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

public record RefInsumo(
    string Grupo,
    string Nome,
    string Unidade,
    decimal Media,
    decimal Mediana,
    decimal Quantidade,
    decimal Valor,
    int Compras);

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

    public static string Dica(string chave) => chave switch
    {
        "receita" => "Soma das vendas (receitas) do período escolhido.",
        "operacao" or "despesaOperacional" => "Custo direto de produzir e despachar: energia + filamentos + outros insumos.",
        "operacional" or "resultadoOperacional" => "Receita − operação (energia, filamentos e outros insumos). Não desconta gastos fixos nem compra de patrimônio.",
        "caixa" or "resultadoCaixa" => "Receita − tudo que saiu no período: operação, gastos fixos e investimentos em patrimônio (ex.: impressoras).",
        "lucro" => "Receita − operação − gastos fixos. Compra de patrimônio não entra neste número.",
        "gastoFixo" or "gastosFixos" => "Despesas recorrentes que não são energia nem insumos de produção (mensalidades, serviços, etc.).",
        "filamento" or "filamentos" => "Compras de PLA, PETG, TPU e outros filamentos tipados.",
        "energia" => "Custo de energia das máquinas no período (com kWh quando houver medição).",
        "investimento" => "Compra de patrimônio (impressoras, equipamentos). Sai do caixa, mas não do resultado operacional.",
        "outroInsumo" or "outros" => "Embalagens, argolas, NFC, verniz, LED e demais insumos que não são filamento.",
        "quantidade" => "Quantidade de peças/itens somada nas receitas do período.",
        "media" => "Preço médio unitário: valor total das compras ÷ quantidade (lançamentos tipados, sem estorno).",
        "mediana" => "Mediana dos preços unitários por competência — menos sensível a picos pontuais que a média.",
        _ => ""
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
        IEnumerable<LancamentoFinanceiro> lancamentos, string? competencia, string tipo) =>
        MediaTipo(lancamentos, competencia, "filamento", tipo);

    public static (decimal Quantidade, decimal Valor, decimal Media) MediaTipo(
        IEnumerable<LancamentoFinanceiro> lancamentos, string? competencia, string categoria, string tipo)
    {
        var lista = Filtrar(lancamentos, competencia)
            .Where(x => x.Categoria == categoria &&
                        string.Equals(x.TipoInsumo, tipo, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var qtd = lista.Sum(x => x.Quantidade);
        var valor = lista.Sum(x => x.Valor);
        return (qtd, valor, qtd > 0 ? valor / qtd : 0);
    }

    public static decimal MedianaKg(IEnumerable<LancamentoFinanceiro> lancamentos, string tipoInsumo, IEnumerable<string> meses) =>
        MedianaUnitaria(lancamentos, "filamento", tipoInsumo, meses);

    public static decimal MedianaUnitaria(
        IEnumerable<LancamentoFinanceiro> lancamentos, string categoria, string tipoInsumo, IEnumerable<string> meses)
    {
        var set = meses.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var precos = lancamentos
            .Where(x => !x.Estornado && x.Categoria == categoria &&
                        string.Equals(x.TipoInsumo, tipoInsumo, StringComparison.OrdinalIgnoreCase) &&
                        set.Contains(x.Competencia) && x.Quantidade > 0)
            .Select(x => x.Valor / x.Quantidade)
            .OrderBy(x => x)
            .ToList();
        if (precos.Count == 0) return 0;
        var mid = precos.Count / 2;
        return precos.Count % 2 == 0 ? (precos[mid - 1] + precos[mid]) / 2 : precos[mid];
    }

    public static List<RefInsumo> ReferenciasInsumos(
        IEnumerable<LancamentoFinanceiro> lancamentos, string? periodoFiltro, IEnumerable<string> mesesMediana)
    {
        var meses = mesesMediana.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var baseLista = Filtrar(lancamentos, periodoFiltro)
            .Where(x => x.Categoria is "filamento" or "outroInsumo")
            .Select(x =>
            {
                var tipo = NormalizarTipo(ResolverTipo(x));
                var qtd = QuantidadeEfetiva(x, tipo);
                return (Lan: x, Tipo: tipo, Qtd: qtd);
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Tipo) && x.Qtd > 0 && x.Lan.Valor > 0)
            .ToList();

        return baseLista
            .GroupBy(x => (Cat: x.Lan.Categoria, Tipo: x.Tipo), new TipoKeyComparer())
            .Select(g =>
            {
                var qtd = g.Sum(x => x.Qtd);
                var valor = g.Sum(x => x.Lan.Valor);
                var media = qtd > 0 ? valor / qtd : 0;
                var precos = g
                    .Where(x => meses.Contains(x.Lan.Competencia) && x.Qtd > 0)
                    .Select(x => x.Lan.Valor / x.Qtd)
                    .OrderBy(x => x)
                    .ToList();
                var mediana = MedianaLista(precos);
                var (grupo, unidade) = ClassificarInsumo(g.Key.Cat, g.Key.Tipo);
                return new RefInsumo(
                    grupo,
                    RotuloTipoInsumo(g.Key.Tipo),
                    unidade,
                    media,
                    mediana,
                    qtd,
                    valor,
                    g.Count());
            })
            .Where(x => x.Media > 0 || x.Valor > 0)
            .OrderBy(x => x.Grupo switch
            {
                "Filamento" => 0,
                "Embalagem" => 1,
                "Ferragem / tags" => 2,
                _ => 3
            })
            .ThenByDescending(x => x.Valor)
            .ToList();
    }

    public static string ResolverTipo(LancamentoFinanceiro lan)
    {
        if (!string.IsNullOrWhiteSpace(lan.TipoInsumo))
            return lan.TipoInsumo.Trim();
        return InferirTipoDaDescricao(lan.Descricao, lan.Categoria);
    }

    public static string NormalizarTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo)) return "";
        var t = tipo.Trim().ToUpperInvariant()
            .Replace('_', ' ')
            .Replace('-', ' ');
        while (t.Contains("  ", StringComparison.Ordinal))
            t = t.Replace("  ", " ", StringComparison.Ordinal);

        return t switch
        {
            "CAIXA PEQUENA" or "CAIXA PEQ" => "CAIXA P",
            "CAIXA MEDIA" or "CAIXA MÉDIA" => "CAIXA M",
            "CAIXA GRANDE" => "CAIXA G",
            "SACOLA P" or "SACOLA KRAFT PEQUENA" => "SACOLA KRAFT P",
            "SACOLA G" or "SACOLA GRANDE" or "SACOLA KRAFT GRANDE" => "SACOLA KRAFT G",
            "IMA" or "IMA 8" or "IMÃ" or "ÍMÃ" or "IMA 8X2" => "IMA 8",
            "IMA 10" or "IMA 10X2" => "IMA 10",
            "ARGOLA" or "ELO" or "LAGOSTA" => "ARGOLA",
            "PLA+" or "PLA PLUS" => "PLA",
            "LAMPADA LED" or "LÂMPADA LED" or "LED" => "LAMPADA LED",
            "PAPEL FOTOGRAFICO" or "PAPEL FOTOGRÁFICO" => "PAPEL FOTOGRAFICO",
            "PLASTICO BOLHA" or "PLÁSTICO BOLHA" or "BOLHA" => "PLASTICO BOLHA",
            "SACO ZIP" or "ZIP" or "ZIP LOCK" => "SACO ZIP",
            "PARAFUSO" or "PARAFUSOS" => "PARAFUSO",
            "PAPEL SULFITE" or "SULFITE" => "PAPEL SULFITE",
            _ => t
        };
    }

    public static string InferirTipoDaDescricao(string? descricao, string? categoria)
    {
        var d = (descricao ?? "").ToUpperInvariant()
            .Replace('Ã', 'A').Replace('Á', 'A').Replace('Â', 'A')
            .Replace('É', 'E').Replace('Ê', 'E')
            .Replace('Í', 'I')
            .Replace('Ó', 'O').Replace('Ô', 'O')
            .Replace('Ú', 'U').Replace('Ü', 'U')
            .Replace('Ç', 'C');

        if (categoria == "filamento" || d.Contains("FILAMENTO"))
        {
            if (d.Contains("PETG")) return "PETG";
            if (d.Contains("TPU")) return "TPU";
            if (d.Contains("PLA")) return "PLA";
        }

        if (d.Contains("CAIXA"))
        {
            if (d.Contains("PEQUENA") || d.Contains("16X11") || d.Contains(" P ")) return "CAIXA P";
            if (d.Contains("MEDIA") || d.Contains("19X12")) return "CAIXA M";
            if (d.Contains("GRANDE") || d.Contains("20X20")) return "CAIXA G";
            return "CAIXA M";
        }

        if (d.Contains("SACOLA") || (d.Contains("KRAFT") && d.Contains("PAPEL")))
        {
            if (d.Contains("GRANDE") || d.Contains(" GG") || d.Contains("34X31")) return "SACOLA KRAFT G";
            return "SACOLA KRAFT P";
        }

        if (d.Contains("ZIP")) return "SACO ZIP";
        if (d.Contains("BOLHA")) return "PLASTICO BOLHA";
        if (d.Contains("BARBANTE")) return "BARBANTE";
        if (d.Contains("FOTOGRAF")) return "PAPEL FOTOGRAFICO";
        if (d.Contains("SULFITE")) return "PAPEL SULFITE";
        if (d.Contains("FITA") && (d.Contains("EMBALAG") || d.Contains("ADESIV") || d.Contains("MARROM") || d.Contains("TRANSP")))
            return "FITA";
        if (d.Contains("ARGOLA") || d.Contains("LAGOSTA") || (d.Contains("ELO") && d.Contains("CHAVEIRO")))
            return "ARGOLA";
        if (d.Contains("IMA") || d.Contains("NEODIMIO") || d.Contains("NEODYM"))
            return d.Contains("10X2") || d.Contains("10MM") ? "IMA 10" : "IMA 8";
        if (d.Contains("PARAFUS")) return "PARAFUSO";
        if (d.Contains("NFC")) return "NFC";
        if (d.Contains("VERNIZ") || (d.Contains("SPRAY") && d.Contains("TINTA"))) return "VERNIZ";
        if (d.Contains("COPO") && d.Contains("ALUMINIO")) return "COPO";
        if (d.Contains("LAMPADA") || (d.Contains("LED") && (d.Contains("KIT") || d.Contains("PECAS"))))
            return "LAMPADA LED";
        if (d.Contains("COLA")) return "COLA";
        if (d.Contains("SILICA")) return "SILICA";
        if (d.Contains("CARTAO") || d.Contains("TAG DE PRESENTE")) return "CARTAO";
        return "";
    }

    public static decimal QuantidadeEfetiva(LancamentoFinanceiro lan, string tipoNormalizado)
    {
        if (lan.Quantidade > 1) return lan.Quantidade;
        if (lan.Categoria == "filamento") return lan.Quantidade > 0 ? lan.Quantidade : 1;

        var d = lan.Descricao ?? "";
        var m = System.Text.RegularExpressions.Regex.Match(
            d,
            @"(?i)(?:kit\s*)?(\d{2,4})\s*(?:unidades?|pecas?|peças?|pcs?|caixas?|sacolas?|folhas?|etiquetas?|tags?|imas?|ímãs?|parafusos?)");
        if (m.Success && decimal.TryParse(m.Groups[1].Value, out var pack) && pack > 1)
            return pack;

        m = System.Text.RegularExpressions.Regex.Match(d, @"(?i)^(\d{2,4})\s+(?:caixas?|sacolas?|sacos?|imas?|ímãs?)");
        if (m.Success && decimal.TryParse(m.Groups[1].Value, out pack) && pack > 1)
            return pack;

        m = System.Text.RegularExpressions.Regex.Match(d, @"(?i)(\d{2,4})\s*pe[cç]as?");
        if (m.Success && decimal.TryParse(m.Groups[1].Value, out pack) && pack > 1)
            return pack;

        if (tipoNormalizado is "BARBANTE" or "PLASTICO BOLHA")
        {
            m = System.Text.RegularExpressions.Regex.Match(d, @"(?i)(\d+(?:[.,]\d+)?)\s*m\b");
            if (m.Success && decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out pack) && pack > 0)
                return pack;
        }

        return lan.Quantidade > 0 ? lan.Quantidade : 1;
    }

    public static (string Grupo, string Unidade) ClassificarInsumo(string categoria, string tipo)
    {
        if (categoria == "filamento")
            return ("Filamento", "kg");

        var t = NormalizarTipo(tipo);
        if (t.Contains("CAIXA") || t.Contains("SACOLA") || t.Contains("SACO") || t.Contains("PAPEL") ||
            t.Contains("FITA") || t.Contains("BOLHA") || t.Contains("ZIP") || t.Contains("BARBANTE") ||
            t.Contains("CARTAO") || t.Contains("CARTÃO"))
            return ("Embalagem", UnidadeDe(t));

        if (t.Contains("ARGOLA") || t.Contains("IMA") || t.Contains("ÍMÃ") || t.Contains("IMÃ") ||
            t.Contains("PARAFUS") || t.Contains("NFC"))
            return ("Ferragem / tags", UnidadeDe(t));

        return ("Acabamento e outros", UnidadeDe(t));
    }

    public static string RotuloTipoInsumo(string tipo)
    {
        var key = NormalizarTipo(tipo);
        return key switch
        {
            "PLA" => "PLA",
            "PETG" => "PETG",
            "TPU" => "TPU",
            "NFC" => "Tags NFC",
            "COPO" => "Copo alumínio",
            "VERNIZ" => "Verniz / spray",
            "LAMPADA LED" => "Lâmpada LED",
            "ARGOLA" => "Argola / elo",
            "IMA 8" => "Ímã 8×2 mm",
            "IMA 10" => "Ímã 10×2 mm",
            "CAIXA P" => "Caixa P",
            "CAIXA M" => "Caixa M",
            "CAIXA G" => "Caixa G",
            "SACOLA KRAFT P" => "Sacola kraft P",
            "SACOLA KRAFT G" => "Sacola kraft G",
            "PAPEL FOTOGRAFICO" => "Papel fotográfico",
            "PAPEL SULFITE" => "Papel sulfite",
            "BARBANTE" => "Barbante",
            "CARTAO" => "Cartão / tag",
            "FITA" => "Fita adesiva",
            "PLASTICO BOLHA" => "Plástico bolha",
            "SACO ZIP" => "Saco zip",
            "COLA" => "Cola",
            "SILICA" => "Sílica gel",
            "PARAFUSO" => "Parafuso / porca",
            "SOQUETE LED" => "Soquete LED",
            _ => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(key.ToLowerInvariant())
        };
    }

    private static string UnidadeDe(string tipoUpper) =>
        tipoUpper.Contains("BARBANTE") || tipoUpper.Contains("BOLHA") ? "m" : "un";

    private static decimal MedianaLista(List<decimal> precos)
    {
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

    private sealed class TipoKeyComparer : IEqualityComparer<(string Cat, string Tipo)>
    {
        public bool Equals((string Cat, string Tipo) x, (string Cat, string Tipo) y) =>
            string.Equals(x.Cat, y.Cat, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Tipo, y.Tipo, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Cat, string Tipo) obj) =>
            HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Cat),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Tipo));
    }
}
