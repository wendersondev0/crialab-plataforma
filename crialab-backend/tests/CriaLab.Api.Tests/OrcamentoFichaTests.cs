using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriaLab.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaLab.Api.Tests;

public class OrcamentoFichaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public OrcamentoFichaTests(WebApplicationFactory<Program> fabrica)
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
        var resposta = await cliente.PostAsJsonAsync("/api/loja/produtos/pers-chaveiro-cachorro/orcamentos", new { detalhes = "Medida: 8 cm" });
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Cliente_grava_aguardando_com_detalhe_e_arquivo_e_o_vazio_vira_sem_detalhes()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        var ids = new List<string>();
        try
        {
            var criado = await PedirAsync(cliente, "pers-chaveiro-cachorro", new
            {
                detalhes = "  Cor: Branco · Medida: 8 cm · Arquivo de referência: referencia.png  ",
                arquivoNome = "  referencia.png "
            });
            ids.Add(criado.GetProperty("id").GetString()!);
            Assert.Equal("aguardando", criado.GetProperty("status").GetString());
            Assert.Equal("Chaveiro do seu Cachorro", criado.GetProperty("titulo").GetString());
            Assert.Equal("Cor: Branco · Medida: 8 cm · Arquivo de referência: referencia.png", criado.GetProperty("detalhes").GetString());
            Assert.Equal("referencia.png", criado.GetProperty("arquivoNome").GetString());

            var vazio = await PedirAsync(cliente, "pers-chaveiro-cachorro", new { detalhes = "   ", arquivoNome = " " });
            ids.Add(vazio.GetProperty("id").GetString()!);
            Assert.Equal("Sem detalhes", vazio.GetProperty("detalhes").GetString());
            Assert.Equal("", vazio.GetProperty("arquivoNome").GetString());
        }
        finally
        {
            await ApagarAsync(ids);
        }
    }

    [Fact]
    public async Task Admin_pede_orcamento_e_produto_inexistente_responde_404()
    {
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        string? id = null;
        try
        {
            var criado = await PedirAsync(admin, "pers-chaveiro-cachorro", new { detalhes = "Prazo desejado: 10 dias" });
            id = criado.GetProperty("id").GetString();
            Assert.Equal("aguardando", criado.GetProperty("status").GetString());
            Assert.Equal("Prazo desejado: 10 dias", criado.GetProperty("detalhes").GetString());
            Assert.Equal("", criado.GetProperty("arquivoNome").GetString());
        }
        finally
        {
            if (id is not null)
                await ApagarAsync([id]);
        }

        var ausente = await admin.PostAsJsonAsync("/api/loja/produtos/nao-existe/orcamentos", new { detalhes = "x" });
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
    }

    private async Task<JsonElement> PedirAsync(HttpClient cliente, string produtoId, object corpo)
    {
        var resposta = await cliente.PostAsJsonAsync($"/api/loja/produtos/{produtoId}/orcamentos", corpo);
        resposta.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await resposta.Content.ReadAsStreamAsync());
        return json.RootElement.Clone();
    }

    private async Task ApagarAsync(IReadOnlyList<string> ids)
    {
        if (ids.Count == 0) return;
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var linhas = await db.OrcamentosCliente.Where(x => ids.Contains(x.Id)).ToListAsync();
        db.OrcamentosCliente.RemoveRange(linhas);
        await db.SaveChangesAsync();
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
