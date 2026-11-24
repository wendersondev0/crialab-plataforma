using CriaLab.Application.Comercio;
using CriaLab.Domain.Calculos;

namespace CriaLab.Application.Features.Comercio.CotarFreteFicha;

public sealed record FreteFichaOpcao(string Id, string Nome, decimal Preco, int Dias, string? Descricao, bool Gratis);

public sealed record CotarFreteFichaResposta(string? Erro, IReadOnlyList<FreteFichaOpcao> Opcoes);

public sealed class CotarFreteFichaHandler(IFreteProduto frete)
{
    public async Task<CotarFreteFichaResposta?> ExecutarAsync(
        string? produtoId,
        string? cep,
        CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(produtoId))
            return null;

        var preco = await frete.PrecoAsync(produtoId.Trim(), cancelamento);
        if (preco is null)
            return null;

        var contexto = await frete.ContextoAsync(cancelamento);
        var (erro, opcoes, freteGratis) = FreteCalculo.Cotar(cep, preco.Value, contexto.Faixas, contexto.FreteGratisAPartir);
        var gratis = preco.Value >= freteGratis;
        return new CotarFreteFichaResposta(
            erro,
            opcoes.Select(o => new FreteFichaOpcao(
                o.Id,
                o.Nome,
                gratis ? 0 : o.Preco,
                o.Dias,
                o.Descricao,
                gratis)).ToList());
    }
}
