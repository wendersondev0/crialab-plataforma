using System.Security.Claims;
using Carter;
using CriaLab.Application.Features.Comercio.EnviarContato;
using CriaLab.Application.Features.Comercio.ObterContato;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloContato : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loja/contato", async (
            ClaimsPrincipal usuario,
            ObterContatoHandler handler,
            CancellationToken cancelamento) =>
        {
            var contato = await handler.ExecutarAsync(usuario.FindFirstValue(ClaimTypes.NameIdentifier), cancelamento);
            return Results.Ok(contato);
        });

        app.MapPost("/api/loja/contato", async (
            EnviarContatoPedido? pedido,
            EnviarContatoHandler handler,
            CancellationToken cancelamento) =>
        {
            var resultado = await handler.ExecutarAsync(pedido, cancelamento);
            return resultado.Gravada
                ? Results.Ok(resultado.Mensagem)
                : Results.BadRequest(new { erros = resultado.Erros });
        });
    }
}
