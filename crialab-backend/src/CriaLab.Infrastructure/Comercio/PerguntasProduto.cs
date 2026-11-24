using CriaLab.Application.Comercio;
using CriaLab.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class PerguntasProduto(ContextoAplicacao db) : IPerguntasProduto
{
    public Task<bool> ExisteProdutoAsync(string produtoId, CancellationToken cancelamento = default) =>
        db.Produtos.AsNoTracking().AnyAsync(x => x.Id == produtoId, cancelamento);

    public Task<string?> NomeUsuarioAsync(string usuarioId, CancellationToken cancelamento = default) =>
        db.Users.AsNoTracking()
            .Where(x => x.Id == usuarioId)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync(cancelamento);

    public async Task CriarAsync(PerguntaProdutoNova pergunta, CancellationToken cancelamento = default)
    {
        db.PerguntasProduto.Add(new PerguntaProduto
        {
            Id = pergunta.Id,
            ProdutoId = pergunta.ProdutoId,
            Autor = pergunta.Autor,
            Texto = pergunta.Texto,
            Resposta = "",
            CriadoEm = pergunta.CriadoEm
        });
        await db.SaveChangesAsync(cancelamento);
    }
}
