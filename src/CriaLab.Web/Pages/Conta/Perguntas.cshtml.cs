using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Conta;

[Authorize]
public class PerguntasModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PerguntasModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();
    public int Total { get; private set; }
    public int Aguardando { get; private set; }
    public int Respondidas { get; private set; }
    public string Filtro { get; private set; } = "todas";

    public async Task OnGetAsync(string? filtro)
    {
        Filtro = Normalizar(filtro);
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return;
        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null) return;

        var nome = usuario.NomeCompleto ?? usuario.UserName ?? "";
        var todas = await _db.PerguntasProduto.AsNoTracking()
            .Where(x => x.Autor == nome || x.Autor == usuario.UserName || x.Autor == usuario.Email)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();

        Total = todas.Count;
        Aguardando = todas.Count(x => string.IsNullOrWhiteSpace(x.Resposta));
        Respondidas = todas.Count(x => !string.IsNullOrWhiteSpace(x.Resposta));

        Perguntas = Filtro switch
        {
            "aguardando" => todas.Where(x => string.IsNullOrWhiteSpace(x.Resposta)).ToList(),
            "respondidas" => todas.Where(x => !string.IsNullOrWhiteSpace(x.Resposta)).ToList(),
            _ => todas
        };
    }

    private static string Normalizar(string? filtro) =>
        filtro is "aguardando" or "respondidas" ? filtro : "todas";
}
