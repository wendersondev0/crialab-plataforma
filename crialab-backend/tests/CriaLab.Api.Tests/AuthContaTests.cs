using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CriaLab.Api.Tests;

public class AuthContaTests
{
    private static WebApplicationFactory<Program> CriarFabrica() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
            builder.UseEnvironment(Environments.Development);
        });

    [Fact]
    public async Task Cadastro_entrada_eu_renovar_e_sair()
    {
        await using var fabrica = CriarFabrica();
        var cliente = fabrica.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"auth.{Guid.NewGuid():N}@teste.crialab.local";

        var cadastro = await cliente.PostAsJsonAsync("/api/conta/cadastrar", new
        {
            nomeCompleto = "Usuário Auth",
            email,
            telefone = "(27) 90000-0000",
            senha = "Senha@123!",
            confirmarSenha = "Senha@123!"
        });
        Assert.Equal(HttpStatusCode.OK, cadastro.StatusCode);
        var sessaoCadastro = await LerSessao(cadastro);
        Assert.Contains("Usuario", sessaoCadastro.GetProperty("usuario").GetProperty("papeis").EnumerateArray().Select(x => x.GetString()));
        Assert.False(string.IsNullOrWhiteSpace(sessaoCadastro.GetProperty("accessToken").GetString()));
        Assert.Contains(cadastro.Headers.GetValues("Set-Cookie"), c => c.StartsWith("crialab_refresh=", StringComparison.Ordinal));

        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", sessaoCadastro.GetProperty("accessToken").GetString());
        var eu = await cliente.GetAsync("/api/conta/eu");
        Assert.Equal(HttpStatusCode.OK, eu.StatusCode);
        var euJson = await eu.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(email, euJson.GetProperty("email").GetString());

        cliente.DefaultRequestHeaders.Authorization = null;
        var renovar = await cliente.PostAsync("/api/conta/renovar", null);
        Assert.Equal(HttpStatusCode.OK, renovar.StatusCode);
        var sessaoRenovada = await LerSessao(renovar);
        Assert.False(string.IsNullOrWhiteSpace(sessaoRenovada.GetProperty("accessToken").GetString()));

        var sair = await cliente.PostAsync("/api/conta/sair", null);
        Assert.Equal(HttpStatusCode.NoContent, sair.StatusCode);

        var renovarDepois = await cliente.PostAsync("/api/conta/renovar", null);
        Assert.Equal(HttpStatusCode.Unauthorized, renovarDepois.StatusCode);
    }

    [Fact]
    public async Task Entrar_com_senha_errada_devolve_mensagem_em_portugues()
    {
        await using var fabrica = CriarFabrica();
        var cliente = fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/conta/entrar", new
        {
            email = "cliente@crialab.local",
            senha = "SenhaErrada@1"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("E-mail ou senha inválidos.", corpo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Cadastro_com_senhas_diferentes_e_recusado()
    {
        await using var fabrica = CriarFabrica();
        var cliente = fabrica.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/conta/cadastrar", new
        {
            nomeCompleto = "Teste",
            email = $"diff.{Guid.NewGuid():N}@teste.crialab.local",
            senha = "Senha@123!",
            confirmarSenha = "Outra@123!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("As senhas não conferem.", corpo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Entrar_admin_seed_funciona()
    {
        await using var fabrica = CriarFabrica();
        var cliente = fabrica.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var resposta = await cliente.PostAsJsonAsync("/api/conta/entrar", new
        {
            email = "crialab.dm@gmail.com",
            senha = "@123Mudar"
        });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var sessao = await LerSessao(resposta);
        var papeis = sessao.GetProperty("usuario").GetProperty("papeis").EnumerateArray().Select(x => x.GetString()).ToArray();
        Assert.Contains("Administrador", papeis);
        Assert.Contains("Usuario", papeis);
    }

    private static async Task<JsonElement> LerSessao(HttpResponseMessage resposta)
    {
        await using var stream = await resposta.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<JsonElement>(stream);
    }
}
