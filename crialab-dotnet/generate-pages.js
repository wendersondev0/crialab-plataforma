const fs = require("fs");
const path = require("path");
const base = path.join("d:", "projetos", "ctgi", "crialab-dotnet", "src", "CriaLab.Web", "Pages");

function w(rel, content) {
  const full = path.join(base, rel);
  fs.mkdirSync(path.dirname(full), { recursive: true });
  fs.writeFileSync(full, content.replace(/\n/g, "\r\n"));
}

w("Index.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Início"; }
<section class="vitrine-hero">
<h1>Impressão 3D do seu jeito</h1>
<p>Pronta entrega, sob demanda e personalizados.</p>
</section>
<section class="modalidades-atalhos">
@foreach (var m in Model.Modalidades)
{
<div class="modalidade-card">
<h2>@m.Nome</h2>
<p>@m.Resumo</p>
<a asp-page="/Produtos" asp-route-modalidade="@m.Id">Ver peças</a>
</div>
}
</section>
<section class="grade-produtos">
<h2>Destaques</h2>
<div class="produtos-grid">
@foreach (var p in Model.Produtos)
{
<article class="produto-card">
<a asp-page="/Produto" asp-route-id="@p.Id">
@if (p.Fotos.Any())
{
<img src="/@p.Fotos.OrderBy(f => f.Ordem).First().Arquivo" alt="@p.Titulo" />
}
<h3>@p.Titulo</h3>
<p class="preco">R$ @p.Preco.ToString("N2")</p>
</a>
</article>
}
</div>
</section>
`);

w("Index.cshtml.cs", `using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IList<Modalidade> Modalidades { get; private set; } = new List<Modalidade>();
    public IList<Produto> Produtos { get; private set; } = new List<Produto>();

    public async Task OnGetAsync()
    {
        Modalidades = await _db.Modalidades.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        Produtos = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Where(x => x.Situacao == "publicado")
            .OrderByDescending(x => x.CriadoEm)
            .Take(12)
            .ToListAsync();
    }
}
`);

w("Produtos.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Produtos"; }
<h1>Produtos</h1>
<form method="get" class="filtros">
<select name="modalidade" asp-for="ModalidadeFiltro" asp-items="Model.ModalidadeOpcoes"></select>
<button type="submit">Filtrar</button>
</form>
<div class="produtos-grid">
@foreach (var p in Model.Produtos)
{
<article class="produto-card">
<a asp-page="/Produto" asp-route-id="@p.Id">
@if (p.Fotos.Any()) { <img src="/@p.Fotos.OrderBy(f => f.Ordem).First().Arquivo" alt="@p.Titulo" /> }
<h3>@p.Titulo</h3>
<p>@p.Modalidade?.Nome</p>
<p class="preco">R$ @p.Preco.ToString("N2")</p>
</a>
</article>
}
</div>
`);

w("Produtos.cshtml.cs", `using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class ProdutosModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ProdutosModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public string? ModalidadeFiltro { get; set; }

    public IList<Produto> Produtos { get; private set; } = new List<Produto>();
    public SelectList ModalidadeOpcoes { get; private set; } = null!;

    public async Task OnGetAsync()
    {
        var modalidades = await _db.Modalidades.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        ModalidadeOpcoes = new SelectList(modalidades, "Id", "Nome", ModalidadeFiltro);
        var q = _db.Produtos.AsNoTracking().Include(x => x.Fotos).Include(x => x.Modalidade).Where(x => x.Situacao == "publicado");
        if (!string.IsNullOrWhiteSpace(ModalidadeFiltro))
            q = q.Where(x => x.ModalidadeId == ModalidadeFiltro);
        Produtos = await q.OrderBy(x => x.Titulo).ToListAsync();
    }
}
`);

w("Produto.cshtml", `@page "{id}"
@{ Layout = "_LayoutLoja"; ViewData["Title"] = Model.Produto?.Titulo ?? "Produto"; }
@if (Model.Produto == null) { <p>Produto não encontrado.</p> }
else {
<article class="ficha-produto">
<h1>@Model.Produto.Titulo</h1>
<div class="ficha-grid">
<div class="ficha-fotos">
@foreach (var f in Model.Produto.Fotos.OrderBy(x => x.Ordem))
{ <img src="/@f.Arquivo" alt="@f.Legenda" /> }
</div>
<div>
<p>@Model.Produto.Descricao</p>
<p><strong>Material:</strong> @Model.Produto.Material</p>
<p><strong>Modalidade:</strong> @Model.Produto.Modalidade?.Nome</p>
<p class="preco">R$ @Model.Produto.Preco.ToString("N2")</p>
<form method="post" asp-page-handler="Adicionar">
<input type="hidden" name="id" value="@Model.Produto.Id" />
<button type="submit">Adicionar ao carrinho</button>
</form>
</div>
</div>
<h2>Avaliações</h2>
@foreach (var a in Model.Avaliacoes) { <div class="avaliacao"><strong>@a.Autor</strong> · @a.Nota/5<p>@a.Texto</p></div> }
<h2>Perguntas</h2>
@foreach (var p in Model.Perguntas) { <div class="pergunta"><strong>@p.Autor</strong><p>@p.Texto</p>@if (!string.IsNullOrEmpty(p.Resposta)) { <p class="resposta">@p.Resposta</p> }</div> }
</article>
}
`);

w("Produto.cshtml.cs", `using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class ProdutoModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly CarrinhoService _carrinho;

    public ProdutoModel(ApplicationDbContext db, CarrinhoService carrinho)
    {
        _db = db;
        _carrinho = carrinho;
    }

    public Produto? Produto { get; private set; }
    public IList<Avaliacao> Avaliacoes { get; private set; } = new List<Avaliacao>();
    public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        Produto = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos).Include(x => x.Modalidade).Include(x => x.Variacoes).ThenInclude(v => v.Opcoes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (Produto == null) return NotFound();
        Avaliacoes = await _db.Avaliacoes.AsNoTracking().Where(x => x.ProdutoId == id).ToListAsync();
        Perguntas = await _db.PerguntasProduto.AsNoTracking().Where(x => x.ProdutoId == id).OrderByDescending(x => x.CriadoEm).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAdicionarAsync(string id)
    {
        var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (produto == null) return NotFound();
        _carrinho.Adicionar(new CarrinhoItem { ProdutoId = produto.Id, Titulo = produto.Titulo, PrecoUnitario = produto.Preco, Quantidade = 1 });
        return RedirectToPage("/Carrinho");
    }
}
`);

const storePages = [
  ["Busca.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Busca"; }
<h1>Busca</h1>
<form method="get"><input type="search" name="q" value="@Model.Termo" placeholder="Buscar peças..." /><button type="submit">Buscar</button></form>
<div class="produtos-grid">
@foreach (var p in Model.Resultados)
{ <article><a asp-page="/Produto" asp-route-id="@p.Id"><h3>@p.Titulo</h3><p>R$ @p.Preco.ToString("N2")</p></a></article> }
</div>`,
  `using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
public class BuscaModel : PageModel {
  private readonly ApplicationDbContext _db;
  public BuscaModel(ApplicationDbContext db) => _db = db;
  [BindProperty(SupportsGet = true)] public string? Termo { get; set; }
  public IList<Produto> Resultados { get; private set; } = new List<Produto>();
  public async Task OnGetAsync() {
    if (string.IsNullOrWhiteSpace(Termo)) return;
    var t = Termo.Trim().ToLower();
    Resultados = await _db.Produtos.AsNoTracking().Where(x => x.Situacao == "publicado" && (x.Titulo.ToLower().Contains(t) || x.Descricao.ToLower().Contains(t))).Take(50).ToListAsync();
  }
}`],
  ["Carrinho.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Carrinho"; }
<h1>Carrinho</h1>
@if (!Model.Itens.Any()) { <p>Seu carrinho está vazio.</p> } else {
<table><thead><tr><th>Peça</th><th>Qtd</th><th>Preço</th><th></th></tr></thead><tbody>
@foreach (var i in Model.Itens) {
<tr><td>@i.Titulo</td><td>@i.Quantidade</td><td>R$ @((i.PrecoUnitario * i.Quantidade).ToString("N2"))</td>
<td><form method="post" asp-page-handler="Remover"><input type="hidden" name="produtoId" value="@i.ProdutoId" /><button type="submit">Remover</button></form></td></tr>
}
</tbody></table>
<p><strong>Subtotal:</strong> R$ @Model.Subtotal.ToString("N2")</p>
<a asp-page="/Checkout" class="btn">Finalizar pedido</a>
}`],
  [`using CriaLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace CriaLab.Web.Pages;
public class CarrinhoModel : PageModel {
  private readonly CarrinhoService _carrinho;
  public CarrinhoModel(CarrinhoService carrinho) => _carrinho = carrinho;
  public IList<CarrinhoItem> Itens => _carrinho.ObterItens();
  public decimal Subtotal => _carrinho.Subtotal();
  public IActionResult OnPostRemover(string produtoId) { _carrinho.Remover(produtoId, null, null); return RedirectToPage(); }
}`],
];

w("Busca.cshtml", storePages[0][0]);
w("Busca.cshtml.cs", storePages[0][1]);
w("Carrinho.cshtml", storePages[1][0]);
w("Carrinho.cshtml.cs", storePages[1][1]);

w("Checkout.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Checkout"; }
<h1>Checkout</h1>
@if (Model.Itens.Any()) {
<p>Subtotal: R$ @Model.Subtotal.ToString("N2")</p>
<form method="post">
<label>CEP<input asp-for="Cep" /></label>
<label>Cupom<input asp-for="Cupom" /></label>
<button type="submit">Confirmar pedido simulado</button>
</form>
} else { <p>Adicione itens ao carrinho primeiro.</p> }
@if (!string.IsNullOrEmpty(Model.Mensagem)) { <p class="sucesso">@Model.Mensagem</p> }
`);

w("Checkout.cshtml.cs", `using System.Security.Claims;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
[Authorize]
public class CheckoutModel : PageModel {
  private readonly ApplicationDbContext _db;
  private readonly CarrinhoService _carrinho;
  public CheckoutModel(ApplicationDbContext db, CarrinhoService carrinho) { _db = db; _carrinho = carrinho; }
  public IList<CarrinhoItem> Itens => _carrinho.ObterItens();
  public decimal Subtotal => _carrinho.Subtotal();
  [BindProperty] public string Cep { get; set; } = "";
  [BindProperty] public string Cupom { get; set; } = "";
  public string? Mensagem { get; set; }
  public void OnGet() {}
  public async Task<IActionResult> OnPostAsync() {
    var itens = _carrinho.ObterItens();
    if (!itens.Any()) return Page();
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var pedidoId = "ped-" + Guid.NewGuid().ToString("N")[..8];
    var subtotal = _carrinho.Subtotal();
    var pedido = new Pedido {
      Id = pedidoId, UsuarioId = userId, Status = "novo", CriadoEm = DateTime.UtcNow,
      Subtotal = subtotal, Total = subtotal, Cep = Cep, Cupom = Cupom, Forma = "simulado", Endereco = "Informado no checkout"
    };
    foreach (var i in itens) pedido.Itens.Add(new PedidoItem { Id = Guid.NewGuid().ToString("N"), ProdutoId = i.ProdutoId, Titulo = i.Titulo, Quantidade = i.Quantidade, PrecoUnitario = i.PrecoUnitario, Fisico = true, Tipo = "pronto" });
    _db.Pedidos.Add(pedido);
    await _db.SaveChangesAsync();
    _carrinho.Limpar();
    Mensagem = "Pedido " + pedidoId + " registrado com sucesso.";
    return Page();
  }
}
`);

w("Contato.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Contato"; }
<h1>Contato</h1>
<p>@Model.Identidade?.Telefone · @Model.Identidade?.Email</p>
<form method="post">
<label>Nome<input asp-for="Nome" required /></label>
<label>E-mail<input asp-for="Email" type="email" required /></label>
<label>Tipo<select asp-for="Tipo"><option value="duvida">Dúvida</option><option value="elogio">Elogio</option><option value="reclamacao">Reclamação</option><option value="orcamento">Orçamento</option></select></label>
<label>Mensagem<textarea asp-for="Texto" required></textarea></label>
<button type="submit">Enviar</button>
</form>
@if (Model.Enviado) { <p class="sucesso">Mensagem enviada. Obrigado!</p> }
`);

w("Contato.cshtml.cs", `using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
public class ContatoModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ContatoModel(ApplicationDbContext db) => _db = db;
  public LojaIdentidade? Identidade { get; private set; }
  [BindProperty] public string Nome { get; set; } = "";
  [BindProperty] public string Email { get; set; } = "";
  [BindProperty] public string Tipo { get; set; } = "duvida";
  [BindProperty] public string Texto { get; set; } = "";
  public bool Enviado { get; set; }
  public async Task OnGetAsync() => Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
  public async Task<IActionResult> OnPostAsync() {
    Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
    _db.MensagensContato.Add(new MensagemContato { Id = "msg-" + Guid.NewGuid().ToString("N")[..8], Nome = Nome, Email = Email, Tipo = Tipo, Texto = Texto, Status = "aberto", CriadoEm = DateTime.UtcNow });
    await _db.SaveChangesAsync();
    Enviado = true;
    return Page();
  }
}
`);

w("Sobre.cshtml", `@{ Layout = "_LayoutLoja"; ViewData["Title"] = "Sobre"; }
<h1>Sobre a Cria Lab</h1>
<p>@Model.Ajuda?.Texto</p>
<p>CNPJ: @Model.Identidade?.Cnpj</p>
`);

w("Sobre.cshtml.cs", `using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
public class SobreModel : PageModel {
  private readonly ApplicationDbContext _db;
  public SobreModel(ApplicationDbContext db) => _db = db;
  public LojaIdentidade? Identidade { get; private set; }
  public MenuAjuda? Ajuda { get; private set; }
  public async Task OnGetAsync() {
    Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
    Ajuda = await _db.MenusAjuda.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "sobre");
  }
}
`);

console.log("store pages ok");
