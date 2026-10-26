using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
[Authorize]
public class CheckoutModel : PageModel {
  private readonly ApplicationDbContext _db;
  private readonly CarrinhoService _carrinho;
  public CheckoutModel(ApplicationDbContext db, CarrinhoService carrinho) { _db = db; _carrinho = carrinho; }
  public IList<CarrinhoItem> Itens => _carrinho.ObterItens();
  public decimal Subtotal => _carrinho.Subtotal();
  [BindProperty] public string Cep { get; set; } = "";
  [BindProperty] public string Cupom { get; set; } = "";
  public string? Mensagem { get; set; }
  public void OnGet() {}
  public async Task<IActionResult> OnPostAsync() {
    var itens = _carrinho.ObterItens();
    if (!itens.Any()) return Page();
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var pedidoId = "ped-" + Guid.NewGuid().ToString("N")[..8];
    var subtotal = _carrinho.Subtotal();
    var pedido = new Pedido {
      Id = pedidoId, UsuarioId = userId, Status = "novo", CriadoEm = DateTime.UtcNow,
      Subtotal = subtotal, Total = subtotal, Cep = Cep, Cupom = Cupom, Forma = "simulado", Endereco = "Informado no checkout"
    };
    foreach (var i in itens) pedido.Itens.Add(new PedidoItem { Id = Guid.NewGuid().ToString("N"), ProdutoId = i.ProdutoId, Titulo = i.Titulo, Quantidade = i.Quantidade, PrecoUnitario = i.PrecoUnitario, Fisico = true, Tipo = "pronto" });
    _db.Pedidos.Add(pedido);
    await _db.SaveChangesAsync();
    _carrinho.Limpar();
    Mensagem = "Pedido " + pedidoId + " registrado com sucesso.";
    return Page();
  }
}
