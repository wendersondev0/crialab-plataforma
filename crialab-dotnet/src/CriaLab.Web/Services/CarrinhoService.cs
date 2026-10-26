using System.Text.Json;

namespace CriaLab.Web.Services;

public class CarrinhoItem
{
    public string ProdutoId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }
    public int Quantidade { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
}

public class CarrinhoService
{
    private const string SessionKey = "crialab-carrinho";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CarrinhoService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public List<CarrinhoItem> ObterItens()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return new List<CarrinhoItem>();
        var json = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return new List<CarrinhoItem>();
        return JsonSerializer.Deserialize<List<CarrinhoItem>>(json) ?? new List<CarrinhoItem>();
    }

    public void SalvarItens(List<CarrinhoItem> itens)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
            return;
        session.SetString(SessionKey, JsonSerializer.Serialize(itens));
    }

    public void Adicionar(CarrinhoItem item)
    {
        var itens = ObterItens();
        var existente = itens.FirstOrDefault(x =>
            x.ProdutoId == item.ProdutoId &&
            x.Cor == item.Cor &&
            x.Tamanho == item.Tamanho);
        if (existente != null)
            existente.Quantidade += item.Quantidade;
        else
            itens.Add(item);
        SalvarItens(itens);
    }

    public void Remover(string produtoId, string? cor, string? tamanho)
    {
        var itens = ObterItens();
        itens.RemoveAll(x => x.ProdutoId == produtoId && x.Cor == cor && x.Tamanho == tamanho);
        SalvarItens(itens);
    }

    public void Limpar() => SalvarItens(new List<CarrinhoItem>());

    public int QuantidadeTotal() => ObterItens().Sum(x => x.Quantidade);

    public decimal Subtotal() => ObterItens().Sum(x => x.PrecoUnitario * x.Quantidade);
}
