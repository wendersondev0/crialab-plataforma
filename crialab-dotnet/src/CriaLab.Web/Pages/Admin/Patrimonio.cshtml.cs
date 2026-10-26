using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class PatrimonioModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public PatrimonioModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "tudo";
    public string? Q { get; private set; }
    public string? Aberto { get; private set; }
    public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
    public IList<EstoqueInsumo> Insumos { get; private set; } = new List<EstoqueInsumo>();
    public int QtdMaquinas { get; private set; }
    public int QtdInsumos { get; private set; }
    public int Alertas { get; private set; }

    public async Task OnGetAsync(string? aba, string? q, string? id)
    {
        Aba = aba is "maquinas" or "filamentos" or "embalagens" or "outros" ? aba : "tudo";
        Q = q;
        Aberto = id;
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostStatusMaquinaAsync(string id, string status, string? aba, string? q)
    {
        var m = await _db.Maquinas.FirstOrDefaultAsync(x => x.Id == id);
        if (m != null && !string.IsNullOrWhiteSpace(status))
        {
            m.Status = status.Trim();
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Status da máquina atualizado.";
        }
        return RedirectToPage(new { aba, q, id });
    }

    public async Task<IActionResult> OnPostAjusteCorAsync(string id, string corId, decimal delta, string? aba, string? q)
    {
        var cor = await _db.InsumoCores.FirstOrDefaultAsync(x => x.Id == corId && x.EstoqueInsumoId == id);
        if (cor != null)
        {
            cor.Saldo = Math.Max(0, cor.Saldo + delta);
            await RecalcularSaldoInsumoAsync(id);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Saldo da cor atualizado.";
        }
        return RedirectToPage(new { aba = aba ?? "filamentos", q, id });
    }

    public async Task<IActionResult> OnPostAjusteVarianteAsync(string id, string varId, decimal delta, string? aba, string? q)
    {
        var v = await _db.InsumoVariantes.FirstOrDefaultAsync(x => x.Id == varId && x.EstoqueInsumoId == id);
        if (v != null)
        {
            v.Saldo = Math.Max(0, v.Saldo + delta);
            await RecalcularSaldoInsumoAsync(id);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Saldo da variante atualizado.";
        }
        return RedirectToPage(new { aba = aba ?? "embalagens", q, id });
    }

    public async Task<IActionResult> OnPostAjusteSaldoAsync(string id, decimal delta, string? aba, string? q)
    {
        var ins = await _db.EstoqueInsumos.FirstOrDefaultAsync(x => x.Id == id);
        if (ins != null)
        {
            ins.Saldo = Math.Max(0, ins.Saldo + delta);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Saldo atualizado.";
        }
        return RedirectToPage(new { aba, q, id });
    }

    public async Task<IActionResult> OnPostNovoInsumoAsync(string nome, string categoria, string unidade, decimal minimo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return RedirectToPage(new { aba = "filamentos" });
        var id = "ins-" + Guid.NewGuid().ToString("N")[..8];
        _db.EstoqueInsumos.Add(new EstoqueInsumo
        {
            Id = id,
            Nome = nome.Trim(),
            Categoria = string.IsNullOrWhiteSpace(categoria) ? "filamento" : categoria.Trim(),
            Unidade = string.IsNullOrWhiteSpace(unidade) ? "kg" : unidade.Trim(),
            Minimo = minimo,
            Saldo = 0
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Insumo criado.";
        return RedirectToPage(new { aba = "filamentos", id });
    }

    public static decimal SaldoDe(EstoqueInsumo item)
    {
        if (item.Cores.Any()) return item.Cores.Sum(c => c.Saldo);
        if (item.Variantes.Any()) return item.Variantes.Sum(v => v.Saldo);
        return item.Saldo;
    }

    public static string NomeCat(string? cat) => cat switch
    {
        "impressora" => "Impressora 3D",
        "impressora-tinta" => "Impressora tinta",
        "secador" => "Secador",
        "ferramenta" => "Ferramenta",
        "acessorio" => "Acessório",
        "filamento" => "Filamento",
        "embalagem" => "Embalagens",
        "acabamento" => "Acabamento",
        "eletronico" => "Elétrico / LED",
        "ferragem" => "Ferragem",
        _ => string.IsNullOrWhiteSpace(cat) ? "Equipamento" : cat
    };

    private async Task CarregarAsync()
    {
        var maquinas = await _db.Maquinas.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        var insumos = await _db.EstoqueInsumos.AsNoTracking()
            .Include(x => x.Cores)
            .Include(x => x.Variantes)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var t = Q.Trim().ToLowerInvariant();
            maquinas = maquinas.Where(x =>
                x.Nome.ToLower().Contains(t) ||
                x.Local.ToLower().Contains(t) ||
                x.Marca.ToLower().Contains(t)).ToList();
            insumos = insumos.Where(x =>
                x.Nome.ToLower().Contains(t) ||
                x.Categoria.ToLower().Contains(t) ||
                x.Fornecedor.ToLower().Contains(t)).ToList();
        }

        QtdMaquinas = maquinas.Count;
        QtdInsumos = insumos.Count;
        Alertas = insumos.Count(x => SaldoDe(x) <= x.Minimo);

        Maquinas = Aba is "tudo" or "maquinas" ? maquinas : new List<Maquina>();
        Insumos = Aba switch
        {
            "maquinas" => new List<EstoqueInsumo>(),
            "filamentos" => insumos.Where(x => x.Categoria == "filamento").ToList(),
            "embalagens" => insumos.Where(x => x.Categoria == "embalagem").ToList(),
            "outros" => insumos.Where(x => x.Categoria is not ("filamento" or "embalagem")).ToList(),
            _ => insumos
        };
        if (Aba == "tudo")
            Maquinas = maquinas;
    }

    private async Task RecalcularSaldoInsumoAsync(string id)
    {
        var ins = await _db.EstoqueInsumos.Include(x => x.Cores).Include(x => x.Variantes).FirstOrDefaultAsync(x => x.Id == id);
        if (ins == null) return;
        if (ins.Cores.Any())
            ins.Saldo = ins.Cores.Sum(c => c.Saldo);
        else if (ins.Variantes.Any())
            ins.Saldo = ins.Variantes.Sum(v => v.Saldo);
    }
}
