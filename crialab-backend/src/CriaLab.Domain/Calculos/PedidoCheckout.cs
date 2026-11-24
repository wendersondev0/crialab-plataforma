using CriaLab.Domain.Status;

namespace CriaLab.Domain.Calculos;

public static class PedidoCheckout
{
    public sealed record ItemGravado(
        string Id,
        string ProdutoId,
        string Titulo,
        int Quantidade,
        decimal PrecoUnitario,
        bool Fisico,
        string Tipo,
        string? CorEscolhida,
        string? TamanhoEscolhido);

    public sealed record OsGravada(string Status, bool InsumoBaixado);

    public sealed record Gravado(
        string Id,
        string UsuarioId,
        string Status,
        DateTime CriadoEm,
        decimal Subtotal,
        decimal DescontoCupom,
        decimal DescontoAVista,
        decimal FreteCobrado,
        decimal Total,
        string Cep,
        string Cupom,
        string Forma,
        string Endereco,
        string FreteNome,
        IReadOnlyList<ItemGravado> Itens,
        OsGravada Os);

    public static Gravado? Montar(
        string id,
        string usuarioId,
        DateTime criadoEm,
        IReadOnlyList<string> idsItens,
        CheckoutCalculo.Resultado resultado,
        string? cep,
        string? cupom,
        string? forma,
        string? endereco)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(usuarioId))
            return null;
        if (resultado.Erros.Count > 0 || resultado.Itens.Count == 0 || idsItens.Count != resultado.Itens.Count)
            return null;

        var itens = new List<ItemGravado>(resultado.Itens.Count);
        for (var i = 0; i < resultado.Itens.Count; i++)
        {
            var item = resultado.Itens[i];
            itens.Add(new ItemGravado(
                idsItens[i],
                item.ProdutoId,
                item.Titulo,
                item.Quantidade,
                item.PrecoUnitario,
                true,
                "pronto",
                item.Cor,
                item.Tamanho));
        }

        return new Gravado(
            id,
            usuarioId,
            StatusPedido.AguardandoPagamento,
            criadoEm,
            resultado.Subtotal,
            resultado.DescontoCupom,
            resultado.DescontoAVista,
            resultado.FreteCobrado,
            resultado.Total,
            cep ?? "",
            string.IsNullOrWhiteSpace(cupom) ? "" : cupom.Trim().ToUpperInvariant(),
            string.IsNullOrWhiteSpace(forma) ? "pix" : forma,
            (endereco ?? "").Trim(),
            resultado.FreteNome,
            itens,
            new OsGravada(StatusOs.Aguardando, false));
    }
}
