using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class FreteFichaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public FreteFichaTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Anonimo_cota_pac_sedex_e_rejeita_cep_curto_ou_sem_faixa()
    {
        var cliente = _fabrica.CreateClient();
        var cotado = await CotarAsync(cliente, "sessao-2026-03-14", "29101-000");
        Assert.Equal(JsonValueKind.Null, cotado.GetProperty("erro").ValueKind);
        var opcoes = cotado.GetProperty("opcoes");
        Assert.Equal(2, opcoes.GetArrayLength());
        var pac = Opcao(opcoes, "pac");
        var sedex = Opcao(opcoes, "sedex");
        Assert.Equal(18.9m, pac.GetProperty("preco").GetDecimal());
        Assert.Equal(5, pac.GetProperty("dias").GetInt32());
        Assert.False(pac.GetProperty("gratis").GetBoolean());
        Assert.Equal(32.4m, sedex.GetProperty("preco").GetDecimal());
        Assert.Equal(2, sedex.GetProperty("dias").GetInt32());

        var curto = await CotarAsync(cliente, "sessao-2026-03-14", "123");
        Assert.Equal("Informe um CEP válido com 8 dígitos.", curto.GetProperty("erro").GetString());
        Assert.Equal(0, curto.GetProperty("opcoes").GetArrayLength());

        var semFaixa = await CotarAsync(cliente, "sessao-2026-03-14", "00000-000");
        Assert.Equal("Não entregamos neste CEP ainda.", semFaixa.GetProperty("erro").GetString());
    }

    [Fact]
    public async Task Cliente_recebe_entrega_local_a_cinco_reais()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        var cotado = await CotarAsync(cliente, "sessao-2026-03-14", "29260-000");
        var local = Opcao(cotado.GetProperty("opcoes"), "entrega-local");
        Assert.Equal("Entrega Local", local.GetProperty("nome").GetString());
        Assert.Equal(5m, local.GetProperty("preco").GetDecimal());
        Assert.Equal(1, local.GetProperty("dias").GetInt32());
        Assert.False(local.GetProperty("gratis").GetBoolean());
        Assert.Contains("região atendida", local.GetProperty("descricao").GetString());
    }

    [Fact]
    public async Task Admin_cota_o_mesmo_pac_e_produto_inexistente_responde_404()
    {
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        var cotado = await CotarAsync(admin, "sessao-2026-03-14", "29101000");
        Assert.Equal(18.9m, Opcao(cotado.GetProperty("opcoes"), "pac").GetProperty("preco").GetDecimal());

        var ausente = await admin.GetAsync("/api/loja/produtos/nao-existe/frete?cep=29101-000");
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
    }

    private static JsonElement Opcao(JsonElement opcoes, string id)
    {
        foreach (var opcao in opcoes.EnumerateArray())
        {
            if (opcao.GetProperty("id").GetString() == id)
                return opcao;
        }

        throw new InvalidOperationException(id);
    }

    private async Task<JsonElement> CotarAsync(HttpClient cliente, string produtoId, string cep)
    {
        var resposta = await cliente.GetAsync($"/api/loja/produtos/{produtoId}/frete?cep={Uri.EscapeDataString(cep)}");
        resposta.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await resposta.Content.ReadAsStreamAsync());
        return json.RootElement.Clone();
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
