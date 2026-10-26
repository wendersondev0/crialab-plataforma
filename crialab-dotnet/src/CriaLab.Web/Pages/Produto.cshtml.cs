using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class ProdutoModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly CarrinhoService _carrinho;

    public ProdutoModel(ApplicationDbContext db, CarrinhoService carrinho)
    {
        _db = db;
        _carrinho = carrinho;
    }

    public Produto? Produto { get; private set; }
    public IList<Avaliacao> Avaliacoes { get; private set; } = new List<Avaliacao>();
    public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        Produto = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Include(x => x.Variacoes).ThenInclude(v => v.Opcoes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (Produto == null) return NotFound();
        Avaliacoes = await _db.Avaliacoes.AsNoTracking().Where(x => x.ProdutoId == id).ToListAsync();
        Perguntas = await _db.PerguntasProduto.AsNoTracking().Where(x => x.ProdutoId == id).OrderByDescending(x => x.CriadoEm).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAdicionarAsync(string id)
    {
        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (produto == null) return NotFound();
        _carrinho.Adicionar(new CarrinhoItem { ProdutoId = produto.Id, Titulo = produto.Titulo, PrecoUnitario = produto.Preco, Quantidade = 1 });
        return RedirectToPage("/Carrinho");
    }
}
