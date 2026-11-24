using CriaLab.Domain.Calculos;
using CriaLab.Domain.Entidades;

namespace CriaLab.Domain.Tests;

public class CheckoutCalculoTests
{
    private static readonly CheckoutCalculo.Item Peca = new("sessao", "Peça", 42m, 1, "PLA areia", null, "Cor: PLA areia");

    [Fact]
    public void Pix_aplica_cupom_desconto_a_vista_e_pac()
    {
        var resultado = CheckoutCalculo.Calcular(
            [Peca], "29101000", "oficina10", "pac", "Rua 1", "pix", "cotar", Cupons(), Faixas(), 200m, 0.1m);

        Assert.Equal(42m, resultado.Subtotal);
        Assert.Equal(4.20m, resultado.DescontoCupom);
        Assert.Equal(3.78m, resultado.DescontoAVista);
        Assert.Equal(18.90m, resultado.FreteCobrado);
        Assert.Equal("PAC", resultado.FreteNome);
        Assert.Equal(52.92m, resultado.Total);
        Assert.Empty(resultado.Erros);
    }

    [Fact]
    public void Cartao_nao_tem_desconto_a_vista_e_boleto_tem()
    {
        var cartao = CheckoutCalculo.Calcular(
            [Peca], "29101000", "oficina10", "pac", "Rua 1", "cartao", "cotar", Cupons(), Faixas(), 200m, 0.1m);
        var boleto = CheckoutCalculo.Calcular(
            [Peca], "29101000", "oficina10", "pac", "Rua 1", "boleto", "cotar", Cupons(), Faixas(), 200m, 0.1m);

        Assert.Equal(0m, cartao.DescontoAVista);
        Assert.Equal(56.70m, cartao.Total);
        Assert.Equal(3.78m, boleto.DescontoAVista);
        Assert.Equal(52.92m, boleto.Total);
    }

    [Fact]
    public void Finalizar_exige_endereco_e_frete_cotado()
    {
        var semEndereco = CheckoutCalculo.Calcular(
            [Peca], "29101000", "", "pac", "  ", "pix", "finalizar", Cupons(), Faixas(), 200m, 0.1m);
        var semFrete = CheckoutCalculo.Calcular(
            [Peca], "", "", "pac", "Rua 1", "pix", "finalizar", Cupons(), Faixas(), 200m, 0.1m);
        var cotar = CheckoutCalculo.Calcular(
            [Peca], "", "", "pac", "", "pix", "cotar", Cupons(), Faixas(), 200m, 0.1m);

        Assert.Equal(["Informe o endereço de entrega."], semEndereco.Erros);
        Assert.Equal(["Informe um CEP válido com 8 dígitos."], semFrete.Erros);
        Assert.Empty(cotar.Erros);
        Assert.Equal("Informe um CEP válido com 8 dígitos.", cotar.ErroFrete);
    }

    [Fact]
    public void Frete_gratis_zera_o_valor_cobrado()
    {
        var item = Peca with { Quantidade = 5 };
        var resultado = CheckoutCalculo.Calcular(
            [item], "29101000", "OFICINA10", "sedex", "Rua 1", "pix", "finalizar", Cupons(), Faixas(), 200m, 0.1m);

        Assert.Equal(210m, resultado.Subtotal);
        Assert.Equal(0m, resultado.FreteCobrado);
        Assert.Equal("SEDEX", resultado.FreteNome);
        Assert.All(resultado.Opcoes, opcao => Assert.Equal(0m, opcao.Preco));
        Assert.Empty(resultado.Erros);
    }

    [Fact]
    public void Carrinho_vazio_nao_exige_endereco()
    {
        var resultado = CheckoutCalculo.Calcular(
            [], "", "", "pac", "", "pix", "finalizar", Cupons(), Faixas(), 200m, 0.1m);

        Assert.Equal(0m, resultado.Total);
        Assert.Empty(resultado.Erros);
    }

    private static List<Cupom> Cupons() =>
    [
        new() { Id = "cup-oficina10", Codigo = "OFICINA10", Percentual = 0.1m, Ativo = true }
    ];

    private static List<FaixaCep> Faixas() =>
    [
        new() { De = 29000, Ate = 29999, Pac = 18.9m, PacDias = 5, Sedex = 32.4m, SedexDias = 2 }
    ];
}
