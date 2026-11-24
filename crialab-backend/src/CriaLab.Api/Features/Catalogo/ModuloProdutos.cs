using Carter;
using CriaLab.Application.Features.Catalogo.ListarProdutos;

namespace CriaLab.Api.Features.Catalogo;

public sealed class ModuloProdutos : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/loja/produtos", async ([AsParameters] ListarProdutosConsulta consulta, ListarProdutosHandler handler) =>
            Results.Ok(await handler.ExecutarAsync(consulta)));
    }
}
