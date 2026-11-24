using Carter;
using CriaLab.Application.Features.Comercio.AdicionarItemCarrinho;
using CriaLab.Application.Features.Comercio.ResumirCarrinho;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloCarrinho : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/loja/carrinho/itens", async (AdicionarItemCarrinhoPedido pedido, AdicionarItemCarrinhoHandler handler) =>
        {
            var item = await handler.ExecutarAsync(pedido);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        app.MapPost("/api/loja/carrinho/resumo", (ResumirCarrinhoPedido pedido, ResumirCarrinhoHandler handler) =>
            Results.Ok(handler.Executar(pedido)));
    }
}
