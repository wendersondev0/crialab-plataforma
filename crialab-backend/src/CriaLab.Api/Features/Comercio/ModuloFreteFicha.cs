using Carter;
using CriaLab.Application.Features.Comercio.CotarFreteFicha;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloFreteFicha : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loja/produtos/{produtoId}/frete", CotarAsync);
    }

    private static async Task<IResult> CotarAsync(
        string produtoId,
        string? cep,
        CotarFreteFichaHandler handler,
        CancellationToken cancelamento)
    {
        var resultado = await handler.ExecutarAsync(produtoId, cep, cancelamento);
        return resultado is null ? Results.NotFound() : Results.Ok(resultado);
    }
}
