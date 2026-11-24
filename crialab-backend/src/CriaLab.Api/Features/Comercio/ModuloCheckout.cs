using System.Security.Claims;
using Carter;
using CriaLab.Application.Features.Comercio.ConfirmarCheckout;
using CriaLab.Application.Features.Comercio.CotarCheckout;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloCheckout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/loja/checkout/cotacao", async (CotarCheckoutPedido pedido, CotarCheckoutHandler handler) =>
        {
            var cotacao = await handler.ExecutarAsync(pedido);
            return cotacao is null ? Results.NotFound() : Results.Ok(cotacao);
        }).RequireAuthorization();

        app.MapPost("/api/loja/checkout/pedidos", async (
            CotarCheckoutPedido? pedido,
            ClaimsPrincipal usuario,
            ConfirmarCheckoutHandler handler,
            CancellationToken cancelamento) =>
        {
            var gravado = await handler.ExecutarAsync(
                usuario.FindFirstValue(ClaimTypes.NameIdentifier),
                pedido,
                cancelamento);
            return gravado is null ? Results.NotFound() : Results.Ok(gravado);
        }).RequireAuthorization();
    }
}
