using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OsModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public OsModel(ApplicationDbContext db) => _db = db;

    public Pedido? Pedido { get; private set; }
    public IList<Pedido> Lista { get; private set; } = new List<Pedido>();

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Lista = await _db.Pedidos.AsNoTracking()
                .Include(x => x.Itens)
                .Include(x => x.Os)
                .Where(x => x.Os != null || x.Itens.Any(i => i.Tipo == "personalizado"))
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync();
            return Page();
        }

        Pedido = await _db.Pedidos.AsNoTracking()
            .Include(x => x.Itens)
            .Include(x => x.Os)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (Pedido == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostConcluirAsync(string id)
    {
        var pedido = await _db.Pedidos.Include(x => x.Os).FirstOrDefaultAsync(x => x.Id == id);
        if (pedido == null) return NotFound();
        pedido.Status = "entregue";
        if (pedido.Os == null)
        {
            pedido.Os = new PedidoOs { PedidoId = id, Status = "concluida", InsumoBaixado = true };
            _db.PedidoOrdensServico.Add(pedido.Os);
        }
        else
        {
            pedido.Os.Status = "concluida";
            pedido.Os.InsumoBaixado = true;
        }

        var insumo = await _db.EstoqueInsumos.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (insumo != null)
            insumo.Saldo = Math.Max(0, insumo.Saldo - 0.05m);

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Ordem concluída.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAbrirAsync(string id)
    {
        var pedido = await _db.Pedidos.Include(x => x.Os).FirstOrDefaultAsync(x => x.Id == id);
        if (pedido == null) return NotFound();
        if (pedido.Os == null)
        {
            _db.PedidoOrdensServico.Add(new PedidoOs { PedidoId = id, Status = "aguardando", InsumoBaixado = false });
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }
}
