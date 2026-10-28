using System.Text.Json;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using CriaLab.Web.Data.Seeding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class SistemaModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IServiceProvider _services;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public SistemaModel(ApplicationDbContext db, IWebHostEnvironment env, IServiceProvider services)
    {
        _db = db;
        _env = env;
        _services = services;
    }

    public bool PodeRestaurar { get; private set; }

    public string Aba { get; private set; } = "loja";
    public string Cep { get; private set; } = "";
    public LojaIdentidade Identidade { get; set; } = new();
    public ConfiguracaoLoja Config { get; set; } = new();
    public Cupom? Cupom { get; private set; }
    public IList<FaixaCep> Faixas { get; private set; } = new List<FaixaCep>();
    public IList<FaixaCep> FaixasVisiveis { get; private set; } = new List<FaixaCep>();
    public IList<TipoInsumoConfig> TiposInsumo { get; private set; } = new List<TipoInsumoConfig>();
    public IList<ParametroCusto> Parametros { get; private set; } = new List<ParametroCusto>();
    public IList<MargemCanal> Margens { get; private set; } = new List<MargemCanal>();
    public IList<Depoimento> Depoimentos { get; private set; } = new List<Depoimento>();
    public IList<Parceiro> Parceiros { get; private set; } = new List<Parceiro>();
    public int DepoimentosVisiveis { get; private set; }
    public int ParceirosVisiveis { get; private set; }
    public List<string> Avisos { get; private set; } = new();

    [BindProperty]
    public decimal DescontoAVistaPct { get; set; }

    [BindProperty]
    public decimal TaxaFalhaPct { get; set; }

    [BindProperty]
    public string FaixasJson { get; set; } = "[]";

    public class FaixaDto
    {
        public int De { get; set; }
        public int Ate { get; set; }
        public decimal Pac { get; set; }
        public int PacDias { get; set; } = 5;
        public decimal Sedex { get; set; }
        public int SedexDias { get; set; } = 2;
    }

    public async Task OnGetAsync(string? aba, string? cep)
    {
        Aba = aba is "vendas" or "frete" or "site" or "avancado" ? aba : "loja";
        Cep = cep ?? "";
        PodeRestaurar = _env.IsDevelopment();
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostRestaurarAsync()
    {
        if (!_env.IsDevelopment())
        {
            TempData["Erro"] = "Restauração só disponível em Development.";
            return RedirectToPage(new { aba = "avancado" });
        }

        await DbSeeder.RestaurarDemoAsync(_services);
        TempData["Ok"] = "Carga inicial restaurada.";
        return RedirectToPage(new { aba = "avancado" });
    }

    public async Task<IActionResult> OnPostLojaAsync()
    {
        var id = await _db.LojaIdentidades.FirstOrDefaultAsync();
        if (id == null)
        {
            id = new LojaIdentidade { Id = "principal" };
            _db.LojaIdentidades.Add(id);
        }

        id.Nome = Identidade.Nome?.Trim() ?? "";
        id.Cnpj = Identidade.Cnpj?.Trim() ?? "";
        id.Email = Identidade.Email?.Trim() ?? "";
        id.Telefone = Identidade.Telefone?.Trim() ?? "";
        id.Site = Identidade.Site?.Trim() ?? "";
        id.Endereco = Identidade.Endereco?.Trim() ?? "";
        id.Horario = Identidade.Horario?.Trim() ?? "";
        id.Whatsapp = Identidade.Whatsapp?.Trim() ?? "";
        id.Instagram = Identidade.Instagram?.Trim() ?? "";
        id.Tiktok = Identidade.Tiktok?.Trim() ?? "";
        id.Youtube = Identidade.Youtube?.Trim() ?? "";
        id.Shopee = Identidade.Shopee?.Trim() ?? "";
        id.MercadoLivre = Identidade.MercadoLivre?.Trim() ?? "";
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Dados da loja salvos.";
        return RedirectToPage(new { aba = "loja" });
    }

    public async Task<IActionResult> OnPostVendasAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.FirstOrDefaultAsync();
        if (cfg == null)
        {
            cfg = new ConfiguracaoLoja { Id = "default" };
            _db.ConfiguracoesLoja.Add(cfg);
        }

        cfg.FreteGratisAPartir = Config.FreteGratisAPartir;
        cfg.DescontoAVista = Math.Clamp(DescontoAVistaPct, 0, 90) / 100m;
        cfg.Parcelas = Math.Clamp(Config.Parcelas, 1, 12);
        cfg.TaxaFalha = Math.Clamp(TaxaFalhaPct, 0, 50) / 100m;
        cfg.FreteCanal = Config.FreteCanal;
        cfg.RepassarFrete = Config.RepassarFrete;
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Regras de venda salvas. Pedidos antigos não mudam.";
        return RedirectToPage(new { aba = "vendas" });
    }

    public async Task<IActionResult> OnPostFreteAsync()
    {
        var dtos = JsonSerializer.Deserialize<List<FaixaDto>>(FaixasJson ?? "[]", JsonOpts) ?? new();
        var atuais = await _db.FaixasCep.ToListAsync();
        _db.FaixasCep.RemoveRange(atuais);
        foreach (var f in dtos.Where(x => x.Ate >= x.De))
        {
            _db.FaixasCep.Add(new FaixaCep
            {
                De = f.De,
                Ate = f.Ate,
                Pac = f.Pac,
                PacDias = f.PacDias <= 0 ? 5 : f.PacDias,
                Sedex = f.Sedex,
                SedexDias = f.SedexDias <= 0 ? 2 : f.SedexDias
            });
        }
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Faixas de CEP salvas.";
        return RedirectToPage(new { aba = "frete" });
    }

    public async Task<IActionResult> OnPostPubDepoimentoAsync(string id, bool podeAparecer)
    {
        var item = await _db.Depoimentos.FirstOrDefaultAsync(x => x.Id == id);
        if (item != null)
        {
            item.PodeAparecer = podeAparecer;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = "site" });
    }

    public async Task<IActionResult> OnPostPubParceiroAsync(string id, bool podeAparecer)
    {
        var item = await _db.Parceiros.FirstOrDefaultAsync(x => x.Id == id);
        if (item != null)
        {
            item.PodeAparecer = podeAparecer;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = "site" });
    }

    public async Task<IActionResult> OnPostTipoAddAsync(string tipo)
    {
        tipo = (tipo ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var existe = await _db.TiposInsumoConfig.AnyAsync(x => x.Nome == tipo);
            if (!existe)
            {
                var ordem = await _db.TiposInsumoConfig.CountAsync();
                _db.TiposInsumoConfig.Add(new TipoInsumoConfig { Nome = tipo, Ordem = ordem });
                await _db.SaveChangesAsync();
                TempData["Ok"] = "Tipo de insumo adicionado.";
            }
        }
        return RedirectToPage(new { aba = "avancado" });
    }

    public async Task<IActionResult> OnPostTipoRmAsync(int id)
    {
        var item = await _db.TiposInsumoConfig.FirstOrDefaultAsync(x => x.Id == id);
        if (item != null)
        {
            _db.TiposInsumoConfig.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Tipo removido da lista.";
        }
        return RedirectToPage(new { aba = "avancado" });
    }

    public async Task<IActionResult> OnPostParametroAsync(string id, decimal preco, bool travado)
    {
        var p = await _db.ParametrosCusto.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            p.Preco = preco;
            p.Travado = travado;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Parâmetro de custo atualizado.";
        }
        return RedirectToPage(new { aba = "avancado" });
    }

    public async Task<IActionResult> OnPostMargemAsync(string id, decimal percentualPct, bool ativo)
    {
        var m = await _db.MargensCanal.FirstOrDefaultAsync(x => x.Id == id);
        if (m != null)
        {
            m.Percentual = Math.Clamp(percentualPct, 0, 95) / 100m;
            m.Ativo = ativo;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Margem atualizada.";
        }
        return RedirectToPage(new { aba = "avancado" });
    }

    private async Task CarregarAsync()
    {
        Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync()
            ?? new LojaIdentidade { Id = "principal" };
        Config = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync()
            ?? new ConfiguracaoLoja { Id = "default", Parcelas = 3, FreteGratisAPartir = 200, DescontoAVista = 0.05m };
        Cupom = await _db.Cupons.AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync();
        Faixas = await _db.FaixasCep.AsNoTracking().OrderBy(x => x.De).ToListAsync();
        TiposInsumo = await _db.TiposInsumoConfig.AsNoTracking().OrderBy(x => x.Ordem).ToListAsync();
        Parametros = await _db.ParametrosCusto.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        Margens = await _db.MargensCanal.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        Depoimentos = await _db.Depoimentos.AsNoTracking().OrderBy(x => x.Autor).ToListAsync();
        Parceiros = await _db.Parceiros.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        DepoimentosVisiveis = Depoimentos.Count(x => x.PodeAparecer);
        ParceirosVisiveis = Parceiros.Count(x => x.PodeAparecer);

        DescontoAVistaPct = Math.Round(Config.DescontoAVista * 100m, 1);
        TaxaFalhaPct = Math.Round(Config.TaxaFalha * 100m, 1);
        FaixasJson = JsonSerializer.Serialize(Faixas.Select(f => new FaixaDto
        {
            De = f.De,
            Ate = f.Ate,
            Pac = f.Pac,
            PacDias = f.PacDias,
            Sedex = f.Sedex,
            SedexDias = f.SedexDias
        }));

        var digitos = new string((Cep ?? "").Where(char.IsDigit).ToArray());
        if (digitos.Length > 0 && int.TryParse(digitos.Length > 8 ? digitos[..8] : digitos, out var n) && n > 0)
            FaixasVisiveis = Faixas.Where(f => n >= f.De && n <= f.Ate).ToList();
        else
            FaixasVisiveis = Faixas;

        if (string.IsNullOrWhiteSpace(Identidade.Endereco)) Avisos.Add("Endereço da loja vazio (some do rodapé).");
        if (string.IsNullOrWhiteSpace(Identidade.Horario)) Avisos.Add("Horário vazio (some do rodapé).");
        if (Cupom is { Ativo: false }) Avisos.Add("Cupom pausado no Marketing.");
    }
}
