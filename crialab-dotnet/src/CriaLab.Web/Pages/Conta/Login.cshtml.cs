using CriaLab.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CriaLab.Web.Pages.Conta;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager) => _signInManager = signInManager;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Senha { get; set; } = string.Empty;

    [BindProperty]
    public bool Lembrar { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _signInManager.PasswordSignInAsync(Email, Senha, Lembrar, lockoutOnFailure: false);
        if (result.Succeeded)
            return RedirectToPage("/Index");
        ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
        return Page();
    }
}
