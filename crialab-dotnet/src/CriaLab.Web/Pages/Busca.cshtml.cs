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

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Termo { get; set; }

    [BindProperty(SupportsGet = true, Name = "categoria")]
    public string? Categoria { get; set; }

    public IList<Produto> Resultados { get; private set; } = new List<Produto>();
    public decimal FreteGratisAPartir { get; private set; } = 200;

    public async Task OnGetAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        if (cfg != null)
            FreteGratisAPartir = cfg.FreteGratisAPartir;
        ViewData["FreteGratisAPartir"] = FreteGratisAPartir;

        var query = _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Where(x => x.Situacao == "publicado");

        if (!string.IsNullOrWhiteSpace(Categoria))
            query = query.Where(x => x.CategoriaId == Categoria);

        if (!string.IsNullOrWhiteSpace(Termo))
        {
            var t = Termo.Trim().ToLower();
            query = query.Where(x =>
                x.Titulo.ToLower().Contains(t) ||
                x.Descricao.ToLower().Contains(t) ||
                x.Material.ToLower().Contains(t));
        }

        if (string.IsNullOrWhiteSpace(Termo) && string.IsNullOrWhiteSpace(Categoria))
            return;

        Resultados = await query.OrderBy(x => x.Titulo).Take(50).ToListAsync();
    }
}
