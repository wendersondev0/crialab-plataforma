using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class FavoritoModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public FavoritoModel(ApplicationDbContext db) => _db = db;

    public IActionResult OnGet(string? returnUrl) =>
        Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);

    public async Task<IActionResult> OnPostAsync(string id, string? returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToPage("/Conta/Login", new { returnUrl = returnUrl ?? Url.Page("/Produto", new { id }) });

        if (string.IsNullOrWhiteSpace(id))
            return Redirect(returnUrl ?? "/");

        var fav = await _db.Favoritos.FirstOrDefaultAsync(x => x.UsuarioId == userId && x.ProdutoId == id);
        if (fav == null)
            _db.Favoritos.Add(new Favorito { UsuarioId = userId, ProdutoId = id, CriadoEm = DateTime.UtcNow });
        else
            _db.Favoritos.Remove(fav);

        await _db.SaveChangesAsync();
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? Url.Page("/Produto", new { id })! : returnUrl);
    }
}
