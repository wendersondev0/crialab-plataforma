using CriaLab.Infrastructure;
using CriaLab.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CriaLab.Api.Tests;

public class SeedRestauracaoTests
{
    [Fact]
    public async Task Restaurar_fora_de_development_e_recusado()
    {
        await using var fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
            builder.UseEnvironment(Environments.Production);
        });

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => DbSeeder.RestaurarDemoAsync(fabrica.Services));
        Assert.Contains("Development", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Restaurar_em_banco_que_nao_e_crialab_api_e_recusado()
    {
        await using var fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab;Username=postgres;Password=@123Mudar");
            builder.UseEnvironment(Environments.Development);
        });

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => DbSeeder.RestaurarDemoAsync(fabrica.Services));
        Assert.Contains("crialab_api", erro.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Seed_preenche_crialab_api()
    {
        await using var fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
            builder.UseEnvironment(Environments.Development);
        });

        await DbSeeder.RestaurarDemoAsync(fabrica.Services);

        await using var escopo = fabrica.Services.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.True(await contexto.Produtos.AnyAsync());
        Assert.True(await contexto.Pedidos.AnyAsync());
        Assert.True(await contexto.LancamentosFinanceiros.AnyAsync());
        Assert.True(await contexto.Maquinas.AnyAsync());
        Assert.True(await contexto.Users.AnyAsync());
    }
}
