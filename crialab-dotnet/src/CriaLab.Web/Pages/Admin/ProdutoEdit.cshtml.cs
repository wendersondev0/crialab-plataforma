using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class ProdutoEditModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ProdutoEditModel(ApplicationDbContext db) => _db = db;
  [BindProperty] public Produto? Produto { get; set; }
  public async Task<IActionResult> OnGetAsync(string? id) {
    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    Produto = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id);
    return Page();
  }
  public async Task<IActionResult> OnPostAsync() {
    if (Produto == null) return NotFound();
    var existente = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == Produto.Id);
    if (existente == null) return NotFound();
    existente.Titulo = Produto.Titulo;
    existente.Preco = Produto.Preco;
    existente.Estoque = Produto.Estoque;
    existente.Situacao = Produto.Situacao;
    await _db.SaveChangesAsync();
    return RedirectToPage("/Admin/Produtos");
  }
}