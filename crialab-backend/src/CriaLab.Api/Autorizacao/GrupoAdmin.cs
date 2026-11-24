using CriaLab.Domain.Identidade;

namespace CriaLab.Api.Autorizacao;

public static class GrupoAdmin
{
    public static RouteGroupBuilder MapGrupoAdmin(this IEndpointRouteBuilder app, string prefixo = "/api/admin")
    {
        return app.MapGroup(prefixo).RequireAuthorization(Politicas.AdminOnly);
    }
}
