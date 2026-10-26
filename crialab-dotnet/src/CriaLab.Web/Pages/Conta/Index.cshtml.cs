using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Conta;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public ApplicationUser? Usuario { get; private set; }
    public IList<Pedido> Pedidos { get; private set; } = new List<Pedido>();
    public IList<OrcamentoCliente> Orcamentos { get; private set; } = new List<OrcamentoCliente>();
    public IList<EnderecoCliente> Enderecos { get; private set; } = new List<EnderecoCliente>();

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return;
        Usuario = await _userManager.FindByIdAsync(userId);
        Pedidos = await _db.Pedidos.AsNoTracking().Where(x => x.UsuarioId == userId).OrderByDescending(x => x.CriadoEm).ToListAsync();
        Orcamentos = await _db.OrcamentosCliente.AsNoTracking().Where(x => x.UsuarioId == userId).OrderByDescending(x => x.CriadoEm).ToListAsync();
        Enderecos = await _db.EnderecosCliente.AsNoTracking().Where(x => x.UsuarioId == userId).ToListAsync();
    }
}
