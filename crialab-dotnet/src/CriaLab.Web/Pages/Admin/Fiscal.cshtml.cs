using System.Text;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class FiscalModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public FiscalModel(ApplicationDbContext db) => _db = db;

    public LojaIdentidade? Identidade { get; private set; }
    public IList<Pedido> Pedidos { get; private set; } = new List<Pedido>();
    public IList<string> StatusDisponiveis { get; private set; } = new List<string>();
    public string? Q { get; private set; }
    public string? Status { get; private set; }
    public string? Doc { get; private set; }
    public string? Nota { get; private set; }
    public string Ordem { get; private set; } = "data";
    public int TotalPedidos { get; private set; }
    public int ComRecibo { get; private set; }
    public int Pendentes { get; private set; }
    public int Revisadas { get; private set; }
    public decimal TotalComRecibo { get; private set; }

    public async Task OnGetAsync(string? q, string? status, string? doc, string? nota, string? ordem)
    {
        Q = q;
        Status = status;
        Doc = doc;
        Nota = nota;
        Ordem = ordem is "valor" or "id" ? ordem : "data";
        await CarregarAsync(export: false);
    }

    public async Task<IActionResult> OnGetCsvAsync(string? q, string? status, string? doc, string? nota, string? ordem)
    {
        Q = q;
        Status = status;
        Doc = doc;
        Nota = nota;
        Ordem = ordem is "valor" or "id" ? ordem : "data";
        await CarregarAsync(export: true);

        var sb = new StringBuilder();
        sb.AppendLine("id;status;criado_em;total;nota;endereco;itens");
        foreach (var p in Pedidos)
        {
            var itens = string.Join(" | ", p.Itens.Select(i => $"{i.Titulo} x{i.Quantidade}"));
            sb.AppendLine(string.Join(';',
                Csv(p.Id),
                Csv(p.Status),
                Csv(p.CriadoEm.ToLocalTime().ToString("yyyy-MM-dd HH:mm")),
                p.Total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Csv(EstadoNota(p)),
                Csv(p.Endereco),
                Csv(itens)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"fiscal-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    public async Task<IActionResult> OnPostNotaAsync(string id, string nota, string? q, string? status, string? doc, string? ordem)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(x => x.Id == id);
        if (pedido != null && TemRecibo(pedido))
        {
            pedido.Nota = nota == "revisada" ? "revisada" : "pendente";
            await _db.SaveChangesAsync();
            TempData["Ok"] = nota == "revisada" ? "Nota marcada como revisada." : "Nota voltou para pendente.";
        }
        return RedirectToPage(new { q, status, doc, nota = "", ordem });
    }

    public static bool TemRecibo(Pedido pedido) =>
        pedido.Status is not ("aguardando pagamento" or "novo" or "cancelado");

    public static string EstadoNota(Pedido pedido)
    {
        if (!TemRecibo(pedido)) return "nao_aplicavel";
        return string.Equals(pedido.Nota, "revisada", StringComparison.OrdinalIgnoreCase) ? "revisada" : "pendente";
    }

    public static string RotuloNota(string estado) => estado switch
    {
        "revisada" => "Revisada · sem transmissão",
        "nao_aplicavel" => "Sem nota neste status",
        _ => "Pendente de transmissão"
    };

    public static string ClasseNota(string estado) => estado switch
    {
        "revisada" => "fis-selo ok",
        "nao_aplicavel" => "fis-selo mute",
        _ => "fis-selo alerta"
    };

    public static string ClasseStatus(string status) => status switch
    {
        "entregue" or "pago" => "fis-status ok",
        "cancelado" or "devolvido" => "fis-status mute",
        "aguardando pagamento" or "novo" => "fis-status alerta",
        _ => "fis-status"
    };

    private async Task CarregarAsync(bool export)
    {
        Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
        var todos = await _db.Pedidos.AsNoTracking()
            .Include(x => x.Itens)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();

        TotalPedidos = todos.Count;
        var recibos = todos.Where(TemRecibo).ToList();
        ComRecibo = recibos.Count;
        Pendentes = recibos.Count(x => EstadoNota(x) == "pendente");
        Revisadas = recibos.Count(x => EstadoNota(x) == "revisada");
        TotalComRecibo = recibos.Sum(x => x.Total);
        StatusDisponiveis = todos.Select(x => x.Status).Distinct().OrderBy(x => x).ToList();

        IEnumerable<Pedido> lista = todos;
        if (!string.IsNullOrWhiteSpace(Status))
            lista = lista.Where(x => x.Status == Status);
        if (Doc == "recibo")
            lista = lista.Where(TemRecibo);
        if (Doc == "sem-recibo")
            lista = lista.Where(x => !TemRecibo(x));
        if (!string.IsNullOrWhiteSpace(Nota))
            lista = lista.Where(x => EstadoNota(x) == Nota);
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var t = Q.Trim().ToLowerInvariant();
            lista = lista.Where(x =>
                x.Id.ToLower().Contains(t) ||
                x.Status.ToLower().Contains(t) ||
                x.Endereco.ToLower().Contains(t) ||
                x.Itens.Any(i => i.Titulo.ToLower().Contains(t)));
        }

        Pedidos = Ordem switch
        {
            "valor" => lista.OrderByDescending(x => x.Total).ToList(),
            "id" => lista.OrderBy(x => x.Id).ToList(),
            _ => lista.OrderByDescending(x => x.CriadoEm).ToList()
        };
    }

    private static string Csv(string? valor)
    {
        var v = (valor ?? "").Replace("\"", "\"\"");
        return $"\"{v}\"";
    }
}
