using CriaLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages;

public class FavoritoModel : PageModel
{
    private readonly FavoritosService _favoritos;

    public FavoritoModel(FavoritosService favoritos) => _favoritos = favoritos;

    public IActionResult OnGet(string? returnUrl) =>
        RedirectLocal(returnUrl, null);

    public async Task<IActionResult> OnPostAsync(string id, string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(id))
            await _favoritos.AlternarAsync(id.Trim());
        return RedirectLocal(returnUrl, id);
    }

    private IActionResult RedirectLocal(string? returnUrl, string? id)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        if (!string.IsNullOrWhiteSpace(id))
            return RedirectToPage("/Produto", new { id });
        return RedirectToPage("/Index");
    }
}
