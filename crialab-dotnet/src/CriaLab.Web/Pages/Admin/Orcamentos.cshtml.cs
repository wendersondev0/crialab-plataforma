using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class OrcamentosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public OrcamentosModel(ApplicationDbContext db) => _db = db;

    public IList<OrcamentoInterno> Internos { get; private set; } = new List<OrcamentoInterno>();
    public IList<OrcamentoCliente> Clientes { get; private set; } = new List<OrcamentoCliente>();

    public async Task OnGetAsync()
    {
        Internos = await _db.OrcamentosInternos.AsNoTracking()
            .Include(x => x.Itens)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
        Clientes = await _db.OrcamentosCliente.AsNoTracking()
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostNovoAsync()
    {
        var id = "orc-" + Guid.NewGuid().ToString("N")[..8];
        _db.OrcamentosInternos.Add(new OrcamentoInterno
        {
            Id = id,
            Nome = "Novo orçamento",
            Material = "PLA",
            Minutos = 60,
            PesoG = 20,
            MargemSite = 0.4m,
            PrecoKg = 85,
            PrecoKgOrigem = "manual",
            EnergiaPorMinuto = 0.005m,
            Depreciacao = 0.02m,
            Manutencao = 0.01m,
            TaxaFalha = 0.05m
        });
        await _db.SaveChangesAsync();
        return RedirectToPage("/Admin/OrcamentoEdit", new { id });
    }
}
