using System.Globalization;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class FinanceiroModel : PageModel
{
    private static readonly HashSet<string> AbasOk = new(StringComparer.OrdinalIgnoreCase)
        { "resumo", "caderno", "lancar", "contas", "lancamentos", "metas" };

    private static readonly HashSet<string> PeriodosOk = new(StringComparer.OrdinalIgnoreCase)
        { "geral", "ano", "mes" };

    private readonly ApplicationDbContext _db;

    public FinanceiroModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "resumo";
    public string Periodo { get; private set; } = "mes";
    public string Ano { get; private set; } = "";
    public string Mes { get; private set; } = "";
    public string? Categoria { get; private set; }
    public string? Texto { get; private set; }
    public string Ordem { get; private set; } = "data";
    public string RotuloPeriodo { get; private set; } = "";
    public string? MesAnterior { get; private set; }
    public string? MesProximo { get; private set; }

    public IList<string> Anos { get; private set; } = new List<string>();
    public IList<string> MesesAno { get; private set; } = new List<string>();
    public IList<string> TodasCompetencias { get; private set; } = new List<string>();
    public IList<ContaFinanceira> Contas { get; private set; } = new List<ContaFinanceira>();
    public IList<LancamentoFinanceiro> LancamentosPeriodo { get; private set; } = new List<LancamentoFinanceiro>();
    public IList<LancamentoFinanceiro> LancamentosCaderno { get; private set; } = new List<LancamentoFinanceiro>();
    public IList<MesResumo> HistoricoBarras { get; private set; } = new List<MesResumo>();
    public IList<TipoInsumoConfig> TiposInsumo { get; private set; } = new List<TipoInsumoConfig>();
    public MesResumo Resumo { get; private set; } = FinanceiroResumo.Resumir(Array.Empty<LancamentoFinanceiro>(), "");
    public MesResumo? ResumoAnterior { get; private set; }
    public decimal APagar { get; private set; }
    public decimal AReceber { get; private set; }
    public decimal MetaMes { get; private set; }
    public decimal MetaAnterior { get; private set; }
    public decimal Projecao { get; private set; }
    public decimal PercentualMeta { get; private set; }
    public bool MetaBatida { get; private set; }
    public decimal MediaPla { get; private set; }
    public decimal MediaPetg { get; private set; }
    public decimal MedianaPla { get; private set; }
    public decimal MedianaPetg { get; private set; }

    [BindProperty] public string ContaTipo { get; set; } = "pagar";
    [BindProperty] public string ContaDescricao { get; set; } = "";
    [BindProperty] public decimal ContaValor { get; set; }
    [BindProperty] public DateOnly ContaVencimento { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty] public string LanDescricao { get; set; } = "";
    [BindProperty] public string LanCategoria { get; set; } = "gastoFixo";
    [BindProperty] public string? LanTipoInsumo { get; set; }
    [BindProperty] public string? LanTipoNovo { get; set; }
    [BindProperty] public decimal LanQuantidade { get; set; } = 1;
    [BindProperty] public decimal LanValor { get; set; }
    [BindProperty] public DateOnly LanData { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty] public string LanCompetencia { get; set; } = "";
    [BindProperty] public string? LanLink { get; set; }
    [BindProperty] public string? LanItemBalcao { get; set; }

    [BindProperty] public string CompraDescricao { get; set; } = "";
    [BindProperty] public string? CompraTipo { get; set; }
    [BindProperty] public decimal CompraQuantidade { get; set; } = 1;
    [BindProperty] public decimal CompraValor { get; set; }

    public async Task OnGetAsync(string? aba, string? periodo, string? ano, string? mes, string? categoria, string? q, string? ordem)
    {
        Aba = NormalizarAba(aba);
        Periodo = !string.IsNullOrWhiteSpace(periodo) && PeriodosOk.Contains(periodo) ? periodo.Trim().ToLowerInvariant() : "mes";
        Categoria = categoria;
        Texto = q;
        Ordem = ordem is "valor" or "dataAsc" ? ordem : "data";
        await CarregarAsync(ano, mes);
    }

    public async Task<IActionResult> OnPostBaixarContaAsync(string id, string? aba, string? periodo, string? ano, string? mes)
    {
        var c = await _db.ContasFinanceiras.FirstOrDefaultAsync(x => x.Id == id);
        if (c != null)
        {
            c.Baixada = true;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Conta baixada.";
        }
        return RedirectToPage(new { aba = aba ?? "contas", periodo, ano, mes });
    }

    public async Task<IActionResult> OnPostNovaContaAsync(string? periodo, string? ano, string? mes)
    {
        if (string.IsNullOrWhiteSpace(ContaDescricao) || ContaValor <= 0)
            return RedirectToPage(new { aba = "contas", periodo, ano, mes });

        _db.ContasFinanceiras.Add(new ContaFinanceira
        {
            Id = "conta-" + Guid.NewGuid().ToString("N")[..8],
            Tipo = ContaTipo is "receber" ? "receber" : "pagar",
            Descricao = ContaDescricao.Trim(),
            Valor = ContaValor,
            Vencimento = ContaVencimento,
            Baixada = false
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Conta registrada.";
        return RedirectToPage(new { aba = "contas", periodo, ano, mes });
    }

    public async Task<IActionResult> OnPostMetaAsync(string competencia, decimal valor, string? periodo, string? ano, string? mes)
    {
        if (string.IsNullOrWhiteSpace(competencia) || competencia.Length < 7)
            return RedirectToPage(new { aba = "resumo", periodo, ano, mes });

        var chave = competencia[..7];
        var meta = await _db.MetasFinanceiras.FirstOrDefaultAsync(x => x.Chave == chave);
        var json = System.Text.Json.JsonSerializer.Serialize(new { valor });
        if (meta == null)
            _db.MetasFinanceiras.Add(new MetaFinanceira { Chave = chave, ValorJson = json });
        else
            meta.ValorJson = json;
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Meta salva.";
        return RedirectToPage(new { aba = "resumo", periodo = periodo ?? "mes", ano = chave[..4], mes = chave });
    }

    public async Task<IActionResult> OnPostBalcaoAsync(string? periodo, string? ano, string? mes)
    {
        if (string.IsNullOrWhiteSpace(LanItemBalcao) || LanValor <= 0)
            return RedirectToPage(new { aba = "lancar", periodo, ano, mes });

        var comp = CompPadrao(LanCompetencia, mes);
        _db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
        {
            Id = "lan-" + Guid.NewGuid().ToString("N")[..8],
            Data = LanData == default ? DateOnly.FromDateTime(DateTime.Today) : LanData,
            Competencia = comp,
            Descricao = LanItemBalcao.Trim(),
            Categoria = "receita",
            Quantidade = LanQuantidade <= 0 ? 1 : LanQuantidade,
            Valor = LanValor,
            Origem = "balcao"
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Receita de balcão lançada.";
        return RedirectToPage(new { aba = "caderno", periodo = "mes", ano = comp[..4], mes = comp, categoria = "receita" });
    }

    public async Task<IActionResult> OnPostLancamentoAsync(string? periodo, string? ano, string? mes)
    {
        if (string.IsNullOrWhiteSpace(LanDescricao) || LanValor <= 0)
            return RedirectToPage(new { aba = "lancar", periodo, ano, mes });

        var comp = CompPadrao(LanCompetencia, mes);
        var tipo = !string.IsNullOrWhiteSpace(LanTipoNovo) ? LanTipoNovo.Trim() : (LanTipoInsumo?.Trim() ?? "");
        var cat = FinanceiroResumo.Categorias.Contains(LanCategoria) ? LanCategoria : "outroInsumo";
        _db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
        {
            Id = "lan-" + Guid.NewGuid().ToString("N")[..8],
            Data = LanData == default ? DateOnly.FromDateTime(DateTime.Today) : LanData,
            Competencia = comp,
            Descricao = LanDescricao.Trim(),
            Categoria = cat,
            TipoInsumo = tipo,
            Quantidade = LanQuantidade <= 0 ? 1 : LanQuantidade,
            Valor = LanValor,
            Link = LanLink?.Trim() ?? "",
            Origem = "manual"
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Lançamento salvo.";
        return RedirectToPage(new { aba = "caderno", periodo = "mes", ano = comp[..4], mes = comp, categoria = cat });
    }

    public async Task<IActionResult> OnPostCompraAsync(string? periodo, string? ano, string? mes)
    {
        if (string.IsNullOrWhiteSpace(CompraDescricao) || CompraValor <= 0)
            return RedirectToPage(new { aba = "caderno", periodo, ano, mes });

        var comp = CompPadrao(null, mes);
        var tipo = CompraTipo?.Trim() ?? CompraDescricao.Trim();
        _db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
        {
            Id = "lan-" + Guid.NewGuid().ToString("N")[..8],
            Data = DateOnly.FromDateTime(DateTime.Today),
            Competencia = comp,
            Descricao = CompraDescricao.Trim(),
            Categoria = "filamento",
            TipoInsumo = tipo,
            Quantidade = CompraQuantidade <= 0 ? 1 : CompraQuantidade,
            Valor = CompraValor,
            Origem = "compra"
        });

        var insumo = await _db.EstoqueInsumos.FirstOrDefaultAsync(x => x.Nome.ToLower() == tipo.ToLower());
        if (insumo != null)
            insumo.Saldo += CompraQuantidade <= 0 ? 1 : CompraQuantidade;

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Compra registrada.";
        return RedirectToPage(new { aba = "caderno", periodo = "mes", ano = comp[..4], mes = comp, categoria = "filamento" });
    }

    public async Task<IActionResult> OnPostEstornarAsync(string id, string? periodo, string? ano, string? mes, string? categoria, string? q)
    {
        var lan = await _db.LancamentosFinanceiros.FirstOrDefaultAsync(x => x.Id == id);
        if (lan != null)
        {
            lan.Estornado = true;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Lançamento estornado.";
        }
        return RedirectToPage(new { aba = "caderno", periodo, ano, mes, categoria, q });
    }

    public IActionResult OnGetCsv(string? periodo, string? ano, string? mes)
    {
        return RedirectToPage(new { aba = "caderno", periodo, ano, mes });
    }

    private static string NormalizarAba(string? aba)
    {
        if (string.IsNullOrWhiteSpace(aba) || !AbasOk.Contains(aba)) return "resumo";
        var a = aba.Trim().ToLowerInvariant();
        if (a is "lancamentos" or "metas") return a == "metas" ? "resumo" : "caderno";
        return a;
    }

    private static string CompPadrao(string? competencia, string? mes)
    {
        if (!string.IsNullOrWhiteSpace(competencia) && competencia.Length >= 7)
            return competencia.Trim()[..7];
        if (!string.IsNullOrWhiteSpace(mes) && mes.Length >= 7)
            return mes.Trim()[..7];
        return DateTime.Today.ToString("yyyy-MM");
    }

    private async Task CarregarAsync(string? ano, string? mes)
    {
        Contas = await _db.ContasFinanceiras.AsNoTracking().OrderBy(x => x.Vencimento).ToListAsync();
        var todos = await _db.LancamentosFinanceiros.AsNoTracking()
            .OrderByDescending(x => x.Data)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
        TiposInsumo = await _db.TiposInsumoConfig.AsNoTracking().OrderBy(x => x.Ordem).ToListAsync();

        TodasCompetencias = todos
            .Select(x => x.Competencia)
            .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 7)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        if (TodasCompetencias.Count == 0)
            TodasCompetencias = new List<string> { DateTime.Today.ToString("yyyy-MM") };

        Anos = TodasCompetencias.Select(x => x[..4]).Distinct().OrderBy(x => x).ToList();
        var ultimo = TodasCompetencias.Last();
        Ano = !string.IsNullOrWhiteSpace(ano) && Anos.Contains(ano) ? ano! : ultimo[..4];

        MesesAno = TodasCompetencias.Where(x => x.StartsWith(Ano)).ToList();
        if (MesesAno.Count == 0)
            MesesAno = Enumerable.Range(1, 12).Select(i => $"{Ano}-{i:00}").ToList();

        if (!string.IsNullOrWhiteSpace(mes) && mes.Length >= 7)
        {
            Mes = mes[..7];
            if (!Mes.StartsWith(Ano))
                Ano = Mes[..4];
            MesesAno = TodasCompetencias.Where(x => x.StartsWith(Ano)).DefaultIfEmpty($"{Ano}-01").ToList();
            if (!MesesAno.Contains(Mes))
                MesesAno = MesesAno.Append(Mes).OrderBy(x => x).ToList();
        }
        else
        {
            Mes = MesesAno.Last();
        }

        var idxMes = TodasCompetencias.IndexOf(Mes);
        MesAnterior = idxMes > 0 ? TodasCompetencias[idxMes - 1] : null;
        MesProximo = idxMes >= 0 && idxMes < TodasCompetencias.Count - 1 ? TodasCompetencias[idxMes + 1] : null;

        string? filtro = Periodo switch
        {
            "geral" => null,
            "ano" => Ano,
            _ => Mes
        };

        Resumo = FinanceiroResumo.Resumir(todos, filtro);
        LancamentosPeriodo = FinanceiroResumo.Filtrar(todos, filtro).ToList();
        RotuloPeriodo = Periodo switch
        {
            "geral" => "Geral",
            "ano" => "Ano " + Ano,
            _ => FinanceiroResumo.CompetenciaBr(Mes)
        };

        HistoricoBarras = Periodo switch
        {
            "geral" => Anos.Select(a => FinanceiroResumo.Resumir(todos, a)).ToList(),
            _ => MesesAno.Select(c => FinanceiroResumo.Resumir(todos, c)).ToList()
        };

        if (Periodo == "mes" && MesAnterior != null)
            ResumoAnterior = FinanceiroResumo.Resumir(todos, MesAnterior);

        APagar = Contas.Where(x => x.Tipo == "pagar" && !x.Baixada).Sum(x => x.Valor);
        AReceber = Contas.Where(x => x.Tipo == "receber" && !x.Baixada).Sum(x => x.Valor);

        MetaMes = await MetaDeAsync(Mes);
        if (MetaMes == 0 && MesAnterior != null)
            MetaMes = FinanceiroResumo.Resumir(todos, MesAnterior).Receita;
        MetaAnterior = MesAnterior == null ? 0 : await MetaDeAsync(MesAnterior);
        if (MetaAnterior == 0 && MesAnterior != null)
        {
            var idxAnt = TodasCompetencias.IndexOf(MesAnterior);
            if (idxAnt > 0)
                MetaAnterior = FinanceiroResumo.Resumir(todos, TodasCompetencias[idxAnt - 1]).Receita;
        }

        if (Periodo == "mes")
        {
            var dia = DateTime.Today.Day;
            var dias = DateTime.DaysInMonth(int.Parse(Mes[..4]), int.Parse(Mes[5..7]));
            PercentualMeta = MetaMes > 0 ? Resumo.Receita / MetaMes * 100m : 0;
            Projecao = dia > 0 ? Resumo.Receita / dia * dias : 0;
            MetaBatida = MetaMes > 0 && Resumo.Receita >= MetaMes;
        }

        var mesesRef = Periodo switch
        {
            "geral" => TodasCompetencias,
            "mes" => new List<string> { Mes },
            _ => MesesAno.ToList()
        };
        var (_, _, mPla) = FinanceiroResumo.MediaFilamento(todos, filtro, "PLA");
        var (_, _, mPetg) = FinanceiroResumo.MediaFilamento(todos, filtro, "PETG");
        MediaPla = mPla;
        MediaPetg = mPetg;
        MedianaPla = FinanceiroResumo.MedianaKg(todos, "PLA", mesesRef);
        MedianaPetg = FinanceiroResumo.MedianaKg(todos, "PETG", mesesRef);

        var caderno = LancamentosPeriodo.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(Categoria))
            caderno = caderno.Where(x => x.Categoria == Categoria);
        if (!string.IsNullOrWhiteSpace(Texto))
        {
            var t = Texto.Trim().ToLowerInvariant();
            caderno = caderno.Where(x =>
                x.Descricao.ToLower().Contains(t) ||
                x.TipoInsumo.ToLower().Contains(t) ||
                x.Link.ToLower().Contains(t));
        }
        LancamentosCaderno = Ordem switch
        {
            "valor" => caderno.OrderByDescending(x => x.Valor).ToList(),
            "dataAsc" => caderno.OrderBy(x => x.Data).ThenBy(x => x.Id).ToList(),
            _ => caderno.OrderByDescending(x => x.Data).ThenByDescending(x => x.Id).ToList()
        };

        LanCompetencia = Mes;
        LanData = DateOnly.FromDateTime(DateTime.Today);
    }

    private async Task<decimal> MetaDeAsync(string competencia)
    {
        var meta = await _db.MetasFinanceiras.AsNoTracking().FirstOrDefaultAsync(x => x.Chave == competencia);
        return LerMeta(meta);
    }

    public static string FormatarReal(decimal valor) =>
        valor.ToString("C", new CultureInfo("pt-BR"));

    public static decimal LerMeta(MetaFinanceira? meta)
    {
        if (meta == null || string.IsNullOrWhiteSpace(meta.ValorJson)) return 0;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(meta.ValorJson);
            if (doc.RootElement.TryGetProperty("valor", out var v))
                return v.GetDecimal();
        }
        catch { /* ignore */ }
        return 0;
    }
}
