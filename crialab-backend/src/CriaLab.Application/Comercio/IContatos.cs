namespace CriaLab.Application.Comercio;

public sealed record CanalContato(
    string? Email,
    string? Telefone,
    string? Whatsapp,
    string? Endereco,
    string? Horario);

public sealed record ContaContato(string Nome, string Email);

public sealed record MensagemContatoNova(
    string Id,
    string Nome,
    string Email,
    string Tipo,
    string Texto,
    string Status,
    DateTime CriadoEm);

public interface IContatos
{
    Task<CanalContato?> CanalAsync(CancellationToken cancelamento = default);

    Task<ContaContato?> ContaAsync(string usuarioId, CancellationToken cancelamento = default);

    Task GravarAsync(MensagemContatoNova mensagem, CancellationToken cancelamento = default);
}
