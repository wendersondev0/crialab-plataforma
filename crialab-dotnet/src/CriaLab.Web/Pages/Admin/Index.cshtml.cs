using CriaLab.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _db;
  public IndexModel(ApplicationDbContext db) => _db = db;
  public int TotalProdutos { get; private set; }
  public int TotalPedidos { get; private set; }
  public int MensagensAbertas { get; private set; }
  public int JobsImpressao { get; private set; }
  public async Task OnGetAsync() {
    TotalProdutos = await _db.Produtos.CountAsync(x => x.Situacao == "publicado");
    TotalPedidos = await _db.Pedidos.CountAsync();
    MensagensAbertas = await _db.MensagensContato.CountAsync(x => x.Status == "aberto");
    JobsImpressao = await _db.ImpressaoJobs.CountAsync();
  }
}