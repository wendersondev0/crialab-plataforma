using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Conta;

[Authorize]
public class OrcamentosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public OrcamentosModel(ApplicationDbContext db) => _db = db;

    public IList<OrcamentoCliente> Orcamentos { get; private set; } = new List<OrcamentoCliente>();
    public int Total { get; private set; }
    public int Abertos { get; private set; }
    public int Aceitos { get; private set; }
    public int Recusados { get; private set; }
    public string Filtro { get; private set; } = "todos";

    public async Task OnGetAsync(string? filtro)
    {
        Filtro = Normalizar(filtro);
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostAceitarAsync(string id, string? filtro)
    {
        var userId = UserId();
        var orc = await _db.OrcamentosCliente.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);
        if (orc != null && orc.Status == "respondido" && orc.Valor.HasValue)
        {
            orc.Status = "aceito";
            _db.Pedidos.Add(new Pedido
            {
                Id = "ped-" + Guid.NewGuid().ToString("N")[..8],
                UsuarioId = userId!,
                Status = "aguardando pagamento",
                CriadoEm = DateTime.UtcNow,
                Subtotal = orc.Valor.Value,
                Total = orc.Valor.Value,
                Forma = "orcamento",
                Endereco = "A confirmar",
                Nota = "Gerado do orçamento " + orc.Id,
                Itens =
                {
                    new PedidoItem
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ProdutoId = orc.ProdutoId,
                        Titulo = orc.Titulo,
                        Tipo = "personalizado",
                        Fisico = true,
                        Quantidade = 1,
                        PrecoUnitario = orc.Valor.Value
                    }
                }
            });
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Orçamento aceito. Pedido criado.";
            return RedirectToPage("Pedidos");
        }
        return RedirectToPage(new { filtro = Normalizar(filtro) });
    }

    public async Task<IActionResult> OnPostRecusarAsync(string id, string? filtro)
    {
        var userId = UserId();
        var orc = await _db.OrcamentosCliente.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);
        if (orc != null && orc.Status == "respondido")
        {
            orc.Status = "recusado";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Orçamento recusado.";
        }
        return RedirectToPage(new { filtro = Normalizar(filtro) });
    }

    private async Task CarregarAsync()
    {
        var userId = UserId();
        if (userId == null) return;

        var todos = await _db.OrcamentosCliente.AsNoTracking()
            .Where(x => x.UsuarioId == userId)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();

        Total = todos.Count;
        Abertos = todos.Count(x => Aberto(x.Status));
        Aceitos = todos.Count(x => x.Status == "aceito");
        Recusados = todos.Count(x => x.Status == "recusado");

        Orcamentos = Filtro switch
        {
            "abertos" => todos.Where(x => Aberto(x.Status)).ToList(),
            "aceitos" => todos.Where(x => x.Status == "aceito").ToList(),
            "recusados" => todos.Where(x => x.Status == "recusado").ToList(),
            _ => todos
        };
    }

    private string? UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static bool Aberto(string status) => status is "respondido" or "aguardando";

    private static string Normalizar(string? filtro) =>
        filtro is "abertos" or "aceitos" or "recusados" ? filtro : "todos";
}
