using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ProdutoEditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ProdutoEditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public Produto Produto { get; set; } = new();

    public SelectList Categorias { get; private set; } = null!;
    public SelectList Modalidades { get; private set; } = null!;
    public bool Novo { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        await CarregarListasAsync();
        if (string.IsNullOrWhiteSpace(id))
        {
            Novo = true;
            Produto = new Produto
            {
                Id = "prod-" + Guid.NewGuid().ToString("N")[..8],
                Situacao = "pausado",
                ModalidadeId = "pronta-entrega",
                Tipo = "pronta-entrega",
                Material = "PLA",
                PrazoDias = 5,
                AVista = true,
                CriadoEm = DateOnly.FromDateTime(DateTime.UtcNow),
                Sessao = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
            };
            var cat = await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).Select(x => x.Id).FirstOrDefaultAsync();
            Produto.CategoriaId = cat ?? "";
            return Page();
        }

        var existente = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (existente == null) return NotFound();
        Produto = existente;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CarregarListasAsync();
        if (string.IsNullOrWhiteSpace(Produto.Titulo))
        {
            ModelState.AddModelError("Produto.Titulo", "Informe o título.");
            Novo = !await _db.Produtos.AnyAsync(x => x.Id == Produto.Id);
            return Page();
        }

        var existente = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == Produto.Id);
        if (existente == null)
        {
            if (string.IsNullOrWhiteSpace(Produto.Id))
                Produto.Id = "prod-" + Guid.NewGuid().ToString("N")[..8];
            if (Produto.CriadoEm == default)
                Produto.CriadoEm = DateOnly.FromDateTime(DateTime.UtcNow);
            if (string.IsNullOrWhiteSpace(Produto.Sessao))
                Produto.Sessao = Produto.CriadoEm.ToString("yyyy-MM-dd");
            if (string.IsNullOrWhiteSpace(Produto.Tipo))
                Produto.Tipo = Produto.ModalidadeId;
            _db.Produtos.Add(Produto);
        }
        else
        {
            existente.Titulo = Produto.Titulo;
            existente.Descricao = Produto.Descricao;
            existente.CategoriaId = Produto.CategoriaId;
            existente.ModalidadeId = Produto.ModalidadeId;
            existente.Tipo = string.IsNullOrWhiteSpace(Produto.Tipo) ? Produto.ModalidadeId : Produto.Tipo;
            existente.Preco = Produto.Preco;
            existente.PrecoComparativo = Produto.PrecoComparativo;
            existente.Estoque = Produto.Estoque;
            existente.Minimo = Produto.Minimo;
            existente.Medidas = Produto.Medidas;
            existente.Peso = Produto.Peso;
            existente.Material = Produto.Material;
            existente.PrazoDias = Produto.PrazoDias;
            existente.Situacao = Produto.Situacao;
            existente.Oferta = Produto.Oferta;
            existente.MaisVendido = Produto.MaisVendido;
            existente.Personalizavel = Produto.Personalizavel;
            existente.AVista = Produto.AVista;
            existente.ComArquivo = Produto.ComArquivo;
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Produto salvo.";
        return RedirectToPage("/Admin/Produtos");
    }

    private async Task CarregarListasAsync()
    {
        Categorias = new SelectList(await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome", Produto?.CategoriaId);
        Modalidades = new SelectList(await _db.Modalidades.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome", Produto?.ModalidadeId);
    }
}
