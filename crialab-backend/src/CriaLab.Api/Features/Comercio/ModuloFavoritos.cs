using System.Security.Claims;
using Carter;
using CriaLab.Application.Features.Comercio.AlternarFavorito;
using CriaLab.Application.Features.Comercio.ConsultarFavorito;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloFavoritos : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/loja/favoritos").RequireAuthorization();
        grupo.MapGet("/{produtoId}", ConsultarAsync);
        grupo.MapPost("/{produtoId}", AlternarAsync);
    }

    private static async Task<IResult> ConsultarAsync(
        string produtoId,
        ClaimsPrincipal usuario,
        ConsultarFavoritoHandler handler,
        CancellationToken cancelamento)
    {
        var favorito = await handler.ExecutarAsync(UsuarioId(usuario), produtoId, cancelamento);
        return favorito is null ? Results.NotFound() : Results.Ok(new { favorito });
    }

    private static async Task<IResult> AlternarAsync(
        string produtoId,
        ClaimsPrincipal usuario,
        AlternarFavoritoHandler handler,
        CancellationToken cancelamento)
    {
        var favorito = await handler.ExecutarAsync(UsuarioId(usuario), produtoId, cancelamento);
        return favorito is null ? Results.NotFound() : Results.Ok(new { favorito });
    }

    private static string? UsuarioId(ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(ClaimTypes.NameIdentifier);
}
