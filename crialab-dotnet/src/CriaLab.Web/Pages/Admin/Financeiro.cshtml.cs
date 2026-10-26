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
    private readonly ApplicationDbContext _db;

    public FinanceiroModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "resumo";
    public string Mes { get; private set; } = "";
    public IList<ContaFinanceira> Contas { get; private set; } = new List<ContaFinanceira>();
    public IList<LancamentoFinanceiro> Lancamentos { get; private set; } = new List<LancamentoFinanceiro>();
    public IList<string> Competencias { get; private set; } = new List<string>();
    public MesResumo ResumoMes { get; private set; } = FinanceiroResumo.Resumir(Array.Empty<LancamentoFinanceiro>(), "");
    public IList<MesResumo> Historico { get; private set; } = new List<MesResumo>();
    public decimal APagar { get; private set; }
    public decimal AReceber { get; private set; }
    public decimal MetaMes { get; private set; }

    [BindProperty]
    public string ContaTipo { get; set; } = "pagar";

    [BindProperty]
    public string ContaDescricao { get; set; } = "";

    [BindProperty]
    public decimal ContaValor { get; set; }

    [BindProperty]
    public DateOnly ContaVencimento { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task OnGetAsync(string? aba, string? mes)
    {
        Aba = aba is "contas" or "metas" or "lancamentos" ? aba : "resumo";
        await CarregarAsync(mes);
    }

    public async Task<IActionResult> OnPostBaixarContaAsync(string id, string? aba, string? mes)
    {
        var c = await _db.ContasFinanceiras.FirstOrDefaultAsync(x => x.Id == id);
        if (c != null)
        {
            c.Baixada = true;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Conta baixada.";
        }
        return RedirectToPage(new { aba = aba ?? "contas", mes });
    }

    public async Task<IActionResult> OnPostNovaContaAsync(string? mes)
    {
        if (string.IsNullOrWhiteSpace(ContaDescricao) || ContaValor <= 0)
            return RedirectToPage(new { aba = "contas", mes });

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
        return RedirectToPage(new { aba = "contas", mes });
    }

    public async Task<IActionResult> OnPostMetaAsync(string competencia, decimal valor)
    {
        if (string.IsNullOrWhiteSpace(competencia))
            return RedirectToPage(new { aba = "metas" });

        var meta = await _db.MetasFinanceiras.FirstOrDefaultAsync(x => x.Chave == competencia);
        var json = System.Text.Json.JsonSerializer.Serialize(new { valor });
        if (meta == null)
            _db.MetasFinanceiras.Add(new MetaFinanceira { Chave = competencia, ValorJson = json });
        else
            meta.ValorJson = json;
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Meta salva.";
        return RedirectToPage(new { aba = "metas", mes = competencia });
    }

    private async Task CarregarAsync(string? mes)
    {
        Contas = await _db.ContasFinanceiras.AsNoTracking().OrderBy(x => x.Vencimento).ToListAsync();
        Lancamentos = await _db.LancamentosFinanceiros.AsNoTracking()
            .OrderByDescending(x => x.Data)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        Competencias = Lancamentos.Select(x => x.Competencia).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
        if (!Competencias.Any())
            Competencias = new List<string> { DateTime.Today.ToString("yyyy-MM") };

        Mes = string.IsNullOrWhiteSpace(mes) || !Competencias.Contains(mes)
            ? Competencias.Last()
            : mes!;

        ResumoMes = FinanceiroResumo.Resumir(Lancamentos, Mes);
        Historico = Competencias.Select(c => FinanceiroResumo.Resumir(Lancamentos, c)).ToList();
        APagar = Contas.Where(x => x.Tipo == "pagar" && !x.Baixada).Sum(x => x.Valor);
        AReceber = Contas.Where(x => x.Tipo == "receber" && !x.Baixada).Sum(x => x.Valor);

        var meta = await _db.MetasFinanceiras.AsNoTracking().FirstOrDefaultAsync(x => x.Chave == Mes);
        MetaMes = LerMeta(meta);
        if (MetaMes == 0)
        {
            var idx = Competencias.IndexOf(Mes);
            if (idx > 0)
                MetaMes = FinanceiroResumo.Resumir(Lancamentos, Competencias[idx - 1]).Receita;
        }
    }

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
