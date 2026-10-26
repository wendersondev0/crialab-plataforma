using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly CarrinhoService _carrinho;
    private readonly FreteService _frete;

    public CheckoutModel(ApplicationDbContext db, CarrinhoService carrinho, FreteService frete)
    {
        _db = db;
        _carrinho = carrinho;
        _frete = frete;
    }

    public IList<CarrinhoItem> Itens => _carrinho.ObterItens();
    public decimal Subtotal => _carrinho.Subtotal();
    public IList<FreteOpcao> OpcoesFrete { get; private set; } = new List<FreteOpcao>();
    public decimal FreteGratisAPartir { get; private set; } = 200m;
    public decimal DescontoAVistaPct { get; private set; }
    public string? ErroFrete { get; private set; }
    public string? Mensagem { get; set; }

    [BindProperty]
    public string Cep { get; set; } = "";

    [BindProperty]
    public string Cupom { get; set; } = "";

    [BindProperty]
    public string FreteId { get; set; } = "pac";

    [BindProperty]
    public string Endereco { get; set; } = "";

    [BindProperty]
    public string Forma { get; set; } = "pix";

    [BindProperty]
    public string Acao { get; set; } = "finalizar";

    public decimal DescontoCupom { get; private set; }
    public decimal DescontoAVista { get; private set; }
    public decimal FreteCobrado { get; private set; }
    public decimal Total { get; private set; }
    public string FreteNome { get; private set; } = "";

    public async Task OnGetAsync()
    {
        await CarregarConfigAsync();
        await RecalcularAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CarregarConfigAsync();
        await RecalcularAsync();

        if (string.Equals(Acao, "cotar", StringComparison.OrdinalIgnoreCase))
            return Page();

        var itens = _carrinho.ObterItens();
        if (!itens.Any()) return Page();
        if (string.IsNullOrWhiteSpace(Endereco))
        {
            ModelState.AddModelError(nameof(Endereco), "Informe o endereço de entrega.");
            return Page();
        }
        if (ErroFrete != null || !OpcoesFrete.Any())
        {
            ModelState.AddModelError(nameof(Cep), ErroFrete ?? "Calcule o frete antes de confirmar.");
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var pedidoId = "ped-" + Guid.NewGuid().ToString("N")[..8];
        var pedido = new Pedido
        {
            Id = pedidoId,
            UsuarioId = userId,
            Status = "novo",
            CriadoEm = DateTime.UtcNow,
            Subtotal = Subtotal,
            DescontoCupom = DescontoCupom,
            DescontoAVista = DescontoAVista,
            FreteCobrado = FreteCobrado,
            Total = Total,
            Cep = Cep,
            Cupom = Cupom?.Trim().ToUpperInvariant() ?? "",
            Forma = Forma,
            Endereco = Endereco.Trim(),
            FreteNome = FreteNome
        };

        foreach (var i in itens)
        {
            pedido.Itens.Add(new PedidoItem
            {
                Id = Guid.NewGuid().ToString("N"),
                ProdutoId = i.ProdutoId,
                Titulo = i.Titulo,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario,
                Fisico = true,
                Tipo = "pronto",
                CorEscolhida = i.Cor,
                TamanhoEscolhido = i.Tamanho
            });
        }

        _db.Pedidos.Add(pedido);
        _db.PedidoOrdensServico.Add(new PedidoOs { PedidoId = pedidoId, Status = "aguardando", InsumoBaixado = false });
        await _db.SaveChangesAsync();
        _carrinho.Limpar();
        return RedirectToPage("/Recibo", new { id = pedidoId });
    }

    private async Task CarregarConfigAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        FreteGratisAPartir = cfg?.FreteGratisAPartir ?? 200m;
        DescontoAVistaPct = cfg?.DescontoAVista ?? 0.05m;
    }

    private async Task RecalcularAsync()
    {
        DescontoCupom = 0;
        DescontoAVista = 0;
        FreteCobrado = 0;
        FreteNome = "";
        Total = Subtotal;

        if (!Itens.Any()) return;

        var cupom = await _frete.CupomPorCodigoAsync(Cupom);
        if (cupom != null)
            DescontoCupom = Math.Round(Subtotal * cupom.Percentual, 2);

        if (string.Equals(Forma, "pix", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Forma, "boleto", StringComparison.OrdinalIgnoreCase))
            DescontoAVista = Math.Round((Subtotal - DescontoCupom) * DescontoAVistaPct, 2);

        var (erro, opcoes, _) = await _frete.CotarAsync(Cep, Subtotal);
        ErroFrete = erro;
        OpcoesFrete = opcoes;
        if (opcoes.Any())
        {
            var escolhida = opcoes.FirstOrDefault(x => x.Id == FreteId) ?? opcoes[0];
            FreteId = escolhida.Id;
            FreteNome = escolhida.Nome;
            FreteCobrado = Subtotal >= FreteGratisAPartir ? 0 : escolhida.Preco;
        }

        Total = Math.Max(0, Subtotal - DescontoCupom - DescontoAVista + FreteCobrado);
    }
}
