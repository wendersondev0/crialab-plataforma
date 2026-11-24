using System.Globalization;
using CriaLab.Domain.Calculos;
using CriaLab.Domain.Status;

namespace CriaLab.Application.Features.Catalogo.Buscar;

public sealed class BuscarHandler(ILeitorSql sql)
{
    private static readonly CultureInfo Brasil = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<BuscaResposta> ExecutarAsync(BuscaConsulta consulta)
    {
        var contexto = await CarregarAsync();
        var filtro = Normalizar(consulta, contexto.Categorias);
        var resultados = BuscaLoja.Aplicar(contexto.Itens, filtro, contexto.FreteGratisAPartir);
        var nomes = contexto.Categorias.ToDictionary(x => x.Id, x => x.Nome, StringComparer.OrdinalIgnoreCase);
        return new BuscaResposta(
            contexto.Ajuda,
            filtro.Mensagem,
            filtro.TemFiltro,
            BuscaLoja.Resumo(resultados.Count),
            contexto.FreteGratisAPartir,
            contexto.FreteGratisAPartir.ToString("C", Brasil),
            Filtros(filtro),
            contexto.Categorias,
            BuscaLoja.Ativos(filtro, nomes).Select(x => new BuscaFiltroAtivoResposta(x.Rotulo, x.Remover)).ToList(),
            resultados.Select(x => Mapear(x, contexto.FreteGratisAPartir)).ToList());
    }

    public async Task<IReadOnlyList<SugestaoResposta>> SugestoesAsync(BuscaConsulta consulta)
    {
        var contexto = await CarregarAsync();
        var filtro = Normalizar(consulta, contexto.Categorias);
        return BuscaLoja.Sugestoes(contexto.Itens, filtro, contexto.FreteGratisAPartir, consulta.Q)
            .Select(x => new SugestaoResposta(x.Id, x.Titulo, x.Preco, x.CategoriaNome, x.Foto))
            .ToList();
    }

    private static BuscaLoja.Normalizada Normalizar(BuscaConsulta consulta, IReadOnlyList<BuscaCategoriaResposta> categorias) =>
        BuscaLoja.Normalizar(
            consulta.Q,
            consulta.Categoria,
            consulta.PrecoMin,
            consulta.PrecoMax,
            consulta.Ordem,
            consulta.Nota,
            consulta.FreteGratis,
            consulta.AVista,
            consulta.Personalizavel,
            categorias.Select(x => (x.Id, x.Nome)));

    private static BuscaFiltrosResposta Filtros(BuscaLoja.Normalizada filtro) =>
        new(filtro.Termo, filtro.CategoriaId, filtro.PrecoMinCampo, filtro.PrecoMaxCampo, filtro.Ordem, filtro.Nota, filtro.FreteGratis, filtro.AVista, filtro.Personalizavel);

    private static BuscaItemResposta Mapear(BuscaLoja.Item item, decimal freteGratisAPartir)
    {
        var prazo = PrazoVitrine.Calcular(item.ModalidadeId, item.PrazoDias, item.Preco, freteGratisAPartir);
        return new BuscaItemResposta(
            item.Id,
            item.Titulo,
            item.Preco,
            item.ModalidadeId,
            item.ModalidadeNome,
            item.CategoriaNome,
            item.Foto,
            prazo.Tipo,
            prazo.FreteGratis,
            prazo.PrazoDias,
            prazo.Inicio,
            prazo.Fim);
    }

    private async Task<Contexto> CarregarAsync()
    {
        var configs = await sql.ConsultarAsync<ConfigLinha>("SELECT frete_gratis_a_partir FROM configuracoes_loja LIMIT 1");
        var frete = configs.FirstOrDefault()?.FreteGratisAPartir ?? 200m;
        var ajudas = await sql.ConsultarAsync<TextoLinha>("SELECT texto FROM menus_ajuda WHERE id = 'busca' LIMIT 1");
        var ajuda = ajudas.FirstOrDefault()?.Texto;
        if (string.IsNullOrWhiteSpace(ajuda))
            ajuda = "Filtre peças por texto, tema e preço.";

        var categorias = (await sql.ConsultarAsync<CategoriaLinha>("SELECT id, nome FROM categorias ORDER BY nome"))
            .Select(x => new BuscaCategoriaResposta(x.Id, x.Nome))
            .ToList();

        var produtos = await sql.ConsultarAsync<ProdutoLinha>("""
            SELECT p.id, p.titulo, p.descricao, p.material, p.categoria_id,
                   COALESCE(c.nome, '') AS categoria_nome,
                   p.modalidade_id, COALESCE(m.nome, '') AS modalidade_nome,
                   p.preco, p.a_vista, p.personalizavel, p.nota, p.vendas, p.criado_em, p.prazo_dias
            FROM produtos p
            LEFT JOIN categorias c ON c.id = p.categoria_id
            LEFT JOIN modalidades m ON m.id = p.modalidade_id
            WHERE p.situacao = @situacao
            """, new { situacao = StatusProduto.Publicado });

        var fotos = await sql.ConsultarAsync<FotoLinha>("SELECT produto_id, arquivo, ordem FROM produto_fotos");
        var capa = fotos
            .GroupBy(x => x.ProdutoId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.Ordem).Select(x => x.Arquivo).FirstOrDefault(),
                StringComparer.OrdinalIgnoreCase);

        var itens = produtos.Select(p =>
        {
            capa.TryGetValue(p.Id, out var foto);
            return new BuscaLoja.Item(
                p.Id,
                p.Titulo,
                p.Descricao,
                p.Material,
                p.CategoriaId,
                p.CategoriaNome,
                p.ModalidadeId,
                p.ModalidadeNome,
                p.Preco,
                p.AVista,
                p.Personalizavel,
                p.Nota,
                p.Vendas,
                p.CriadoEm,
                p.PrazoDias,
                string.IsNullOrWhiteSpace(foto) ? null : foto);
        }).ToList();

        return new Contexto(frete, ajuda, categorias, itens);
    }

    private sealed record Contexto(decimal FreteGratisAPartir, string Ajuda, List<BuscaCategoriaResposta> Categorias, List<BuscaLoja.Item> Itens);

    private sealed class ConfigLinha
    {
        public decimal FreteGratisAPartir { get; set; }
    }

    private sealed class TextoLinha
    {
        public string Texto { get; set; } = "";
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
        public string Descricao { get; set; } = "";
        public string Material { get; set; } = "";
        public string CategoriaId { get; set; } = "";
        public string CategoriaNome { get; set; } = "";
        public string ModalidadeId { get; set; } = "";
        public string ModalidadeNome { get; set; } = "";
        public decimal Preco { get; set; }
        public bool AVista { get; set; }
        public bool Personalizavel { get; set; }
        public decimal Nota { get; set; }
        public int Vendas { get; set; }
        public DateOnly CriadoEm { get; set; }
        public int PrazoDias { get; set; }
    }

    private sealed class FotoLinha
    {
        public string ProdutoId { get; set; } = "";
        public string Arquivo { get; set; } = "";
        public int Ordem { get; set; }
    }
}
