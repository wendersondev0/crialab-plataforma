using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [BindProperty(SupportsGet = true)]
    public decimal? PrecoMin { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? PrecoMax { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Ordem { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Nota { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FreteGratis { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AVista { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Personalizavel { get; set; }

    public IList<Produto> Resultados { get; private set; } = new List<Produto>();
    public SelectList Categorias { get; private set; } = null!;
    public decimal FreteGratisAPartir { get; private set; } = 200;
    public string Ajuda { get; private set; } = "Filtre peças por texto, tema e preço.";
    public bool TemFiltro { get; private set; }

    public async Task OnGetAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        if (cfg != null)
            FreteGratisAPartir = cfg.FreteGratisAPartir;
        ViewData["FreteGratisAPartir"] = FreteGratisAPartir;

        var cats = await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        Categorias = new SelectList(cats, "Id", "Nome", Categoria);

        var ajuda = await _db.MenusAjuda.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "busca");
        if (ajuda != null && !string.IsNullOrWhiteSpace(ajuda.Texto))
            Ajuda = ajuda.Texto;

        TemFiltro = !string.IsNullOrWhiteSpace(Termo)
            || !string.IsNullOrWhiteSpace(Categoria)
            || PrecoMin.HasValue
            || PrecoMax.HasValue
            || FreteGratis == "1"
            || AVista == "1"
            || Personalizavel == "1"
            || !string.IsNullOrWhiteSpace(Nota)
            || !string.IsNullOrWhiteSpace(Ordem);

        if (!TemFiltro)
            return;

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

        if (PrecoMin.HasValue)
            query = query.Where(x => x.Preco >= PrecoMin.Value);
        if (PrecoMax.HasValue)
            query = query.Where(x => x.Preco <= PrecoMax.Value);
        if (FreteGratis == "1")
            query = query.Where(x => x.Preco >= FreteGratisAPartir);
        if (AVista == "1")
            query = query.Where(x => x.AVista);
        if (Personalizavel == "1")
            query = query.Where(x => x.Personalizavel);
        if (!string.IsNullOrWhiteSpace(Nota) && decimal.TryParse(Nota, out var notaMin))
            query = query.Where(x => x.Nota >= notaMin);

        var lista = await query.ToListAsync();
        Resultados = Ordem switch
        {
            "preco" => lista.OrderBy(x => x.Preco).ToList(),
            "vendas" => lista.OrderByDescending(x => x.Vendas).ToList(),
            "novidade" => lista.OrderByDescending(x => x.CriadoEm).ToList(),
            _ => lista.OrderByDescending(x =>
                    !string.IsNullOrWhiteSpace(Termo) && x.Titulo.Contains(Termo, StringComparison.OrdinalIgnoreCase) ? 2 : 1)
                .ThenByDescending(x => x.Vendas)
                .ThenBy(x => x.Titulo)
                .ToList()
        };
    }

    public async Task<IActionResult> OnGetSugestoesAsync(string? q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return new JsonResult(Array.Empty<object>());

        var t = q.Trim().ToLower();
        var lista = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Where(x => x.Situacao == "publicado" &&
                        (x.Titulo.ToLower().Contains(t) || x.Material.ToLower().Contains(t)))
            .OrderBy(x => x.Titulo)
            .Take(8)
            .Select(x => new
            {
                id = x.Id,
                titulo = x.Titulo,
                preco = x.Preco,
                foto = x.Fotos.OrderBy(f => f.Ordem).Select(f => f.Arquivo).FirstOrDefault()
            })
            .ToListAsync();
        return new JsonResult(lista);
    }
}
