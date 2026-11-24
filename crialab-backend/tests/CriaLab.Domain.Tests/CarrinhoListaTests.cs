using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class CarrinhoListaTests
{
    [Fact]
    public void Remover_ignora_detalhes_e_casa_produto_cor_e_tamanho()
    {
        Assert.True(CarrinhoLista.SaiNaRemocao("peca", "PLA branco", null, "peca", "PLA branco", null));
        Assert.False(CarrinhoLista.SaiNaRemocao("peca", "PLA branco", null, "peca", "PLA teal", null));
        Assert.False(CarrinhoLista.SaiNaRemocao("peca", null, null, "peca", "PLA branco", null));
        Assert.False(CarrinhoLista.SaiNaRemocao("peca", null, "Único", "peca", null, null));
    }

    [Fact]
    public void Subtotal_multiplica_preco_pela_quantidade()
    {
        var subtotal = CarrinhoLista.Subtotal(
        [
            new CarrinhoLista.Linha(42m, 2),
            new CarrinhoLista.Linha(10m, 1)
        ]);
        Assert.Equal(94m, subtotal);
    }

    [Fact]
    public void Carrinho_vazio_soma_zero()
    {
        Assert.Equal(0m, CarrinhoLista.Subtotal([]));
    }
}
