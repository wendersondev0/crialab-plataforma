using Carter;
using CriaLab.Application.Features.Catalogo.ObterHome;

namespace CriaLab.Api.Features.Catalogo;

public sealed class ModuloHome : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loja/home", async (ObterHomeHandler handler) => Results.Ok(await handler.ExecutarAsync()));
    }
}
