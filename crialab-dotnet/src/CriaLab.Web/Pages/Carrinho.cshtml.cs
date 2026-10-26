using CriaLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages;

public class CarrinhoModel : PageModel
{
    private readonly CarrinhoService _carrinho;

    public CarrinhoModel(CarrinhoService carrinho) => _carrinho = carrinho;

    public IList<CarrinhoItem> Itens => _carrinho.ObterItens();

    public decimal Subtotal => _carrinho.Subtotal();

    public IActionResult OnPostRemover(string produtoId)
    {
        _carrinho.Remover(produtoId, null, null);
        return RedirectToPage();
    }
}
