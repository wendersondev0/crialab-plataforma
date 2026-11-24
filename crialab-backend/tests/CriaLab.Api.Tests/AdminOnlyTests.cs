using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CriaLab.Api.Tests;

public class AdminOnlyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public AdminOnlyTests(WebApplicationFactory<Program> fabrica)
    {
        _fabrica = fabrica.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
            builder.UseEnvironment(Environments.Development);
        });
    }

    [Fact]
    public async Task Acesso_admin_exige_papel_administrador()
    {
        var cliente = _fabrica.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var anonimo = await cliente.GetAsync("/api/admin/acesso");
        Assert.Equal(HttpStatusCode.Unauthorized, anonimo.StatusCode);

        var email = $"usuario.{Guid.NewGuid():N}@teste.crialab.local";
        var senha = "Senha@123!";
        var cadastro = await cliente.PostAsJsonAsync("/api/conta/cadastrar", new
        {
            nomeCompleto = "Cliente",
            email,
            senha,
            confirmarSenha = senha
        });
        Assert.Equal(HttpStatusCode.OK, cadastro.StatusCode);
        var sessaoUsuario = await cadastro.Content.ReadFromJsonAsync<JsonElement>();
        var tokenUsuario = sessaoUsuario.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(tokenUsuario));

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsuario);
        var soUsuario = await cliente.GetAsync("/api/admin/acesso");
        Assert.Equal(HttpStatusCode.Forbidden, soUsuario.StatusCode);

        cliente.DefaultRequestHeaders.Authorization = null;
        var loginAdmin = await cliente.PostAsJsonAsync("/api/conta/entrar", new
        {
            email = "crialab.dm@gmail.com",
            senha = "@123Mudar"
        });
        Assert.Equal(HttpStatusCode.OK, loginAdmin.StatusCode);
        var sessaoAdmin = await loginAdmin.Content.ReadFromJsonAsync<JsonElement>();
        var tokenAdmin = sessaoAdmin.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(tokenAdmin));

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);
        var admin = await cliente.GetAsync("/api/admin/acesso");
        Assert.Equal(HttpStatusCode.OK, admin.StatusCode);
        var corpo = await admin.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.GetProperty("autorizado").GetBoolean());
    }
}
