using CriaLab.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public int ProntaEntrega { get; private set; }
    public int SobDemanda { get; private set; }
    public int Personalizados { get; private set; }
    public int MensagensAbertas { get; private set; }

    public async Task OnGetAsync()
    {
        var publicados = _db.Produtos.AsNoTracking().Where(x => x.Situacao == "publicado");
        ProntaEntrega = await publicados.CountAsync(x => x.ModalidadeId == "pronta-entrega");
        SobDemanda = await publicados.CountAsync(x => x.ModalidadeId == "sob-demanda");
        Personalizados = await publicados.CountAsync(x => x.ModalidadeId == "personalizado");
        MensagensAbertas = await _db.MensagensContato.AsNoTracking().CountAsync(x => x.Status == "aberto");
    }
}
