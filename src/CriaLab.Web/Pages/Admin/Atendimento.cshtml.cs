using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class AtendimentoModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public AtendimentoModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "mensagens";
    public string? StatusFiltro { get; private set; }
    public string? Q { get; private set; }
    public IList<MensagemContato> Mensagens { get; private set; } = new List<MensagemContato>();
    public IList<PerguntaProduto> Perguntas { get; private set; } = new List<PerguntaProduto>();
    public IList<Depoimento> Depoimentos { get; private set; } = new List<Depoimento>();
    public int TotalMensagens { get; private set; }
    public int Abertas { get; private set; }
    public int EmAtendimento { get; private set; }
    public int PerguntasPendentes { get; private set; }

    public async Task OnGetAsync(string? aba, string? status, string? q)
    {
        Aba = aba is "perguntas" or "depoimentos" ? aba : "mensagens";
        StatusFiltro = status;
        Q = q;
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostStatusMensagemAsync(string id, string status, string? aba)
    {
        var msg = await _db.MensagensContato.FirstOrDefaultAsync(x => x.Id == id);
        if (msg != null && !string.IsNullOrWhiteSpace(status))
        {
            msg.Status = status.Trim();
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Status da mensagem atualizado.";
        }
        return RedirectToPage(new { aba = "mensagens" });
    }

    public async Task<IActionResult> OnPostResponderPerguntaAsync(string id, string resposta)
    {
        var p = await _db.PerguntasProduto.FirstOrDefaultAsync(x => x.Id == id);
        if (p != null)
        {
            p.Resposta = resposta?.Trim() ?? "";
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Resposta publicada.";
        }
        return RedirectToPage(new { aba = "perguntas" });
    }

    public async Task<IActionResult> OnPostDepoimentoAsync(string? id, string autor, string texto, bool podeAparecer)
    {
        if (string.IsNullOrWhiteSpace(autor) || string.IsNullOrWhiteSpace(texto))
            return RedirectToPage(new { aba = "depoimentos" });

        if (string.IsNullOrWhiteSpace(id))
        {
            _db.Depoimentos.Add(new Depoimento
            {
                Id = "dep-" + Guid.NewGuid().ToString("N")[..8],
                Autor = autor.Trim(),
                Texto = texto.Trim(),
                PodeAparecer = podeAparecer
            });
        }
        else
        {
            var d = await _db.Depoimentos.FirstOrDefaultAsync(x => x.Id == id);
            if (d != null)
            {
                d.Autor = autor.Trim();
                d.Texto = texto.Trim();
                d.PodeAparecer = podeAparecer;
            }
        }
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Depoimento salvo.";
        return RedirectToPage(new { aba = "depoimentos" });
    }

    public async Task<IActionResult> OnPostToggleDepoimentoAsync(string id)
    {
        var d = await _db.Depoimentos.FirstOrDefaultAsync(x => x.Id == id);
        if (d != null)
        {
            d.PodeAparecer = !d.PodeAparecer;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { aba = "depoimentos" });
    }

    public async Task<IActionResult> OnPostRemoverDepoimentoAsync(string id)
    {
        var d = await _db.Depoimentos.FirstOrDefaultAsync(x => x.Id == id);
        if (d != null)
        {
            _db.Depoimentos.Remove(d);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Depoimento removido.";
        }
        return RedirectToPage(new { aba = "depoimentos" });
    }

    private async Task CarregarAsync()
    {
        var msgs = await _db.MensagensContato.AsNoTracking().OrderByDescending(x => x.CriadoEm).ToListAsync();
        TotalMensagens = msgs.Count;
        Abertas = msgs.Count(x => x.Status == "aberto");
        EmAtendimento = msgs.Count(x => x.Status == "em atendimento");

        var pergs = await _db.PerguntasProduto.AsNoTracking().OrderByDescending(x => x.CriadoEm).ToListAsync();
        PerguntasPendentes = pergs.Count(x => string.IsNullOrWhiteSpace(x.Resposta));

        if (!string.IsNullOrWhiteSpace(StatusFiltro))
            msgs = msgs.Where(x => x.Status == StatusFiltro).ToList();
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var t = Q.Trim().ToLowerInvariant();
            msgs = msgs.Where(x =>
                x.Nome.ToLower().Contains(t) ||
                x.Email.ToLower().Contains(t) ||
                x.Texto.ToLower().Contains(t)).ToList();
            pergs = pergs.Where(x =>
                x.Autor.ToLower().Contains(t) ||
                x.Texto.ToLower().Contains(t) ||
                x.ProdutoId.ToLower().Contains(t)).ToList();
        }

        Mensagens = msgs;
        Perguntas = pergs;
        Depoimentos = await _db.Depoimentos.AsNoTracking().OrderBy(x => x.Autor).ToListAsync();
    }
}
