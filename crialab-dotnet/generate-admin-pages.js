const fs = require("fs");
const path = require("path");
const admin = path.join("d:", "projetos", "ctgi", "crialab-dotnet", "src", "CriaLab.Web", "Pages", "Admin");

function w(name, cshtml, cs) {
  fs.writeFileSync(path.join(admin, name + ".cshtml"), cshtml.replace(/\n/g, "\r\n"));
  fs.writeFileSync(path.join(admin, name + ".cshtml.cs"), cs.replace(/\n/g, "\r\n"));
}

const auth = `[Authorize(Policy = "AdminOnly")]`;

w("Index", `@page
@model CriaLab.Web.Pages.Admin.IndexModel
@{ ViewData["Title"] = "Painel"; }
<h1>Painel administrativo</h1>
<ul>
<li>@Model.TotalProdutos produtos publicados</li>
<li>@Model.TotalPedidos pedidos</li>
<li>@Model.MensagensAbertas mensagens abertas</li>
<li>@Model.JobsImpressao jobs de impressão</li>
</ul>`,
`using CriaLab.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class IndexModel : PageModel {
  private readonly ApplicationDbContext _db;
  public IndexModel(ApplicationDbContext db) => _db = db;
  public int TotalProdutos { get; private set; }
  public int TotalPedidos { get; private set; }
  public int MensagensAbertas { get; private set; }
  public int JobsImpressao { get; private set; }
  public async Task OnGetAsync() {
    TotalProdutos = await _db.Produtos.CountAsync(x => x.Situacao == "publicado");
    TotalPedidos = await _db.Pedidos.CountAsync();
    MensagensAbertas = await _db.MensagensContato.CountAsync(x => x.Status == "aberto");
    JobsImpressao = await _db.ImpressaoJobs.CountAsync();
  }
}`);

w("Produtos", `@page
@model CriaLab.Web.Pages.Admin.ProdutosModel
@{ ViewData["Title"] = "Produtos"; }
<h1>Produtos</h1>
<table><thead><tr><th>Título</th><th>Categoria</th><th>Modalidade</th><th>Preço</th><th></th></tr></thead><tbody>
@foreach (var p in Model.Produtos) {
<tr><td>@p.Titulo</td><td>@p.CategoriaId</td><td>@p.ModalidadeId</td><td>R$ @p.Preco.ToString("N2")</td>
<td><a asp-page="/Admin/ProdutoEdit" asp-route-id="@p.Id">Editar</a></td></tr>
}
</tbody></table>`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class ProdutosModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ProdutosModel(ApplicationDbContext db) => _db = db;
  public IList<Produto> Produtos { get; private set; } = new List<Produto>();
  public async Task OnGetAsync() => Produtos = await _db.Produtos.AsNoTracking().OrderBy(x => x.Titulo).ToListAsync();
}`);

w("ProdutoEdit", `@page "{id?}"
@model CriaLab.Web.Pages.Admin.ProdutoEditModel
@{ ViewData["Title"] = "Editar produto"; }
<h1>Editar produto</h1>
@if (Model.Produto == null) { <p>Produto não encontrado.</p> } else {
<form method="post">
<input type="hidden" asp-for="Produto.Id" />
<label>Título<input asp-for="Produto.Titulo" /></label>
<label>Preço<input asp-for="Produto.Preco" /></label>
<label>Estoque<input asp-for="Produto.Estoque" /></label>
<label>Situação<input asp-for="Produto.Situacao" /></label>
<button type="submit">Salvar</button>
</form>
}` ,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class ProdutoEditModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ProdutoEditModel(ApplicationDbContext db) => _db = db;
  [BindProperty] public Produto? Produto { get; set; }
  public async Task<IActionResult> OnGetAsync(string? id) {
    if (string.IsNullOrWhiteSpace(id)) return NotFound();
    Produto = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id);
    return Page();
  }
  public async Task<IActionResult> OnPostAsync() {
    if (Produto == null) return NotFound();
    var existente = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == Produto.Id);
    if (existente == null) return NotFound();
    existente.Titulo = Produto.Titulo;
    existente.Preco = Produto.Preco;
    existente.Estoque = Produto.Estoque;
    existente.Situacao = Produto.Situacao;
    await _db.SaveChangesAsync();
    return RedirectToPage("/Admin/Produtos");
  }
}`);

const simpleList = (title, prop, queryDesc) => w(title, `@page
@model CriaLab.Web.Pages.Admin.${title}Model
@{ ViewData["Title"] = "${queryDesc}"; }
<h1>${queryDesc}</h1>
<ul>@foreach (var item in Model.Itens) { <li>@item</li> }</ul>`, `using CriaLab.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class ${title}Model : PageModel {
  private readonly ApplicationDbContext _db;
  public ${title}Model(ApplicationDbContext db) => _db = db;
  public IList<string> Itens { get; private set; } = new List<string>();
  public async Task OnGetAsync() { Itens = await ${prop}; }
}`);

w("Pedidos", `@page
@model CriaLab.Web.Pages.Admin.PedidosModel
@{ ViewData["Title"] = "Pedidos"; }
<h1>Pedidos</h1>
@foreach (var p in Model.Pedidos) { <article>@p.Id · @p.Status · R$ @p.Total.ToString("N2") · @p.CriadoEm</article> }`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class PedidosModel : PageModel {
  private readonly ApplicationDbContext _db;
  public PedidosModel(ApplicationDbContext db) => _db = db;
  public IList<Pedido> Pedidos { get; private set; } = new List<Pedido>();
  public async Task OnGetAsync() => Pedidos = await _db.Pedidos.AsNoTracking().Include(x => x.Itens).OrderByDescending(x => x.CriadoEm).ToListAsync();
}`);

w("Marketing", `@page
@model CriaLab.Web.Pages.Admin.MarketingModel
@{ ViewData["Title"] = "Marketing"; }
<h1>Marketing</h1>
<h2>Depoimentos</h2>
@foreach (var d in Model.Depoimentos) { <p>@d.Autor: @d.Texto</p> }
<h2>Parceiros</h2>
@foreach (var p in Model.Parceiros) { <p>@p.Nome</p> }
<h2>Cupons</h2>
@foreach (var c in Model.Cupons) { <p>@c.Codigo · @(c.Ativo ? "ativo" : "inativo")</p> }`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class MarketingModel : PageModel {
  private readonly ApplicationDbContext _db;
  public MarketingModel(ApplicationDbContext db) => _db = db;
  public IList<Depoimento> Depoimentos { get; private set; } = new List<Depoimento>();
  public IList<Parceiro> Parceiros { get; private set; } = new List<Parceiro>();
  public IList<Cupom> Cupons { get; private set; } = new List<Cupom>();
  public async Task OnGetAsync() {
    Depoimentos = await _db.Depoimentos.AsNoTracking().ToListAsync();
    Parceiros = await _db.Parceiros.AsNoTracking().ToListAsync();
    Cupons = await _db.Cupons.AsNoTracking().ToListAsync();
  }
}`);

w("Atendimento", `@page
@model CriaLab.Web.Pages.Admin.AtendimentoModel
@{ ViewData["Title"] = "Atendimento"; }
<h1>Atendimento</h1>
<h2>Mensagens</h2>
@foreach (var m in Model.Mensagens) { <article>@m.Tipo · @m.Nome · @m.Status<p>@m.Texto</p></article> }
<h2>Perguntas</h2>
@foreach (var p in Model.Perguntas) { <article>@p.ProdutoId · @p.Autor<p>@p.Texto</p></article> }`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class AtendimentoModel : PageModel {
  private readonly ApplicationDbContext _db;
  public AtendimentoModel(ApplicationDbContext db) => _db = db;
  public IList<MensagemContato> Mensagens { get; private set; } = new List<MensagemContato>();
  public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();
  public async Task OnGetAsync() {
    Mensagens = await _db.MensagensContato.AsNoTracking().OrderByDescending(x => x.CriadoEm).ToListAsync();
    Perguntas = await _db.PerguntasProduto.AsNoTracking().OrderByDescending(x => x.CriadoEm).Take(20).ToListAsync();
  }
}`);

w("Patrimonio", `@page
@model CriaLab.Web.Pages.Admin.PatrimonioModel
@{ ViewData["Title"] = "Patrimônio"; }
<h1>Patrimônio</h1>
<h2>Máquinas</h2>
@foreach (var m in Model.Maquinas) { <article>@m.Nome · @m.Status · @m.Local</article> }
<h2>Estoque de insumos</h2>
@foreach (var i in Model.Insumos) { <article>@i.Nome · @i.Saldo @i.Unidade</article> }`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class PatrimonioModel : PageModel {
  private readonly ApplicationDbContext _db;
  public PatrimonioModel(ApplicationDbContext db) => _db = db;
  public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
  public IList<EstoqueInsumo> Insumos { get; private set; } = new List<EstoqueInsumo>();
  public async Task OnGetAsync() {
    Maquinas = await _db.Maquinas.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
    Insumos = await _db.EstoqueInsumos.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
  }
}`);

w("Financeiro", `@page
@model CriaLab.Web.Pages.Admin.FinanceiroModel
@{ ViewData["Title"] = "Financeiro"; }
<h1>Financeiro</h1>
<h2>Contas</h2>
@foreach (var c in Model.Contas) { <p>@c.Tipo · @c.Descricao · R$ @c.Valor.ToString("N2")</p> }
<h2>Lançamentos recentes</h2>
@foreach (var l in Model.Lancamentos) { <p>@l.Competencia · @l.Categoria · @l.Descricao · R$ @l.Valor.ToString("N2")</p> }`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class FinanceiroModel : PageModel {
  private readonly ApplicationDbContext _db;
  public FinanceiroModel(ApplicationDbContext db) => _db = db;
  public IList<ContaFinanceira> Contas { get; private set; } = new List<ContaFinanceira>();
  public IList<LancamentoFinanceiro> Lancamentos { get; private set; } = new List<LancamentoFinanceiro>();
  public async Task OnGetAsync() {
    Contas = await _db.ContasFinanceiras.AsNoTracking().ToListAsync();
    Lancamentos = await _db.LancamentosFinanceiros.AsNoTracking().OrderByDescending(x => x.Competencia).Take(30).ToListAsync();
  }
}`);

w("Fiscal", `@page
@model CriaLab.Web.Pages.Admin.FiscalModel
@{ ViewData["Title"] = "Fiscal"; }
<h1>Fiscal</h1>
<p>CNPJ: @Model.Identidade?.Cnpj</p>
<p>Pedidos com nota pendente: @Model.PedidosNotaPendente</p>`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class FiscalModel : PageModel {
  private readonly ApplicationDbContext _db;
  public FiscalModel(ApplicationDbContext db) => _db = db;
  public LojaIdentidade? Identidade { get; private set; }
  public int PedidosNotaPendente { get; private set; }
  public async Task OnGetAsync() {
    Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
    PedidosNotaPendente = await _db.Pedidos.CountAsync(x => x.Nota == "pendente");
  }
}`);

w("Sistema", `@page
@model CriaLab.Web.Pages.Admin.SistemaModel
@{ ViewData["Title"] = "Sistema"; }
<h1>Sistema</h1>
<p>Frete grátis a partir de R$ @Model.Config?.FreteGratisAPartir.ToString("N2")</p>
<p>Desconto à vista: @(Model.Config != null ? (Model.Config.DescontoAVista * 100).ToString("N0") + "%" : "")</p>
<h2>Faixas de CEP</h2>
@foreach (var f in Model.Faixas) { <p>@f.De-@f.Ate · PAC R$ @f.Pac.ToString("N2") · SEDEX R$ @f.Sedex.ToString("N2")</p> }
<h2>Tipos de insumo</h2>
<p>@string.Join(", ", Model.TiposInsumo.Select(x => x.Nome))</p>`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class SistemaModel : PageModel {
  private readonly ApplicationDbContext _db;
  public SistemaModel(ApplicationDbContext db) => _db = db;
  public ConfiguracaoLoja? Config { get; private set; }
  public IList<FaixaCep> Faixas { get; private set; } = new List<FaixaCep>();
  public IList<TipoInsumoConfig> TiposInsumo { get; private set; } = new List<TipoInsumoConfig>();
  public async Task OnGetAsync() {
    Config = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
    Faixas = await _db.FaixasCep.AsNoTracking().OrderBy(x => x.De).ToListAsync();
    TiposInsumo = await _db.TiposInsumoConfig.AsNoTracking().OrderBy(x => x.Ordem).ToListAsync();
  }
}`);

w("Impressao", `@page
@model CriaLab.Web.Pages.Admin.ImpressaoModel
@{ ViewData["Title"] = "Impressão"; }
<h1>Fila de impressão</h1>
@foreach (var j in Model.Jobs) {
<article>@j.Titulo · @j.Maquina?.Nome · @j.Status · @j.PesoPrevisto g</article>
}`,
`using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
${auth}
public class ImpressaoModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ImpressaoModel(ApplicationDbContext db) => _db = db;
  public IList<ImpressaoJob> Jobs { get; private set; } = new List<ImpressaoJob>();
  public async Task OnGetAsync() => Jobs = await _db.ImpressaoJobs.AsNoTracking().Include(x => x.Maquina).OrderByDescending(x => x.Inicio).ToListAsync();
}`);

console.log("admin ok");
