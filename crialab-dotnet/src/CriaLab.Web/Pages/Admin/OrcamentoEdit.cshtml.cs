using System.Text.Json;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OrcamentoEditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public OrcamentoEditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public OrcamentoInterno Orcamento { get; set; } = new();

    [BindProperty]
    public string ItensJson { get; set; } = "[]";

    public decimal CustoMaterial { get; private set; }
    public decimal CustoEnergia { get; private set; }
    public decimal CustoBase { get; private set; }
    public decimal PrecoSugerido { get; private set; }

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
        Calcular();
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

        _db.OrcamentoItens.RemoveRange(existente.Itens);
        var itens = JsonSerializer.Deserialize<List<ItemDto>>(ItensJson ?? "[]",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
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

    private void Calcular()
    {
        CustoMaterial = (Orcamento.PesoG + Orcamento.LixoG) / 1000m * Orcamento.PrecoKg;
        CustoEnergia = Orcamento.Minutos * Orcamento.EnergiaPorMinuto;
        var extras = 0m;
        try
        {
            var itens = JsonSerializer.Deserialize<List<ItemDto>>(ItensJson ?? "[]",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            extras = itens.Sum(x => x.Quantidade * x.Preco);
        }
        catch { /* ignore */ }
        CustoBase = CustoMaterial + CustoEnergia + Orcamento.Depreciacao + Orcamento.Manutencao + Orcamento.Servico + extras;
        var comFalha = CustoBase * (1 + Orcamento.TaxaFalha);
        PrecoSugerido = Math.Round(comFalha / Math.Max(0.01m, 1 - Orcamento.MargemSite) + Orcamento.FreteCanal, 2);
    }
}
