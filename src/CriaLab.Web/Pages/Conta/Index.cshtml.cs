using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
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
    private readonly FavoritosService _favoritos;

    public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager, FavoritosService favoritos)
    {
        _db = db;
        _userManager = userManager;
        _favoritos = favoritos;
    }

    public ApplicationUser? Usuario { get; private set; }
    public int TotalPedidos { get; private set; }
    public IList<OrcamentoCliente> Orcamentos { get; private set; } = new List<OrcamentoCliente>();
    public IList<Produto> Favoritos { get; private set; } = new List<Produto>();
    public IList<EnderecoCliente> Enderecos { get; private set; } = new List<EnderecoCliente>();
    public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();
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

    public async Task<IActionResult> OnPostAceitarOrcamentoAsync(string id, string? aba)
    {
        var userId = UserId();
        var orc = await _db.OrcamentosCliente.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);
        if (orc != null && orc.Status == "respondido" && orc.Valor.HasValue)
        {
            orc.Status = "aceito";
            var pedidoId = "ped-" + Guid.NewGuid().ToString("N")[..8];
            _db.Pedidos.Add(new Pedido
            {
                Id = pedidoId,
                UsuarioId = userId!,
                Status = "aguardando pagamento",
                CriadoEm = DateTime.UtcNow,
                Subtotal = orc.Valor.Value,
                Total = orc.Valor.Value,
                Forma = "orcamento",
                Endereco = "A confirmar",
                Nota = "Gerado do orçamento " + orc.Id,
                Itens =
                {
                    new PedidoItem
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        ProdutoId = orc.ProdutoId,
                        Titulo = orc.Titulo,
                        Tipo = "personalizado",
                        Fisico = true,
                        Quantidade = 1,
                        PrecoUnitario = orc.Valor.Value
                    }
                }
            });
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Orçamento aceito. Pedido criado.";
            return RedirectToPage("Pedidos");
        }
        return RedirectToPage(new { aba = NormalizarAba(aba) });
    }

    public async Task<IActionResult> OnPostRecusarOrcamentoAsync(string id, string? aba)
    {
        var userId = UserId();
        var orc = await _db.OrcamentosCliente.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == userId);
        if (orc != null && orc.Status == "respondido")
        {
            orc.Status = "recusado";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Orçamento recusado.";
        }
        return RedirectToPage(new { aba = NormalizarAba(aba) });
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

    public async Task<IActionResult> OnPostRemoverFavoritoAsync(string id, string? aba)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            await _favoritos.RemoverAsync(id);
            TempData["Ok"] = "Removido dos favoritos.";
        }
        return RedirectToPage(new { aba = "favoritos" });
    }

    private async Task CarregarAsync()
    {
        var userId = UserId();
        if (userId == null) return;
        Usuario = await _userManager.FindByIdAsync(userId);
        TotalPedidos = await _db.Pedidos.CountAsync(x => x.UsuarioId == userId);
        Orcamentos = await _db.OrcamentosCliente.AsNoTracking()
            .Where(x => x.UsuarioId == userId)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();
        Enderecos = await _db.EnderecosCliente.AsNoTracking().Where(x => x.UsuarioId == userId).ToListAsync();
        var favIds = (await _favoritos.ObterIdsAsync()).ToList();
        Favoritos = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Where(x => favIds.Contains(x.Id))
            .ToListAsync();
        var nome = Usuario?.NomeCompleto ?? Usuario?.UserName ?? "";
        Perguntas = await _db.PerguntasProduto.AsNoTracking()
            .Where(x => x.Autor == nome || x.Autor == Usuario!.UserName || x.Autor == Usuario.Email)
            .OrderByDescending(x => x.CriadoEm)
            .ToListAsync();
    }

    private string? UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static string NormalizarAba(string? aba) =>
        aba is "orcamentos" or "favoritos" or "enderecos" or "perguntas" or "dados"
            ? aba
            : "dados";
}
