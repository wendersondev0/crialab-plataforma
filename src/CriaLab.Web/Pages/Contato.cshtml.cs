using System.ComponentModel.DataAnnotations;
using System.Text;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class ContatoModel : PageModel
{
    public static readonly (string Valor, string Rotulo)[] Opcoes =
    [
        ("duvida", "Dúvida"),
        ("sugestao", "Sugestão"),
        ("elogio", "Elogio"),
        ("reclamacao", "Reclamação"),
        ("orcamento", "Orçamento"),
        ("outro", "Outro")
    ];

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public ContatoModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public LojaIdentidade? Identidade { get; private set; }
    public bool Enviado { get; private set; }

    [BindProperty]
    public string Nome { get; set; } = "";

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Tipo { get; set; } = "duvida";

    [BindProperty]
    public string Texto { get; set; } = "";

    public string? TelefoneHref => Ligacao(Identidade?.Telefone);
    public string? WhatsappHref => LinkWhatsapp(Identidade?.Whatsapp);

    public string? Erro(string campo) =>
        ModelState.TryGetValue(campo, out var estado) && estado is { Errors.Count: > 0 }
            ? estado.Errors[0].ErrorMessage
            : null;

    public async Task OnGetAsync()
    {
        await CarregarAsync();
        Enviado = TempData["ContatoEnviado"] as string == "1";
        await PreencherUsuarioAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CarregarAsync();
        Nome = (Nome ?? "").Trim();
        Email = (Email ?? "").Trim();
        Texto = (Texto ?? "").Trim().Replace("\r\n", "\n");
        Tipo = (Tipo ?? "").Trim().ToLowerInvariant();

        if (Nome.Length < 2)
            ModelState.AddModelError(nameof(Nome), "Diga como podemos te chamar.");
        else if (Nome.Length > 120)
            ModelState.AddModelError(nameof(Nome), "Use um nome mais curto.");

        if (string.IsNullOrWhiteSpace(Email) || Email.Length > 180 || !new EmailAddressAttribute().IsValid(Email))
            ModelState.AddModelError(nameof(Email), "Informe um e-mail válido.");

        var opcao = Opcoes.FirstOrDefault(x => x.Valor == Tipo);
        if (opcao.Valor is null)
            ModelState.AddModelError(nameof(Tipo), "Escolha o tipo da mensagem.");

        if (Texto.Length < 8)
            ModelState.AddModelError(nameof(Texto), "Escreva a mensagem com um pouco mais de detalhe.");
        else if (Texto.Length > 4000)
            ModelState.AddModelError(nameof(Texto), "A mensagem passou de 4000 caracteres.");

        if (!ModelState.IsValid)
            return Page();

        _db.MensagensContato.Add(new MensagemContato
        {
            Id = "msg-" + Guid.NewGuid().ToString("N"),
            Nome = Nome,
            Email = Email,
            Tipo = opcao.Rotulo,
            Texto = Texto,
            Status = "aberto",
            CriadoEm = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        TempData["ContatoEnviado"] = "1";
        return RedirectToPage();
    }

    private async Task CarregarAsync() =>
        Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();

    private async Task PreencherUsuarioAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return;
        var usuario = await _users.GetUserAsync(User);
        if (usuario == null)
            return;
        if (string.IsNullOrWhiteSpace(Nome))
            Nome = usuario.NomeCompleto;
        if (string.IsNullOrWhiteSpace(Email))
            Email = usuario.Email ?? "";
    }

    private static string Digitos(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return "";
        var sb = new StringBuilder(valor.Length);
        foreach (var c in valor)
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }
        return sb.ToString();
    }

    private static string ComPais(string digitos)
    {
        if (!digitos.StartsWith("55", StringComparison.Ordinal) && digitos.Length <= 11)
            return "55" + digitos;
        return digitos;
    }

    private static string? Ligacao(string? telefone)
    {
        var digitos = Digitos(telefone);
        if (digitos.Length < 8)
            return null;
        return "tel:+" + ComPais(digitos);
    }

    private static string? LinkWhatsapp(string? valor)
    {
        var texto = (valor ?? "").Trim();
        if (texto.Length == 0)
            return null;
        if (texto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || texto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return texto;
        var digitos = Digitos(texto);
        if (digitos.Length < 8)
            return null;
        return "https://wa.me/" + ComPais(digitos);
    }
}
