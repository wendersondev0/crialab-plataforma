using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriaLab.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaLab.Api.Tests;

public class CheckoutPedidoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public CheckoutPedidoTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Visitante_recebe_401_ao_gravar_e_ao_abrir_recibo()
    {
        var cliente = _fabrica.CreateClient();
        var gravar = await cliente.PostAsJsonAsync("/api/loja/checkout/pedidos", Corpo("Rua 1", "29101-000", "pix"));
        var recibo = await cliente.GetAsync("/api/loja/pedidos/ped-entregue");
        Assert.Equal(HttpStatusCode.Unauthorized, gravar.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, recibo.StatusCode);
    }

    [Fact]
    public async Task Confirmacao_invalida_nao_grava()
    {
        using var usuario = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        var semFrete = "Rua sem frete " + Guid.NewGuid().ToString("N");
        using var endereco = await PostAsync(usuario, Corpo("   ", "29101-000", "pix"));
        using var frete = await PostAsync(usuario, Corpo(semFrete, "", "pix"));
        Assert.Equal("Informe o endereço de entrega.", endereco.RootElement.GetProperty("erros")[0].GetString());
        Assert.Equal("Informe um CEP válido com 8 dígitos.", frete.RootElement.GetProperty("erros")[0].GetString());
        Assert.Equal(JsonValueKind.Null, endereco.RootElement.GetProperty("pedidoId").ValueKind);
        Assert.Equal(JsonValueKind.Null, frete.RootElement.GetProperty("pedidoId").ValueKind);
        Assert.Empty(await IdsPorEnderecoAsync(semFrete));
    }

    [Fact]
    public async Task Cliente_grava_pedido_aguardando_os_sem_baixa_e_so_o_dono_ve_o_recibo()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var outra = await EntrarAsync("ana.silva@teste.crialab.local", "Teste@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        var endereco = "Rua da gravacao " + Guid.NewGuid().ToString("N");
        string? id = null;
        try
        {
            using var json = await PostAsync(cliente, Corpo("  " + endereco + "  ", "29101-000", "pix"));
            var raiz = json.RootElement;
            id = raiz.GetProperty("pedidoId").GetString();
            Assert.Matches("^ped-[0-9a-f]{8}$", id);
            Assert.Equal(42m, raiz.GetProperty("subtotal").GetDecimal());
            Assert.Equal(4.20m, raiz.GetProperty("descontoCupom").GetDecimal());
            Assert.Equal(3.78m, raiz.GetProperty("descontoAVista").GetDecimal());
            Assert.Equal(18.90m, raiz.GetProperty("freteCobrado").GetDecimal());
            Assert.Equal(52.92m, raiz.GetProperty("total").GetDecimal());
            Assert.Equal("Peça da oficina — 14/03/2026", raiz.GetProperty("itens")[0].GetProperty("titulo").GetString());
            Assert.Empty(raiz.GetProperty("erros").EnumerateArray());

            using var escopo = _fabrica.Services.CreateScope();
            var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var pedido = await db.Pedidos.Include(x => x.Itens).Include(x => x.Os).SingleAsync(x => x.Id == id);
            Assert.Equal("aguardando pagamento", pedido.Status);
            Assert.Equal(endereco, pedido.Endereco);
            Assert.Equal("OFICINA10", pedido.Cupom);
            Assert.Equal("pix", pedido.Forma);
            Assert.Equal("PAC", pedido.FreteNome);
            Assert.Equal(52.92m, pedido.Total);
            Assert.False(pedido.RepassarFrete);
            Assert.Equal("", pedido.Nota);
            var item = Assert.Single(pedido.Itens);
            Assert.Equal(32, item.Id.Length);
            Assert.Equal("sessao-2026-03-14", item.ProdutoId);
            Assert.Equal("Peça da oficina — 14/03/2026", item.Titulo);
            Assert.Equal(1, item.Quantidade);
            Assert.Equal(42m, item.PrecoUnitario);
            Assert.True(item.Fisico);
            Assert.Equal("pronto", item.Tipo);
            Assert.False(item.Arquivo);
            Assert.Equal("PLA areia", item.CorEscolhida);
            Assert.Null(item.TamanhoEscolhido);
            Assert.NotNull(pedido.Os);
            Assert.Equal("aguardando", pedido.Os.Status);
            Assert.False(pedido.Os.InsumoBaixado);

            using var doDono = await GetAsync(cliente, id!);
            Assert.Equal(id, doDono.RootElement.GetProperty("id").GetString());
            Assert.Equal(endereco, doDono.RootElement.GetProperty("endereco").GetString());
            Assert.Equal("Cria Lab DM", doDono.RootElement.GetProperty("nomeLoja").GetString());
            Assert.Equal("68.914.190/0001-96", doDono.RootElement.GetProperty("cnpj").GetString());
            Assert.Equal(42m, doDono.RootElement.GetProperty("itens")[0].GetProperty("totalLinha").GetDecimal());

            var daOutra = await outra.GetAsync("/api/loja/pedidos/" + id);
            Assert.Equal(HttpStatusCode.NotFound, daOutra.StatusCode);

            using var doAdmin = await GetAsync(admin, id!);
            Assert.Equal(id, doAdmin.RootElement.GetProperty("id").GetString());
        }
        finally
        {
            if (id is not null)
                await ApagarAsync(id);
        }
    }

    [Fact]
    public async Task Administrador_grava_pedido_e_cartao_nao_leva_desconto_a_vista()
    {
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        var endereco = "Rua do admin " + Guid.NewGuid().ToString("N");
        string? id = null;
        try
        {
            using var json = await PostAsync(admin, Corpo(endereco, "29101000", "cartao"));
            id = json.RootElement.GetProperty("pedidoId").GetString();
            Assert.Matches("^ped-[0-9a-f]{8}$", id);
            Assert.Equal(0m, json.RootElement.GetProperty("descontoAVista").GetDecimal());
            Assert.Equal(56.70m, json.RootElement.GetProperty("total").GetDecimal());

            using var escopo = _fabrica.Services.CreateScope();
            var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var pedido = await db.Pedidos.Include(x => x.Os).SingleAsync(x => x.Id == id);
            Assert.Equal("cartao", pedido.Forma);
            Assert.Equal(0m, pedido.DescontoAVista);
            Assert.Equal("aguardando pagamento", pedido.Status);
            Assert.NotNull(pedido.Os);
            Assert.False(pedido.Os.InsumoBaixado);
        }
        finally
        {
            if (id is not null)
                await ApagarAsync(id);
        }
    }

    [Fact]
    public async Task Recibo_de_outro_cliente_some_e_peca_inexistente_responde_404()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var ana = await EntrarAsync("ana.silva@teste.crialab.local", "Teste@123!");
        var doCliente = await cliente.GetAsync("/api/loja/pedidos/ped-ana-1");
        var daAna = await ana.GetAsync("/api/loja/pedidos/ped-ana-1");
        var ausente = await cliente.GetAsync("/api/loja/pedidos/ped-nao-existe");
        var peca = await cliente.PostAsJsonAsync("/api/loja/checkout/pedidos", new
        {
            itens = new[] { new { produtoId = "nao-existe", quantidade = 1, precoUnitario = 10m, total = 9999m } },
            endereco = "Rua 1",
            cep = "29101-000",
            forma = "pix",
            total = 9999m
        });
        Assert.Equal(HttpStatusCode.NotFound, doCliente.StatusCode);
        Assert.Equal(HttpStatusCode.OK, daAna.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, peca.StatusCode);
    }

    private static object Corpo(string endereco, string cep, string forma) => new
    {
        total = 9999m,
        cep,
        cupom = "oficina10",
        freteId = "pac",
        endereco,
        forma,
        acao = "cotar",
        itens = new[]
        {
            new
            {
                produtoId = "sessao-2026-03-14",
                titulo = "ERRADO",
                precoUnitario = 1m,
                quantidade = 1,
                cor = "PLA areia",
                tamanho = (string?)null,
                detalhes = "Cor: PLA areia"
            }
        }
    };

    private static async Task<JsonDocument> PostAsync(HttpClient cliente, object corpo)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/loja/checkout/pedidos", corpo);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
    }

    private static async Task<JsonDocument> GetAsync(HttpClient cliente, string id)
    {
        var resposta = await cliente.GetAsync("/api/loja/pedidos/" + id);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
    }

    private async Task<List<string>> IdsPorEnderecoAsync(string endereco)
    {
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        return await db.Pedidos.Where(x => x.Endereco == endereco).Select(x => x.Id).ToListAsync();
    }

    private async Task ApagarAsync(string id)
    {
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var pedido = await db.Pedidos.Include(x => x.Itens).Include(x => x.Os).FirstOrDefaultAsync(x => x.Id == id);
        if (pedido is null)
            return;
        db.Pedidos.Remove(pedido);
        await db.SaveChangesAsync();
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
