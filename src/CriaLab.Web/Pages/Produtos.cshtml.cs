using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class ProdutosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ProdutosModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? ModalidadeFiltro { get; set; }

    public IList<Produto> Produtos { get; private set; } = new List<Produto>();
    public Modalidade? Modalidade { get; private set; }
    public decimal FreteGratisAPartir { get; private set; } = 200;

    public async Task OnGetAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        if (cfg != null)
            FreteGratisAPartir = cfg.FreteGratisAPartir;
        ViewData["FreteGratisAPartir"] = FreteGratisAPartir;

        if (!string.IsNullOrWhiteSpace(ModalidadeFiltro))
            Modalidade = await _db.Modalidades.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ModalidadeFiltro);

        var q = _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Where(x => x.Situacao == "publicado");
        if (!string.IsNullOrWhiteSpace(ModalidadeFiltro))
            q = q.Where(x => x.ModalidadeId == ModalidadeFiltro);
        Produtos = await q.OrderBy(x => x.Titulo).ToListAsync();
    }
}
