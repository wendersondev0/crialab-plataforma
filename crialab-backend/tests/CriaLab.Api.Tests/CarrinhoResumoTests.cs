using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class CarrinhoResumoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public CarrinhoResumoTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Subtotal_ignora_o_total_enviado_pelo_cliente()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await PostAsync(cliente, Corpo(999m));
        var raiz = json.RootElement;
        Assert.Equal(94m, raiz.GetProperty("subtotal").GetDecimal());
        Assert.Equal(2, raiz.GetProperty("itens").GetArrayLength());
        Assert.Equal(84m, raiz.GetProperty("itens")[0].GetProperty("totalLinha").GetDecimal());
    }

    [Fact]
    public async Task Lista_vazia_soma_zero()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await PostAsync(cliente, new { itens = Array.Empty<object>(), total = 10m });
        Assert.Equal(0m, json.RootElement.GetProperty("subtotal").GetDecimal());
    }

    [Fact]
    public async Task Visitante_usuario_e_administrador_recebem_o_mesmo_resumo()
    {
        var corpo = Corpo(1m);
        var anonimo = _fabrica.CreateClient();
        var anonima = await (await anonimo.PostAsJsonAsync("/api/loja/carrinho/resumo", corpo)).Content.ReadAsStringAsync();
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        Assert.Equal(HttpStatusCode.OK, (await anonimo.PostAsJsonAsync("/api/loja/carrinho/resumo", corpo)).StatusCode);
        Assert.Equal(anonima, await (await usuario.PostAsJsonAsync("/api/loja/carrinho/resumo", corpo)).Content.ReadAsStringAsync());
        Assert.Equal(anonima, await (await admin.PostAsJsonAsync("/api/loja/carrinho/resumo", corpo)).Content.ReadAsStringAsync());
    }

    private static object Corpo(decimal total) => new
    {
        total,
        itens = new object[]
        {
            new
            {
                produtoId = "sessao-2026-03-14",
                titulo = "Peça da oficina — 14/03/2026",
                precoUnitario = 42m,
                quantidade = 2,
                cor = "PLA branco",
                tamanho = (string?)null,
                detalhes = "Cor: PLA branco"
            },
            new
            {
                produtoId = "sessao-2026-03-14",
                titulo = "Peça da oficina — 14/03/2026",
                precoUnitario = 10m,
                quantidade = 1,
                cor = "PLA branco",
                tamanho = (string?)null,
                detalhes = "Outro texto"
            }
        }
    };

    private static async Task<JsonDocument> PostAsync(HttpClient cliente, object corpo)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/loja/carrinho/resumo", corpo);
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
