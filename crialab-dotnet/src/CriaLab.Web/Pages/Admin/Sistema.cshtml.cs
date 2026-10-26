using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class SistemaModel : PageModel {
  private readonly ApplicationDbContext _db;
  public SistemaModel(ApplicationDbContext db) => _db = db;
  public ConfiguracaoLoja? Config { get; private set; }
  public IList<FaixaCep> Faixas { get; private set; } = new List<FaixaCep>();
  public IList<TipoInsumoConfig> TiposInsumo { get; private set; } = new List<TipoInsumoConfig>();
  public async Task OnGetAsync() {
    Config = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
    Faixas = await _db.FaixasCep.AsNoTracking().OrderBy(x => x.De).ToListAsync();
    TiposInsumo = await _db.TiposInsumoConfig.AsNoTracking().OrderBy(x => x.Ordem).ToListAsync();
  }
}