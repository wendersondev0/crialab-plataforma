using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class PedidosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public PedidosModel(ApplicationDbContext db) => _db = db;

    public IList<Pedido> Pedidos { get; private set; } = new List<Pedido>();
    public IList<OrcamentoCliente> OrcamentosPendentes { get; private set; } = new List<OrcamentoCliente>();
    public bool RepassarFrete { get; private set; }

    public async Task OnGetAsync()
    {
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostStatusAsync(string id, string status)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(x => x.Id == id);
        if (pedido != null && !string.IsNullOrWhiteSpace(status))
        {
            pedido.Status = status.Trim();
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Status atualizado.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNotaAsync(string id, string nota)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(x => x.Id == id);
        if (pedido != null)
        {
            pedido.Nota = nota?.Trim() ?? "";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Nota salva.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResponderOrcamentoAsync(string id, decimal valor)
    {
        var orc = await _db.OrcamentosCliente.FirstOrDefaultAsync(x => x.Id == id);
        if (orc != null)
        {
            orc.Valor = valor;
            orc.Status = "respondido";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Valor devolvido ao cliente.";
        }
        return RedirectToPage();
    }

    private async Task CarregarAsync()
    {
        Pedidos = await _db.Pedidos.AsNoTracking()
            .Include(x => x.Itens)
            .Include(x => x.Os)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();
        OrcamentosPendentes = await _db.OrcamentosCliente.AsNoTracking()
            .Where(x => x.Status == "aguardando" || x.Status == "respondido")
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        RepassarFrete = cfg?.RepassarFrete ?? false;
    }
}
