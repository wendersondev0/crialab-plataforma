using CriaLab.Web.Data;
using CriaLab.Web.Data.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IServiceProvider _services;

    public IndexModel(ApplicationDbContext db, IWebHostEnvironment env, IServiceProvider services)
    {
        _db = db;
        _env = env;
        _services = services;
    }

    public int ProntaEntrega { get; private set; }
    public int SobDemanda { get; private set; }
    public int Personalizados { get; private set; }
    public int MensagensAbertas { get; private set; }
    public bool MostrarGuia { get; private set; }
    public bool PodeRestaurar { get; private set; }

    public async Task OnGetAsync()
    {
        PodeRestaurar = _env.IsDevelopment();
        MostrarGuia = Request.Cookies["crialab_guia_visto"] != "1";
        if (MostrarGuia)
            Response.Cookies.Append("crialab_guia_visto", "1", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        var publicados = _db.Produtos.AsNoTracking().Where(x => x.Situacao == "publicado");
        ProntaEntrega = await publicados.CountAsync(x => x.ModalidadeId == "pronta-entrega");
        SobDemanda = await publicados.CountAsync(x => x.ModalidadeId == "sob-demanda");
        Personalizados = await publicados.CountAsync(x => x.ModalidadeId == "personalizado");
        MensagensAbertas = await _db.MensagensContato.AsNoTracking().CountAsync(x => x.Status == "aberto");
    }

    public IActionResult OnPostPularGuia()
    {
        Response.Cookies.Append("crialab_guia_visto", "1", new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRestaurarAsync()
    {
        if (!_env.IsDevelopment())
        {
            TempData["Erro"] = "Restauração só disponível em Development.";
            return RedirectToPage();
        }

        await DbSeeder.RestaurarDemoAsync(_services);
        TempData["Ok"] = "Carga inicial restaurada.";
        return RedirectToPage();
    }
}
