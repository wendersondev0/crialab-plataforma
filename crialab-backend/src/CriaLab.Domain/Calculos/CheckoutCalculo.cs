using CriaLab.Domain.Entidades;

namespace CriaLab.Domain.Calculos;

public static class CheckoutCalculo
{
    public sealed record Item(
        string ProdutoId,
        string Titulo,
        decimal PrecoUnitario,
        int Quantidade,
        string? Cor,
        string? Tamanho,
        string? Detalhes);

    public sealed record Resultado(
        decimal Subtotal,
        decimal DescontoCupom,
        decimal DescontoAVista,
        decimal FreteCobrado,
        string FreteNome,
        string FreteId,
        decimal FreteGratisAPartir,
        decimal Total,
        string? ErroFrete,
        IReadOnlyList<string> Erros,
        IReadOnlyList<Item> Itens,
        IReadOnlyList<FreteOpcao> Opcoes);

    public static Resultado Calcular(
        IReadOnlyList<Item> itens,
        string? cep,
        string? cupomCodigo,
        string? freteId,
        string? endereco,
        string? forma,
        string? acao,
        IEnumerable<Cupom> cupons,
        IEnumerable<FaixaCep> faixas,
        decimal freteGratisAPartir,
        decimal descontoAVistaPct)
    {
        var subtotal = itens.Sum(item => item.PrecoUnitario * item.Quantidade);
        if (itens.Count == 0)
        {
            return new Resultado(0, 0, 0, 0, "", freteId ?? "", freteGratisAPartir, 0, null, [], itens, []);
        }

        var descontoCupom = FreteCalculo.DescontoCupom(subtotal, FreteCalculo.CupomPorCodigo(cupomCodigo, cupons));
        var formaEfetiva = string.IsNullOrWhiteSpace(forma) ? "pix" : forma;
        var aVista = string.Equals(formaEfetiva, "pix", StringComparison.OrdinalIgnoreCase)
            || string.Equals(formaEfetiva, "boleto", StringComparison.OrdinalIgnoreCase);
        var descontoAVista = aVista
            ? Math.Round((subtotal - descontoCupom) * descontoAVistaPct, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var (erro, opcoes, _) = FreteCalculo.Cotar(cep, subtotal, faixas, freteGratisAPartir);
        var nome = "";
        var id = freteId ?? "";
        var cobrado = 0m;
        if (opcoes.Count > 0)
        {
            var escolhida = opcoes.FirstOrDefault(opcao => opcao.Id == freteId) ?? opcoes[0];
            id = escolhida.Id;
            nome = escolhida.Nome;
            cobrado = subtotal >= freteGratisAPartir ? 0 : escolhida.Preco;
        }

        var total = Math.Max(0, subtotal - descontoCupom - descontoAVista + cobrado);
        var erros = new List<string>();
        if (!string.Equals(acao, "cotar", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(endereco))
                erros.Add("Informe o endereço de entrega.");
            if (erro != null || opcoes.Count == 0)
                erros.Add(erro ?? "Calcule o frete antes de confirmar.");
        }

        return new Resultado(
            subtotal,
            descontoCupom,
            descontoAVista,
            cobrado,
            nome,
            id,
            freteGratisAPartir,
            total,
            erro,
            erros,
            itens,
            opcoes.ToList());
    }
}
