using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class PedidosModel : PageModel {
  private readonly ApplicationDbContext _db;
  public PedidosModel(ApplicationDbContext db) => _db = db;
  public IList<Pedido> Pedidos { get; private set; } = new List<Pedido>();
  public async Task OnGetAsync() => Pedidos = await _db.Pedidos.AsNoTracking().Include(x => x.Itens).OrderByDescending(x => x.CriadoEm).ToListAsync();
}