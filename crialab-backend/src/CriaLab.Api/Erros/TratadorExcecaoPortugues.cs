using Microsoft.AspNetCore.Diagnostics;

namespace CriaLab.Api.Erros;

public sealed class TratadorExcecaoPortugues(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno",
                Detail = "Ocorreu um erro inesperado.",
                Type = "about:blank"
            }
        });
    }
}
