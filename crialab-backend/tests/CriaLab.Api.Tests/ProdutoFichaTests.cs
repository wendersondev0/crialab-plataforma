using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class ProdutoFichaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public ProdutoFichaTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Ficha_traz_fotos_variacao_legado_avaliacoes_e_perguntas()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await LerAsync(cliente, "/api/loja/produtos/sessao-2026-03-14");
        var raiz = json.RootElement;
        Assert.Equal("Peça da oficina — 14/03/2026", raiz.GetProperty("titulo").GetString());
        Assert.True(raiz.GetProperty("fotos").GetArrayLength() > 0);
        Assert.Contains(raiz.GetProperty("variacoes").EnumerateArray(), eixo => eixo.GetProperty("nome").GetString() == "Cor");
        Assert.True(raiz.GetProperty("cores").GetArrayLength() >= 4);
        Assert.Contains(raiz.GetProperty("tamanhos").EnumerateArray(), t => t.GetProperty("nome").GetString() == "Único");
        Assert.True(raiz.GetProperty("avaliacoes").GetArrayLength() >= 3);
        var perguntas = raiz.GetProperty("perguntas").EnumerateArray().ToList();
        Assert.Contains(perguntas, p => p.GetProperty("respondida").GetBoolean());
        Assert.Contains(perguntas, p => !p.GetProperty("respondida").GetBoolean() && p.GetProperty("texto").GetString()!.Contains("Brasil"));
        Assert.True(raiz.GetProperty("relacionados").GetArrayLength() <= 4);
        Assert.DoesNotContain(raiz.GetProperty("relacionados").EnumerateArray(), p => p.GetProperty("id").GetString() == "sessao-2026-03-14");
    }

    [Fact]
    public async Task Personalizado_traz_etapas()
    {
        var cliente = _fabrica.CreateClient();
        using var json = await LerAsync(cliente, "/api/loja/produtos/pers-chaveiro-cachorro");
        var raiz = json.RootElement;
        Assert.True(raiz.GetProperty("personalizavel").GetBoolean());
        Assert.Equal("personalizado", raiz.GetProperty("modalidadeId").GetString());
        var tipos = raiz.GetProperty("etapas").EnumerateArray().Select(x => x.GetProperty("tipo").GetString()).ToList();
        Assert.Contains("arquivo", tipos);
        Assert.Contains("texto", tipos);
    }

    [Fact]
    public async Task Id_inexistente_responde_404()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.GetAsync("/api/loja/produtos/nao-existe");
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Usuario_e_administrador_recebem_a_mesma_ficha()
    {
        var cliente = _fabrica.CreateClient();
        var anonima = await cliente.GetStringAsync("/api/loja/produtos/sessao-2026-03-14");
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        Assert.Equal(anonima, await usuario.GetStringAsync("/api/loja/produtos/sessao-2026-03-14"));
        Assert.Equal(anonima, await admin.GetStringAsync("/api/loja/produtos/sessao-2026-03-14"));
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
