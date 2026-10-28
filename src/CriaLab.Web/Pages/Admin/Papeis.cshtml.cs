using System.Text.RegularExpressions;
using CriaLab.Web.Constants;
using CriaLab.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class PapeisModel : PageModel
{
    private static readonly Regex NomeValido = new(@"^[\p{L}\p{N}][\p{L}\p{N} _\-]*$", RegexOptions.CultureInvariant);

    private readonly RoleManager<IdentityRole> _roles;
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;

    public PapeisModel(RoleManager<IdentityRole> roles, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    {
        _roles = roles;
        _users = users;
        _signIn = signIn;
    }

    public IList<PapelLinha> Papeis { get; private set; } = new List<PapelLinha>();
    public IList<PessoaLinha> Pessoas { get; private set; } = new List<PessoaLinha>();

    public async Task OnGetAsync() => await CarregarAsync();

    public async Task<IActionResult> OnPostCriarAsync(string? novoNome)
    {
        var nome = (novoNome ?? "").Trim();
        if (nome.Length < 2 || nome.Length > 40 || !NomeValido.IsMatch(nome))
        {
            TempData["Erro"] = "Use de 2 a 40 letras ou números. Pode ter espaço, hífen ou _.";
            return RedirectToPage();
        }

        var todos = await _roles.Roles.AsNoTracking().Select(x => x.Name!).ToListAsync();
        if (todos.Any(x => string.Equals(x, nome, StringComparison.OrdinalIgnoreCase)))
        {
            TempData["Erro"] = "Esse papel já existe.";
            return RedirectToPage();
        }

        var criado = await _roles.CreateAsync(new IdentityRole(nome));
        TempData[criado.Succeeded ? "Ok" : "Erro"] = criado.Succeeded
            ? "Papel criado."
            : string.Join(" ", criado.Errors.Select(x => x.Description));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostExcluirAsync(string? nome)
    {
        var papel = (nome ?? "").Trim();
        if (EhSistema(papel))
        {
            TempData["Erro"] = "Usuário e Administrador são papéis do sistema.";
            return RedirectToPage();
        }

        var role = await _roles.FindByNameAsync(papel);
        if (role == null)
            return RedirectToPage();

        var pessoas = await _users.GetUsersInRoleAsync(role.Name!);
        if (pessoas.Count > 0)
        {
            TempData["Erro"] = "Tire esse papel das pessoas antes de apagar.";
            return RedirectToPage();
        }

        var apagado = await _roles.DeleteAsync(role);
        TempData[apagado.Succeeded ? "Ok" : "Erro"] = apagado.Succeeded
            ? "Papel apagado."
            : string.Join(" ", apagado.Errors.Select(x => x.Description));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPessoaAsync(string? userId, string[]? papeis)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToPage();

        var pessoa = await _users.FindByIdAsync(userId);
        if (pessoa == null)
            return RedirectToPage();

        var catalogo = await _roles.Roles.AsNoTracking().Select(x => x.Name!).ToListAsync();
        var escolhidos = (papeis ?? [])
            .Select(x => (x ?? "").Trim())
            .Where(x => catalogo.Any(c => string.Equals(c, x, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var atuais = (await _users.GetRolesAsync(pessoa)).ToList();

        var tiraAdmin = atuais.Any(x => string.Equals(x, RoleNames.Administrador, StringComparison.OrdinalIgnoreCase))
            && !escolhidos.Any(x => string.Equals(x, RoleNames.Administrador, StringComparison.OrdinalIgnoreCase));
        if (tiraAdmin)
        {
            if (pessoa.Id == _users.GetUserId(User))
            {
                TempData["Erro"] = "Você não pode tirar Administrador da sua própria conta.";
                return RedirectToPage();
            }

            var admins = await _users.GetUsersInRoleAsync(RoleNames.Administrador);
            if (admins.Count <= 1)
            {
                TempData["Erro"] = "A oficina precisa de pelo menos uma pessoa com o papel Administrador.";
                return RedirectToPage();
            }
        }

        foreach (var atual in atuais)
        {
            if (!escolhidos.Any(x => string.Equals(x, atual, StringComparison.OrdinalIgnoreCase)))
                await _users.RemoveFromRoleAsync(pessoa, atual);
        }

        foreach (var escolhido in escolhidos)
        {
            if (!atuais.Any(x => string.Equals(x, escolhido, StringComparison.OrdinalIgnoreCase)))
            {
                var oficial = catalogo.First(x => string.Equals(x, escolhido, StringComparison.OrdinalIgnoreCase));
                await _users.AddToRoleAsync(pessoa, oficial);
            }
        }

        if (pessoa.Id == _users.GetUserId(User))
            await _signIn.RefreshSignInAsync(pessoa);

        var nome = string.IsNullOrWhiteSpace(pessoa.NomeCompleto) ? pessoa.Email : pessoa.NomeCompleto;
        TempData["Ok"] = "Papéis de " + nome + " atualizados.";
        return RedirectToPage();
    }

    private async Task CarregarAsync()
    {
        var roles = await _roles.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        var linhas = new List<PapelLinha>();
        foreach (var role in roles)
        {
            var nome = role.Name ?? "";
            var pessoas = await _users.GetUsersInRoleAsync(nome);
            linhas.Add(new PapelLinha(nome, Rotulo(nome), pessoas.Count, EhSistema(nome)));
        }

        Papeis = linhas
            .OrderBy(x => x.Nome == RoleNames.Administrador ? 0 : x.Nome == RoleNames.Usuario ? 1 : 2)
            .ThenBy(x => x.Rotulo, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var usuarios = await _users.Users.AsNoTracking().OrderBy(x => x.NomeCompleto).ThenBy(x => x.Email).ToListAsync();
        var pessoasLista = new List<PessoaLinha>();
        foreach (var usuario in usuarios)
        {
            var doUsuario = await _users.GetRolesAsync(usuario);
            pessoasLista.Add(new PessoaLinha(
                usuario.Id,
                string.IsNullOrWhiteSpace(usuario.NomeCompleto) ? usuario.Email ?? usuario.UserName ?? "Pessoa" : usuario.NomeCompleto,
                usuario.Email ?? usuario.UserName ?? "",
                doUsuario.ToList()));
        }

        Pessoas = pessoasLista;
    }

    public static string Rotulo(string nome) => nome switch
    {
        RoleNames.Usuario => "Usuário",
        RoleNames.Administrador => "Administrador",
        _ => nome
    };

    public static bool Tem(IReadOnlyList<string> papeis, string nome) =>
        papeis.Any(x => string.Equals(x, nome, StringComparison.OrdinalIgnoreCase));

    private static bool EhSistema(string nome) =>
        string.Equals(nome, RoleNames.Usuario, StringComparison.OrdinalIgnoreCase)
        || string.Equals(nome, RoleNames.Administrador, StringComparison.OrdinalIgnoreCase);

    public sealed record PapelLinha(string Nome, string Rotulo, int Pessoas, bool Sistema);
    public sealed record PessoaLinha(string Id, string Nome, string Email, IReadOnlyList<string> Papeis);
}
