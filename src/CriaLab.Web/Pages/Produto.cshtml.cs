using System.Security.Claims;
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
    private readonly FavoritosService _favoritos;
    private readonly FreteService _frete;

    public ProdutoModel(ApplicationDbContext db, CarrinhoService carrinho, FavoritosService favoritos, FreteService frete)
    {
        _db = db;
        _carrinho = carrinho;
        _favoritos = favoritos;
        _frete = frete;
    }

    public Produto? Produto { get; private set; }
    public IList<Avaliacao> Avaliacoes { get; private set; } = new List<Avaliacao>();
    public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();
    public IList<Produto> Relacionados { get; private set; } = new List<Produto>();
    public LojaIdentidade? Identidade { get; private set; }
    public ConfiguracaoLoja? Config { get; private set; }
    public bool Favorito { get; private set; }
    public bool ToastCarrinho { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        await CarregarAsync(id);
        if (Produto == null) return NotFound();
        ToastCarrinho = TempData["ToastCarrinho"] as string == "1";
        return Page();
    }

    public async Task<IActionResult> OnPostAdicionarAsync(string id, int quantidade, string? cor, string? tamanho, string? detalhes, string? destino)
    {
        var produto = await _db.Produtos.AsNoTracking()
            .Include(x => x.EtapasPersonalizacao)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (produto == null) return NotFound();

        var qtd = Math.Max(1, quantidade);
        if (string.Equals(produto.ModalidadeId, "pronta-entrega", StringComparison.OrdinalIgnoreCase) && produto.Estoque > 0 && qtd > produto.Estoque)
            qtd = produto.Estoque;

        _carrinho.Adicionar(new CarrinhoItem
        {
            ProdutoId = produto.Id,
            Titulo = produto.Titulo,
            PrecoUnitario = produto.Preco,
            Quantidade = qtd,
            Cor = string.IsNullOrWhiteSpace(cor) ? null : cor.Trim(),
            Tamanho = string.IsNullOrWhiteSpace(tamanho) ? null : tamanho.Trim(),
            Detalhes = string.IsNullOrWhiteSpace(detalhes) ? null : detalhes.Trim()
        });

        if (string.Equals(destino, "checkout", StringComparison.OrdinalIgnoreCase))
            return RedirectToPage("/Checkout");

        TempData["ToastCarrinho"] = "1";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostFavoritoAsync(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
            await _favoritos.AlternarAsync(id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostOrcamentoAsync(string id, string? detalhes)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToPage("/Conta/Login", new { returnUrl = Url.Page("/Produto", new { id }) });

        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (produto == null) return NotFound();

        _db.OrcamentosCliente.Add(new OrcamentoCliente
        {
            Id = "orc-" + Guid.NewGuid().ToString("N")[..8],
            UsuarioId = userId,
            ProdutoId = produto.Id,
            Titulo = produto.Titulo,
            Status = "aguardando",
            Detalhes = string.IsNullOrWhiteSpace(detalhes) ? "Sem detalhes" : detalhes.Trim(),
            CriadoEm = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToPage("/Conta/Index", new { aba = "orcamentos" });
    }

    public async Task<IActionResult> OnPostPerguntaAsync(string id, string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return RedirectToPage(new { id });
        var autor = User.Identity?.IsAuthenticated == true
            ? (User.Identity.Name ?? "Cliente")
            : "Visitante";
        _db.PerguntasProduto.Add(new PerguntaProduto
        {
            Id = "per-" + Guid.NewGuid().ToString("N")[..8],
            ProdutoId = id,
            Autor = autor,
            Texto = texto.Trim(),
            Resposta = "",
            CriadoEm = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnGetCotarFreteAsync(string id, string? cep)
    {
        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (produto == null) return NotFound();
        var (erro, opcoes, freteGratis) = await _frete.CotarAsync(cep, produto.Preco);
        if (erro != null) return new JsonResult(new { erro });
        var gratis = produto.Preco >= freteGratis;
        return new JsonResult(new
        {
            opcoes = opcoes.Select(o => new
            {
                o.Id,
                o.Nome,
                preco = gratis ? 0m : o.Preco,
                o.Dias,
                gratis
            })
        });
    }

    private async Task CarregarAsync(string id)
    {
        Produto = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Include(x => x.EtapasPersonalizacao)
            .Include(x => x.Variacoes).ThenInclude(v => v.Opcoes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (Produto == null) return;

        Avaliacoes = await _db.Avaliacoes.AsNoTracking().Where(x => x.ProdutoId == id).ToListAsync();
        Perguntas = await _db.PerguntasProduto.AsNoTracking().Where(x => x.ProdutoId == id).OrderByDescending(x => x.CriadoEm).ToListAsync();
        Relacionados = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Where(x => x.CategoriaId == Produto.CategoriaId && x.Id != id && x.Situacao == "publicado")
            .Take(4)
            .ToListAsync();
        Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
        Config = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        ViewData["FreteGratisAPartir"] = Config?.FreteGratisAPartir ?? 200m;

        Favorito = await _favoritos.EstaFavoritoAsync(id);
    }
}
