using CriaLab.Web.Data.Entities;
using CriaLab.Web.Services;

namespace CriaLab.Web.Tests;

public class OrcamentoCalculoHelperTests
{
    [Fact]
    public void Orcar_usa_formula_custo_mais_margem_mais_falha()
    {
        var r = OrcamentoCalculoHelper.Orcar(
            precoKg: 100m,
            pesoG: 100m,
            lixoG: 0m,
            energiaPorMinuto: 0.01m,
            depreciacao: 0,
            manutencao: 0,
            minutos: 60,
            servico: 10m,
            margem: 0.5m,
            taxaFalha: 0.1m,
            desconto: 0,
            itens: null);

        Assert.Equal(10m, r.Filamento);
        Assert.Equal(0.6m, r.Tempo);
        Assert.Equal(20.6m, r.Custo);
        Assert.Equal(10.3m, r.Lucro);
        Assert.Equal(2.06m, r.Reserva);
        Assert.Equal(33m, r.Direto);
        Assert.Equal(1, r.Horas);
    }

    [Fact]
    public void PrecoCanal_escolhe_faixa_valida_shopee()
    {
        var faixas = new List<FaixaMarketplace>
        {
            new() { Canal = "Shopee", De = 0, Ate = 79.99m, Percentual = 0.2m, Fixo = 4.5m },
            new() { Canal = "Shopee", De = 80, Ate = 99.99m, Percentual = 0.14m, Fixo = 16 },
            new() { Canal = "Shopee", De = 100, Ate = 199.99m, Percentual = 0.14m, Fixo = 20 }
        };

        var r = OrcamentoCalculoHelper.PrecoCanal(50m, faixas, freteVendedor: 0);
        Assert.False(r.FaixasVazias);
        Assert.NotNull(r.Preco);
        Assert.InRange(r.Preco!.Value, 0m, 79.99m);
        Assert.NotNull(r.Liquido);
        Assert.True(r.Liquido < r.Preco);
    }

    [Fact]
    public void PrecoCanal_sem_faixas_marca_vazio()
    {
        var r = OrcamentoCalculoHelper.PrecoCanal(100m, new List<FaixaMarketplace>(), 0);
        Assert.True(r.FaixasVazias);
        Assert.Null(r.Preco);
    }
}

