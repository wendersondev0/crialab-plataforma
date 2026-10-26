using Microsoft.AspNetCore.Identity;

namespace CriaLab.Web.Data;

public class ApplicationUser : IdentityUser
{
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public string? TelefoneContato { get; set; }
    public string? Documento { get; set; }
}
