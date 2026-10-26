using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class MarketingModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public MarketingModel(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public string Aba { get; private set; } = "cupom";
    public Cupom Cupom { get; private set; } = new() { Codigo = "OFICINA10", Percentual = 0.1m, Ativo = true };
    public IList<Parceiro> Parceiros { get; private set; } = new List<Parceiro>();
    public IList<MenuAjuda> MenusAjuda { get; private set; } = new List<MenuAjuda>();

    [BindProperty]
    public string Codigo { get; set; } = "OFICINA10";

    [BindProperty]
    public decimal PercentualPct { get; set; } = 10;

    [BindProperty]
    public bool Ativo { get; set; } = true;

    public async Task OnGetAsync(string? aba)
    {
        Aba = aba is "parceiros" or "ajuda" ? aba : "cupom";
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostCupomAsync()
    {
        var cupom = await _db.Cupons.OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pct = PercentualPct;
        if (pct < 0) pct = 0;
        if (pct > 90) pct = 90;
        var codigo = (Codigo ?? "").Trim().ToUpperInvariant().Replace(" ", "");
        if (cupom == null)
        {
            cupom = new Cupom { Id = "cup-" + Guid.NewGuid().ToString("N")[..6] };
            _db.Cupons.Add(cupom);
        }
        cupom.Codigo = string.IsNullOrWhiteSpace(codigo) ? "OFICINA10" : codigo;
        cupom.Percentual = Math.Round(pct / 100m, 4);
        cupom.Ativo = Ativo;
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Cupom salvo.";
        return RedirectToPage(new { aba = "cupom" });
    }

    public async Task<IActionResult> OnPostParceiroAsync(string? id, string nome, bool podeAparecer, IFormFile? logo)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return RedirectToPage(new { aba = "parceiros" });

        Parceiro? parceiro = null;
        if (!string.IsNullOrWhiteSpace(id))
            parceiro = await _db.Parceiros.FirstOrDefaultAsync(x => x.Id == id);

        if (parceiro == null)
        {
            parceiro = new Parceiro { Id = "par-" + Guid.NewGuid().ToString("N")[..8] };
            _db.Parceiros.Add(parceiro);
        }

        parceiro.Nome = nome.Trim();
        parceiro.PodeAparecer = podeAparecer;
        if (logo is { Length: > 0 })
        {
            var pasta = Path.Combine(_env.WebRootPath, "parceiros");
            Directory.CreateDirectory(pasta);
            var ext = Path.GetExtension(logo.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".png";
            var arquivo = parceiro.Id + ext.ToLowerInvariant();
            await using (var stream = System.IO.File.Create(Path.Combine(pasta, arquivo)))
                await logo.CopyToAsync(stream);
            parceiro.Logo = "parceiros/" + arquivo;
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Parceiro salvo.";
        return RedirectToPage(new { aba = "parceiros" });
    }

    public async Task<IActionResult> OnPostToggleParceiroAsync(string id)
    {
        var p = await _db.Parceiros.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            p.PodeAparecer = !p.PodeAparecer;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = "parceiros" });
    }

    public async Task<IActionResult> OnPostRemoverParceiroAsync(string id)
    {
        var p = await _db.Parceiros.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            _db.Parceiros.Remove(p);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Parceiro removido.";
        }
        return RedirectToPage(new { aba = "parceiros" });
    }

    public async Task<IActionResult> OnPostAjudaAsync(string id, string texto)
    {
        var m = await _db.MenusAjuda.FirstOrDefaultAsync(x => x.Id == id);
        if (m != null)
        {
            m.Texto = texto?.Trim() ?? "";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Texto de ajuda salvo.";
        }
        return RedirectToPage(new { aba = "ajuda" });
    }

    private async Task CarregarAsync()
    {
        Cupom = await _db.Cupons.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync()
            ?? new Cupom { Codigo = "OFICINA10", Percentual = 0.1m, Ativo = true };
        Codigo = Cupom.Codigo;
        PercentualPct = Math.Round(Cupom.Percentual * 100m, 1);
        Ativo = Cupom.Ativo;
        Parceiros = await _db.Parceiros.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        MenusAjuda = await _db.MenusAjuda.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
    }
}
