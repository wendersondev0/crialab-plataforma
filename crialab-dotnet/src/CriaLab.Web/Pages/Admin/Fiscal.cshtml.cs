using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class FiscalModel : PageModel {
  private readonly ApplicationDbContext _db;
  public FiscalModel(ApplicationDbContext db) => _db = db;
  public LojaIdentidade? Identidade { get; private set; }
  public int PedidosNotaPendente { get; private set; }
  public async Task OnGetAsync() {
    Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
    PedidosNotaPendente = await _db.Pedidos.CountAsync(x => x.Nota == "pendente");
  }
}