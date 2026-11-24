using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.AdicionarItemCarrinho;

public sealed class AdicionarItemCarrinhoHandler(ILeitorSql sql)
{
    public async Task<AdicionarItemCarrinhoResposta?> ExecutarAsync(AdicionarItemCarrinhoPedido pedido)
    {
        if (string.IsNullOrWhiteSpace(pedido.ProdutoId))
            return null;

        var produtos = await sql.ConsultarAsync<ProdutoLinha>("""
            SELECT id, titulo, preco, estoque, modalidade_id
            FROM produtos
            WHERE id = @id
            LIMIT 1
            """, new { id = pedido.ProdutoId });
        var produto = produtos.FirstOrDefault();
        if (produto == null)
            return null;

        var item = ItemCarrinho.Preparar(
            produto.ModalidadeId,
            produto.Estoque,
            pedido.Quantidade,
            pedido.Cor,
            pedido.Tamanho,
            pedido.Detalhes,
            pedido.Destino);

        return new AdicionarItemCarrinhoResposta(
            produto.Id,
            produto.Titulo,
            produto.Preco,
            item.Quantidade,
            item.Cor,
            item.Tamanho,
            item.Detalhes,
            item.Destino);
    }

    private sealed class ProdutoLinha
    {
        public string Id { get; set; } = "";
        public string Titulo { get; set; } = "";
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
        public string ModalidadeId { get; set; } = "";
    }
}
