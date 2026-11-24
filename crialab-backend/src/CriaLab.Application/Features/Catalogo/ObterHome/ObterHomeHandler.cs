using CriaLab.Domain.Calculos;
using CriaLab.Application;
using CriaLab.Domain.Status;

namespace CriaLab.Application.Features.Catalogo.ObterHome;

public sealed class ObterHomeHandler(ILeitorSql sql)
{
    public async Task<HomeResposta> ExecutarAsync()
    {
        var identidades = await sql.ConsultarAsync<IdentidadeLinha>("""
            SELECT nome, site, cnpj, email, telefone, whatsapp, instagram, tiktok, youtube,
                   shopee, mercado_livre, endereco, horario
            FROM loja_identidades
            ORDER BY id
            LIMIT 1
            """);
        var identidade = identidades.FirstOrDefault();

        var configs = await sql.ConsultarAsync<ConfigLinha>("""
            SELECT frete_gratis_a_partir FROM configuracoes_loja LIMIT 1
            """);
        var freteGratis = configs.FirstOrDefault()?.FreteGratisAPartir ?? 200m;

        var ajudas = await sql.ConsultarAsync<TextoLinha>("""
            SELECT texto FROM menus_ajuda WHERE id = 'inicio' LIMIT 1
            """);
        var intro = ajudas.FirstOrDefault()?.Texto;
        if (string.IsNullOrWhiteSpace(intro))
            intro = "Explore a vitrine da Cria Lab.";

        var modalidades = (await sql.ConsultarAsync<ModalidadeLinha>("""
            SELECT id, nome, resumo, explicacao FROM modalidades
            """))
            .OrderBy(x => OrdemModalidade(x.Id))
            .ThenBy(x => x.Nome, StringComparer.OrdinalIgnoreCase)
            .Select(x => new ModalidadeHomeResposta(x.Id, x.Nome, x.Resumo, x.Explicacao))
            .ToList();

        var categorias = (await sql.ConsultarAsync<CategoriaLinha>("""
            SELECT id, nome FROM categorias ORDER BY nome
            """))
            .Select(x => new CategoriaHomeResposta(x.Id, x.Nome))
            .ToList();

        var produtos = await sql.ConsultarAsync<ProdutoLinha>("""
            SELECT p.id, p.titulo, p.preco, p.modalidade_id, COALESCE(m.nome, '') AS modalidade_nome,
                   COALESCE(c.nome, '') AS categoria_nome, p.prazo_dias, p.mais_vendido
            FROM produtos p
            LEFT JOIN modalidades m ON m.id = p.modalidade_id
            LEFT JOIN categorias c ON c.id = p.categoria_id
            WHERE p.situacao = @situacao
            ORDER BY CASE lower(p.modalidade_id)
                       WHEN 'pronta-entrega' THEN 0
                       WHEN 'sob-demanda' THEN 1
                       WHEN 'personalizado' THEN 2
                       ELSE 9
                     END,
                     p.criado_em DESC
            """, new { situacao = StatusProduto.Publicado });

        var fotos = await sql.ConsultarAsync<FotoLinha>("""
            SELECT produto_id, arquivo, ordem FROM produto_fotos
            """);
        var capa = fotos
            .GroupBy(x => x.ProdutoId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Ordem).Select(x => x.Arquivo).FirstOrDefault(),
                StringComparer.OrdinalIgnoreCase);

        var destaques = produtos.Select(p =>
        {
            var prazo = PrazoVitrine.Calcular(p.ModalidadeId, p.PrazoDias, p.Preco, freteGratis);
            capa.TryGetValue(p.Id, out var foto);
            return new DestaqueHomeResposta(
                p.Id,
                p.Titulo,
                p.Preco,
                p.ModalidadeId,
                p.ModalidadeNome,
                p.CategoriaNome,
                string.IsNullOrWhiteSpace(foto) ? null : foto,
                p.MaisVendido,
                prazo.Tipo,
                prazo.FreteGratis,
                prazo.PrazoDias,
                prazo.Inicio,
                prazo.Fim);
        }).ToList();

        var depoimentos = (await sql.ConsultarAsync<DepoimentoLinha>("""
            SELECT id, autor, texto
            FROM depoimentos
            WHERE pode_aparecer
            ORDER BY autor
            """))
            .Select(x => new DepoimentoHomeResposta(x.Id, x.Autor, x.Texto))
            .ToList();

        var parceiros = (await sql.ConsultarAsync<ParceiroLinha>("""
            SELECT id, nome, logo, url
            FROM parceiros
            WHERE pode_aparecer
            ORDER BY nome
            """))
            .Select(x => new ParceiroHomeResposta(x.Id, x.Nome, x.Logo, x.Url))
            .ToList();

        return new HomeResposta(
            new IdentidadeHomeResposta(
                string.IsNullOrWhiteSpace(identidade?.Nome) ? "Cria Lab DM" : identidade!.Nome,
                identidade?.Site ?? "",
                identidade?.Cnpj ?? "",
                identidade?.Email ?? "",
                identidade?.Telefone ?? "",
                identidade?.Whatsapp ?? "",
                identidade?.Instagram ?? "",
                identidade?.Tiktok ?? "",
                identidade?.Youtube ?? "",
                identidade?.Shopee ?? "",
                identidade?.MercadoLivre ?? "",
                identidade?.Endereco ?? "",
                identidade?.Horario ?? ""),
            freteGratis,
            intro,
            modalidades,
            categorias,
            destaques,
            depoimentos,
            parceiros);
    }

    private static int OrdemModalidade(string? id) => id?.ToLowerInvariant() switch
    {
        "pronta-entrega" => 0,
        "sob-demanda" => 1,
        "personalizado" => 2,
        _ => 9
    };

    private sealed class IdentidadeLinha
    {
        public string Nome { get; set; } = "";
        public string Site { get; set; } = "";
        public string Cnpj { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefone { get; set; } = "";
        public string Whatsapp { get; set; } = "";
        public string Instagram { get; set; } = "";
        public string Tiktok { get; set; } = "";
        public string Youtube { get; set; } = "";
        public string Shopee { get; set; } = "";
        public string MercadoLivre { get; set; } = "";
        public string Endereco { get; set; } = "";
        public string Horario { get; set; } = "";
    }

    private sealed class ConfigLinha
    {
        public decimal FreteGratisAPartir { get; set; }
    }

    private sealed class TextoLinha
    {
        public string Texto { get; set; } = "";
    }

    private sealed class ModalidadeLinha
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Resumo { get; set; } = "";
        public string Explicacao { get; set; } = "";
    }

    private sealed class CategoriaLinha
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
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
        public bool MaisVendido { get; set; }
    }

    private sealed class FotoLinha
    {
        public string ProdutoId { get; set; } = "";
        public string Arquivo { get; set; } = "";
        public int Ordem { get; set; }
    }

    private sealed class DepoimentoLinha
    {
        public string Id { get; set; } = "";
        public string Autor { get; set; } = "";
        public string Texto { get; set; } = "";
    }

    private sealed class ParceiroLinha
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Logo { get; set; } = "";
        public string Url { get; set; } = "";
    }
}
