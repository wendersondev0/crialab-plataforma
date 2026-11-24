using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class PedidoOrcamentoTests
{
    [Fact]
    public void Detalhe_vazio_vira_sem_detalhes_e_arquivo_branco_some()
    {
        var pedido = PedidoOrcamento.Preparar("   ", "  ");
        Assert.Equal("Sem detalhes", pedido.Detalhes);
        Assert.Equal("", pedido.ArquivoNome);
    }

    [Fact]
    public void Detalhe_e_arquivo_guardam_o_texto_aparado()
    {
        var pedido = PedidoOrcamento.Preparar("  Cor: Branco · Medida: 8 cm  ", "  referencia.png ");
        Assert.Equal("Cor: Branco · Medida: 8 cm", pedido.Detalhes);
        Assert.Equal("referencia.png", pedido.ArquivoNome);
    }
}
