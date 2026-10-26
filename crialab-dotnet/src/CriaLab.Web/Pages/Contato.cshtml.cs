using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages;
public class ContatoModel : PageModel {
  private readonly ApplicationDbContext _db;
  public ContatoModel(ApplicationDbContext db) => _db = db;
  public LojaIdentidade? Identidade { get; private set; }
  [BindProperty] public string Nome { get; set; } = "";
  [BindProperty] public string Email { get; set; } = "";
  [BindProperty] public string Tipo { get; set; } = "duvida";
  [BindProperty] public string Texto { get; set; } = "";
  public bool Enviado { get; set; }
  public async Task OnGetAsync() => Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
  public async Task<IActionResult> OnPostAsync() {
    Identidade = await _db.LojaIdentidades.AsNoTracking().FirstOrDefaultAsync();
    _db.MensagensContato.Add(new MensagemContato { Id = "msg-" + Guid.NewGuid().ToString("N")[..8], Nome = Nome, Email = Email, Tipo = Tipo, Texto = Texto, Status = "aberto", CriadoEm = DateTime.UtcNow });
    await _db.SaveChangesAsync();
    Enviado = true;
    return Page();
  }
}
