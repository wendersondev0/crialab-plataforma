using System.Text.Json;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ProdutoEditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ProdutoEditModel(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [BindProperty]
    public Produto Produto { get; set; } = new();

    [BindProperty]
    public string VariacoesJson { get; set; } = "[]";

    [BindProperty]
    public string EtapasJson { get; set; } = "[]";

    [BindProperty]
    public List<FotoLegendaInput> FotoLegendas { get; set; } = new();

    [BindProperty]
    public List<IFormFile> NovasFotos { get; set; } = new();

    public SelectList Categorias { get; private set; } = null!;
    public SelectList Modalidades { get; private set; } = null!;
    public IList<ProdutoFoto> Fotos { get; private set; } = new List<ProdutoFoto>();
    public bool Novo { get; private set; }

    public class FotoLegendaInput
    {
        public int Id { get; set; }
        public string Legenda { get; set; } = string.Empty;
    }

    public class VariacaoDto
    {
        public string? Id { get; set; }
        public string Nome { get; set; } = "Variação";
        public string Tipo { get; set; } = "texto";
        public List<OpcaoDto> Opcoes { get; set; } = new();
    }

    public class OpcaoDto
    {
        public string? Id { get; set; }
        public string Nome { get; set; } = "Opção";
        public string Hex { get; set; } = "#cccccc";
    }

    public class EtapaDto
    {
        public string? Id { get; set; }
        public string Nome { get; set; } = "Etapa";
        public string Tipo { get; set; } = "texto";
        public decimal Acresce { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        await CarregarListasAsync();
        if (string.IsNullOrWhiteSpace(id))
        {
            Novo = true;
            Produto = new Produto
            {
                Id = "prod-" + Guid.NewGuid().ToString("N")[..8],
                Situacao = "pausado",
                ModalidadeId = "pronta-entrega",
                Tipo = "pronta-entrega",
                Material = "PLA",
                PrazoDias = 5,
                AVista = true,
                CriadoEm = DateOnly.FromDateTime(DateTime.UtcNow),
                Sessao = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
            };
            var cat = await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).Select(x => x.Id).FirstOrDefaultAsync();
            Produto.CategoriaId = cat ?? "";
            VariacoesJson = "[]";
            EtapasJson = "[]";
            return Page();
        }

        var existente = await _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.EtapasPersonalizacao)
            .Include(x => x.Variacoes).ThenInclude(v => v.Opcoes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (existente == null) return NotFound();
        Produto = existente;
        Fotos = existente.Fotos.OrderBy(x => x.Ordem).ToList();
        VariacoesJson = JsonSerializer.Serialize(existente.Variacoes.OrderBy(x => x.Ordem).Select(v => new VariacaoDto
        {
            Id = v.Id,
            Nome = v.Nome,
            Tipo = v.Tipo,
            Opcoes = v.Opcoes.OrderBy(o => o.Ordem).Select(o => new OpcaoDto { Id = o.Id, Nome = o.Nome, Hex = o.Hex }).ToList()
        }));
        EtapasJson = JsonSerializer.Serialize(existente.EtapasPersonalizacao.Select(e => new EtapaDto
        {
            Id = e.Id,
            Nome = e.Nome,
            Tipo = e.Tipo,
            Acresce = e.Acresce
        }));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CarregarListasAsync();
        if (string.IsNullOrWhiteSpace(Produto.Titulo))
        {
            ModelState.AddModelError("Produto.Titulo", "Informe o título.");
            Novo = !await _db.Produtos.AnyAsync(x => x.Id == Produto.Id);
            Fotos = await _db.ProdutoFotos.AsNoTracking().Where(x => x.ProdutoId == Produto.Id).OrderBy(x => x.Ordem).ToListAsync();
            return Page();
        }

        var existente = await _db.Produtos
            .Include(x => x.Fotos)
            .Include(x => x.EtapasPersonalizacao)
            .Include(x => x.Variacoes).ThenInclude(v => v.Opcoes)
            .FirstOrDefaultAsync(x => x.Id == Produto.Id);

        if (existente == null)
        {
            if (string.IsNullOrWhiteSpace(Produto.Id))
                Produto.Id = "prod-" + Guid.NewGuid().ToString("N")[..8];
            if (Produto.CriadoEm == default)
                Produto.CriadoEm = DateOnly.FromDateTime(DateTime.UtcNow);
            if (string.IsNullOrWhiteSpace(Produto.Sessao))
                Produto.Sessao = Produto.CriadoEm.ToString("yyyy-MM-dd");
            Produto.Tipo = Produto.ModalidadeId == "personalizado" ? "personalizado" : "pronto";
            Produto.Personalizavel = Produto.ModalidadeId == "personalizado" || Produto.Personalizavel;
            _db.Produtos.Add(Produto);
            existente = Produto;
        }
        else
        {
            existente.Titulo = Produto.Titulo;
            existente.Descricao = Produto.Descricao;
            existente.CategoriaId = Produto.CategoriaId;
            existente.ModalidadeId = Produto.ModalidadeId;
            existente.Tipo = Produto.ModalidadeId == "personalizado" ? "personalizado" : "pronto";
            existente.Preco = Produto.Preco;
            existente.PrecoComparativo = Produto.PrecoComparativo;
            existente.Estoque = Produto.Estoque;
            existente.Minimo = Produto.Minimo;
            existente.Medidas = Produto.Medidas;
            existente.Peso = Produto.Peso;
            existente.Material = Produto.Material;
            existente.PrazoDias = Produto.PrazoDias;
            existente.Situacao = Produto.Situacao;
            existente.Oferta = Produto.Oferta;
            existente.MaisVendido = Produto.MaisVendido;
            existente.Personalizavel = Produto.ModalidadeId == "personalizado" || Produto.Personalizavel;
            existente.AVista = Produto.AVista;
            existente.ComArquivo = Produto.ComArquivo;
        }

        foreach (var leg in FotoLegendas)
        {
            var foto = existente.Fotos.FirstOrDefault(x => x.Id == leg.Id);
            if (foto != null) foto.Legenda = leg.Legenda ?? "";
        }

        await SalvarFotosNovasAsync(existente);
        await SalvarVariacoesAsync(existente);
        await SalvarEtapasAsync(existente);

        await _db.SaveChangesAsync();
        TempData["Ok"] = "Produto salvo.";
        return RedirectToPage("/Admin/Produtos");
    }

    public async Task<IActionResult> OnPostRemoverFotoAsync(string id, int fotoId)
    {
        var foto = await _db.ProdutoFotos.FirstOrDefaultAsync(x => x.Id == fotoId && x.ProdutoId == id);
        if (foto != null)
        {
            var abs = Path.Combine(_env.WebRootPath, foto.Arquivo.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(abs))
                System.IO.File.Delete(abs);
            _db.ProdutoFotos.Remove(foto);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    private async Task SalvarFotosNovasAsync(Produto produto)
    {
        if (NovasFotos == null || NovasFotos.Count == 0) return;
        var pastaRel = Path.Combine("fotos", DateTime.UtcNow.ToString("yyyy-MM-dd"));
        var pastaAbs = Path.Combine(_env.WebRootPath, pastaRel);
        Directory.CreateDirectory(pastaAbs);
        var ordem = produto.Fotos.Any() ? produto.Fotos.Max(x => x.Ordem) + 1 : 0;
        foreach (var arquivo in NovasFotos.Where(f => f.Length > 0))
        {
            if (!ImageUploadHelper.EhImagemValida(arquivo, out var erro))
            {
                TempData["Erro"] = erro;
                continue;
            }
            var ext = ImageUploadHelper.ExtensaoSegura(arquivo);
            var nome = Guid.NewGuid().ToString("N")[..12] + ext;
            var caminhoAbs = Path.Combine(pastaAbs, nome);
            await using (var stream = System.IO.File.Create(caminhoAbs))
                await arquivo.CopyToAsync(stream);
            _db.ProdutoFotos.Add(new ProdutoFoto
            {
                ProdutoId = produto.Id,
                Arquivo = Path.Combine(pastaRel, nome).Replace('\\', '/'),
                Sessao = produto.Sessao,
                Legenda = produto.Titulo,
                Ordem = ordem++
            });
        }
    }

    private async Task SalvarVariacoesAsync(Produto produto)
    {
        var dtos = JsonSerializer.Deserialize<List<VariacaoDto>>(VariacoesJson ?? "[]", JsonOpts) ?? new();
        var antigos = await _db.VariacaoEixos.Where(x => x.ProdutoId == produto.Id).Include(x => x.Opcoes).ToListAsync();
        if (antigos.Count > 0)
            _db.VariacaoEixos.RemoveRange(antigos);

        var ordem = 0;
        foreach (var eixo in dtos.Where(x => x.Opcoes.Any(o => !string.IsNullOrWhiteSpace(o.Nome))))
        {
            var eixoId = string.IsNullOrWhiteSpace(eixo.Id) ? "var-" + Guid.NewGuid().ToString("N")[..8] : eixo.Id!;
            var entidade = new VariacaoEixo
            {
                Id = eixoId,
                ProdutoId = produto.Id,
                Nome = string.IsNullOrWhiteSpace(eixo.Nome) ? "Variação" : eixo.Nome.Trim(),
                Tipo = eixo.Tipo == "cor" ? "cor" : "texto",
                Ordem = ordem++
            };
            var j = 0;
            foreach (var op in eixo.Opcoes.Where(o => !string.IsNullOrWhiteSpace(o.Nome)))
            {
                entidade.Opcoes.Add(new VariacaoOpcao
                {
                    Id = string.IsNullOrWhiteSpace(op.Id) ? "opc-" + Guid.NewGuid().ToString("N")[..8] : op.Id!,
                    VariacaoEixoId = eixoId,
                    Nome = op.Nome.Trim(),
                    Hex = string.IsNullOrWhiteSpace(op.Hex) ? "#cccccc" : op.Hex,
                    Ordem = j++
                });
            }
            _db.VariacaoEixos.Add(entidade);
        }
    }

    private async Task SalvarEtapasAsync(Produto produto)
    {
        var dtos = JsonSerializer.Deserialize<List<EtapaDto>>(EtapasJson ?? "[]", JsonOpts) ?? new();
        var antigos = await _db.ProdutoEtapas.Where(x => x.ProdutoId == produto.Id).ToListAsync();
        if (antigos.Count > 0)
            _db.ProdutoEtapas.RemoveRange(antigos);

        foreach (var e in dtos.Where(x => !string.IsNullOrWhiteSpace(x.Nome)))
        {
            var tipo = e.Tipo switch
            {
                "arquivo" => "arquivo",
                "numero" => "numero",
                _ => "texto"
            };
            _db.ProdutoEtapas.Add(new ProdutoEtapaPersonalizacao
            {
                Id = string.IsNullOrWhiteSpace(e.Id) ? "etp-" + Guid.NewGuid().ToString("N")[..8] : e.Id!,
                ProdutoId = produto.Id,
                Nome = e.Nome.Trim(),
                Tipo = tipo,
                Acresce = e.Acresce
            });
        }
    }

    private async Task CarregarListasAsync()
    {
        Categorias = new SelectList(await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome", Produto?.CategoriaId);
        Modalidades = new SelectList(await _db.Modalidades.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(), "Id", "Nome", Produto?.ModalidadeId);
    }
}
