using CriaLab.Infrastructure;
using CriaLab.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace CriaLab.Api.Tests;

public class ContagensSeedTests
{
    [Fact]
    public async Task Contagens_coincidem_com_razor_recem_semeado()
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
        var api = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var contagensApi = new Contagens(
            await api.Produtos.CountAsync(),
            await api.Pedidos.CountAsync(),
            await api.LancamentosFinanceiros.CountAsync(),
            await api.Maquinas.CountAsync(),
            await api.Users.CountAsync());

        var contagensRazor = await LerContagensAsync(
            "Host=localhost;Port=5432;Database=crialab;Username=postgres;Password=@123Mudar");

        Assert.Equal(contagensRazor, contagensApi);
        Assert.Equal(new Contagens(8, 5, 229, 13, 7), contagensApi);
    }

    private static async Task<Contagens> LerContagensAsync(string conexao)
    {
        await using var npgsql = new NpgsqlConnection(conexao);
        await npgsql.OpenAsync();
        return new Contagens(
            await ContarAsync(npgsql, "produtos"),
            await ContarAsync(npgsql, "pedidos"),
            await ContarAsync(npgsql, "lancamentos_financeiros"),
            await ContarAsync(npgsql, "maquinas"),
            await ContarAsync(npgsql, "asp_net_users"));
    }

    private static async Task<int> ContarAsync(NpgsqlConnection conexao, string tabela)
    {
        await using var comando = new NpgsqlCommand($"SELECT COUNT(*)::int FROM {tabela}", conexao);
        return (int)(await comando.ExecuteScalarAsync())!;
    }

    private sealed record Contagens(int Produtos, int Pedidos, int Lancamentos, int Maquinas, int Usuarios);
}
