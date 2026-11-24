using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class BuscaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public BuscaTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Sem_filtro_traz_a_vitrine_publicada()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await BuscaAsync(cliente, "/api/loja/busca");
        var raiz = json.RootElement;
        Assert.False(raiz.GetProperty("temFiltro").GetBoolean());
        Assert.Equal(8, raiz.GetProperty("resultados").GetArrayLength());
        Assert.Equal("8 peças", raiz.GetProperty("resumo").GetString());
        Assert.Equal(0, raiz.GetProperty("ativos").GetArrayLength());
    }

    [Fact]
    public async Task Texto_filtra_e_sugestao_curta_vem_vazia()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await BuscaAsync(cliente, "/api/loja/busca?q=chaveiro");
        var titulos = json.RootElement.GetProperty("resultados").EnumerateArray().Select(x => x.GetProperty("titulo").GetString()).ToList();
        Assert.Contains(titulos, t => t!.Contains("Chaveiro", StringComparison.OrdinalIgnoreCase));
        Assert.True(json.RootElement.GetProperty("temFiltro").GetBoolean());

        var curta = await cliente.GetAsync("/api/loja/busca/sugestoes?q=c");
        curta.EnsureSuccessStatusCode();
        Assert.Equal("[]", (await curta.Content.ReadAsStringAsync()).Trim());
    }

    [Fact]
    public async Task Categoria_inexistente_avisa_e_chip_some_ao_remover()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await BuscaAsync(cliente, "/api/loja/busca?categoria=sumiu&ordem=preco");
        Assert.Equal(
            "A categoria desse link não existe mais. Mostrando todas.",
            json.RootElement.GetProperty("mensagem").GetString());
        var ativos = json.RootElement.GetProperty("ativos").EnumerateArray().ToList();
        Assert.Contains(ativos, x => x.GetProperty("rotulo").GetString() == "Menor preço");
        Assert.DoesNotContain(ativos, x => x.GetProperty("rotulo").GetString() == "sumiu");
    }

    [Fact]
    public async Task Usuario_e_administrador_recebem_a_mesma_busca()
    {
        var cliente = _fabrica.CreateClient();
        var anonima = await cliente.GetStringAsync("/api/loja/busca?q=peca");
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        Assert.Equal(anonima, await usuario.GetStringAsync("/api/loja/busca?q=peca"));
        Assert.Equal(anonima, await admin.GetStringAsync("/api/loja/busca?q=peca"));
    }

    private static async Task<JsonDocument> BuscaAsync(HttpClient cliente, string url)
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
