using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IList<Modalidade> Modalidades { get; private set; } = new List<Modalidade>();
    public IList<Produto> Produtos { get; private set; } = new List<Produto>();

    public async Task OnGetAsync()
    {
        Modalidades = await _db.Modalidades.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        Produtos = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Where(x => x.Situacao == "publicado")
            .OrderByDescending(x => x.CriadoEm)
            .Take(12)
            .ToListAsync();
    }
}
