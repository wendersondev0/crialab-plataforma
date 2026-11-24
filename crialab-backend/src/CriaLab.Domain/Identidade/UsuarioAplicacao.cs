using Microsoft.AspNetCore.Identity;

namespace CriaLab.Domain.Identidade;

public class UsuarioAplicacao : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? TelefoneContato { get; set; }
    public string? Documento { get; set; }
}
