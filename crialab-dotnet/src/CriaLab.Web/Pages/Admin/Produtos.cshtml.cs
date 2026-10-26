using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class ProdutosModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ProdutosModel(ApplicationDbContext db) => _db = db;
  public IList<Produto> Produtos { get; private set; } = new List<Produto>();
  public async Task OnGetAsync() => Produtos = await _db.Produtos.AsNoTracking().OrderBy(x => x.Titulo).ToListAsync();
}