using CriaLab.Domain.Entidades;

namespace CriaLab.Application.Comercio;

public sealed record FreteContexto(decimal FreteGratisAPartir, IReadOnlyList<FaixaCep> Faixas);

public interface IFreteProduto
{
    Task<decimal?> PrecoAsync(string produtoId, CancellationToken cancelamento = default);

    Task<FreteContexto> ContextoAsync(CancellationToken cancelamento = default);
}
