using System.Globalization;
using System.Text;
using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages;

public class BuscaModel : PageModel
{
    private static readonly CultureInfo Brasil = CultureInfo.GetCultureInfo("pt-BR");
    private readonly ApplicationDbContext _db;

    public BuscaModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Termo { get; set; }

    [BindProperty(SupportsGet = true, Name = "categoria")]
    public string? Categoria { get; set; }

    [BindProperty(SupportsGet = true, Name = "precoMin")]
    public string? PrecoMinTexto { get; set; }

    [BindProperty(SupportsGet = true, Name = "precoMax")]
    public string? PrecoMaxTexto { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Ordem { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Nota { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FreteGratis { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? AVista { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Personalizavel { get; set; }

    public IList<Produto> Resultados { get; private set; } = new List<Produto>();
    public IList<Categoria> ListaCategorias { get; private set; } = new List<Categoria>();
    public IReadOnlyList<FiltroAtivo> Ativos { get; private set; } = Array.Empty<FiltroAtivo>();
    public decimal? PrecoMin { get; private set; }
    public decimal? PrecoMax { get; private set; }
    public decimal FreteGratisAPartir { get; private set; } = 200;
    public string Ajuda { get; private set; } = "Filtre peças por texto, tema e preço.";
    public string? Mensagem { get; private set; }
    public bool TemFiltro { get; private set; }
    public string PrecoMinCampo => CampoPreco(PrecoMin);
    public string PrecoMaxCampo => CampoPreco(PrecoMax);
    public string FreteGratisTexto => FreteGratisAPartir.ToString("C", Brasil);
    public string Resumo => Resultados.Count == 1 ? "1 peça" : $"{Resultados.Count} peças";

    public readonly record struct FiltroAtivo(string Rotulo, string Href);

    public async Task OnGetAsync()
    {
        await CarregarContextoAsync();
        Normalizar();
        var lista = await ConsultaBase().ToListAsync();
        if (!string.IsNullOrWhiteSpace(Termo))
            lista = FiltrarTexto(lista, Termo);
        Resultados = Ordenar(lista);
        MontarAtivos();
        ViewData["Title"] = string.IsNullOrWhiteSpace(Termo) ? "Busca" : $"Busca: {Termo}";
    }

    public async Task<IActionResult> OnGetSugestoesAsync(string? q)
    {
        await CarregarContextoAsync();
        Normalizar();
        var texto = (q ?? Termo ?? "").Trim();
        if (texto.Length < 2)
            return new JsonResult(Array.Empty<object>());

        var lista = FiltrarTexto(await ConsultaBase().ToListAsync(), texto)
            .OrderByDescending(x => SemAcento(x.Titulo).Contains(SemAcento(texto), StringComparison.Ordinal))
            .ThenBy(x => x.Titulo)
            .Take(6)
            .Select(x => new
            {
                id = x.Id,
                titulo = x.Titulo,
                preco = x.Preco,
                categoria = x.Categoria?.Nome ?? "",
                foto = x.Fotos.OrderBy(f => f.Ordem).Select(f => f.Arquivo).FirstOrDefault()
            });
        return new JsonResult(lista);
    }

    private async Task CarregarContextoAsync()
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        if (cfg != null)
            FreteGratisAPartir = cfg.FreteGratisAPartir;
        ViewData["FreteGratisAPartir"] = FreteGratisAPartir;

        ListaCategorias = await _db.Categorias.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();
        var ajuda = await _db.MenusAjuda.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "busca");
        if (ajuda != null && !string.IsNullOrWhiteSpace(ajuda.Texto))
            Ajuda = ajuda.Texto;
    }

    private void Normalizar()
    {
        Termo = string.IsNullOrWhiteSpace(Termo) ? null : Termo.Trim();

        var cat = ListaCategorias.FirstOrDefault(c => string.Equals(c.Id, Categoria?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(Categoria) && cat == null)
        {
            Mensagem = "A categoria desse link não existe mais. Mostrando todas.";
            Categoria = null;
        }
        else
        {
            Categoria = cat?.Id;
        }

        var okMin = TentarPreco(PrecoMinTexto, out var min);
        var okMax = TentarPreco(PrecoMaxTexto, out var max);
        if ((!string.IsNullOrWhiteSpace(PrecoMinTexto) && !okMin) || (!string.IsNullOrWhiteSpace(PrecoMaxTexto) && !okMax))
            Mensagem = "Use apenas números no preço. Exemplo: 25 ou 25,90.";
        if (okMin) PrecoMin = min;
        if (okMax) PrecoMax = max;
        if (PrecoMin.HasValue && PrecoMax.HasValue && PrecoMin > PrecoMax)
        {
            (PrecoMin, PrecoMax) = (PrecoMax, PrecoMin);
            Mensagem = "O preço mínimo estava acima do máximo. A faixa foi invertida.";
        }

        var ordem = (Ordem ?? "").Trim().ToLowerInvariant();
        Ordem = ordem is "preco" or "preco-desc" or "vendas" or "novidade" ? ordem : "relevancia";

        var nota = (Nota ?? "").Trim().Replace(',', '.');
        Nota = nota is "4" or "4.0" or "5" or "5.0" ? nota[..1] : null;

        FreteGratis = Flag(FreteGratis);
        AVista = Flag(AVista);
        Personalizavel = Flag(Personalizavel);

        TemFiltro = Termo != null
            || Categoria != null
            || PrecoMin.HasValue
            || PrecoMax.HasValue
            || FreteGratis == "1"
            || AVista == "1"
            || Personalizavel == "1"
            || Nota != null
            || Ordem != "relevancia";
    }

    private IQueryable<Produto> ConsultaBase()
    {
        var query = _db.Produtos.AsNoTracking()
            .Include(x => x.Fotos)
            .Include(x => x.Modalidade)
            .Include(x => x.Categoria)
            .Where(x => x.Situacao == "publicado");

        if (!string.IsNullOrWhiteSpace(Categoria))
            query = query.Where(x => x.CategoriaId == Categoria);
        if (PrecoMin.HasValue)
            query = query.Where(x => x.Preco >= PrecoMin.Value);
        if (PrecoMax.HasValue)
            query = query.Where(x => x.Preco <= PrecoMax.Value);
        if (FreteGratis == "1")
            query = query.Where(x => x.Preco >= FreteGratisAPartir);
        if (AVista == "1")
            query = query.Where(x => x.AVista);
        if (Personalizavel == "1")
            query = query.Where(x => x.Personalizavel);
        if (Nota is "4" or "5" && decimal.TryParse(Nota, NumberStyles.Number, CultureInfo.InvariantCulture, out var notaMin))
            query = query.Where(x => x.Nota >= notaMin);

        return query;
    }

    private static List<Produto> FiltrarTexto(IEnumerable<Produto> lista, string termo)
    {
        var t = SemAcento(termo);
        return lista.Where(x =>
            SemAcento(x.Titulo).Contains(t, StringComparison.Ordinal) ||
            SemAcento(x.Descricao).Contains(t, StringComparison.Ordinal) ||
            SemAcento(x.Material).Contains(t, StringComparison.Ordinal) ||
            SemAcento(x.Categoria?.Nome).Contains(t, StringComparison.Ordinal)).ToList();
    }

    private List<Produto> Ordenar(List<Produto> lista)
    {
        var termo = SemAcento(Termo);
        return Ordem switch
        {
            "preco" => lista.OrderBy(x => x.Preco).ThenBy(x => x.Titulo).ToList(),
            "preco-desc" => lista.OrderByDescending(x => x.Preco).ThenBy(x => x.Titulo).ToList(),
            "vendas" => lista.OrderByDescending(x => x.Vendas).ThenBy(x => x.Titulo).ToList(),
            "novidade" => lista.OrderByDescending(x => x.CriadoEm).ThenBy(x => x.Titulo).ToList(),
            _ => lista
                .OrderByDescending(x => termo.Length > 0 && SemAcento(x.Titulo).Contains(termo, StringComparison.Ordinal) ? 2 : 1)
                .ThenByDescending(x => x.Vendas)
                .ThenBy(x => x.Titulo)
                .ToList()
        };
    }

    private void MontarAtivos()
    {
        var itens = new List<FiltroAtivo>();
        if (!string.IsNullOrWhiteSpace(Termo))
            itens.Add(new FiltroAtivo($"Texto: {Termo}", Sem("q")));
        if (!string.IsNullOrWhiteSpace(Categoria))
        {
            var nome = ListaCategorias.FirstOrDefault(c => c.Id == Categoria)?.Nome ?? Categoria;
            itens.Add(new FiltroAtivo(nome, Sem("categoria")));
        }
        if (PrecoMin.HasValue && PrecoMax.HasValue)
            itens.Add(new FiltroAtivo($"{PrecoMin.Value.ToString("C", Brasil)} – {PrecoMax.Value.ToString("C", Brasil)}", Sem("precoMin", "precoMax")));
        else if (PrecoMin.HasValue)
            itens.Add(new FiltroAtivo($"A partir de {PrecoMin.Value.ToString("C", Brasil)}", Sem("precoMin")));
        else if (PrecoMax.HasValue)
            itens.Add(new FiltroAtivo($"Até {PrecoMax.Value.ToString("C", Brasil)}", Sem("precoMax")));
        if (FreteGratis == "1")
            itens.Add(new FiltroAtivo("Frete grátis", Sem("freteGratis")));
        if (AVista == "1")
            itens.Add(new FiltroAtivo("À vista", Sem("aVista")));
        if (Personalizavel == "1")
            itens.Add(new FiltroAtivo("Personalizável", Sem("personalizavel")));
        if (Nota == "4")
            itens.Add(new FiltroAtivo("Nota 4 ou mais", Sem("nota")));
        else if (Nota == "5")
            itens.Add(new FiltroAtivo("Nota 5", Sem("nota")));
        if (Ordem != "relevancia")
            itens.Add(new FiltroAtivo(RotuloOrdem(Ordem), Sem("ordem")));
        Ativos = itens;
    }

    private string Sem(params string[] chaves)
    {
        var valores = Parametros();
        foreach (var chave in chaves)
            valores.Remove(chave);
        return MontarLink(valores);
    }

    private Dictionary<string, string?> Parametros()
    {
        var valores = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(Termo)) valores["q"] = Termo;
        if (!string.IsNullOrWhiteSpace(Categoria)) valores["categoria"] = Categoria;
        if (PrecoMin.HasValue) valores["precoMin"] = PrecoMin.Value.ToString("0.00", CultureInfo.InvariantCulture);
        if (PrecoMax.HasValue) valores["precoMax"] = PrecoMax.Value.ToString("0.00", CultureInfo.InvariantCulture);
        if (FreteGratis == "1") valores["freteGratis"] = "1";
        if (AVista == "1") valores["aVista"] = "1";
        if (Personalizavel == "1") valores["personalizavel"] = "1";
        if (Nota is "4" or "5") valores["nota"] = Nota;
        if (Ordem is "preco" or "preco-desc" or "vendas" or "novidade") valores["ordem"] = Ordem;
        return valores;
    }

    private string MontarLink(Dictionary<string, string?> valores)
    {
        var rota = new RouteValueDictionary();
        foreach (var par in valores)
        {
            if (!string.IsNullOrWhiteSpace(par.Value))
                rota[par.Key] = par.Value;
        }
        return Url.Page("/Busca", rota) ?? "/Busca";
    }

    private static string CampoPreco(decimal? valor) => valor?.ToString("0.00", Brasil) ?? "";

    private static string? Flag(string? valor)
    {
        var v = (valor ?? "").Trim().ToLowerInvariant();
        return v is "1" or "true" or "on" or "sim" ? "1" : null;
    }

    private static bool TentarPreco(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        var t = texto.Trim().Replace("R$", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "");
        if (t.Contains(',') && t.Contains('.'))
            t = t.Replace(".", "").Replace(',', '.');
        else
            t = t.Replace(',', '.');
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor >= 0;
    }

    private static string RotuloOrdem(string? ordem) => ordem switch
    {
        "preco" => "Menor preço",
        "preco-desc" => "Maior preço",
        "vendas" => "Mais vendidos",
        "novidade" => "Novidades",
        _ => "Relevância"
    };

    private static string SemAcento(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var form = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(form.Length);
        foreach (var c in form)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
