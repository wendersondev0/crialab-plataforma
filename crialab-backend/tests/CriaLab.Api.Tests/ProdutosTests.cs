using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class ProdutosTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public ProdutosTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Sem_filtro_lista_as_publicadas_por_titulo()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await LerAsync(cliente, "/api/loja/produtos");
        var raiz = json.RootElement;
        Assert.Equal("Produtos", raiz.GetProperty("titulo").GetString());
        Assert.Equal("Todas as peças publicadas na vitrine.", raiz.GetProperty("texto").GetString());
        var titulos = raiz.GetProperty("produtos").EnumerateArray().Select(x => x.GetProperty("titulo").GetString()!).ToList();
        Assert.Equal(8, titulos.Count);
        Assert.Equal(titulos.OrderBy(x => x, StringComparer.Ordinal).ToList(), titulos);
    }

    [Fact]
    public async Task Modalidade_filtra_e_desconhecida_deixa_a_lista_vazia()
    {
        var cliente = _fabrica.CreateClient();
        using var pronta = await LerAsync(cliente, "/api/loja/produtos?modalidadeFiltro=pronta-entrega");
        var itens = pronta.RootElement.GetProperty("produtos").EnumerateArray().ToList();
        Assert.NotEmpty(itens);
        Assert.All(itens, x => Assert.Equal("pronta-entrega", x.GetProperty("modalidadeId").GetString()));
        Assert.Equal("Pronta Entrega", pronta.RootElement.GetProperty("titulo").GetString());

        using var sumiu = await LerAsync(cliente, "/api/loja/produtos?modalidadeFiltro=sumiu");
        Assert.Equal("Produtos", sumiu.RootElement.GetProperty("titulo").GetString());
        Assert.Equal(0, sumiu.RootElement.GetProperty("produtos").GetArrayLength());
    }

    [Fact]
    public async Task Usuario_e_administrador_recebem_a_mesma_lista()
    {
        var cliente = _fabrica.CreateClient();
        var anonima = await cliente.GetStringAsync("/api/loja/produtos?modalidadeFiltro=personalizado");
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        Assert.Equal(anonima, await usuario.GetStringAsync("/api/loja/produtos?modalidadeFiltro=personalizado"));
        Assert.Equal(anonima, await admin.GetStringAsync("/api/loja/produtos?modalidadeFiltro=personalizado"));
    }

    private static async Task<JsonDocument> LerAsync(HttpClient cliente, string url)
    {
        var resposta = await cliente.GetAsync(url);
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
