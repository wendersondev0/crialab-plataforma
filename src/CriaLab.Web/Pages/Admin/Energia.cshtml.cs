using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class EnergiaModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EnergiaModel(ApplicationDbContext db) => _db = db;

    public decimal PrecoKwh { get; set; } = 0.9m;
    public bool Confirmado { get; private set; }
    public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
    public IList<FechamentoEnergia> Historico { get; private set; } = new List<FechamentoEnergia>();

    [BindProperty]
    public decimal PrecoInformado { get; set; }

    [BindProperty]
    public List<LinhaMaquina> Linhas { get; set; } = new();

    public class LinhaMaquina
    {
        public string MaquinaId { get; set; } = "";
        public decimal Kwh { get; set; }
        public decimal Watts { get; set; }
        public decimal Custo { get; set; }
        public decimal EnergiaPorMinuto { get; set; }
    }

    public async Task OnGetAsync(bool? confirmado)
    {
        Confirmado = confirmado == true;
        await CarregarAsync();
        PrecoInformado = PrecoKwh;
        Linhas = Maquinas.Select(m => new LinhaMaquina
        {
            MaquinaId = m.Id,
            Watts = m.Watts,
            EnergiaPorMinuto = m.EnergiaPorMinuto,
            Kwh = 0,
            Custo = 0
        }).ToList();
    }

    public async Task<IActionResult> OnPostConfirmarPrecoAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.FirstOrDefaultAsync();
        if (cfg != null)
        {
            cfg.PrecoKwhAtual = PrecoInformado;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { confirmado = true });
    }

    public async Task<IActionResult> OnPostFecharAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.FirstOrDefaultAsync();
        var preco = cfg?.PrecoKwhAtual ?? PrecoInformado;
        var competencia = DateTime.Today.ToString("yyyy-MM");
        var kwhTotal = 0m;
        var valorTotal = 0m;

        foreach (var linha in Linhas)
        {
            var maq = await _db.Maquinas.FirstOrDefaultAsync(x => x.Id == linha.MaquinaId);
            if (maq == null) continue;
            maq.Watts = linha.Watts;
            maq.EnergiaPorMinuto = linha.EnergiaPorMinuto > 0
                ? linha.EnergiaPorMinuto
                : (linha.Watts / 1000m) * preco / 60m;
            var custo = linha.Custo > 0 ? linha.Custo : linha.Kwh * preco;
            kwhTotal += linha.Kwh;
            valorTotal += custo;
            if (custo > 0)
            {
                _db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
                {
                    Id = "lan-" + Guid.NewGuid().ToString("N")[..8],
                    Data = DateOnly.FromDateTime(DateTime.Today),
                    Competencia = competencia,
                    Descricao = "Energia " + maq.Nome,
                    Categoria = "energia",
                    Quantidade = 1,
                    Valor = custo,
                    Kwh = linha.Kwh,
                    Tarifa = preco,
                    Origem = "fechamento"
                });
            }
        }

        _db.FechamentosEnergia.Add(new FechamentoEnergia
        {
            Id = "energia-" + Guid.NewGuid().ToString("N")[..8],
            Competencia = competencia,
            KwhTotal = kwhTotal,
            Tarifa = preco,
            Valor = valorTotal,
            Notas = "Fechamento admin"
        });

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Fechamento de energia salvo.";
        return RedirectToPage("/Admin/Financeiro", new { aba = "resumo", mes = competencia });
    }

    private async Task CarregarAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        PrecoKwh = cfg?.PrecoKwhAtual > 0 ? cfg.PrecoKwhAtual : 0.9m;
        Maquinas = await _db.Maquinas.AsNoTracking().Where(x => x.ControlaHora).OrderBy(x => x.Nome).ToListAsync();
        Historico = await _db.FechamentosEnergia.AsNoTracking().OrderByDescending(x => x.Competencia).Take(12).ToListAsync();
    }
}
