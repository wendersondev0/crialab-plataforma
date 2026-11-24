using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class FavoritoFichaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public FavoritoFichaTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Anonimo_recebe_401()
    {
        var cliente = _fabrica.CreateClient();
        var consulta = await cliente.GetAsync("/api/loja/favoritos/sessao-2026-03-14");
        var alternar = await cliente.PostAsync("/api/loja/favoritos/sessao-2026-03-14", null);
        Assert.Equal(HttpStatusCode.Unauthorized, consulta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, alternar.StatusCode);
    }

    [Fact]
    public async Task Cliente_alterna_o_favorito_e_volta_ao_estado_inicial()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        Assert.False(await LerAsync(cliente, "sessao-2026-03-14"));
        Assert.True(await AlternarAsync(cliente, "sessao-2026-03-14"));
        Assert.True(await LerAsync(cliente, "sessao-2026-03-14"));
        Assert.False(await AlternarAsync(cliente, "sessao-2026-03-14"));
        Assert.False(await LerAsync(cliente, "sessao-2026-03-14"));

        using var ana = await EntrarAsync("ana.silva@teste.crialab.local", "Teste@123!");
        Assert.True(await LerAsync(ana, "sessao-2026-03-14"));
    }

    [Fact]
    public async Task Admin_nao_herda_o_favorito_do_cliente_e_produto_inexistente_responde_404()
    {
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        Assert.False(await LerAsync(admin, "sessao-2026-03-14"));

        const string produto = "sessao-2026-04-11";
        try
        {
            Assert.True(await AlternarAsync(admin, produto));
            Assert.True(await LerAsync(admin, produto));
        }
        finally
        {
            if (await LerAsync(admin, produto))
                Assert.False(await AlternarAsync(admin, produto));
        }

        var ausente = await admin.PostAsync("/api/loja/favoritos/nao-existe", null);
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
        var consulta = await admin.GetAsync("/api/loja/favoritos/nao-existe");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    private async Task<bool> LerAsync(HttpClient cliente, string produtoId)
    {
        var resposta = await cliente.GetAsync($"/api/loja/favoritos/{produtoId}");
        resposta.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await resposta.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("favorito").GetBoolean();
    }

    private async Task<bool> AlternarAsync(HttpClient cliente, string produtoId)
    {
        var resposta = await cliente.PostAsync($"/api/loja/favoritos/{produtoId}", null);
        resposta.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await resposta.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("favorito").GetBoolean();
    }

    private async Task<HttpClient> EntrarAsync(string email, string senha)
    {
        var anonimo = _fabrica.CreateClient();
        var entrada = await anonimo.PostAsJsonAsync("/api/conta/entrar", new { email, senha });
        entrada.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await entrada.Content.ReadAsStreamAsync());
        var token = json.RootElement.GetProperty("accessToken").GetString();
        var cliente = _fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
