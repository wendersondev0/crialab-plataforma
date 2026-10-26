using CriaLab.Web.Data;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ConciliacaoModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ConciliacaoModel(ApplicationDbContext db) => _db = db;

    public IList<string> MesesJanela { get; private set; } = new List<string>();
    public decimal MedianaPlaAba { get; private set; }
    public decimal MedianaPetgAba { get; private set; }
    public decimal MedianaPlaHist { get; private set; }
    public decimal MedianaPetgHist { get; private set; }
    public decimal QuantidadeAba { get; private set; }
    public decimal QuantidadeHist { get; private set; }
    public decimal ReceitaAba { get; private set; }
    public decimal ReceitaHist { get; private set; }
    public decimal DespesaAba { get; private set; }
    public decimal DespesaHist { get; private set; }

    public async Task OnGetAsync()
    {
        var lancamentos = await _db.LancamentosFinanceiros.AsNoTracking().ToListAsync();
        var todas = lancamentos.Select(x => x.Competencia).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList();
        MesesJanela = todas.Take(7).ToList();
        if (!MesesJanela.Any())
            MesesJanela = new List<string> { DateTime.Today.ToString("yyyy-MM") };

        MedianaPlaHist = FinanceiroResumo.MedianaKg(lancamentos, "PLA", MesesJanela);
        MedianaPetgHist = FinanceiroResumo.MedianaKg(lancamentos, "PETG", MesesJanela);
        MedianaPlaAba = MedianaPlaHist;
        MedianaPetgAba = MedianaPetgHist;

        foreach (var mes in MesesJanela)
        {
            var r = FinanceiroResumo.Resumir(lancamentos, mes);
            ReceitaHist += r.Receita;
            QuantidadeHist += r.Quantidade;
            DespesaHist += r.DespesaOperacional + r.Investimento;
        }

        ReceitaAba = ReceitaHist;
        QuantidadeAba = QuantidadeHist;
        DespesaAba = DespesaHist;
    }
}
