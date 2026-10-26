using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class FinanceiroModel : PageModel {
  private readonly ApplicationDbContext _db;
  public FinanceiroModel(ApplicationDbContext db) => _db = db;
  public IList<ContaFinanceira> Contas { get; private set; } = new List<ContaFinanceira>();
  public IList<LancamentoFinanceiro> Lancamentos { get; private set; } = new List<LancamentoFinanceiro>();
  public async Task OnGetAsync() {
    Contas = await _db.ContasFinanceiras.AsNoTracking().ToListAsync();
    Lancamentos = await _db.LancamentosFinanceiros.AsNoTracking().OrderByDescending(x => x.Competencia).Take(30).ToListAsync();
  }
}