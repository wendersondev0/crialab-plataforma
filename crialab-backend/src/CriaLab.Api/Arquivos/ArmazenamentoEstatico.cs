using CriaLab.Infrastructure.Arquivos;
using Microsoft.Extensions.FileProviders;

namespace CriaLab.Api.Arquivos;

public static class ArmazenamentoEstatico
{
    public static readonly string[] PastasPublicas = ["fotos", "parceiros", "marca"];

    public static void Usar(WebApplication app)
    {
        var raiz = RaizStorage.Resolver(app.Environment.ContentRootPath);
        foreach (var pasta in PastasPublicas)
        {
            var caminho = Path.Combine(raiz, pasta);
            Directory.CreateDirectory(caminho);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(caminho),
                RequestPath = "/" + pasta
            });
        }
    }
}
