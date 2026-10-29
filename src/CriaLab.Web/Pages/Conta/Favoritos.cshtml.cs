using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Conta;

[Authorize]
public class FavoritosModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly FavoritosService _favoritos;

    public FavoritosModel(ApplicationDbContext db, FavoritosService favoritos)
    {
        _db = db;
        _favoritos = favoritos;
    }

    public IList<Produto> Favoritos { get; private set; } = new List<Produto>();

    public async Task OnGetAsync() => await CarregarAsync();

    public async Task<IActionResult> OnPostRemoverAsync(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            await _favoritos.RemoverAsync(id);
            TempData["Ok"] = "Removido dos favoritos.";
        }
        return RedirectToPage();
    }

    private async Task CarregarAsync()
    {
        var favIds = (await _favoritos.ObterIdsAsync()).ToList();
        Favoritos = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Where(x => favIds.Contains(x.Id))
            .ToListAsync();
    }
}
