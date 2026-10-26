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
    private static readonly HashSet<string> AbasOk = new(StringComparer.OrdinalIgnoreCase)
    {
        "tudo", "impressoras", "filamentos", "embalagens", "insumos", "equipamentos", "compras"
    };

    private readonly ApplicationDbContext _db;

    public PatrimonioModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "tudo";
    public string? Q { get; private set; }
    public string? Aberto { get; private set; }
    public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
    public IList<EstoqueInsumo> Insumos { get; private set; } = new List<EstoqueInsumo>();
    public IList<CompraPatrimonioVm> Compras { get; private set; } = new List<CompraPatrimonioVm>();
    public int TotalTudo { get; private set; }
    public int QtdImpressoras { get; private set; }
    public int QtdFilamentos { get; private set; }
    public int QtdEmbalagens { get; private set; }
    public int QtdInsumosOutros { get; private set; }
    public int QtdEquipamentos { get; private set; }
    public int Alertas { get; private set; }
    public string TituloSecaoMaquinas { get; private set; } = "Máquinas e equipamentos";
    public string TituloSecaoInsumos { get; private set; } = "Filamentos e insumos";
    public string MesAtual { get; private set; } = DateTime.Now.ToString("yyyy-MM");

    public async Task OnGetAsync(string? aba, string? q, string? id)
    {
        Aba = !string.IsNullOrWhiteSpace(aba) && AbasOk.Contains(aba) ? aba.Trim().ToLowerInvariant() : "tudo";
        if (Aba == "maquinas") Aba = "tudo";
        if (Aba == "outros") Aba = "insumos";
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
            cor.Saldo = Arred(cor.Saldo + delta);
            await RecalcularSaldoInsumoAsync(id);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = aba ?? "filamentos", q, id });
    }

    public async Task<IActionResult> OnPostAjusteVarianteAsync(string id, string varId, decimal delta, string? aba, string? q)
    {
        var v = await _db.InsumoVariantes.FirstOrDefaultAsync(x => x.Id == varId && x.EstoqueInsumoId == id);
        if (v != null)
        {
            v.Saldo = Arred(v.Saldo + delta);
            await RecalcularSaldoInsumoAsync(id);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = aba ?? "embalagens", q, id });
    }

    public async Task<IActionResult> OnPostAjusteSaldoAsync(string id, decimal delta, string? aba, string? q)
    {
        var ins = await _db.EstoqueInsumos.FirstOrDefaultAsync(x => x.Id == id);
        if (ins != null)
        {
            ins.Saldo = Arred(ins.Saldo + delta);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = aba ?? "insumos", q, id });
    }

    public static decimal SaldoDe(EstoqueInsumo item)
    {
        if (item.Cores.Any()) return item.Cores.Sum(c => c.Saldo);
        if (item.Variantes.Any()) return item.Variantes.Sum(v => v.Saldo);
        return item.Saldo;
    }

    public static bool EhImpressora(Maquina m) =>
        m.Categoria is "impressora" or "impressora-tinta";

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
        "equipamento" => "Equipamento",
        _ => string.IsNullOrWhiteSpace(cat) ? "Equipamento" : cat
    };

    public static string DataAq(DateOnly? data, string? competencia)
    {
        if (data.HasValue) return data.Value.ToString("dd/MM/yyyy");
        if (!string.IsNullOrWhiteSpace(competencia) && competencia.Length >= 7)
        {
            if (DateOnly.TryParse(competencia + "-01", out var d))
                return d.ToString("MM/yyyy");
            return competencia;
        }
        return "Sem data";
    }

    public static string CompetenciaBr(string? competencia)
    {
        if (string.IsNullOrWhiteSpace(competencia)) return "—";
        if (competencia.Length >= 7 && DateOnly.TryParse(competencia[..7] + "-01", out var d))
            return d.ToString("MMM/yyyy", new System.Globalization.CultureInfo("pt-BR"));
        return competencia;
    }

    private static decimal Arred(decimal valor) =>
        Math.Max(0, Math.Round(valor * 10m, MidpointRounding.AwayFromZero) / 10m);

    private async Task CarregarAsync()
    {
        var maquinas = await _db.Maquinas.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        var insumos = await _db.EstoqueInsumos.AsNoTracking()
            .Include(x => x.Cores)
            .Include(x => x.Variantes)
            .OrderBy(x => x.Nome)
            .ToListAsync();

        var impressoras = maquinas.Where(EhImpressora).ToList();
        var equipamentos = maquinas.Where(x => !EhImpressora(x)).ToList();
        var filamentos = insumos.Where(x => x.Categoria == "filamento").ToList();
        var embalagens = insumos.Where(x => x.Categoria == "embalagem").ToList();
        var outrosIns = insumos.Where(x => x.Categoria is not ("filamento" or "embalagem")).ToList();

        QtdImpressoras = impressoras.Count;
        QtdEquipamentos = equipamentos.Count;
        QtdFilamentos = filamentos.Count;
        QtdEmbalagens = embalagens.Count;
        QtdInsumosOutros = outrosIns.Count;
        TotalTudo = maquinas.Count + insumos.Count;
        Alertas = insumos.Count(x => SaldoDe(x) <= x.Minimo);
        MesAtual = DateTime.Now.ToString("yyyy-MM");

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var t = Q.Trim().ToLowerInvariant();
            maquinas = maquinas.Where(x =>
                x.Nome.ToLower().Contains(t) ||
                x.Local.ToLower().Contains(t) ||
                x.Marca.ToLower().Contains(t) ||
                x.Modelo.ToLower().Contains(t) ||
                NomeCat(x.Categoria).ToLower().Contains(t)).ToList();
            insumos = insumos.Where(x =>
                x.Nome.ToLower().Contains(t) ||
                x.Categoria.ToLower().Contains(t) ||
                x.Fornecedor.ToLower().Contains(t) ||
                x.OrigemPlanilha.ToLower().Contains(t) ||
                x.Cores.Any(c => c.Nome.ToLower().Contains(t)) ||
                x.Variantes.Any(v => v.Nome.ToLower().Contains(t)) ||
                NomeCat(x.Categoria).ToLower().Contains(t)).ToList();
        }

        if (Aba == "compras")
        {
            Maquinas = new List<Maquina>();
            Insumos = new List<EstoqueInsumo>();
            Compras = await CarregarComprasAsync();
            TituloSecaoMaquinas = "";
            TituloSecaoInsumos = "";
            return;
        }

        Compras = new List<CompraPatrimonioVm>();

        Maquinas = Aba switch
        {
            "impressoras" => maquinas.Where(EhImpressora).ToList(),
            "equipamentos" => maquinas.Where(x => !EhImpressora(x)).ToList(),
            "filamentos" or "embalagens" or "insumos" => new List<Maquina>(),
            _ => maquinas
        };

        Insumos = Aba switch
        {
            "impressoras" or "equipamentos" => new List<EstoqueInsumo>(),
            "filamentos" => insumos.Where(x => x.Categoria == "filamento").ToList(),
            "embalagens" => insumos.Where(x => x.Categoria == "embalagem").ToList(),
            "insumos" => insumos.Where(x => x.Categoria is not ("filamento" or "embalagem")).ToList(),
            _ => insumos
        };

        TituloSecaoMaquinas = Aba switch
        {
            "equipamentos" => "Equipamentos e ferramentas",
            "impressoras" => "Impressoras",
            _ => "Máquinas e equipamentos"
        };
        TituloSecaoInsumos = Aba switch
        {
            "filamentos" => "Filamentos (cores e kg em estoque)",
            "embalagens" => "Embalagens (caixas, fitas, bolha, papéis e cartões)",
            "insumos" => "Insumos (LED, copos, argolas, verniz...)",
            _ => "Filamentos e insumos"
        };
    }

    private async Task<List<CompraPatrimonioVm>> CarregarComprasAsync()
    {
        var investimentos = await _db.LancamentosFinanceiros.AsNoTracking()
            .Where(x => x.Categoria == "investimento" && !x.Estornado)
            .OrderByDescending(x => x.Data)
            .Select(x => new CompraPatrimonioVm
            {
                Descricao = x.Descricao,
                Competencia = x.Competencia,
                Valor = x.Valor
            })
            .ToListAsync();

        if (investimentos.Count > 0)
            return investimentos;

        return await _db.Maquinas.AsNoTracking()
            .Where(x => x.CompraValor > 0)
            .OrderByDescending(x => x.AdquiridoEm)
            .Select(x => new CompraPatrimonioVm
            {
                Descricao = x.Nome,
                Competencia = x.CompraCompetencia,
                Valor = x.CompraValor
            })
            .ToListAsync();
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

    public sealed class CompraPatrimonioVm
    {
        public string Descricao { get; set; } = "";
        public string Competencia { get; set; } = "";
        public decimal Valor { get; set; }
    }
}
