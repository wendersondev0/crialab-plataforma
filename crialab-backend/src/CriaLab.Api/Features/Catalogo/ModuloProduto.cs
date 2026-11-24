using Carter;
using CriaLab.Application.Features.Catalogo.ObterProduto;

namespace CriaLab.Api.Features.Catalogo;

public sealed class ModuloProduto : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loja/produtos/{id}", async (string id, ObterProdutoHandler handler) =>
        {
            var ficha = await handler.ExecutarAsync(id);
            return ficha is null ? Results.NotFound() : Results.Ok(ficha);
        });
    }
}
