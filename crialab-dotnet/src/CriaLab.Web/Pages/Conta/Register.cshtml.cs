using CriaLab.Web.Constants;
using CriaLab.Web.Data;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages.Conta;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly FavoritosService _favoritos;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, FavoritosService favoritos)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _favoritos = favoritos;
    }

    [BindProperty]
    public string NomeCompleto { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Telefone { get; set; } = string.Empty;

    [BindProperty]
    public string Senha { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmarSenha { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Senha != ConfirmarSenha)
        {
            ModelState.AddModelError(string.Empty, "As senhas não conferem.");
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = true,
            NomeCompleto = NomeCompleto,
            TelefoneContato = Telefone,
            CriadoEm = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Senha);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Usuario);
        await _signInManager.SignInAsync(user, isPersistent: false);
        await _favoritos.SincronizarSessaoNoUsuarioAsync(user.Id);
        return RedirectToPage("/Conta/Index");
    }
}
