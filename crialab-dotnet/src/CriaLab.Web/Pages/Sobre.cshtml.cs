using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
public class SobreModel : PageModel {
  private readonly ApplicationDbContext _db;
  public SobreModel(ApplicationDbContext db) => _db = db;
  public LojaIdentidade? Identidade { get; private set; }
  public MenuAjuda? Ajuda { get; private set; }
  public async Task OnGetAsync() {
    Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
    Ajuda = await _db.MenusAjuda.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "sobre");
  }
}
