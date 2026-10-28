using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Conta;

[Authorize]
public class PedidosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public PedidosModel(ApplicationDbContext db) => _db = db;

    public IList<Pedido> Pedidos { get; private set; } = new List<Pedido>();
    public int Total { get; private set; }
    public int Abertos { get; private set; }
    public int Andamento { get; private set; }
    public int Entregues { get; private set; }
    public int Encerrados { get; private set; }
    public string Filtro { get; private set; } = "todos";

    public async Task OnGetAsync(string? filtro)
    {
        Filtro = Normalizar(filtro);
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostPagarAsync(string id, string? filtro)
    {
        var pedido = await AcharAsync(id);
        if (pedido != null && Aberto(pedido.Status))
        {
            pedido.Status = "pago";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Pagamento simulado. Pedido marcado como pago.";
        }
        return RedirectToPage(new { filtro = Normalizar(filtro) });
    }

    public async Task<IActionResult> OnPostCancelarAsync(string id, string? filtro)
    {
        var pedido = await AcharAsync(id);
        if (pedido != null && Aberto(pedido.Status))
        {
            pedido.Status = "cancelado";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Pedido cancelado.";
        }
        return RedirectToPage(new { filtro = Normalizar(filtro) });
    }

    public async Task<IActionResult> OnPostDevolverAsync(string id, string? filtro)
    {
        var pedido = await AcharAsync(id);
        if (pedido != null && pedido.Status == "entregue")
        {
            pedido.Status = "devolvido";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Pedido de devolução registrado.";
        }
        return RedirectToPage(new { filtro = Normalizar(filtro) });
    }

    private async Task CarregarAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return;

        var todos = await _db.Pedidos.AsNoTracking()
            .Include(x => x.Itens)
            .Where(x => x.UsuarioId == userId)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();

        Total = todos.Count;
        Abertos = todos.Count(x => Aberto(x.Status));
        Andamento = todos.Count(x => x.Status is "pago" or "enviado");
        Entregues = todos.Count(x => x.Status == "entregue");
        Encerrados = todos.Count(x => x.Status is "cancelado" or "devolvido");

        Pedidos = Filtro switch
        {
            "abertos" => todos.Where(x => Aberto(x.Status)).ToList(),
            "andamento" => todos.Where(x => x.Status is "pago" or "enviado").ToList(),
            "entregues" => todos.Where(x => x.Status == "entregue").ToList(),
            "encerrados" => todos.Where(x => x.Status is "cancelado" or "devolvido").ToList(),
            _ => todos
        };
    }

    private async Task<Pedido?> AcharAsync(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(id))
            return null;
        return await _db.Pedidos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);
    }

    private static bool Aberto(string status) => status is "aguardando pagamento" or "novo";

    private static string Normalizar(string? filtro) =>
        filtro is "abertos" or "andamento" or "entregues" or "encerrados" ? filtro : "todos";
}
