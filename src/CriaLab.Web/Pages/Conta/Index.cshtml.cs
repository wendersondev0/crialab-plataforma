using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Conta;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public ApplicationUser? Usuario { get; private set; }
    public IList<EnderecoCliente> Enderecos { get; private set; } = new List<EnderecoCliente>();
    public string Aba { get; private set; } = "dados";

    [BindProperty]
    public string NomeCompleto { get; set; } = "";

    [BindProperty]
    public string? TelefoneContato { get; set; }

    [BindProperty]
    public string? Documento { get; set; }

    [BindProperty]
    public string Rotulo { get; set; } = "Casa";

    [BindProperty]
    public string Linha { get; set; } = "";

    [BindProperty]
    public string Cep { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(string? aba)
    {
        if (string.Equals(aba, "pedidos", StringComparison.OrdinalIgnoreCase))
            return RedirectToPage("Pedidos");
        if (string.Equals(aba, "favoritos", StringComparison.OrdinalIgnoreCase))
            return RedirectToPage("Favoritos");
        if (string.Equals(aba, "orcamentos", StringComparison.OrdinalIgnoreCase))
            return RedirectToPage("Orcamentos");
        if (string.Equals(aba, "perguntas", StringComparison.OrdinalIgnoreCase))
            return RedirectToPage("Perguntas");

        Aba = NormalizarAba(aba);
        await CarregarAsync();
        if (Usuario != null)
        {
            NomeCompleto = Usuario.NomeCompleto;
            TelefoneContato = Usuario.TelefoneContato;
            Documento = Usuario.Documento;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostEnderecoAsync(string? aba)
    {
        var userId = UserId();
        if (userId == null || string.IsNullOrWhiteSpace(Linha) || string.IsNullOrWhiteSpace(Cep))
            return RedirectToPage(new { aba = "enderecos" });

        _db.EnderecosCliente.Add(new EnderecoCliente
        {
            Id = "end-" + Guid.NewGuid().ToString("N")[..8],
            UsuarioId = userId,
            Rotulo = string.IsNullOrWhiteSpace(Rotulo) ? "Extra" : Rotulo.Trim(),
            Linha = Linha.Trim(),
            Cep = new string(Cep.Where(char.IsDigit).ToArray())
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Endereço guardado.";
        return RedirectToPage(new { aba = "enderecos" });
    }

    public async Task<IActionResult> OnPostRemoverEnderecoAsync(string id, string? aba)
    {
        var userId = UserId();
        var end = await _db.EnderecosCliente.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);
        if (end != null)
        {
            _db.EnderecosCliente.Remove(end);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Endereço removido.";
        }
        return RedirectToPage(new { aba = "enderecos" });
    }

    public async Task<IActionResult> OnPostDadosAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage();
        user.NomeCompleto = NomeCompleto?.Trim() ?? "";
        user.TelefoneContato = TelefoneContato?.Trim();
        user.Documento = Documento?.Trim();
        await _userManager.UpdateAsync(user);
        TempData["Ok"] = "Dados salvos.";
        return RedirectToPage(new { aba = "dados" });
    }

    private async Task CarregarAsync()
    {
        var userId = UserId();
        if (userId == null) return;
        Usuario = await _userManager.FindByIdAsync(userId);
        Enderecos = await _db.EnderecosCliente.AsNoTracking().Where(x => x.UsuarioId == userId).ToListAsync();
    }

    private string? UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static string NormalizarAba(string? aba) => aba == "enderecos" ? aba : "dados";
}
