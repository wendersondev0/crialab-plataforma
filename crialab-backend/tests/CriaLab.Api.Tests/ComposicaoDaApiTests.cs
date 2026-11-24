using System.Net;
using Carter;
using CriaLab.Application;
using CriaLab.Domain.Identidade;
using CriaLab.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CriaLab.Api.Tests;

public class ComposicaoDaApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public ComposicaoDaApiTests(WebApplicationFactory<Program> fabrica)
    {
        _fabrica = fabrica.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
        });
    }

    [Fact]
    public async Task Excecao_vira_problem_details_em_portugues()
    {
        var fabrica = _fabrica.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ExporFalhaDeTeste", "true");
        });
        var cliente = fabrica.CreateClient();
        var resposta = await cliente.GetAsync("/api/teste/erro");
        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Erro interno", corpo, StringComparison.Ordinal);
        Assert.Contains("Ocorreu um erro inesperado.", corpo, StringComparison.Ordinal);
        Assert.DoesNotContain("falha-interna-de-teste", corpo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Saude_responde_disponivel()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.GetAsync("/api/saude");
        resposta.EnsureSuccessStatusCode();
        Assert.Equal("application/json", resposta.Content.Headers.ContentType?.MediaType);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("disponivel", corpo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OpenApi_responde_documento()
    {
        var cliente = _fabrica.CreateClient();
        var resposta = await cliente.GetAsync("/openapi/v1.json");
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("openapi", corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Servicos_registram_a_base_da_api()
    {
        await using var escopo = _fabrica.Services.CreateAsyncScope();
        var provedor = escopo.ServiceProvider;

        var identidade = provedor.GetRequiredService<IOptions<IdentityOptions>>().Value;
        Assert.Equal(8, identidade.Password.RequiredLength);
        Assert.True(identidade.Password.RequireDigit);
        Assert.True(identidade.Password.RequireLowercase);
        Assert.True(identidade.Password.RequireUppercase);
        Assert.True(identidade.Password.RequireNonAlphanumeric);
        Assert.False(identidade.SignIn.RequireConfirmedAccount);

        var politica = await provedor.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(Politicas.AdminOnly);
        Assert.NotNull(politica);
        Assert.NotNull(provedor.GetService<IProblemDetailsService>());
        Assert.NotNull(provedor.GetService<ILeitorSql>());
        Assert.NotEmpty(provedor.GetServices<ICarterModule>());

        await using var contexto = provedor.GetRequiredService<ContextoAplicacao>();
        var usuario = contexto.Model.FindEntityType(typeof(UsuarioAplicacao));
        Assert.NotNull(usuario);
        Assert.Equal("asp_net_users", usuario.GetTableName());
        var coluna = usuario.FindProperty(nameof(UsuarioAplicacao.NomeCompleto));
        Assert.NotNull(coluna);
        Assert.Equal("nome_completo", coluna.FindAnnotation("Relational:ColumnName")?.Value);
    }
}
