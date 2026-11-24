using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class CheckoutCotacaoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public CheckoutCotacaoTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Visitante_recebe_401()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/loja/checkout/cotacao", Corpo(1, 1m, 9999m, "pix", "cotar", "29101-000", "Rua 1"));
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Usuario_recalcula_preco_cupom_desconto_e_frete_e_ignora_o_total()
    {
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var json = await PostAsync(usuario, Corpo(1, 1m, 9999m, "pix", "cotar", "29101-000", "Rua 1"));
        var raiz = json.RootElement;
        Assert.Equal("Peça da oficina — 14/03/2026", raiz.GetProperty("itens")[0].GetProperty("titulo").GetString());
        Assert.Equal(42m, raiz.GetProperty("itens")[0].GetProperty("precoUnitario").GetDecimal());
        Assert.Equal(42m, raiz.GetProperty("subtotal").GetDecimal());
        Assert.Equal(4.20m, raiz.GetProperty("descontoCupom").GetDecimal());
        Assert.Equal(3.78m, raiz.GetProperty("descontoAVista").GetDecimal());
        Assert.Equal(18.90m, raiz.GetProperty("freteCobrado").GetDecimal());
        Assert.Equal(52.92m, raiz.GetProperty("total").GetDecimal());
        Assert.Equal("PAC", raiz.GetProperty("freteNome").GetString());
        Assert.Empty(raiz.GetProperty("erros").EnumerateArray());
    }

    [Fact]
    public async Task Cartao_nao_recebe_desconto_a_vista_e_boleto_recebe()
    {
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var cartao = await PostAsync(usuario, Corpo(1, 42m, 0m, "cartao", "cotar", "29101000", "Rua 1"));
        using var boleto = await PostAsync(usuario, Corpo(1, 42m, 0m, "boleto", "cotar", "29101000", "Rua 1"));
        Assert.Equal(0m, cartao.RootElement.GetProperty("descontoAVista").GetDecimal());
        Assert.Equal(56.70m, cartao.RootElement.GetProperty("total").GetDecimal());
        Assert.Equal(3.78m, boleto.RootElement.GetProperty("descontoAVista").GetDecimal());
        Assert.Equal(52.92m, boleto.RootElement.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Finalizar_bloqueia_sem_endereco_e_sem_frete()
    {
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var semEndereco = await PostAsync(usuario, Corpo(1, 42m, 0m, "pix", "finalizar", "29101-000", "   "));
        using var semFrete = await PostAsync(usuario, Corpo(1, 42m, 0m, "pix", "finalizar", "", "Rua 1"));
        Assert.Equal("Informe o endereço de entrega.", semEndereco.RootElement.GetProperty("erros")[0].GetString());
        Assert.Equal("Informe um CEP válido com 8 dígitos.", semFrete.RootElement.GetProperty("erros")[0].GetString());
    }

    [Fact]
    public async Task Frete_gratis_e_peca_inexistente()
    {
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var gratis = await PostAsync(usuario, Corpo(5, 1m, 1m, "pix", "finalizar", "29260-000", "Rua 1", "sedex"));
        Assert.Equal(210m, gratis.RootElement.GetProperty("subtotal").GetDecimal());
        Assert.Equal(0m, gratis.RootElement.GetProperty("freteCobrado").GetDecimal());
        Assert.Equal("SEDEX", gratis.RootElement.GetProperty("freteNome").GetString());
        Assert.Empty(gratis.RootElement.GetProperty("erros").EnumerateArray());

        var ausente = await usuario.PostAsJsonAsync("/api/loja/checkout/cotacao", new
        {
            itens = new[] { new { produtoId = "nao-existe", quantidade = 1, precoUnitario = 10m } },
            acao = "cotar"
        });
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
    }

    [Fact]
    public async Task Administrador_recebe_a_mesma_cotacao_do_cliente()
    {
        var corpo = Corpo(1, 1m, 9999m, "pix", "cotar", "29101-000", "Rua 1");
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        var doCliente = await (await usuario.PostAsJsonAsync("/api/loja/checkout/cotacao", corpo)).Content.ReadAsStringAsync();
        var doAdmin = await (await admin.PostAsJsonAsync("/api/loja/checkout/cotacao", corpo)).Content.ReadAsStringAsync();
        Assert.Equal(doCliente, doAdmin);
    }

    private static object Corpo(int quantidade, decimal preco, decimal total, string forma, string acao, string cep, string endereco, string freteId = "pac") => new
    {
        total,
        cep,
        cupom = "oficina10",
        freteId,
        endereco,
        forma,
        acao,
        itens = new[]
        {
            new
            {
                produtoId = "sessao-2026-03-14",
                titulo = "ERRADO",
                precoUnitario = preco,
                quantidade,
                cor = "PLA areia",
                tamanho = (string?)null,
                detalhes = "Cor: PLA areia"
            }
        }
    };

    private static async Task<JsonDocument> PostAsync(HttpClient cliente, object corpo)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/loja/checkout/cotacao", corpo);
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
