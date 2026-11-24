using Carter;

namespace CriaLab.Api.Features;

public sealed class ModuloSaude : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/saude", () => Results.Ok(new SaudeResposta("disponivel")));
    }
}

public sealed record SaudeResposta(string Situacao);
