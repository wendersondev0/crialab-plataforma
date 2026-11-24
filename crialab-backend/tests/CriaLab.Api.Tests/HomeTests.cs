using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class HomeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public HomeTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Home_anonima_traz_identidade_destaques_e_so_quem_pode_aparecer()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.GetAsync("/api/loja/home");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        var raiz = json.RootElement;

        Assert.Equal("Cria Lab DM", raiz.GetProperty("identidade").GetProperty("nome").GetString());
        Assert.Equal(200m, raiz.GetProperty("freteGratisAPartir").GetDecimal());
        Assert.False(string.IsNullOrWhiteSpace(raiz.GetProperty("intro").GetString()));

        var modalidades = raiz.GetProperty("modalidades").EnumerateArray().Select(x => x.GetProperty("id").GetString()).ToList();
        Assert.Equal("pronta-entrega", modalidades[0]);
        Assert.Contains("sob-demanda", modalidades);
        Assert.Contains("personalizado", modalidades);

        var destaques = raiz.GetProperty("destaques").EnumerateArray().ToList();
        Assert.NotEmpty(destaques);
        Assert.All(destaques, d => Assert.False(string.IsNullOrWhiteSpace(d.GetProperty("titulo").GetString())));

        var autores = raiz.GetProperty("depoimentos").EnumerateArray().Select(x => x.GetProperty("autor").GetString()).ToList();
        Assert.Contains("Mileni", autores);
        Assert.DoesNotContain("Oficina vizinha", autores);

        var parceiros = raiz.GetProperty("parceiros").EnumerateArray().Select(x => x.GetProperty("nome").GetString()).ToList();
        Assert.Contains("GoFit", parceiros);
        Assert.Contains("Mercearia Vila Verde", parceiros);
    }

    [Fact]
    public async Task Home_com_usuario_e_administrador_responde_igual()
    {
        var cliente = _fabrica.CreateClient();
        var anonima = await cliente.GetStringAsync("/api/loja/home");

        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        var comoUsuario = await usuario.GetStringAsync("/api/loja/home");

        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        var comoAdmin = await admin.GetStringAsync("/api/loja/home");

        Assert.Equal(anonima, comoUsuario);
        Assert.Equal(anonima, comoAdmin);
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
