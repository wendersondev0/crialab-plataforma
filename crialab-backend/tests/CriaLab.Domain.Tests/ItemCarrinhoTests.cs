using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class ItemCarrinhoTests
{
    [Fact]
    public void Pronta_entrega_limita_a_quantidade_ao_estoque()
    {
        var item = ItemCarrinho.Preparar("pronta-entrega", 2, 9, "  PLA branco ", "  ", "", "CHECKOUT");
        Assert.Equal(2, item.Quantidade);
        Assert.Equal("PLA branco", item.Cor);
        Assert.Null(item.Tamanho);
        Assert.Null(item.Detalhes);
        Assert.Equal("checkout", item.Destino);
    }

    [Fact]
    public void Quantidade_menor_que_um_vira_um_e_outro_destino_e_toast()
    {
        var item = ItemCarrinho.Preparar("sob-demanda", 0, 0, null, null, "Cor: Azul", "loja");
        Assert.Equal(1, item.Quantidade);
        Assert.Equal("Cor: Azul", item.Detalhes);
        Assert.Equal("toast", item.Destino);
    }

    [Fact]
    public void Personalizado_nao_corta_quantidade_pelo_estoque_zero()
    {
        var item = ItemCarrinho.Preparar("personalizado", 0, 4, null, "Médio", null, null);
        Assert.Equal(4, item.Quantidade);
        Assert.Equal("Médio", item.Tamanho);
    }
}
