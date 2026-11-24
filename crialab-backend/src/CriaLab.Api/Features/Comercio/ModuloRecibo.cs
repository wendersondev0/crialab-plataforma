using System.Security.Claims;
using Carter;
using CriaLab.Application.Features.Comercio.ObterRecibo;
using CriaLab.Domain.Identidade;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloRecibo : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loja/pedidos/{id}", async (
            string id,
            ClaimsPrincipal usuario,
            ObterReciboHandler handler,
            CancellationToken cancelamento) =>
        {
            var recibo = await handler.ExecutarAsync(
                usuario.FindFirstValue(ClaimTypes.NameIdentifier),
                usuario.IsInRole(Papeis.Administrador),
                id,
                cancelamento);
            return recibo is null ? Results.NotFound() : Results.Ok(recibo);
        }).RequireAuthorization();
    }
}