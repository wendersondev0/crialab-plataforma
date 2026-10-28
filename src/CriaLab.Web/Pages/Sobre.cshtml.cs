using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class SobreModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public SobreModel(ApplicationDbContext db) => _db = db;

    public LojaIdentidade? Identidade { get; private set; }
    public IList<Parceiro> Parceiros { get; private set; } = new List<Parceiro>();

    public async Task OnGetAsync()
    {
        Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
        Parceiros = await _db.Parceiros.AsNoTracking()
            .Where(x => x.PodeAparecer)
            .OrderBy(x => x.Nome)
            .ToListAsync();
    }
}
