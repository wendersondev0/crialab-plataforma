using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class PatrimonioModel : PageModel {
  private readonly ApplicationDbContext _db;
  public PatrimonioModel(ApplicationDbContext db) => _db = db;
  public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
  public IList<EstoqueInsumo> Insumos { get; private set; } = new List<EstoqueInsumo>();
  public async Task OnGetAsync() {
    Maquinas = await _db.Maquinas.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
    Insumos = await _db.EstoqueInsumos.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
  }
}