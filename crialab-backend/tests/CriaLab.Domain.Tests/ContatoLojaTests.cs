using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class ContatoLojaTests
{
    [Fact]
    public void Mensagem_valida_guarda_o_rotulo_e_apara_o_texto()
    {
        var mensagem = ContatoLoja.Preparar("  Ana  ", "  ana@oficina.test  ", "  SUGESTAO ", "  Linha 1\r\nlinha 2  ");

        Assert.Empty(mensagem.Erros);
        Assert.Equal("Ana", mensagem.Nome);
        Assert.Equal("ana@oficina.test", mensagem.Email);
        Assert.Equal("Sugestão", mensagem.Tipo);
        Assert.Equal("Linha 1\nlinha 2", mensagem.Texto);
    }

    [Fact]
    public void Campos_invalidos_repetem_as_frases_do_razor()
    {
        var curta = ContatoLoja.Preparar("A", "sem-arroba", "nenhum", "curta");
        Assert.Equal(
            [
                "Diga como podemos te chamar.",
                "Informe um e-mail válido.",
                "Escolha o tipo da mensagem.",
                "Escreva a mensagem com um pouco mais de detalhe."
            ],
            curta.Erros);

        var longa = ContatoLoja.Preparar(new string('n', 121), new string('a', 181) + "@b.co", "orcamento", new string('m', 4001));
        Assert.Equal(
            [
                "Use um nome mais curto.",
                "Informe um e-mail válido.",
                "A mensagem passou de 4000 caracteres."
            ],
            longa.Erros);
        Assert.Equal("Orçamento", longa.Tipo);
    }

    [Fact]
    public void Whatsapp_e_telefone_viram_links_clicaveis()
    {
        Assert.Equal("https://wa.me/5527996530954", ContatoLoja.Whatsapp(" https://wa.me/5527996530954 "));
        Assert.Equal("http://wa.me/1", ContatoLoja.Whatsapp("http://wa.me/1"));
        Assert.Equal("https://wa.me/5527999990000", ContatoLoja.Whatsapp("(27) 99999-0000"));
        Assert.Equal("tel:+5527996530954", ContatoLoja.Ligacao("(27) 99653-0954"));
        Assert.Equal("tel:+5527996530954", ContatoLoja.Ligacao("5527996530954"));
        Assert.Null(ContatoLoja.Whatsapp("  "));
        Assert.Null(ContatoLoja.Whatsapp("123"));
        Assert.Null(ContatoLoja.Ligacao("1234567"));
        Assert.Null(ContatoLoja.Visivel("  "));
        Assert.Equal("Rua 1", ContatoLoja.Visivel("Rua 1"));
    }
}
