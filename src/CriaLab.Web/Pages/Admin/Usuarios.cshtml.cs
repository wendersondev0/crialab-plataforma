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
public class UsuariosModel : PageModel
{
    private static readonly Regex NomeValido = new(@"^[\p{L}\p{N}][\p{L}\p{N} .'\-]*$", RegexOptions.CultureInvariant);
    private static readonly Regex EmailValido = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant);
    private static readonly Regex TelefoneValido = new(@"^[\d+\-() ]+$", RegexOptions.CultureInvariant);
    private static readonly Regex DocumentoValido = new(@"^[\p{L}\p{N}.\-/]+$", RegexOptions.CultureInvariant);

    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ApplicationDbContext _db;

    public UsuariosModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, ApplicationDbContext db)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
    }

    public string Busca { get; private set; } = "";
    public UsuarioEdicao? Editando { get; private set; }
    public IList<UsuarioLinha> Pessoas { get; private set; } = new List<UsuarioLinha>();

    public async Task OnGetAsync(string? id, string? q)
    {
        await CarregarAsync(q, id);
    }

    public async Task<IActionResult> OnPostCriarAsync(
        string? nome,
        string? email,
        string? telefone,
        string? documento,
        string? senha,
        string? confirmar,
        bool administrador,
        string? q)
    {
        var dados = Validar(nome, email, telefone, documento, senha, confirmar, senhaObrigatoria: true);
        if (dados.Erro != null)
        {
            TempData["Erro"] = dados.Erro;
            return RedirectToPage(new { q });
        }

        var user = new ApplicationUser
        {
            UserName = dados.Email,
            Email = dados.Email,
            EmailConfirmed = true,
            NomeCompleto = dados.Nome,
            TelefoneContato = dados.Telefone,
            Documento = dados.Documento,
            CriadoEm = DateTime.UtcNow
        };

        var criado = await _users.CreateAsync(user, dados.Senha!);
        if (!criado.Succeeded)
        {
            TempData["Erro"] = Traduzir(criado);
            return RedirectToPage(new { q });
        }

        await _users.AddToRoleAsync(user, RoleNames.Usuario);
        if (administrador)
            await _users.AddToRoleAsync(user, RoleNames.Administrador);

        TempData["Ok"] = "Conta de " + dados.Nome + " criada.";
        return RedirectToPage(new { q });
    }

    public async Task<IActionResult> OnPostSalvarAsync(
        string? userId,
        string? nome,
        string? email,
        string? telefone,
        string? documento,
        string? senha,
        string? confirmar,
        string? q)
    {
        var pessoa = await AcharAsync(userId);
        if (pessoa == null)
            return RedirectToPage(new { q });

        var trocaSenha = !string.IsNullOrWhiteSpace(senha) || !string.IsNullOrWhiteSpace(confirmar);
        var dados = Validar(nome, email, telefone, documento, senha, confirmar, senhaObrigatoria: false);
        if (!trocaSenha)
            dados = dados with { Senha = null };
        if (dados.Erro != null)
        {
            TempData["Erro"] = dados.Erro;
            return RedirectToPage(new { id = pessoa.Id, q });
        }

        var outro = await _users.FindByEmailAsync(dados.Email);
        if (outro != null && outro.Id != pessoa.Id)
        {
            TempData["Erro"] = "Esse e-mail já está em uso.";
            return RedirectToPage(new { id = pessoa.Id, q });
        }

        pessoa.NomeCompleto = dados.Nome;
        pessoa.TelefoneContato = dados.Telefone;
        pessoa.Documento = dados.Documento;
        var atualizado = await _users.UpdateAsync(pessoa);
        if (!atualizado.Succeeded)
        {
            TempData["Erro"] = Traduzir(atualizado);
            return RedirectToPage(new { id = pessoa.Id, q });
        }

        if (!string.Equals(pessoa.Email, dados.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailOk = await _users.SetEmailAsync(pessoa, dados.Email);
            if (!emailOk.Succeeded)
            {
                TempData["Erro"] = Traduzir(emailOk);
                return RedirectToPage(new { id = pessoa.Id, q });
            }

            var usuarioOk = await _users.SetUserNameAsync(pessoa, dados.Email);
            if (!usuarioOk.Succeeded)
            {
                TempData["Erro"] = Traduzir(usuarioOk);
                return RedirectToPage(new { id = pessoa.Id, q });
            }
        }

        if (trocaSenha)
        {
            var token = await _users.GeneratePasswordResetTokenAsync(pessoa);
            var senhaOk = await _users.ResetPasswordAsync(pessoa, token, dados.Senha!);
            if (!senhaOk.Succeeded)
            {
                TempData["Erro"] = Traduzir(senhaOk);
                return RedirectToPage(new { id = pessoa.Id, q });
            }
        }

        if (pessoa.Id == _users.GetUserId(User))
            await _signIn.RefreshSignInAsync(pessoa);

        TempData["Ok"] = "Conta de " + dados.Nome + " atualizada.";
        return RedirectToPage(new { q });
    }

    public async Task<IActionResult> OnPostBloquearAsync(string? userId, string? q)
    {
        var pessoa = await AcharAsync(userId);
        if (pessoa == null)
            return RedirectToPage(new { q });

        if (pessoa.Id == _users.GetUserId(User))
        {
            TempData["Erro"] = "Você não pode bloquear a sua própria conta.";
            return RedirectToPage(new { q });
        }

        var bloqueado = EstaBloqueado(pessoa);
        if (!bloqueado && await EhUltimoAdminAtivoAsync(pessoa))
        {
            TempData["Erro"] = "A oficina precisa de pelo menos uma pessoa com acesso de administrador.";
            return RedirectToPage(new { q });
        }

        if (bloqueado)
        {
            await _users.SetLockoutEndDateAsync(pessoa, null);
            TempData["Ok"] = "Conta desbloqueada.";
        }
        else
        {
            await _users.SetLockoutEnabledAsync(pessoa, true);
            await _users.SetLockoutEndDateAsync(pessoa, DateTimeOffset.UtcNow.AddYears(100));
            TempData["Ok"] = "Conta bloqueada. Essa pessoa não consegue entrar.";
        }

        return RedirectToPage(new { q });
    }

    public async Task<IActionResult> OnPostApagarAsync(string? userId, string? q)
    {
        var pessoa = await AcharAsync(userId);
        if (pessoa == null)
            return RedirectToPage(new { q });

        if (pessoa.Id == _users.GetUserId(User))
        {
            TempData["Erro"] = "Você não pode apagar a sua própria conta.";
            return RedirectToPage(new { q });
        }

        var papeis = await _users.GetRolesAsync(pessoa);
        if (papeis.Any(x => string.Equals(x, RoleNames.Administrador, StringComparison.OrdinalIgnoreCase)))
        {
            var admins = await _users.GetUsersInRoleAsync(RoleNames.Administrador);
            if (admins.Count <= 1)
            {
                TempData["Erro"] = "A oficina precisa de pelo menos uma pessoa com o papel Administrador.";
                return RedirectToPage(new { q });
            }
        }

        if (await TemHistoricoAsync(pessoa.Id))
        {
            TempData["Erro"] = "Essa conta tem pedidos ou orçamentos. Bloqueie em vez de apagar.";
            return RedirectToPage(new { q });
        }

        var nome = NomeDe(pessoa);
        await using var tx = await _db.Database.BeginTransactionAsync();
        var favoritos = await _db.Favoritos.Where(x => x.UsuarioId == pessoa.Id).ToListAsync();
        var enderecos = await _db.EnderecosCliente.Where(x => x.UsuarioId == pessoa.Id).ToListAsync();
        _db.Favoritos.RemoveRange(favoritos);
        _db.EnderecosCliente.RemoveRange(enderecos);

        var apagado = await _users.DeleteAsync(pessoa);
        if (!apagado.Succeeded)
        {
            await tx.RollbackAsync();
            TempData["Erro"] = Traduzir(apagado);
            return RedirectToPage(new { q });
        }

        await tx.CommitAsync();
        TempData["Ok"] = "Conta de " + nome + " apagada.";
        return RedirectToPage(new { q });
    }

    private async Task CarregarAsync(string? q, string? id)
    {
        Busca = (q ?? "").Trim();
        var eu = _users.GetUserId(User);
        var usuarios = await _users.Users.AsNoTracking()
            .OrderBy(x => x.NomeCompleto)
            .ThenBy(x => x.Email)
            .ToListAsync();

        if (Busca.Length > 0)
        {
            usuarios = usuarios.Where(x =>
                Contem(x.NomeCompleto, Busca)
                || Contem(x.Email, Busca)
                || Contem(x.TelefoneContato, Busca)
                || Contem(x.Documento, Busca)).ToList();
        }

        var ids = usuarios.Select(x => x.Id).ToList();
        var comPedido = await _db.Pedidos.AsNoTracking()
            .Where(x => ids.Contains(x.UsuarioId))
            .Select(x => x.UsuarioId)
            .Distinct()
            .ToListAsync();
        var comOrcamento = await _db.OrcamentosCliente.AsNoTracking()
            .Where(x => ids.Contains(x.UsuarioId))
            .Select(x => x.UsuarioId)
            .Distinct()
            .ToListAsync();
        var historico = comPedido.Concat(comOrcamento).ToHashSet();

        var linhas = new List<UsuarioLinha>();
        foreach (var usuario in usuarios)
        {
            var papeis = await _users.GetRolesAsync(usuario);
            linhas.Add(new UsuarioLinha(
                usuario.Id,
                NomeDe(usuario),
                usuario.Email ?? "",
                usuario.TelefoneContato ?? "",
                usuario.Documento ?? "",
                papeis.Select(PapeisModel.Rotulo).ToList(),
                EstaBloqueado(usuario),
                historico.Contains(usuario.Id),
                usuario.Id == eu,
                usuario.CriadoEm.ToLocalTime().ToString("dd/MM/yyyy")));
        }

        Pessoas = linhas;

        if (string.IsNullOrWhiteSpace(id))
            return;

        var alvo = await _users.FindByIdAsync(id);
        if (alvo == null)
            return;

        Editando = new UsuarioEdicao(
            alvo.Id,
            alvo.NomeCompleto,
            alvo.Email ?? "",
            alvo.TelefoneContato ?? "",
            alvo.Documento ?? "");
    }

    private async Task<ApplicationUser?> AcharAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;
        return await _users.FindByIdAsync(userId);
    }

    private async Task<bool> TemHistoricoAsync(string userId) =>
        await _db.Pedidos.AnyAsync(x => x.UsuarioId == userId)
        || await _db.OrcamentosCliente.AnyAsync(x => x.UsuarioId == userId);

    private async Task<bool> EhUltimoAdminAtivoAsync(ApplicationUser pessoa)
    {
        var papeis = await _users.GetRolesAsync(pessoa);
        if (!papeis.Any(x => string.Equals(x, RoleNames.Administrador, StringComparison.OrdinalIgnoreCase)))
            return false;

        var admins = await _users.GetUsersInRoleAsync(RoleNames.Administrador);
        var ativos = admins.Count(x => !EstaBloqueado(x));
        return ativos <= 1;
    }

    private static bool EstaBloqueado(ApplicationUser usuario) =>
        usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow;

    private static string NomeDe(ApplicationUser usuario) =>
        string.IsNullOrWhiteSpace(usuario.NomeCompleto)
            ? usuario.Email ?? usuario.UserName ?? "Pessoa"
            : usuario.NomeCompleto.Trim();

    private static bool Contem(string? texto, string busca) =>
        (texto ?? "").Contains(busca, StringComparison.CurrentCultureIgnoreCase);

    private static Dados Validar(
        string? nome,
        string? email,
        string? telefone,
        string? documento,
        string? senha,
        string? confirmar,
        bool senhaObrigatoria)
    {
        var nomeOk = (nome ?? "").Trim();
        var emailOk = (email ?? "").Trim();
        var tel = (telefone ?? "").Trim();
        var doc = (documento ?? "").Trim();
        var senhaOk = senha ?? "";
        var confirmarOk = confirmar ?? "";

        if (nomeOk.Length < 2 || nomeOk.Length > 80 || !NomeValido.IsMatch(nomeOk))
            return Dados.Falha("Informe o nome com 2 a 80 letras ou números.");

        if (emailOk.Length > 120 || !EmailValido.IsMatch(emailOk))
            return Dados.Falha("Informe um e-mail válido.");

        if (tel.Length > 0 && (tel.Length > 20 || !TelefoneValido.IsMatch(tel) || tel.Count(char.IsDigit) < 8))
            return Dados.Falha("Informe um telefone válido.");

        if (doc.Length > 20 || (doc.Length > 0 && !DocumentoValido.IsMatch(doc)))
            return Dados.Falha("Documento inválido.");

        string? senhaFinal = null;
        if (senhaObrigatoria || senhaOk.Length > 0 || confirmarOk.Length > 0)
        {
            if (senhaOk != confirmarOk)
                return Dados.Falha("As senhas não conferem.");
            var senhaErro = SenhaInvalida(senhaOk);
            if (senhaErro != null)
                return Dados.Falha(senhaErro);
            senhaFinal = senhaOk;
        }

        return new Dados(nomeOk, emailOk, tel.Length == 0 ? null : tel, doc.Length == 0 ? null : doc, senhaFinal, null);
    }

    private static string? SenhaInvalida(string senha)
    {
        var ok = senha.Length >= 8
            && senha.Any(char.IsUpper)
            && senha.Any(char.IsLower)
            && senha.Any(char.IsDigit)
            && senha.Any(c => !char.IsLetterOrDigit(c));
        return ok ? null : "A senha precisa de pelo menos 8 caracteres, com maiúscula, minúscula, número e símbolo.";
    }

    private static string Traduzir(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(erro => erro.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail" => "Esse e-mail já está em uso.",
            "InvalidEmail" or "InvalidUserName" => "Informe um e-mail válido.",
            "PasswordTooShort" or "PasswordRequiresDigit" or "PasswordRequiresLower" or "PasswordRequiresUpper" or "PasswordRequiresNonAlphanumeric" =>
                "A senha precisa de pelo menos 8 caracteres, com maiúscula, minúscula, número e símbolo.",
            _ => erro.Description
        }));

    private sealed record Dados(string Nome, string Email, string? Telefone, string? Documento, string? Senha, string? Erro)
    {
        public static Dados Falha(string erro) => new("", "", null, null, null, erro);
    }

    public sealed record UsuarioEdicao(string Id, string Nome, string Email, string Telefone, string Documento);

    public sealed record UsuarioLinha(
        string Id,
        string Nome,
        string Email,
        string Telefone,
        string Documento,
        IReadOnlyList<string> Papeis,
        bool Bloqueado,
        bool TemHistorico,
        bool Eu,
        string Desde);
}
