using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ProdutosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ProdutosModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Modalidade { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Situacao { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Ordem { get; set; } = "titulo";

    [BindProperty(SupportsGet = true)]
    public bool Desc { get; set; }

    public IList<Produto> Produtos { get; private set; } = new List<Produto>();
    public int TotalTodos { get; private set; }
    public int TotalPronta { get; private set; }
    public int TotalDemanda { get; private set; }
    public int TotalPers { get; private set; }

    public async Task OnGetAsync()
    {
        var baseQ = _db.Produtos.AsNoTracking();
        TotalTodos = await baseQ.CountAsync();
        TotalPronta = await baseQ.CountAsync(x => x.ModalidadeId == "pronta-entrega");
        TotalDemanda = await baseQ.CountAsync(x => x.ModalidadeId == "sob-demanda");
        TotalPers = await baseQ.CountAsync(x => x.ModalidadeId == "personalizado");

        var q = _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Categoria)
            .Include(x => x.Modalidade)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Modalidade))
            q = q.Where(x => x.ModalidadeId == Modalidade);
        if (!string.IsNullOrWhiteSpace(Situacao))
            q = q.Where(x => x.Situacao == Situacao);
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var termo = Q.Trim().ToLower();
            q = q.Where(x => x.Titulo.ToLower().Contains(termo) || x.Descricao.ToLower().Contains(termo));
        }

        q = (Ordem?.ToLower()) switch
        {
            "situacao" => Desc ? q.OrderByDescending(x => x.Situacao) : q.OrderBy(x => x.Situacao),
            "preco" => Desc ? q.OrderByDescending(x => x.Preco) : q.OrderBy(x => x.Preco),
            "estoque" => Desc ? q.OrderByDescending(x => x.Estoque) : q.OrderBy(x => x.Estoque),
            "vendas" => Desc ? q.OrderByDescending(x => x.Vendas) : q.OrderBy(x => x.Vendas),
            _ => Desc ? q.OrderByDescending(x => x.Titulo) : q.OrderBy(x => x.Titulo)
        };

        Produtos = await q.ToListAsync();
    }

    public async Task<IActionResult> OnPostPausarAsync(string id)
    {
        var p = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null) { p.Situacao = "pausado"; await _db.SaveChangesAsync(); }
        return RedirectToPage(new { Q, Modalidade, Situacao, Ordem, Desc });
    }

    public async Task<IActionResult> OnPostPublicarAsync(string id)
    {
        var p = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null) { p.Situacao = "publicado"; await _db.SaveChangesAsync(); }
        return RedirectToPage(new { Q, Modalidade, Situacao, Ordem, Desc });
    }

    public async Task<IActionResult> OnPostExcluirAsync(string id)
    {
        var p = await _db.Produtos
            .Include(x => x.Fotos)
            .Include(x => x.EtapasPersonalizacao)
            .Include(x => x.Variacoes).ThenInclude(v => v.Opcoes)
            .Include(x => x.CoresLegado)
            .Include(x => x.TamanhosLegado)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            _db.Produtos.Remove(p);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { Q, Modalidade, Situacao, Ordem, Desc });
    }

    public async Task<IActionResult> OnPostDuplicarAsync(string id)
    {
        var origem = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (origem == null) return RedirectToPage();

        var novoId = "prod-" + Guid.NewGuid().ToString("N")[..8];
        var copia = new Produto
        {
            Id = novoId,
            Titulo = origem.Titulo + " (cópia)",
            Sessao = origem.Sessao,
            CategoriaId = origem.CategoriaId,
            ModalidadeId = origem.ModalidadeId,
            Tipo = origem.Tipo,
            Descricao = origem.Descricao,
            Preco = origem.Preco,
            PrecoComparativo = origem.PrecoComparativo,
            PrecoArquivo = origem.PrecoArquivo,
            Estoque = origem.Estoque,
            Minimo = origem.Minimo,
            Medidas = origem.Medidas,
            Peso = origem.Peso,
            Material = origem.Material,
            PrazoDias = origem.PrazoDias,
            ComArquivo = origem.ComArquivo,
            Arquivo = origem.Arquivo,
            Personalizavel = origem.Personalizavel,
            Vendas = 0,
            Nota = 0,
            Oferta = origem.Oferta,
            MaisVendido = false,
            Situacao = "pausado",
            AVista = origem.AVista,
            CriadoEm = DateOnly.FromDateTime(DateTime.UtcNow)
        };
        _db.Produtos.Add(copia);
        foreach (var f in origem.Fotos.OrderBy(x => x.Ordem))
        {
            _db.ProdutoFotos.Add(new ProdutoFoto
            {
                ProdutoId = novoId,
                Arquivo = f.Arquivo,
                Sessao = f.Sessao,
                Legenda = f.Legenda,
                Ordem = f.Ordem
            });
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/ProdutoEdit", new { id = novoId });
    }
}
