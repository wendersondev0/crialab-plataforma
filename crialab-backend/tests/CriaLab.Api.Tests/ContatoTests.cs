using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CriaLab.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaLab.Api.Tests;

public class ContatoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public ContatoTests(WebApplicationFactory<Program> fabrica)
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
    public async Task Visitante_ve_os_canais_e_nao_recebe_dados_da_conta()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.GetAsync("/api/loja/contato");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        var raiz = json.RootElement;
        Assert.Equal("crialab.dm@gmail.com", raiz.GetProperty("email").GetString());
        Assert.Equal("mailto:crialab.dm@gmail.com", raiz.GetProperty("emailHref").GetString());
        Assert.Equal("(27) 99653-0954", raiz.GetProperty("telefone").GetString());
        Assert.Equal("tel:+5527996530954", raiz.GetProperty("telefoneHref").GetString());
        Assert.Equal("https://wa.me/5527996530954", raiz.GetProperty("whatsappHref").GetString());
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("endereco").ValueKind);
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("horario").ValueKind);
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("nomeConta").ValueKind);
        Assert.Equal(JsonValueKind.Null, raiz.GetProperty("emailConta").ValueKind);
    }

    [Fact]
    public async Task Cliente_e_administrador_recebem_o_proprio_nome_para_preencher()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        using var doCliente = await GetAsync(cliente);
        using var doAdmin = await GetAsync(admin);
        Assert.Equal("Cliente exemplo", doCliente.RootElement.GetProperty("nomeConta").GetString());
        Assert.Equal("cliente@crialab.local", doCliente.RootElement.GetProperty("emailConta").GetString());
        Assert.Equal("Administrador Cria Lab", doAdmin.RootElement.GetProperty("nomeConta").GetString());
        Assert.Equal("crialab.dm@gmail.com", doAdmin.RootElement.GetProperty("emailConta").GetString());
    }

    [Fact]
    public async Task Formulario_invalido_nao_grava()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/loja/contato", new
        {
            nome = " A ",
            email = "sem-arroba",
            tipo = "nenhum",
            texto = "curta"
        });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        var erros = json.RootElement.GetProperty("erros").EnumerateArray().Select(item => item.GetString() ?? "").ToArray();
        Assert.Equal(
            [
                "Diga como podemos te chamar.",
                "Informe um e-mail válido.",
                "Escolha o tipo da mensagem.",
                "Escreva a mensagem com um pouco mais de detalhe."
            ],
            erros);
        Assert.Empty(await IdsPorTextoAsync("curta"));
    }

    [Fact]
    public async Task Visitante_cliente_e_administrador_gravam_mensagem_aberta()
    {
        using var cliente = await EntrarAsync("cliente@crialab.local", "Cliente@123!");
        using var admin = await EntrarAsync("crialab.dm@gmail.com", "@123Mudar");
        var anonimo = _fabrica.CreateClient();
        var ids = new List<string>();
        try
        {
            ids.Add(await GravarAsync(anonimo, "Visitante da tela", "visita@oficina.test", "orcamento", "  Quero duas peças\r\nna cor areia.  "));
            ids.Add(await GravarAsync(cliente, "Cliente exemplo", "cliente@crialab.local", "duvida", "Qual o prazo dessa peça personalizada?"));
            ids.Add(await GravarAsync(admin, "Administrador Cria Lab", "crialab.dm@gmail.com", "elogio", "A oficina recebeu o recado de teste."));

            using var escopo = _fabrica.Services.CreateScope();
            var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var gravadas = await db.MensagensContato.Where(x => ids.Contains(x.Id)).ToListAsync();
            Assert.Equal(3, gravadas.Count);
            Assert.All(gravadas, mensagem => Assert.Equal("aberto", mensagem.Status));
            var orcamento = gravadas.Single(x => x.Id == ids[0]);
            Assert.Equal("Orçamento", orcamento.Tipo);
            Assert.Equal("Quero duas peças\nna cor areia.", orcamento.Texto);
            Assert.Equal("Visitante da tela", orcamento.Nome);
            Assert.Equal("Dúvida", gravadas.Single(x => x.Id == ids[1]).Tipo);
            Assert.Equal("Elogio", gravadas.Single(x => x.Id == ids[2]).Tipo);
        }
        finally
        {
            await ApagarAsync(ids);
        }
    }

    private static async Task<string> GravarAsync(HttpClient cliente, string nome, string email, string tipo, string texto)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/loja/contato", new { nome, email, tipo, texto });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        var id = json.RootElement.GetProperty("id").GetString();
        Assert.Matches("^msg-[0-9a-f]{32}$", id);
        Assert.Equal("aberto", json.RootElement.GetProperty("status").GetString());
        return id!;
    }

    private static async Task<JsonDocument> GetAsync(HttpClient cliente)
    {
        var resposta = await cliente.GetAsync("/api/loja/contato");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
    }

    private async Task<List<string>> IdsPorTextoAsync(string texto)
    {
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        return await db.MensagensContato.Where(x => x.Texto == texto).Select(x => x.Id).ToListAsync();
    }

    private async Task ApagarAsync(IReadOnlyList<string> ids)
    {
        if (ids.Count == 0)
            return;
        using var escopo = _fabrica.Services.CreateScope();
        var db = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var linhas = await db.MensagensContato.Where(x => ids.Contains(x.Id)).ToListAsync();
        db.MensagensContato.RemoveRange(linhas);
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
