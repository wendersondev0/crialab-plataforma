using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class ImpressaoModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ImpressaoModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "mesa";
    public string? MaquinaFiltro { get; private set; }
    public string? StatusFiltro { get; private set; }
    public string? Q { get; private set; }
    public string? ConcluirId { get; private set; }
    public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
    public IList<ImpressaoJob> Jobs { get; private set; } = new List<ImpressaoJob>();
    public SelectList MaquinasSelect { get; private set; } = null!;
    public int NaFila { get; private set; }
    public int Imprimindo { get; private set; }
    public int Concluidas { get; private set; }
    public int Falhas { get; private set; }

    [BindProperty]
    public string NovoTitulo { get; set; } = "";

    [BindProperty]
    public string NovoMaquinaId { get; set; } = "";

    [BindProperty]
    public string? NovoProdutoId { get; set; }

    [BindProperty]
    public decimal NovoPeso { get; set; } = 40;

    [BindProperty]
    public int NovoMinutos { get; set; } = 180;

    public async Task OnGetAsync(string? aba, string? maquina, string? status, string? q, string? concluir)
    {
        Aba = aba is "fila" or "qualidade" or "historico" or "manutencao" ? aba : "mesa";
        MaquinaFiltro = maquina;
        StatusFiltro = status;
        Q = q;
        ConcluirId = concluir;
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostStatusAsync(string id, string status, string? aba, string? maquina, string? q)
    {
        var job = await _db.ImpressaoJobs.FirstOrDefaultAsync(x => x.Id == id);
        if (job != null && !string.IsNullOrWhiteSpace(status))
        {
            job.Status = status.Trim();
            if (status == "imprimindo" && job.Inicio == null)
                job.Inicio = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Status atualizado.";
        }
        return RedirectToPage(new { aba = aba ?? "fila", maquina, q });
    }

    public async Task<IActionResult> OnPostConcluirAsync(string id, bool sucesso, decimal pesoReal, string? motivo, decimal kwh, string? aba)
    {
        var job = await _db.ImpressaoJobs.FirstOrDefaultAsync(x => x.Id == id);
        if (job == null) return RedirectToPage(new { aba = aba ?? "fila" });

        job.Status = "concluida";
        job.Sucesso = sucesso;
        job.PesoReal = pesoReal;
        job.Motivo = sucesso ? "" : (string.IsNullOrWhiteSpace(motivo) ? "Falha" : motivo.Trim());
        job.Kwh = kwh > 0 ? kwh : job.Kwh;
        job.Fim ??= DateTime.UtcNow;

        var insumo = await _db.EstoqueInsumos.FirstOrDefaultAsync(x => x.Nome == "PLA");
        if (insumo != null && pesoReal > 0)
            insumo.Saldo = Math.Max(0, insumo.Saldo - pesoReal / 1000m);

        if (sucesso && !string.IsNullOrWhiteSpace(job.ProdutoId))
        {
            var produto = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == job.ProdutoId);
            if (produto != null && produto.Estoque > 0)
                produto.Estoque -= 1;
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = sucesso ? "Impressão concluída com sucesso." : "Falha registrada. Filamento baixado.";
        return RedirectToPage(new { aba = aba ?? "historico" });
    }

    public async Task<IActionResult> OnPostNovoAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoTitulo) || string.IsNullOrWhiteSpace(NovoMaquinaId))
            return RedirectToPage(new { aba = "fila" });

        var agora = DateTime.UtcNow;
        _db.ImpressaoJobs.Add(new ImpressaoJob
        {
            Id = "imp-" + Guid.NewGuid().ToString("N")[..8],
            Titulo = NovoTitulo.Trim(),
            MaquinaId = NovoMaquinaId,
            ProdutoId = NovoProdutoId ?? "",
            PesoPrevisto = NovoPeso,
            Status = "na fila",
            Inicio = agora,
            Fim = agora.AddMinutes(Math.Max(30, NovoMinutos))
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Trabalho adicionado à fila.";
        return RedirectToPage(new { aba = "fila" });
    }

    public string RotuloMaquina(string maquinaId)
    {
        var lista = Jobs.Where(x => x.MaquinaId == maquinaId).ToList();
        if (lista.Any(x => x.Status == "erro")) return "Com erro";
        if (lista.Any(x => x.Status == "pausada")) return "Pausada";
        if (lista.Any(x => x.Status == "imprimindo")) return "Imprimindo";
        if (lista.Any(x => x.Status == "na fila")) return "Na fila";
        return "Disponível";
    }

    public string ClasseRotulo(string txt) => txt switch
    {
        "Com erro" => "imp-selo alerta",
        "Pausada" => "imp-selo mute",
        "Imprimindo" or "Disponível" => "imp-selo ok",
        _ => "imp-selo"
    };

    private async Task CarregarAsync()
    {
        Maquinas = await _db.Maquinas.AsNoTracking().Where(x => x.ControlaHora).OrderBy(x => x.Nome).ToListAsync();
        MaquinasSelect = new SelectList(Maquinas, "Id", "Nome", NovoMaquinaId);

        var query = _db.ImpressaoJobs.AsNoTracking().Include(x => x.Maquina).AsQueryable();
        if (!string.IsNullOrWhiteSpace(MaquinaFiltro))
            query = query.Where(x => x.MaquinaId == MaquinaFiltro);
        if (!string.IsNullOrWhiteSpace(StatusFiltro))
            query = query.Where(x => x.Status == StatusFiltro);
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var t = Q.Trim().ToLower();
            query = query.Where(x => x.Titulo.ToLower().Contains(t) || x.ProdutoId.ToLower().Contains(t));
        }

        var todos = await _db.ImpressaoJobs.AsNoTracking().ToListAsync();
        NaFila = todos.Count(x => x.Status == "na fila");
        Imprimindo = todos.Count(x => x.Status == "imprimindo");
        Concluidas = todos.Count(x => x.Status == "concluida");
        Falhas = todos.Count(x => x.Status == "concluida" && x.Sucesso == false);

        var lista = await query.OrderByDescending(x => x.Inicio).ToListAsync();
        Jobs = Aba switch
        {
            "historico" => lista.Where(x => x.Status is "concluida" or "cancelada").ToList(),
            "qualidade" => lista.Where(x => x.Status == "concluida").ToList(),
            "fila" => lista.Where(x => x.Status is "na fila" or "imprimindo" or "pausada" or "erro").ToList(),
            "manutencao" => lista.Where(x => x.Status is "pausada" or "erro").ToList(),
            _ => lista
        };
    }
}
