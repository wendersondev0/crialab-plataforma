using Carter;
using CriaLab.Api.Autorizacao;

namespace CriaLab.Api.Features.Admin;

public sealed class ModuloAdminAcesso : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGrupoAdmin();
        grupo.MapGet("/acesso", () => Results.Ok(new { autorizado = true }));
    }
}
