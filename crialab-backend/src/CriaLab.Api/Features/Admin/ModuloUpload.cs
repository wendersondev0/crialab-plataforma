using Carter;
using CriaLab.Api.Autorizacao;
using CriaLab.Domain.Arquivos;
using CriaLab.Infrastructure.Arquivos;
using Microsoft.AspNetCore.Mvc;

namespace CriaLab.Api.Features.Admin;

public sealed class ModuloUpload : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGrupoAdmin();

        grupo.MapPost("/uploads/foto", async (IFormFile arquivo, [FromServices] GravadorImagem gravador, CancellationToken cancelamento) =>
        {
            if (!RegrasImagem.EhValida(arquivo.FileName, arquivo.ContentType, arquivo.Length, out var erro))
            {
                return Results.BadRequest(new { erros = new[] { erro } });
            }

            await using var stream = arquivo.OpenReadStream();
            var caminho = await gravador.GravarFotoAsync(stream, arquivo.FileName, cancelamento);
            return Results.Ok(new { arquivo = caminho });
        }).DisableAntiforgery();

        grupo.MapPost("/uploads/logo", async (IFormFile arquivo, [FromForm] string? id, [FromServices] GravadorImagem gravador, CancellationToken cancelamento) =>
        {
            if (!RegrasImagem.EhValida(arquivo.FileName, arquivo.ContentType, arquivo.Length, out var erro))
            {
                return Results.BadRequest(new { erros = new[] { erro } });
            }

            var identificador = string.IsNullOrWhiteSpace(id) ? "par-" + Guid.NewGuid().ToString("N")[..8] : id.Trim();
            await using var stream = arquivo.OpenReadStream();
            var caminho = await gravador.GravarLogoAsync(stream, identificador, arquivo.FileName, cancelamento);
            return Results.Ok(new { arquivo = caminho });
        }).DisableAntiforgery();
    }
}
