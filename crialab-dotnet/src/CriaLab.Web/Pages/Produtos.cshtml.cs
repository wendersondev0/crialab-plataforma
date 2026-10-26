using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class ProdutosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ProdutosModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? ModalidadeFiltro { get; set; }

    public IList<Produto> Produtos { get; private set; } = new List<Produto>();
    public SelectList ModalidadeOpcoes { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        var modalidades = await _db.Modalidades.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        ModalidadeOpcoes = new SelectList(modalidades, "Id", "Nome", ModalidadeFiltro);
        var q = _db.Produtos.AsNoTracking().Include(x => x.Fotos).Include(x => x.Modalidade).Where(x => x.Situacao == "publicado");
        if (!string.IsNullOrWhiteSpace(ModalidadeFiltro))
            q = q.Where(x => x.ModalidadeId == ModalidadeFiltro);
        Produtos = await q.OrderBy(x => x.Titulo).ToListAsync();
    }
}
