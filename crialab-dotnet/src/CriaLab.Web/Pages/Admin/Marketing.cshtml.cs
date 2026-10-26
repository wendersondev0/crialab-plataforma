using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class MarketingModel : PageModel {
  private readonly ApplicationDbContext _db;
  public MarketingModel(ApplicationDbContext db) => _db = db;
  public IList<Depoimento> Depoimentos { get; private set; } = new List<Depoimento>();
  public IList<Parceiro> Parceiros { get; private set; } = new List<Parceiro>();
  public IList<Cupom> Cupons { get; private set; } = new List<Cupom>();
  public async Task OnGetAsync() {
    Depoimentos = await _db.Depoimentos.AsNoTracking().ToListAsync();
    Parceiros = await _db.Parceiros.AsNoTracking().ToListAsync();
    Cupons = await _db.Cupons.AsNoTracking().ToListAsync();
  }
}