using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace CriaLab.Web.Pages.Admin;
[Authorize(Policy = "AdminOnly")]
public class AtendimentoModel : PageModel {
  private readonly ApplicationDbContext _db;
  public AtendimentoModel(ApplicationDbContext db) => _db = db;
  public IList<MensagemContato> Mensagens { get; private set; } = new List<MensagemContato>();
  public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();
  public async Task OnGetAsync() {
    Mensagens = await _db.MensagensContato.AsNoTracking().OrderByDescending(x => x.CriadoEm).ToListAsync();
    Perguntas = await _db.PerguntasProduto.AsNoTracking().OrderByDescending(x => x.CriadoEm).Take(20).ToListAsync();
  }
}