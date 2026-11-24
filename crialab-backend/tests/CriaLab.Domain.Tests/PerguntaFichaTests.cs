using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class PerguntaFichaTests
{
    [Fact]
    public void Texto_em_branco_nao_entra()
    {
        Assert.Null(PerguntaFicha.Texto(null));
        Assert.Null(PerguntaFicha.Texto("   "));
    }

    [Fact]
    public void Texto_guarda_o_miolo()
    {
        Assert.Equal("Envia para o exterior?", PerguntaFicha.Texto("  Envia para o exterior?  "));
    }

    [Fact]
    public void Autor_anonimo_e_visitante_e_conta_sem_nome_vira_cliente()
    {
        Assert.Equal("Visitante", PerguntaFicha.Autor(false, "cliente@crialab.local"));
        Assert.Equal("Cliente", PerguntaFicha.Autor(true, null));
        Assert.Equal("Cliente", PerguntaFicha.Autor(true, ""));
        Assert.Equal("cliente@crialab.local", PerguntaFicha.Autor(true, "cliente@crialab.local"));
    }
}
