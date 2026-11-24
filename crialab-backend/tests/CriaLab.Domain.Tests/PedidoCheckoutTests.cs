using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class PedidoCheckoutTests
{
    private static readonly DateTime CriadoEm = new(2026, 11, 24, 7, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Pedido_nasce_aguardando_pagamento_com_item_pronto_e_os_sem_baixa()
    {
        var peca = new CheckoutCalculo.Item("sessao", "Peça", 42m, 2, "PLA areia", "M", "Cor: PLA areia");
        var resultado = new CheckoutCalculo.Resultado(
            84m, 8.40m, 7.56m, 18.90m, "PAC", "pac", 200m, 86.94m, null, [], [peca], []);

        var pedido = PedidoCheckout.Montar(
            "ped-abc12345", "user-1", CriadoEm, ["item-1"], resultado, "29101-000", " oficina10 ", "pix", "  Rua 1  ");

        Assert.NotNull(pedido);
        Assert.Equal("ped-abc12345", pedido.Id);
        Assert.Equal("user-1", pedido.UsuarioId);
        Assert.Equal("aguardando pagamento", pedido.Status);
        Assert.Equal(CriadoEm, pedido.CriadoEm);
        Assert.Equal(84m, pedido.Subtotal);
        Assert.Equal(8.40m, pedido.DescontoCupom);
        Assert.Equal(7.56m, pedido.DescontoAVista);
        Assert.Equal(18.90m, pedido.FreteCobrado);
        Assert.Equal(86.94m, pedido.Total);
        Assert.Equal("29101-000", pedido.Cep);
        Assert.Equal("OFICINA10", pedido.Cupom);
        Assert.Equal("pix", pedido.Forma);
        Assert.Equal("Rua 1", pedido.Endereco);
        Assert.Equal("PAC", pedido.FreteNome);
        var item = Assert.Single(pedido.Itens);
        Assert.Equal("item-1", item.Id);
        Assert.Equal("sessao", item.ProdutoId);
        Assert.Equal("Peça", item.Titulo);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(42m, item.PrecoUnitario);
        Assert.True(item.Fisico);
        Assert.Equal("pronto", item.Tipo);
        Assert.Equal("PLA areia", item.CorEscolhida);
        Assert.Equal("M", item.TamanhoEscolhido);
        Assert.Equal("aguardando", pedido.Os.Status);
        Assert.False(pedido.Os.InsumoBaixado);
    }

    [Fact]
    public void Forma_em_branco_vira_pix_e_cupom_vazio_fica_vazio()
    {
        var peca = new CheckoutCalculo.Item("sessao", "Peça", 42m, 1, null, null, null);
        var resultado = new CheckoutCalculo.Resultado(
            42m, 0m, 0m, 32.40m, "SEDEX", "sedex", 200m, 74.40m, null, [], [peca], []);

        var pedido = PedidoCheckout.Montar(
            "ped-abc12345", "user-1", CriadoEm, ["item-1"], resultado, null, "   ", null, "Rua 1");

        Assert.NotNull(pedido);
        Assert.Equal("pix", pedido.Forma);
        Assert.Equal("", pedido.Cupom);
        Assert.Equal("", pedido.Cep);
        Assert.Equal("cartao", PedidoCheckout.Montar(
            "ped-abc12345", "user-1", CriadoEm, ["item-1"], resultado, "", "", "cartao", "Rua 1")!.Forma);
    }

    [Fact]
    public void Nao_grava_com_erro_carrinho_vazio_ou_ids_trocados()
    {
        var peca = new CheckoutCalculo.Item("sessao", "Peça", 42m, 1, null, null, null);
        var ok = new CheckoutCalculo.Resultado(42m, 0, 0, 0, "PAC", "pac", 200m, 42m, null, [], [peca], []);
        var comErro = ok with { Erros = ["Informe o endereço de entrega."] };
        var vazio = ok with { Itens = [] };

        Assert.Null(PedidoCheckout.Montar("ped-abc12345", "user-1", CriadoEm, ["item-1"], comErro, "", "", "pix", "Rua 1"));
        Assert.Null(PedidoCheckout.Montar("ped-abc12345", "user-1", CriadoEm, [], vazio, "", "", "pix", "Rua 1"));
        Assert.Null(PedidoCheckout.Montar("ped-abc12345", "user-1", CriadoEm, ["a", "b"], ok, "", "", "pix", "Rua 1"));
        Assert.Null(PedidoCheckout.Montar("  ", "user-1", CriadoEm, ["item-1"], ok, "", "", "pix", "Rua 1"));
        Assert.Null(PedidoCheckout.Montar("ped-abc12345", "  ", CriadoEm, ["item-1"], ok, "", "", "pix", "Rua 1"));
    }
}
