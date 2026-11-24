using System.Security.Claims;
using Carter;
using CriaLab.Application.Features.Comercio.PedirOrcamento;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloOrcamento : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/loja/produtos/{produtoId}/orcamentos", PedirAsync).RequireAuthorization();
    }

    private static async Task<IResult> PedirAsync(
        string produtoId,
        PedirOrcamentoPedido? pedido,
        ClaimsPrincipal usuario,
        PedirOrcamentoHandler handler,
        CancellationToken cancelamento)
    {
        var criado = await handler.ExecutarAsync(
            usuario.FindFirstValue(ClaimTypes.NameIdentifier),
            produtoId,
            pedido,
            cancelamento);
        return criado is null ? Results.NotFound() : Results.Ok(criado);
    }
}
