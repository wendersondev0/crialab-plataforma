using CriaLab.Domain.Calculos;
using CriaLab.Domain.Entidades;

namespace CriaLab.Domain.Tests;

public class FreteCalculoTests
{
    private static List<FaixaCep> FaixasSeed() =>
    [
        new() { De = 1000, Ate = 19999, Pac = 28.9m, PacDias = 8, Sedex = 46.5m, SedexDias = 3 },
        new() { De = 20000, Ate = 28999, Pac = 24.5m, PacDias = 6, Sedex = 39.9m, SedexDias = 2 },
        new() { De = 29000, Ate = 29999, Pac = 18.9m, PacDias = 5, Sedex = 32.4m, SedexDias = 2 },
        new()
        {
            De = 29260,
            Ate = 29279,
            Pac = 18.9m,
            PacDias = 5,
            Sedex = 32.4m,
            SedexDias = 2,
            EntregaLocal = true,
            EntregaLocalPreco = 5m,
            EntregaLocalDias = 1,
            EntregaLocalDescricao = "Entrega local"
        },
        new() { De = 30000, Ate = 39999, Pac = 22.4m, PacDias = 7, Sedex = 36.8m, SedexDias = 3 }
    ];

    [Fact]
    public void Cotar_cep_curto_retorna_erro()
    {
        var (erro, opcoes, _) = FreteCalculo.Cotar("29260", 50m, FaixasSeed());
        Assert.Equal("Informe um CEP válido com 8 dígitos.", erro);
        Assert.Empty(opcoes);
    }

    [Fact]
    public void Cotar_cep_sem_faixa_retorna_erro()
    {
        var (erro, opcoes, _) = FreteCalculo.Cotar("99999999", 50m, FaixasSeed());
        Assert.Equal("Não entregamos neste CEP ainda.", erro);
        Assert.Empty(opcoes);
    }

    [Fact]
    public void Cotar_retorna_pac_e_sedex()
    {
        var (erro, opcoes, _) = FreteCalculo.Cotar("29101000", 50m, FaixasSeed());
        Assert.Null(erro);
        var pac = Assert.Single(opcoes, o => o.Id == "pac");
        var sedex = Assert.Single(opcoes, o => o.Id == "sedex");
        Assert.Equal(18.9m, pac.Preco);
        Assert.Equal(5, pac.Dias);
        Assert.Equal(32.4m, sedex.Preco);
        Assert.Equal(2, sedex.Dias);
    }

    [Fact]
    public void Cotar_entrega_local_29260_a_5_reais()
    {
        var (erro, opcoes, _) = FreteCalculo.Cotar("29260000", 50m, FaixasSeed());
        Assert.Null(erro);
        var local = Assert.Single(opcoes, o => o.Id == "entrega-local");
        Assert.Equal(5m, local.Preco);
        Assert.Equal(1, local.Dias);
        Assert.Equal("Entrega Local", local.Nome);
    }

    [Fact]
    public void Cotar_frete_gratis_zera_pac_sedex_e_local()
    {
        var (erro, opcoes, freteGratis) = FreteCalculo.Cotar("29260000", 200m, FaixasSeed(), 200m);
        Assert.Null(erro);
        Assert.Equal(200m, freteGratis);
        Assert.All(opcoes, o => Assert.Equal(0m, o.Preco));
        Assert.Contains(opcoes, o => o.Id == "pac");
        Assert.Contains(opcoes, o => o.Id == "sedex");
        Assert.Contains(opcoes, o => o.Id == "entrega-local");
    }

    [Fact]
    public void Cupom_inativo_nao_aplica()
    {
        var cupons = new List<Cupom>
        {
            new() { Id = "cup-1", Codigo = "OFICINA10", Percentual = 0.1m, Ativo = false }
        };

        var cupom = FreteCalculo.CupomPorCodigo("oficina10", cupons);
        Assert.Null(cupom);
        Assert.Equal(0m, FreteCalculo.DescontoCupom(100m, cupom));
    }

    [Fact]
    public void Cupom_ativo_em_maiusculas_aplica_percentual()
    {
        var cupons = new List<Cupom>
        {
            new() { Id = "cup-1", Codigo = "OFICINA10", Percentual = 0.1m, Ativo = true }
        };

        var cupom = FreteCalculo.CupomPorCodigo("oficina10", cupons);
        Assert.NotNull(cupom);
        Assert.Equal(12m, FreteCalculo.DescontoCupom(120m, cupom));
    }
}
