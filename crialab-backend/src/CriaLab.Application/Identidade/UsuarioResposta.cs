namespace CriaLab.Application.Identidade;

public sealed record UsuarioResposta(
    string Id,
    string Email,
    string NomeCompleto,
    string? TelefoneContato,
    IReadOnlyList<string> Papeis);
