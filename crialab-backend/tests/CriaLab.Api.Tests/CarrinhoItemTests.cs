using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class CarrinhoItemTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public CarrinhoItemTests(WebApplicationFactory<Program> fabrica)
    {
        _fabrica = fabrica.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "true");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
        });
    }

    [Fact]
    public async Task Pronta_entrega_usa_o_preco_do_banco_e_corta_no_estoque()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await PostAsync(cliente, new
        {
            produtoId = "sessao-2026-03-14",
            quantidade = 9,
            cor = "  PLA branco ",
            tamanho = " ",
            detalhes = "Cor: PLA branco",
            destino = "CHECKOUT"
        });
        var raiz = json.RootElement;
        Assert.Equal(42m, raiz.GetProperty("precoUnitario").GetDecimal());
        Assert.Equal(2, raiz.GetProperty("quantidade").GetInt32());
        Assert.Equal("PLA branco", raiz.GetProperty("cor").GetString());
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("tamanho").ValueKind);
        Assert.Equal("checkout", raiz.GetProperty("destino").GetString());
    }

    [Fact]
    public async Task Produto_inexistente_responde_404()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/loja/carrinho/itens", new { produtoId = "nao-existe", quantidade = 1 });
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Usuario_e_administrador_recebem_o_mesmo_item()
    {
        var corpo = new
        {
            produtoId = "sessao-2026-03-14",
            quantidade = 1,
            cor = "PLA teal",
            tamanho = (string?)null,
            detalhes = "Cor: PLA teal",
            destino = "toast"
        };
        var anonimo = _fabrica.CreateClient();
        var anonima = await (await anonimo.PostAsJsonAsync("/api/loja/carrinho/itens", corpo)).Content.ReadAsStringAsync();
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        Assert.Equal(anonima, await (await usuario.PostAsJsonAsync("/api/loja/carrinho/itens", corpo)).Content.ReadAsStringAsync());
        Assert.Equal(anonima, await (await admin.PostAsJsonAsync("/api/loja/carrinho/itens", corpo)).Content.ReadAsStringAsync());
    }

    private static async Task<JsonDocument> PostAsync(HttpClient cliente, object corpo)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/loja/carrinho/itens", corpo);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
    }

    private async Task<HttpClient> EntrarAsync(string email, string senha)
    {
        var anonimo = _fabrica.CreateClient();
        var resposta = await anonimo.PostAsJsonAsync("/api/conta/entrar", new { email, senha });
        resposta.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("accessToken").GetString();
        var cliente = _fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
