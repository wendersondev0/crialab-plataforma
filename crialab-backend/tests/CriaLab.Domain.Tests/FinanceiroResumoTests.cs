using System.Globalization;
using CriaLab.Domain.Calculos;
using CriaLab.Domain.Entidades;

namespace CriaLab.Domain.Tests;

public class FinanceiroResumoTests
{
    [Fact]
    public void Categorias_contem_os_seis_ids_do_caderno()
    {
        Assert.Equal(
            ["gastoFixo", "energia", "filamento", "outroInsumo", "receita", "investimento"],
            FinanceiroResumo.Categorias);
    }

    [Fact]
    public void Resumir_geral_soma_todas_as_categorias_e_ignora_estornado()
    {
        var lancamentos = new[]
        {
            Lan("2026-01", "receita", 100m, 2m),
            Lan("2026-02", "gastoFixo", 20m),
            Lan("2026-02", "energia", 15m, kwh: 50m),
            Lan("2026-03", "filamento", 30m, 1m),
            Lan("2025-12", "outroInsumo", 5m),
            Lan("2026-01", "investimento", 40m),
            Lan("2026-01", "receita", 999m, estornado: true)
        };

        var r = FinanceiroResumo.Resumir(lancamentos, "geral");

        Assert.Equal("geral", r.Competencia);
        Assert.Equal(100m, r.Receita);
        Assert.Equal(20m, r.GastosFixos);
        Assert.Equal(30m, r.Filamentos);
        Assert.Equal(5m, r.Outros);
        Assert.Equal(15m, r.Energia);
        Assert.Equal(40m, r.Investimento);
        Assert.Equal(50m, r.DespesaOperacional);
        Assert.Equal(50m, r.ResultadoOperacional);
        Assert.Equal(-10m, r.ResultadoCaixa);
        Assert.Equal(30m, r.Lucro);
        Assert.Equal(2m, r.Quantidade);
        Assert.Equal(50m, r.Kwh);
        Assert.Equal("Geral", FinanceiroResumo.CompetenciaBr("geral"));
    }

    [Fact]
    public void Resumir_por_ano_filtra_prefixo_da_competencia()
    {
        var lancamentos = new[]
        {
            Lan("2026-01", "receita", 80m),
            Lan("2026-11", "filamento", 10m),
            Lan("2025-12", "receita", 200m),
            Lan("2025-01", "gastoFixo", 50m)
        };

        var r = FinanceiroResumo.Resumir(lancamentos, "2026");

        Assert.Equal("2026", r.Competencia);
        Assert.Equal(80m, r.Receita);
        Assert.Equal(10m, r.Filamentos);
        Assert.Equal(0m, r.GastosFixos);
        Assert.Equal("Ano 2026", FinanceiroResumo.CompetenciaBr("2026"));
    }

    [Fact]
    public void CompetenciaBr_mes_usa_formato_MMM_yyyy_pt_BR()
    {
        var esperado = DateOnly.Parse("2026-03-01").ToString("MMM/yyyy", new CultureInfo("pt-BR"));
        Assert.Equal(esperado, FinanceiroResumo.CompetenciaBr("2026-03"));
        Assert.Equal(esperado, FinanceiroResumo.CompetenciaBr("2026-03-15"));
    }

    [Fact]
    public void Resumir_por_mes_filtra_competencia_exata()
    {
        var lancamentos = new[]
        {
            Lan("2026-03", "receita", 70m, 3m),
            Lan("2026-03", "energia", 12m, kwh: 40m),
            Lan("2026-04", "receita", 90m)
        };

        var r = FinanceiroResumo.Resumir(lancamentos, "2026-03");

        Assert.Equal("2026-03", r.Competencia);
        Assert.Equal(70m, r.Receita);
        Assert.Equal(12m, r.Energia);
        Assert.Equal(3m, r.Quantidade);
        Assert.Equal(40m, r.Kwh);
    }

    private static LancamentoFinanceiro Lan(
        string competencia,
        string categoria,
        decimal valor,
        decimal quantidade = 0m,
        decimal? kwh = null,
        bool estornado = false) =>
        new()
        {
            Id = Guid.NewGuid().ToString("N"),
            Competencia = competencia,
            Categoria = categoria,
            Valor = valor,
            Quantidade = quantidade,
            Kwh = kwh,
            Estornado = estornado
        };
}
