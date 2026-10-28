using System.Security.Claims;
using CriaLab.Web.Constants;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

[Authorize]
public class ReciboModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ReciboModel(ApplicationDbContext db) => _db = db;

    public Pedido? Pedido { get; private set; }
    public LojaIdentidade? Identidade { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole(RoleNames.Administrador);
        Pedido = await _db.Pedidos.AsNoTracking()
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id && (isAdmin || x.UsuarioId == userId));
        if (Pedido == null) return NotFound();
        Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
        return Page();
    }
}
