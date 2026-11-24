using CriaLab.Application.Comercio;
using CriaLab.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Infrastructure.Comercio;

public sealed class Contatos(ContextoAplicacao db) : IContatos
{
    public async Task<CanalContato?> CanalAsync(CancellationToken cancelamento = default)
    {
        var identidade = await db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync(cancelamento);
        if (identidade is null)
            return null;
        return new CanalContato(identidade.Email, identidade.Telefone, identidade.Whatsapp, identidade.Endereco, identidade.Horario);
    }

    public async Task<ContaContato?> ContaAsync(string usuarioId, CancellationToken cancelamento = default)
    {
        var usuario = await db.Users.AsNoTracking()
            .Where(x => x.Id == usuarioId)
            .Select(x => new { x.NomeCompleto, x.Email })
            .FirstOrDefaultAsync(cancelamento);
        return usuario is null ? null : new ContaContato(usuario.NomeCompleto, usuario.Email ?? "");
    }

    public async Task GravarAsync(MensagemContatoNova mensagem, CancellationToken cancelamento = default)
    {
        db.MensagensContato.Add(new MensagemContato
        {
            Id = mensagem.Id,
            Nome = mensagem.Nome,
            Email = mensagem.Email,
            Tipo = mensagem.Tipo,
            Texto = mensagem.Texto,
            Status = mensagem.Status,
            CriadoEm = mensagem.CriadoEm
        });
        await db.SaveChangesAsync(cancelamento);
    }
}
