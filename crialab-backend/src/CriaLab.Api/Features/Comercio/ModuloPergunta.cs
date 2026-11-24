using System.Security.Claims;
using Carter;
using CriaLab.Application.Features.Comercio.EnviarPergunta;

namespace CriaLab.Api.Features.Comercio;

public sealed class ModuloPergunta : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/loja/produtos/{produtoId}/perguntas", EnviarAsync);
    }

    private static async Task<IResult> EnviarAsync(
        string produtoId,
        EnviarPerguntaPedido? pedido,
        ClaimsPrincipal usuario,
        EnviarPerguntaHandler handler,
        CancellationToken cancelamento)
    {
        var resultado = await handler.ExecutarAsync(
            usuario.FindFirstValue(ClaimTypes.NameIdentifier),
            produtoId,
            pedido,
            cancelamento);
        return resultado.Estado switch
        {
            EnviarPerguntaEstado.ProdutoAusente => Results.NotFound(),
            EnviarPerguntaEstado.TextoVazio => Results.BadRequest(new { erros = new[] { "Escreva sua pergunta." } }),
            _ => Results.Ok(resultado.Pergunta)
        };
    }
}
