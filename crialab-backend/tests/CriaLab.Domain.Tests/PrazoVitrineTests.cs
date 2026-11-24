using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class PrazoVitrineTests
{
    private static readonly DateOnly Hoje = new(2026, 3, 10);

    [Fact]
    public void Personalizado_devolve_prazo_em_dias_sem_faixa()
    {
        var r = PrazoVitrine.Calcular("personalizado", 7, 50m, 200m, 5, Hoje);

        Assert.Equal(PrazoVitrine.AposOrcamento, r.Tipo);
        Assert.Equal("personalizado", r.Modalidade);
        Assert.Equal(7, r.PrazoDias);
        Assert.False(r.FreteGratis);
        Assert.Null(r.Inicio);
        Assert.Null(r.Fim);
    }

    [Fact]
    public void Personalizado_sem_prazo_usa_cinco_dias()
    {
        var r = PrazoVitrine.Calcular("personalizado", 0, 10m, 200m, 5, Hoje);
        Assert.Equal(5, r.PrazoDias);
    }

    [Fact]
    public void Sob_demanda_soma_prazo_e_pac()
    {
        var r = PrazoVitrine.Calcular("sob-demanda", 4, 80m, 200m, 5, Hoje);

        Assert.Equal(PrazoVitrine.Entrega, r.Tipo);
        Assert.Equal("sob-demanda", r.Modalidade);
        Assert.Equal(new DateOnly(2026, 3, 14), r.Inicio);
        Assert.Equal(new DateOnly(2026, 3, 19), r.Fim);
        Assert.False(r.FreteGratis);
    }

    [Fact]
    public void Pronta_entrega_usa_janela_do_pac()
    {
        var r = PrazoVitrine.Calcular("pronta-entrega", 9, 80m, 200m, 5, Hoje);

        Assert.Equal("pronta-entrega", r.Modalidade);
        Assert.Equal(new DateOnly(2026, 3, 14), r.Inicio);
        Assert.Equal(new DateOnly(2026, 3, 18), r.Fim);
    }

    [Fact]
    public void Pronta_entrega_com_pac_curto_espera_no_minimo_dois_dias()
    {
        var r = PrazoVitrine.Calcular("pronta-entrega", 1, 10m, 200m, 2, Hoje);

        Assert.Equal(new DateOnly(2026, 3, 12), r.Inicio);
        Assert.Equal(new DateOnly(2026, 3, 15), r.Fim);
    }

    [Fact]
    public void Frete_gratis_quando_preco_atinge_o_limite()
    {
        var gratis = PrazoVitrine.Calcular("pronta-entrega", 1, 200m, 200m, 5, Hoje);
        var pago = PrazoVitrine.Calcular("sob-demanda", 1, 199.99m, 200m, 5, Hoje);

        Assert.True(gratis.FreteGratis);
        Assert.False(pago.FreteGratis);
    }
}
