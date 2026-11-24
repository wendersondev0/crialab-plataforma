using CriaLab.Domain.Arquivos;
using Microsoft.AspNetCore.Hosting;

namespace CriaLab.Infrastructure.Arquivos;

public sealed class GravadorImagem(IWebHostEnvironment ambiente)
{
    public Task<string> GravarFotoAsync(Stream conteudo, string nomeOriginal, CancellationToken cancelamento = default)
    {
        var ext = RegrasImagem.ExtensaoSegura(nomeOriginal);
        var pastaRel = Path.Combine("fotos", DateTime.UtcNow.ToString("yyyy-MM-dd"));
        var nome = Guid.NewGuid().ToString("N")[..12] + ext;
        return GravarAsync(conteudo, pastaRel, nome, cancelamento);
    }

    public Task<string> GravarLogoAsync(Stream conteudo, string id, string nomeOriginal, CancellationToken cancelamento = default)
    {
        var ext = RegrasImagem.ExtensaoSegura(nomeOriginal, ".png");
        return GravarAsync(conteudo, "parceiros", id + ext, cancelamento);
    }

    private async Task<string> GravarAsync(Stream conteudo, string pastaRel, string nome, CancellationToken cancelamento)
    {
        var raiz = RaizStorage.Resolver(ambiente.ContentRootPath);
        var pastaAbs = Path.Combine(raiz, pastaRel);
        Directory.CreateDirectory(pastaAbs);
        var destino = Path.Combine(pastaAbs, nome);
        await using var arquivo = File.Create(destino);
        await conteudo.CopyToAsync(arquivo, cancelamento);
        return Path.Combine(pastaRel, nome).Replace('\\', '/');
    }
}
