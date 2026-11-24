using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriaLab.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaLab.Api.Tests;

public class PerguntaFichaTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public PerguntaFichaTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Anonimo_grava_visitante_com_texto_aparado_e_branco_nao_grava()
    {
        var cliente = _fabrica.CreateClient();
        var antes = await ContarAsync("pers-chaveiro-cachorro");
        string? id = null;
        try
        {
            var criada = await EnviarAsync(cliente, "pers-chaveiro-cachorro", "  Envia para o exterior?  ");
            id = criada.GetProperty("id").GetString();
            Assert.Equal("Visitante", criada.GetProperty("autor").GetString());
            Assert.Equal("Envia para o exterior?", criada.GetProperty("texto").GetString());
            Assert.Equal("", criada.GetProperty("resposta").GetString());
            Assert.False(criada.GetProperty("respondida").GetBoolean());

            var ficha = await cliente.GetAsync("/api/loja/produtos/pers-chaveiro-cachorro");
            ficha.EnsureSuccessStatusCode();
            using var json = await JsonDocument.ParseAsync(await ficha.Content.ReadAsStreamAsync());
            Assert.Equal(id, json.RootElement.GetProperty("perguntas")[0].GetProperty("id").GetString());
        }
        finally
        {
            if (id is not null)
                await ApagarAsync([id]);
        }

        var vazio = await cliente.PostAsJsonAsync("/api/loja/produtos/pers-chaveiro-cachorro/perguntas", new { texto = "   " });
        Assert.Equal(HttpStatusCode.BadRequest, vazio.StatusCode);
        Assert.Equal(antes, await ContarAsync("pers-chaveiro-cachorro"));
    }

    [Fact]
    public async Task Cliente_grava_o_email_da_conta()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        string? id = null;
        try
        {
            var criada = await EnviarAsync(cliente, "sessao-2026-03-14", "Cabe na mochila?");
            id = criada.GetProperty("id").GetString();
            Assert.Equal("cliente@crialab.local", criada.GetProperty("autor").GetString());
            Assert.Equal("Cabe na mochila?", criada.GetProperty("texto").GetString());
            Assert.False(criada.GetProperty("respondida").GetBoolean());
        }
        finally
        {
            if (id is not null)
                await ApagarAsync([id]);
        }
    }

    [Fact]
    public async Task Admin_grava_o_email_e_produto_inexistente_responde_404()
    {
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        string? id = null;
        try
        {
            var criada = await EnviarAsync(admin, "sessao-2026-04-14", "Qual o filamento?");
            id = criada.GetProperty("id").GetString();
            Assert.Equal("crialab.dm@gmail.com", criada.GetProperty("autor").GetString());
        }
        finally
        {
            if (id is not null)
                await ApagarAsync([id]);
        }

        var ausente = await admin.PostAsJsonAsync("/api/loja/produtos/nao-existe/perguntas", new { texto = "Tem?" });
        Assert.Equal(HttpStatusCode.NotFound, ausente.StatusCode);
    }

    private async Task<JsonElement> EnviarAsync(HttpClient cliente, string produtoId, string texto)
    {
        var resposta = await cliente.PostAsJsonAsync($"/api/loja/produtos/{produtoId}/perguntas", new { texto });
        resposta.EnsureSuccessStatusCode();
        using var json = await JsonDocument.ParseAsync(await resposta.Content.ReadAsStreamAsync());
        return json.RootElement.Clone();
    }

    private async Task<int> ContarAsync(string produtoId)
    {
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        return await db.PerguntasProduto.CountAsync(x => x.ProdutoId == produtoId);
    }

    private async Task ApagarAsync(IReadOnlyList<string> ids)
    {
        if (ids.Count == 0) return;
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var linhas = await db.PerguntasProduto.Where(x => ids.Contains(x.Id)).ToListAsync();
        db.PerguntasProduto.RemoveRange(linhas);
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
