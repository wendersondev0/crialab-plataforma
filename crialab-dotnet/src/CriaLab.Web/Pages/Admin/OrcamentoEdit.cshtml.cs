using System.Text.Json;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OrcamentoEditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public OrcamentoEditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public OrcamentoInterno Orcamento { get; set; } = new();

    [BindProperty]
    public string ItensJson { get; set; } = "[]";

    [BindProperty]
    public decimal DescontoPct { get; set; }

    public OrcamentoCalculoHelper.ResultadoOrcamento Calc { get; private set; } = new();
    public decimal PrecoSite { get; private set; }
    public OrcamentoCalculoHelper.ResultadoCanal CanalShopee { get; private set; } = new();
    public IList<FaixaMarketplace> FaixasShopee { get; private set; } = new List<FaixaMarketplace>();

    public class ItemDto
    {
        public string Nome { get; set; } = "";
        public decimal Quantidade { get; set; } = 1;
        public decimal Preco { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var existente = await _db.OrcamentosInternos.AsNoTracking()
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (existente == null) return NotFound();
        Orcamento = existente;
        ItensJson = JsonSerializer.Serialize(existente.Itens.Select(i => new ItemDto
        {
            Nome = i.Nome,
            Quantidade = i.Quantidade,
            Preco = i.Preco
        }));
        await CalcularAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existente = await _db.OrcamentosInternos
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == Orcamento.Id);
        if (existente == null) return NotFound();

        existente.Nome = Orcamento.Nome;
        existente.Material = Orcamento.Material;
        existente.Minutos = Orcamento.Minutos;
        existente.PesoG = Orcamento.PesoG;
        existente.LixoG = Orcamento.LixoG;
        existente.Servico = Orcamento.Servico;
        existente.MargemSite = Orcamento.MargemSite;
        existente.PrecoKg = Orcamento.PrecoKg;
        existente.EnergiaPorMinuto = Orcamento.EnergiaPorMinuto;
        existente.Depreciacao = Orcamento.Depreciacao;
        existente.Manutencao = Orcamento.Manutencao;
        existente.TaxaFalha = Orcamento.TaxaFalha;
        existente.FreteCanal = Orcamento.FreteCanal;
        existente.Enviado = Orcamento.Enviado;
        existente.DescontosMarcadosJson = JsonSerializer.Serialize(new[] { Math.Clamp(DescontoPct, 0, 90) / 100m });

        _db.OrcamentoItens.RemoveRange(existente.Itens);
        var itens = JsonSerializer.Deserialize<List<ItemDto>>(ItensJson ?? "[]", JsonOpts) ?? new();
        foreach (var i in itens.Where(x => !string.IsNullOrWhiteSpace(x.Nome)))
        {
            existente.Itens.Add(new OrcamentoItem
            {
                OrcamentoInternoId = existente.Id,
                Nome = i.Nome.Trim(),
                Quantidade = i.Quantidade,
                Preco = i.Preco
            });
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Orçamento salvo.";
        return RedirectToPage(new { id = existente.Id });
    }

    private async Task CalcularAsync()
    {
        FaixasShopee = await _db.FaixasMarketplace.AsNoTracking()
            .Where(x => x.Canal == "Shopee")
            .OrderBy(x => x.De)
            .ToListAsync();

        var desconto = 0m;
        try
        {
            var marcados = JsonSerializer.Deserialize<List<decimal>>(Orcamento.DescontosMarcadosJson ?? "[]", JsonOpts) ?? new();
            desconto = marcados.FirstOrDefault();
            DescontoPct = Math.Round(desconto * 100m, 1);
        }
        catch { DescontoPct = 0; }

        var itens = JsonSerializer.Deserialize<List<ItemDto>>(ItensJson ?? "[]", JsonOpts) ?? new();
        Calc = OrcamentoCalculoHelper.Orcar(
            Orcamento.PrecoKg,
            Orcamento.PesoG,
            Orcamento.LixoG,
            Orcamento.EnergiaPorMinuto,
            Orcamento.Depreciacao,
            Orcamento.Manutencao,
            Orcamento.Minutos,
            Orcamento.Servico,
            Orcamento.MargemSite,
            Orcamento.TaxaFalha,
            desconto,
            itens.Select(i => new OrcamentoCalculoHelper.ItemEntrada { Quantidade = i.Quantidade, Preco = i.Preco }));

        PrecoSite = Calc.Direto;
        CanalShopee = OrcamentoCalculoHelper.PrecoCanal(Calc.Original, FaixasShopee, Orcamento.FreteCanal);
    }
}
