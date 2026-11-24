using System.Text.RegularExpressions;
using CriaLab.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CriaLab.Api.Tests;

public class ModeloIgualAoSnapshotTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public ModeloIgualAoSnapshotTests(WebApplicationFactory<Program> fabrica)
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
    public void Colunas_coincidem_com_o_snapshot_do_razor()
    {
        var snapshot = LerSnapshot();
        using var escopo = _fabrica.Services.CreateScope();
        using var contexto = escopo.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var modelo = LerModelo(contexto);
        Assert.True(snapshot.Count > 100, $"Snapshot leu {snapshot.Count} colunas.");
        Assert.True(snapshot.ContainsKey("asp_net_users.id"), "Snapshot sem asp_net_users.id.");
        Assert.True(snapshot.ContainsKey("produtos.preco"), "Snapshot sem produtos.preco.");

        var faltando = snapshot.Keys.Except(modelo.Keys).Order().ToArray();
        var sobrando = modelo.Keys.Except(snapshot.Keys).Order().ToArray();
        var tipos = snapshot.Keys
            .Intersect(modelo.Keys)
            .Where(chave => snapshot[chave] != modelo[chave])
            .Select(chave => $"{chave}: snapshot={snapshot[chave]} modelo={modelo[chave]}")
            .Order()
            .ToArray();

        Assert.True(faltando.Length == 0 && sobrando.Length == 0 && tipos.Length == 0,
            "Diferença de coluna."
            + Environment.NewLine + "Faltando: " + string.Join(", ", faltando)
            + Environment.NewLine + "Sobrando: " + string.Join(", ", sobrando)
            + Environment.NewLine + "Tipo: " + string.Join(", ", tipos));
    }

    private static Dictionary<string, string> LerModelo(ContextoAplicacao contexto)
    {
        var colunas = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entidade in contexto.Model.GetEntityTypes())
        {
            var tabela = entidade.GetTableName();
            if (tabela is null)
            {
                continue;
            }

            foreach (var propriedade in entidade.GetProperties())
            {
                var coluna = propriedade.FindAnnotation("Relational:ColumnName")?.Value as string;
                if (coluna is null)
                {
                    continue;
                }

                colunas[$"{tabela}.{coluna}"] = propriedade.GetColumnType();
            }
        }

        return colunas;
    }

    private static Dictionary<string, string> LerSnapshot()
    {
        var caminho = AcharSnapshot();
        var texto = File.ReadAllText(caminho);
        var colunas = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match bloco in Regex.Matches(texto, @"modelBuilder\.Entity\([\s\S]*?\.ToTable\(""([^""]+)"""))
        {
            var tabela = bloco.Groups[1].Value;
            foreach (Match propriedade in Regex.Matches(bloco.Value, @"b\.Property<[^;]+;"))
            {
                var nome = Regex.Match(propriedade.Value, @"HasColumnName\(""([^""]+)""\)");
                var tipo = Regex.Match(propriedade.Value, @"HasColumnType\(""([^""]+)""\)");
                if (!nome.Success || !tipo.Success)
                {
                    continue;
                }

                colunas[$"{tabela}.{nome.Groups[1].Value}"] = tipo.Groups[1].Value;
            }
        }

        return colunas;
    }

    private static string AcharSnapshot()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (diretorio is not null)
        {
            var candidato = Path.Combine(
                diretorio.FullName,
                "crialab-plataforma",
                "src",
                "CriaLab.Web",
                "Data",
                "Migrations",
                "ApplicationDbContextModelSnapshot.cs");
            if (File.Exists(candidato))
            {
                return candidato;
            }

            diretorio = diretorio.Parent;
        }

        throw new InvalidOperationException("Snapshot do Razor não encontrado.");
    }
}
