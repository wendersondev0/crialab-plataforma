using Carter;
using CriaLab.Application.Features.Identidade.Cadastrar;
using CriaLab.Application.Features.Identidade.Entrar;
using CriaLab.Application.Features.Identidade.Eu;
using CriaLab.Application.Features.Identidade.Renovar;
using CriaLab.Application.Features.Identidade.Sair;
using CriaLab.Application.Identidade;
using CriaLab.Infrastructure.Identidade;
using Microsoft.Extensions.Options;

namespace CriaLab.Api.Features;

public sealed class ModuloContaAuth : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/conta");
        grupo.MapPost("/cadastrar", CadastrarAsync);
        grupo.MapPost("/entrar", EntrarAsync);
        grupo.MapPost("/renovar", RenovarAsync);
        grupo.MapPost("/sair", SairAsync);
        grupo.MapGet("/eu", EuAsync).RequireAuthorization();
    }

    private static async Task<IResult> CadastrarAsync(
        CadastrarPedido pedido,
        CadastrarHandler handler,
        HttpContext http,
        IOptions<OpcoesJwt> jwt)
    {
        var resultado = await handler.ExecutarAsync(pedido);
        return ResponderSessao(resultado, http, jwt.Value);
    }

    private static async Task<IResult> EntrarAsync(
        EntrarPedido pedido,
        EntrarHandler handler,
        HttpContext http,
        IOptions<OpcoesJwt> jwt)
    {
        var resultado = await handler.ExecutarAsync(pedido);
        return ResponderSessao(resultado, http, jwt.Value);
    }

    private static async Task<IResult> RenovarAsync(
        RenovarHandler handler,
        HttpContext http,
        IOptions<OpcoesJwt> jwt)
    {
        var cookie = http.Request.Cookies[jwt.Value.CookieRefresh];
        var resultado = await handler.ExecutarAsync(cookie);
        return ResponderSessao(resultado, http, jwt.Value);
    }

    private static async Task<IResult> SairAsync(
        SairHandler handler,
        HttpContext http,
        IOptions<OpcoesJwt> jwt)
    {
        var cookie = http.Request.Cookies[jwt.Value.CookieRefresh];
        await handler.ExecutarAsync(http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, cookie);
        http.Response.Cookies.Delete(jwt.Value.CookieRefresh);
        return Results.NoContent();
    }

    private static async Task<IResult> EuAsync(EuHandler handler, HttpContext http)
    {
        var resultado = await handler.ExecutarAsync(http.User);
        if (!resultado.Sucesso)
        {
            return Results.Json(new { erros = resultado.Erros }, statusCode: resultado.Status);
        }

        return Results.Ok(resultado.Valor);
    }

    private static IResult ResponderSessao(
        ResultadoIdentidade<SessaoResposta> resultado,
        HttpContext http,
        OpcoesJwt jwt)
    {
        if (!resultado.Sucesso || resultado.Valor is null)
        {
            return Results.Json(new { erros = resultado.Erros }, statusCode: resultado.Status);
        }

        http.Response.Cookies.Append(jwt.CookieRefresh, resultado.Valor.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = http.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(jwt.RefreshTokenDias),
            IsEssential = true,
            Path = "/api/conta"
        });

        return Results.Ok(new
        {
            accessToken = resultado.Valor.AccessToken,
            usuario = resultado.Valor.Usuario
        });
    }
}
