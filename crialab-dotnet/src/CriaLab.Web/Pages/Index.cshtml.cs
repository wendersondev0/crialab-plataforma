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
    public IList<Categoria> Categorias { get; private set; } = new List<Categoria>();
    public IList<Produto> Produtos { get; private set; } = new List<Produto>();
    public string IntroHome { get; private set; } = "Explore a vitrine da Cria Lab.";
    public decimal FreteGratisAPartir { get; private set; } = 200;

    public async Task OnGetAsync()
    {
        var ordem = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["pronta-entrega"] = 0,
            ["sob-demanda"] = 1,
            ["personalizado"] = 2
        };

        Modalidades = (await _db.Modalidades.AsNoTracking().ToListAsync())
            .OrderBy(x => ordem.GetValueOrDefault(x.Id, 9))
            .ThenBy(x => x.Nome)
            .ToList();

        Categorias = await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();

        var ajuda = await _db.MenusAjuda.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "inicio");
        if (!string.IsNullOrWhiteSpace(ajuda?.Texto))
            IntroHome = ajuda.Texto;

        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        if (cfg != null)
            FreteGratisAPartir = cfg.FreteGratisAPartir;

        Produtos = (await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Where(x => x.Situacao == "publicado")
            .ToListAsync())
            .OrderBy(x => ordem.GetValueOrDefault(x.ModalidadeId, 9))
            .ThenByDescending(x => x.CriadoEm)
            .ToList();
    }
}
