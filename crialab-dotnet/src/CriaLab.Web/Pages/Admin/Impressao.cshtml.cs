using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class ImpressaoModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ImpressaoModel(ApplicationDbContext db) => _db = db;
  public IList<ImpressaoJob> Jobs { get; private set; } = new List<ImpressaoJob>();
  public async Task OnGetAsync() => Jobs = await _db.ImpressaoJobs.AsNoTracking().Include(x => x.Maquina).OrderByDescending(x => x.Inicio).ToListAsync();
}