using CriaLab.Domain.Calculos;

namespace CriaLab.Domain.Tests;

public class BuscaLojaTests
{
    private static readonly (string Id, string Nome)[] Categorias = [("chaveiros", "Chaveiros"), ("decoracao", "Decoração")];

    private static BuscaLoja.Item Peca(
        string id,
        string titulo,
        decimal preco,
        string categoria = "decoracao",
        int vendas = 0,
        decimal nota = 0,
        bool personalizavel = false,
        DateOnly? criado = null) =>
        new(id, titulo, "", "PLA", categoria, categoria == "chaveiros" ? "Chaveiros" : "Decoração", "pronta-entrega", "Pronta Entrega", preco, true, personalizavel, nota, vendas, criado ?? new DateOnly(2026, 4, 1), 5, null);

    [Fact]
    public void Sem_filtro_devolve_a_vitrine_inteira_por_relevancia()
    {
        var itens = new[]
        {
            Peca("a", "Zebra", 10, vendas: 1),
            Peca("b", "Alfa", 20, vendas: 3)
        };
        var filtro = BuscaLoja.Normalizar(null, null, null, null, null, null, null, null, null, Categorias);
        var lista = BuscaLoja.Aplicar(itens, filtro, 200);

        Assert.False(filtro.TemFiltro);
        Assert.Equal(["b", "a"], lista.Select(x => x.Id));
        Assert.Equal("2 peças", BuscaLoja.Resumo(lista.Count));
    }

    [Fact]
    public void Texto_ignora_acento_e_sugestao_curta_vem_vazia()
    {
        var itens = new[] { Peca("1", "Peça da oficina", 42) };
        var filtro = BuscaLoja.Normalizar("peca", null, null, null, null, null, null, null, null, Categorias);

        Assert.Equal(["1"], BuscaLoja.Aplicar(itens, filtro, 200).Select(x => x.Id));
        Assert.Empty(BuscaLoja.Sugestoes(itens, filtro, 200, "p"));
        Assert.Single(BuscaLoja.Sugestoes(itens, filtro, 200, "pec"));
    }

    [Fact]
    public void Categoria_inexistente_avisa_e_preco_invertido_troca_a_faixa()
    {
        var invalida = BuscaLoja.Normalizar(null, "sumiu", null, null, null, null, null, null, null, Categorias);
        Assert.Null(invalida.CategoriaId);
        Assert.Equal("A categoria desse link não existe mais. Mostrando todas.", invalida.Mensagem);

        var faixa = BuscaLoja.Normalizar(null, null, "80", "10", null, null, null, null, null, Categorias);
        Assert.Equal(10m, faixa.PrecoMin);
        Assert.Equal(80m, faixa.PrecoMax);
        Assert.Equal("O preço mínimo estava acima do máximo. A faixa foi invertida.", faixa.Mensagem);
        Assert.Equal("10,00", faixa.PrecoMinCampo);
    }

    [Fact]
    public void Preco_invalido_avisa_e_ordem_menor_preco_ordena()
    {
        var aviso = BuscaLoja.Normalizar(null, null, "abc", null, null, null, null, null, null, Categorias);
        Assert.Equal("Use apenas números no preço. Exemplo: 25 ou 25,90.", aviso.Mensagem);
        Assert.Null(aviso.PrecoMin);

        var filtro = BuscaLoja.Normalizar(null, null, null, null, "PRECO", null, null, null, null, Categorias);
        var lista = BuscaLoja.Aplicar(
            [Peca("caro", "Caro", 90), Peca("barato", "Barato", 10)],
            filtro,
            200);
        Assert.Equal(["barato", "caro"], lista.Select(x => x.Id));
        Assert.Equal("Menor preço", BuscaLoja.Ativos(filtro, new Dictionary<string, string>()).Single().Rotulo);
    }

    [Fact]
    public void Chip_de_faixa_remove_os_dois_precos()
    {
        var filtro = BuscaLoja.Normalizar(null, "chaveiros", "25,90", "100", null, "4,0", "sim", null, null, Categorias);
        var ativos = BuscaLoja.Ativos(filtro, new Dictionary<string, string> { ["chaveiros"] = "Chaveiros" });

        Assert.Contains(ativos, x => x.Rotulo == "Chaveiros" && x.Remover.SequenceEqual(new[] { "categoria" }));
        Assert.Contains(ativos, x => x.Remover.SequenceEqual(new[] { "precoMin", "precoMax" }));
        Assert.Contains(ativos, x => x.Rotulo == "Nota 4 ou mais");
        Assert.Contains(ativos, x => x.Rotulo == "Frete grátis");
    }
}
