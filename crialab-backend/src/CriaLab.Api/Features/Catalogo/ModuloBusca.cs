using Carter;
using CriaLab.Application.Features.Catalogo.Buscar;

namespace CriaLab.Api.Features.Catalogo;

public sealed class ModuloBusca : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/loja/busca");
        grupo.MapGet("/", async ([AsParameters] BuscaConsulta consulta, BuscarHandler handler) => Results.Ok(await handler.ExecutarAsync(consulta)));
        grupo.MapGet("/sugestoes", async ([AsParameters] BuscaConsulta consulta, BuscarHandler handler) => Results.Ok(await handler.SugestoesAsync(consulta)));
    }
}
