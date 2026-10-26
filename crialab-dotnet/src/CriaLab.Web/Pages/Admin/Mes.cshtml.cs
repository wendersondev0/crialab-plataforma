using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class MesModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public MesModel(ApplicationDbContext db) => _db = db;

    public string Ano { get; private set; } = "";
    public string Mes { get; private set; } = "";
    public string? Categoria { get; private set; }
    public string? Texto { get; private set; }
    public string Ordem { get; private set; } = "data";
    public IList<string> Anos { get; private set; } = new List<string>();
    public IList<string> MesesAno { get; private set; } = new List<string>();
    public IList<LancamentoFinanceiro> Lancamentos { get; private set; } = new List<LancamentoFinanceiro>();
    public MesResumo Resumo { get; private set; } = FinanceiroResumo.Resumir(Array.Empty<LancamentoFinanceiro>(), "");
    public IList<TipoInsumoConfig> TiposInsumo { get; private set; } = new List<TipoInsumoConfig>();

    [BindProperty]
    public string LanDescricao { get; set; } = "";

    [BindProperty]
    public string LanCategoria { get; set; } = "receita";

    [BindProperty]
    public string? LanTipoInsumo { get; set; }

    [BindProperty]
    public decimal LanQuantidade { get; set; } = 1;

    [BindProperty]
    public decimal LanValor { get; set; }

    [BindProperty]
    public DateOnly LanData { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public string LanCompetencia { get; set; } = "";

    public async Task OnGetAsync(string? ano, string? mes, string? categoria, string? q, string? ordem)
    {
        Categoria = categoria;
        Texto = q;
        Ordem = ordem is "valor" or "descricao" ? ordem : "data";
        await CarregarAsync(ano, mes);
    }

    public async Task<IActionResult> OnPostLancamentoAsync()
    {
        if (string.IsNullOrWhiteSpace(LanDescricao) || LanValor <= 0)
            return RedirectToPage(new { mes = LanCompetencia });

        var comp = string.IsNullOrWhiteSpace(LanCompetencia) ? DateTime.Today.ToString("yyyy-MM") : LanCompetencia.Trim();
        _db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
        {
            Id = "lan-" + Guid.NewGuid().ToString("N")[..8],
            Data = LanData,
            Competencia = comp,
            Descricao = LanDescricao.Trim(),
            Categoria = FinanceiroResumo.Categorias.Contains(LanCategoria) ? LanCategoria : "outroInsumo",
            TipoInsumo = LanTipoInsumo?.Trim() ?? "",
            Quantidade = LanQuantidade <= 0 ? 1 : LanQuantidade,
            Valor = LanValor,
            Origem = "caderno"
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Lançamento salvo.";
        return RedirectToPage(new { mes = comp, ano = comp[..4] });
    }

    public async Task<IActionResult> OnPostEstornarAsync(string id, string? mes, string? ano)
    {
        var lan = await _db.LancamentosFinanceiros.FirstOrDefaultAsync(x => x.Id == id);
        if (lan != null)
        {
            lan.Estornado = true;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Lançamento estornado.";
        }
        return RedirectToPage(new { mes, ano });
    }

    private async Task CarregarAsync(string? ano, string? mes)
    {
        var todos = await _db.LancamentosFinanceiros.AsNoTracking().ToListAsync();
        Anos = todos.Select(x => x.Competencia.Length >= 4 ? x.Competencia[..4] : "")
            .Where(x => x.Length == 4)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        if (!Anos.Any())
            Anos = new List<string> { DateTime.Today.ToString("yyyy") };

        Ano = !string.IsNullOrWhiteSpace(ano) && Anos.Contains(ano) ? ano! : Anos.Last();
        MesesAno = todos.Select(x => x.Competencia)
            .Where(x => x.StartsWith(Ano))
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        if (!MesesAno.Any())
        {
            MesesAno = Enumerable.Range(1, 12).Select(i => $"{Ano}-{i:00}").ToList();
        }

        Mes = !string.IsNullOrWhiteSpace(mes) && mes.StartsWith(Ano) ? mes! : MesesAno.Last();
        LanCompetencia = Mes;
        LanData = DateOnly.FromDateTime(DateTime.Today);

        Resumo = FinanceiroResumo.Resumir(todos, Mes);
        TiposInsumo = await _db.TiposInsumoConfig.AsNoTracking().OrderBy(x => x.Ordem).ToListAsync();

        var query = todos.Where(x => x.Competencia == Mes && !x.Estornado).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(Categoria))
            query = query.Where(x => x.Categoria == Categoria);
        if (!string.IsNullOrWhiteSpace(Texto))
        {
            var t = Texto.Trim().ToLowerInvariant();
            query = query.Where(x => x.Descricao.ToLower().Contains(t) || x.TipoInsumo.ToLower().Contains(t));
        }

        Lancamentos = Ordem switch
        {
            "valor" => query.OrderByDescending(x => x.Valor).ToList(),
            "descricao" => query.OrderBy(x => x.Descricao).ToList(),
            _ => query.OrderByDescending(x => x.Data).ThenByDescending(x => x.Id).ToList()
        };
    }
}
