using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class BuscaModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public BuscaModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Termo { get; set; }

    public IList<Produto> Resultados { get; private set; } = new List<Produto>();

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Termo))
            return;
        var t = Termo.Trim().ToLower();
        Resultados = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Where(x => x.Situacao == "publicado" && (x.Titulo.ToLower().Contains(t) || x.Descricao.ToLower().Contains(t)))
            .Take(50)
            .ToListAsync();
    }
}
