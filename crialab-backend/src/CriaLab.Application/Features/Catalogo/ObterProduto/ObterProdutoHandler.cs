using CriaLab.Domain.Calculos;
using CriaLab.Domain.Status;

namespace CriaLab.Application.Features.Catalogo.ObterProduto;

public sealed class ObterProdutoHandler(ILeitorSql sql)
{
    public async Task<ProdutoFichaResposta?> ExecutarAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var produtos = await sql.ConsultarAsync<ProdutoLinha>("""
            SELECT p.id, p.titulo, p.sessao, p.descricao, p.preco, p.preco_comparativo, p.oferta, p.a_vista,
                   p.estoque, p.medidas, p.peso, p.situacao, p.personalizavel, p.prazo_dias,
                   p.modalidade_id, COALESCE(m.nome, '') AS modalidade_nome,
                   p.categoria_id, COALESCE(c.nome, '') AS categoria_nome
            FROM produtos p
            LEFT JOIN modalidades m ON m.id = p.modalidade_id
            LEFT JOIN categorias c ON c.id = p.categoria_id
            WHERE p.id = @id
            LIMIT 1
            """, new { id });
        var produto = produtos.FirstOrDefault();
        if (produto == null)
            return null;

        var configs = await sql.ConsultarAsync<ConfigLinha>("""
            SELECT desconto_a_vista, frete_gratis_a_partir, parcelas FROM configuracoes_loja LIMIT 1
            """);
        var config = configs.FirstOrDefault();
        var frete = config?.FreteGratisAPartir ?? 200m;
        var desconto = config?.DescontoAVista ?? 0.05m;
        var parcelas = config is { Parcelas: > 0 } ? config.Parcelas : 3;

        var identidades = await sql.ConsultarAsync<IdentidadeLinha>("""
            SELECT whatsapp, shopee, mercado_livre FROM loja_identidades ORDER BY id LIMIT 1
            """);
        var identidade = identidades.FirstOrDefault();

        var fotos = (await sql.ConsultarAsync<FotoLinha>("""
            SELECT id, arquivo, legenda, ordem FROM produto_fotos WHERE produto_id = @id ORDER BY ordem
            """, new { id })).ToList();

        var etapas = (await sql.ConsultarAsync<EtapaLinha>("""
            SELECT id, nome, tipo, acresce FROM produto_etapas WHERE produto_id = @id ORDER BY id
            """, new { id })).ToList();

        var eixos = (await sql.ConsultarAsync<EixoLinha>("""
            SELECT id, nome, tipo, ordem FROM variacao_eixos WHERE produto_id = @id ORDER BY ordem
            """, new { id })).ToList();
        var opcoes = eixos.Count == 0
            ? []
            : (await sql.ConsultarAsync<OpcaoLinha>("""
                SELECT id, variacao_eixo_id, nome, hex, ordem
                FROM variacao_opcoes
                WHERE variacao_eixo_id = ANY(@ids)
                ORDER BY ordem
                """, new { ids = eixos.Select(x => x.Id).ToArray() })).ToList();

        var cores = (await sql.ConsultarAsync<CorLinha>("""
            SELECT id, nome, hex FROM produto_cores_legado WHERE produto_id = @id ORDER BY id
            """, new { id })).ToList();
        var tamanhos = (await sql.ConsultarAsync<TamanhoLinha>("""
            SELECT id, nome FROM produto_tamanhos_legado WHERE produto_id = @id ORDER BY id
            """, new { id })).ToList();
        var avaliacoes = (await sql.ConsultarAsync<AvaliacaoLinha>("""
            SELECT id, autor, nota, texto FROM avaliacoes WHERE produto_id = @id ORDER BY id
            """, new { id })).ToList();
        var perguntas = (await sql.ConsultarAsync<PerguntaLinha>("""
            SELECT id, autor, texto, resposta, criado_em
            FROM perguntas_produto
            WHERE produto_id = @id
            ORDER BY criado_em DESC
            """, new { id })).ToList();

        var relacionados = (await sql.ConsultarAsync<RelacionadoLinha>("""
            SELECT p.id, p.titulo, p.preco, p.modalidade_id, COALESCE(m.nome, '') AS modalidade_nome, p.prazo_dias
            FROM produtos p
            LEFT JOIN modalidades m ON m.id = p.modalidade_id
            WHERE p.categoria_id = @categoria AND p.id <> @id AND p.situacao = @situacao
            LIMIT 4
            """, new { categoria = produto.CategoriaId, id, situacao = StatusProduto.Publicado })).ToList();

        var fotosRel = relacionados.Count == 0
            ? []
            : (await sql.ConsultarAsync<FotoRelLinha>("""
                SELECT produto_id, arquivo, ordem FROM produto_fotos WHERE produto_id = ANY(@ids)
                """, new { ids = relacionados.Select(x => x.Id).ToArray() })).ToList();
        var capa = fotosRel
            .GroupBy(x => x.ProdutoId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Ordem).Select(x => x.Arquivo).FirstOrDefault(),
                StringComparer.OrdinalIgnoreCase);

        var prazo = PrazoVitrine.Calcular(produto.ModalidadeId, produto.PrazoDias, produto.Preco, frete);
        return new ProdutoFichaResposta(
            produto.Id,
            produto.Titulo,
            produto.Sessao,
            produto.Descricao,
            produto.Preco,
            produto.PrecoComparativo,
            produto.Oferta,
            produto.AVista,
            desconto,
            parcelas,
            produto.Estoque,
            produto.Medidas,
            produto.Peso,
            produto.Situacao,
            produto.Personalizavel,
            produto.ModalidadeId,
            produto.ModalidadeNome,
            produto.CategoriaId,
            produto.CategoriaNome,
            prazo.Tipo,
            prazo.FreteGratis,
            prazo.PrazoDias,
            prazo.Inicio,
            prazo.Fim,
            identidade?.Whatsapp,
            identidade?.Shopee,
            identidade?.MercadoLivre,
            fotos.Select(x => new ProdutoFotoResposta(x.Id, x.Arquivo, x.Legenda, x.Ordem)).ToList(),
            eixos.Select(eixo => new VariacaoResposta(
                eixo.Id,
                eixo.Nome,
                eixo.Tipo,
                opcoes.Where(x => string.Equals(x.VariacaoEixoId, eixo.Id, StringComparison.Ordinal))
                    .OrderBy(x => x.Ordem)
                    .Select(x => new VariacaoOpcaoResposta(x.Id, x.Nome, x.Hex))
                    .ToList())).ToList(),
            cores.Select(x => new CorLegadoResposta(x.Id, x.Nome, x.Hex)).ToList(),
            tamanhos.Select(x => new TamanhoLegadoResposta(x.Id, x.Nome)).ToList(),
            etapas.Select(x => new EtapaResposta(x.Id, x.Nome, x.Tipo, x.Acresce)).ToList(),
            avaliacoes.Select(x => new AvaliacaoResposta(x.Id, x.Autor, x.Nota, x.Texto)).ToList(),
            perguntas.Select(x => new PerguntaFichaResposta(x.Id, x.Autor, x.Texto, x.Resposta, !string.IsNullOrWhiteSpace(x.Resposta))).ToList(),
            relacionados.Select(x =>
            {
                var prazoRel = PrazoVitrine.Calcular(x.ModalidadeId, x.PrazoDias, x.Preco, frete);
                capa.TryGetValue(x.Id, out var foto);
                return new ProdutoRelacionadoResposta(
                    x.Id,
                    x.Titulo,
                    x.Preco,
                    x.ModalidadeId,
                    x.ModalidadeNome,
                    string.IsNullOrWhiteSpace(foto) ? null : foto,
                    prazoRel.Tipo,
                    prazoRel.FreteGratis,
                    prazoRel.PrazoDias,
                    prazoRel.Inicio,
                    prazoRel.Fim);
            }).ToList());
    }

    private sealed class ProdutoLinha
    {
        public string Id { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Sessao { get; set; } = "";
        public string Descricao { get; set; } = "";
        public decimal Preco { get; set; }
        public decimal PrecoComparativo { get; set; }
        public bool Oferta { get; set; }
        public bool AVista { get; set; }
        public int Estoque { get; set; }
        public string Medidas { get; set; } = "";
        public decimal Peso { get; set; }
        public string Situacao { get; set; } = "";
        public bool Personalizavel { get; set; }
        public int PrazoDias { get; set; }
        public string ModalidadeId { get; set; } = "";
        public string ModalidadeNome { get; set; } = "";
        public string CategoriaId { get; set; } = "";
        public string CategoriaNome { get; set; } = "";
    }

    private sealed class ConfigLinha
    {
        public decimal DescontoAVista { get; set; }
        public decimal FreteGratisAPartir { get; set; }
        public int Parcelas { get; set; }
    }

    private sealed class IdentidadeLinha
    {
        public string? Whatsapp { get; set; }
        public string? Shopee { get; set; }
        public string? MercadoLivre { get; set; }
    }

    private sealed class FotoLinha
    {
        public int Id { get; set; }
        public string Arquivo { get; set; } = "";
        public string Legenda { get; set; } = "";
        public int Ordem { get; set; }
    }

    private sealed class EtapaLinha
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public decimal Acresce { get; set; }
    }

    private sealed class EixoLinha
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int Ordem { get; set; }
    }

    private sealed class OpcaoLinha
    {
        public string Id { get; set; } = "";
        public string VariacaoEixoId { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Hex { get; set; } = "";
        public int Ordem { get; set; }
    }

    private sealed class CorLinha
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Hex { get; set; } = "";
    }

    private sealed class TamanhoLinha
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
    }

    private sealed class AvaliacaoLinha
    {
        public string Id { get; set; } = "";
        public string Autor { get; set; } = "";
        public int Nota { get; set; }
        public string Texto { get; set; } = "";
    }

    private sealed class PerguntaLinha
    {
        public string Id { get; set; } = "";
        public string Autor { get; set; } = "";
        public string Texto { get; set; } = "";
        public string Resposta { get; set; } = "";
    }

    private sealed class RelacionadoLinha
    {
        public string Id { get; set; } = "";
        public string Titulo { get; set; } = "";
        public decimal Preco { get; set; }
        public string ModalidadeId { get; set; } = "";
        public string ModalidadeNome { get; set; } = "";
        public int PrazoDias { get; set; }
    }

    private sealed class FotoRelLinha
    {
        public string ProdutoId { get; set; } = "";
        public string Arquivo { get; set; } = "";
        public int Ordem { get; set; }
    }
}
