using CriaLab.Domain.Calculos;
using CriaLab.Domain.Status;

namespace CriaLab.Application.Features.Catalogo.ListarProdutos;

public sealed class ListarProdutosHandler(ILeitorSql sql)
{
    public async Task<ListarProdutosResposta> ExecutarAsync(ListarProdutosConsulta consulta)
    {
        var filtro = string.IsNullOrWhiteSpace(consulta.ModalidadeFiltro) ? null : consulta.ModalidadeFiltro;
        var configs = await sql.ConsultarAsync<ConfigLinha>("SELECT frete_gratis_a_partir FROM configuracoes_loja LIMIT 1");
        var frete = configs.FirstOrDefault()?.FreteGratisAPartir ?? 200m;

        ModalidadeLinha? modalidade = null;
        if (filtro != null)
        {
            var encontradas = await sql.ConsultarAsync<ModalidadeLinha>("""
                SELECT id, nome, resumo, explicacao FROM modalidades WHERE id = @id LIMIT 1
                """, new { id = filtro });
            modalidade = encontradas.FirstOrDefault();
        }

        var produtos = await sql.ConsultarAsync<ProdutoLinha>("""
            SELECT p.id, p.titulo, p.preco, p.modalidade_id, COALESCE(m.nome, '') AS modalidade_nome,
                   COALESCE(c.nome, '') AS categoria_nome, p.prazo_dias
            FROM produtos p
            LEFT JOIN modalidades m ON m.id = p.modalidade_id
            LEFT JOIN categorias c ON c.id = p.categoria_id
            WHERE p.situacao = @situacao
              AND (@modalidade IS NULL OR p.modalidade_id = @modalidade)
            ORDER BY p.titulo
            """, new { situacao = StatusProduto.Publicado, modalidade = filtro });

        var fotos = await sql.ConsultarAsync<FotoLinha>("SELECT produto_id, arquivo, ordem FROM produto_fotos");
        var capa = fotos
            .GroupBy(x => x.ProdutoId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Ordem).Select(x => x.Arquivo).FirstOrDefault(),
                StringComparer.OrdinalIgnoreCase);

        var lista = produtos.Select(p =>
        {
            var prazo = PrazoVitrine.Calcular(p.ModalidadeId, p.PrazoDias, p.Preco, frete);
            capa.TryGetValue(p.Id, out var foto);
            return new ProdutoListaResposta(
                p.Id,
                p.Titulo,
                p.Preco,
                p.ModalidadeId,
                p.ModalidadeNome,
                p.CategoriaNome,
                string.IsNullOrWhiteSpace(foto) ? null : foto,
                prazo.Tipo,
                prazo.FreteGratis,
                prazo.PrazoDias,
                prazo.Inicio,
                prazo.Fim);
        }).ToList();

        return new ListarProdutosResposta(
            modalidade?.Nome ?? "Produtos",
            modalidade?.Resumo,
            modalidade != null ? modalidade.Explicacao : "Todas as peças publicadas na vitrine.",
            frete,
            lista);
    }

    private sealed class ConfigLinha
    {
        public decimal FreteGratisAPartir { get; set; }
    }

    private sealed class ModalidadeLinha
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Resumo { get; set; } = "";
        public string Explicacao { get; set; } = "";
    }

    private sealed class ProdutoLinha
    {
        public string Id { get; set; } = "";
        public string Titulo { get; set; } = "";
        public decimal Preco { get; set; }
        public string ModalidadeId { get; set; } = "";
        public string ModalidadeNome { get; set; } = "";
        public string CategoriaNome { get; set; } = "";
        public int PrazoDias { get; set; }
    }

    private sealed class FotoLinha
    {
        public string ProdutoId { get; set; } = "";
        public string Arquivo { get; set; } = "";
        public int Ordem { get; set; }
    }
}
