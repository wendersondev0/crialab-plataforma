using CriaLab.Domain.Status;

namespace CriaLab.Domain.Tests;

public class StatusTests
{
    [Fact]
    public void Pedido_usa_os_textos_da_tela()
    {
        Assert.Equal(
            ["novo", "aguardando pagamento", "pago", "em produção", "enviado", "entregue", "cancelado", "devolvido"],
            StatusPedido.Todos);
    }

    [Fact]
    public void Os_escreve_producao_sem_acento()
    {
        Assert.Equal(["aguardando", "em producao", "concluida"], StatusOs.Todos);
        Assert.NotEqual(StatusPedido.EmProducao, StatusOs.EmProducao);
    }

    [Fact]
    public void Orcamento_usa_os_quatro_textos()
    {
        Assert.Equal(["aguardando", "respondido", "aceito", "recusado"], StatusOrcamento.Todos);
    }

    [Fact]
    public void Mensagem_inclui_fechado_que_a_tela_ja_reconhece()
    {
        Assert.Equal(["aberto", "em atendimento", "resolvido", "fechado"], StatusMensagem.Todos);
    }

    [Fact]
    public void Impressao_repete_o_filtro_da_fila()
    {
        Assert.Equal(
            ["na fila", "imprimindo", "pausada", "erro", "concluida", "cancelada"],
            StatusImpressao.Todos);
    }

    [Fact]
    public void Produto_e_maquina_usam_os_textos_do_admin()
    {
        Assert.Equal(["publicado", "pausado"], StatusProduto.Todos);
        Assert.Equal(["ativa", "manutencao", "parada", "baixada"], StatusMaquina.Todos);
    }
}
