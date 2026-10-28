using System.Globalization;
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
    private static readonly HashSet<string> AbasOk = new(StringComparer.OrdinalIgnoreCase)
        { "mesa", "fila", "qualidade", "historico", "manutencao" };

    private readonly ApplicationDbContext _db;

    public ImpressaoModel(ApplicationDbContext db) => _db = db;

    public string Aba { get; private set; } = "mesa";
    public string? MaquinaFiltro { get; private set; }
    public string? StatusFiltro { get; private set; }
    public string? Q { get; private set; }
    public string? ConcluirId { get; private set; }

    public IList<Maquina> Maquinas { get; private set; } = new List<Maquina>();
    public IList<ImpressaoJob> TodosJobs { get; private set; } = new List<ImpressaoJob>();
    public IList<ImpressaoJob> Jobs { get; private set; } = new List<ImpressaoJob>();
    public IList<Produto> Produtos { get; private set; } = new List<Produto>();
    public SelectList MaquinasSelect { get; private set; } = null!;
    public Dictionary<string, QualidadeMaquinaVm> QualidadePorMaquina { get; private set; } = new();
    public HashSet<string> JobsEmChoque { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public int NaFila { get; private set; }
    public int Imprimindo { get; private set; }
    public int TotalTrabalhos { get; private set; }
    public int Choques { get; private set; }
    public int ConcluidasTotal { get; private set; }
    public int FalhasTotal { get; private set; }
    public decimal TaxaFalha { get; private set; }
    public decimal HorasConcluidas { get; private set; }
    public decimal GramasConcluidas { get; private set; }

    [BindProperty] public string NovoMaquinaId { get; set; } = "";
    [BindProperty] public string? NovoProdutoId { get; set; }
    [BindProperty] public decimal NovoPeso { get; set; } = 40;
    [BindProperty] public string NovoInicio { get; set; } = "";
    [BindProperty] public string NovoFim { get; set; } = "";

    [BindProperty] public string ManutTipo { get; set; } = "Bico";
    [BindProperty] public string? ManutMaquinaId { get; set; }
    [BindProperty] public decimal ManutValor { get; set; }

    public async Task OnGetAsync(string? aba, string? maquina, string? status, string? q, string? concluir)
    {
        Aba = !string.IsNullOrWhiteSpace(aba) && AbasOk.Contains(aba) ? aba.Trim().ToLowerInvariant() : "mesa";
        MaquinaFiltro = maquina;
        StatusFiltro = status;
        Q = q;
        ConcluirId = concluir;
        await CarregarAsync();
    }

    public async Task<IActionResult> OnPostStatusAsync(string id, string status, string? aba, string? maquina, string? statusFiltro, string? q)
    {
        var job = await _db.ImpressaoJobs.FirstOrDefaultAsync(x => x.Id == id);
        if (job != null && !string.IsNullOrWhiteSpace(status))
        {
            job.Status = status.Trim();
            if (status == "imprimindo" && job.Inicio == null)
                job.Inicio = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Ok"] = status == "cancelada" ? "Trabalho cancelado." : "Status atualizado.";
        }
        return RedirectToPage(new { aba = aba ?? "fila", maquina, status = statusFiltro, q });
    }

    public async Task<IActionResult> OnPostConcluirAsync(
        string id, string sucesso, decimal peso, string? motivo, decimal kwh,
        string? aba, string? maquina, string? status, string? q)
    {
        var job = await _db.ImpressaoJobs.FirstOrDefaultAsync(x => x.Id == id);
        if (job == null)
            return RedirectToPage(new { aba = aba ?? "fila", maquina, status, q });

        var ok = sucesso is "1" or "true" or "True";
        job.Status = "concluida";
        job.Sucesso = ok;
        job.PesoReal = peso;
        job.Motivo = ok ? "" : (string.IsNullOrWhiteSpace(motivo) ? "Falha" : motivo.Trim());
        job.Kwh = kwh > 0 ? kwh : job.Kwh;
        job.Fim ??= DateTime.UtcNow;

        var insumo = await _db.EstoqueInsumos.FirstOrDefaultAsync(x => x.Nome == "PLA");
        if (insumo != null && peso > 0)
            insumo.Saldo = Math.Max(0, insumo.Saldo - peso / 1000m);

        if (ok && !string.IsNullOrWhiteSpace(job.ProdutoId))
        {
            var produto = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == job.ProdutoId);
            if (produto != null && produto.Estoque > 0)
                produto.Estoque -= 1;
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = ok ? "Impressão concluída com sucesso." : "Falha registrada. Filamento baixado.";
        return RedirectToPage(new { aba = "historico", maquina, q });
    }

    public async Task<IActionResult> OnPostNovoAsync(string? aba, string? maquina, string? status, string? q)
    {
        if (string.IsNullOrWhiteSpace(NovoMaquinaId))
            return RedirectToPage(new { aba = "fila", maquina, status, q });

        if (!TryParseLocal(NovoInicio, out var inicio) || !TryParseLocal(NovoFim, out var fim) || fim <= inicio)
        {
            TempData["Ok"] = "O fim precisa ser depois do início.";
            return RedirectToPage(new { aba = "fila", maquina, status, q });
        }

        string titulo = "Peça avulsa";
        var produtoId = NovoProdutoId?.Trim() ?? "";
        if (!string.IsNullOrWhiteSpace(produtoId))
        {
            var produto = await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == produtoId);
            if (produto != null) titulo = produto.Titulo;
        }

        _db.ImpressaoJobs.Add(new ImpressaoJob
        {
            Id = "imp-" + Guid.NewGuid().ToString("N")[..8],
            Titulo = titulo,
            MaquinaId = NovoMaquinaId,
            ProdutoId = produtoId,
            PesoPrevisto = NovoPeso,
            Status = "na fila",
            Inicio = inicio,
            Fim = fim
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Trabalho colocado na fila.";
        return RedirectToPage(new { aba = "fila", status = "na fila", maquina, q });
    }

    public async Task<IActionResult> OnPostCopiarTaxaAsync()
    {
        var todos = await _db.ImpressaoJobs.AsNoTracking().Where(x => x.Status == "concluida").ToListAsync();
        var falhas = todos.Count(x => x.Sucesso == false);
        var taxa = todos.Count == 0 ? 0 : (decimal)falhas / todos.Count;
        var cfg = await _db.ConfiguracoesLoja.FirstOrDefaultAsync();
        if (cfg != null)
        {
            cfg.TaxaFalha = Math.Round(taxa, 4);
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Taxa de falha copiada para o Sistema.";
        }
        return RedirectToPage(new { aba = "qualidade" });
    }

    public async Task<IActionResult> OnPostManutencaoAsync()
    {
        if (ManutValor < 0)
            return RedirectToPage(new { aba = "manutencao" });

        string? nomeMaq = null;
        if (!string.IsNullOrWhiteSpace(ManutMaquinaId))
        {
            var maq = await _db.Maquinas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ManutMaquinaId);
            nomeMaq = maq?.Nome;
        }

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        _db.LancamentosFinanceiros.Add(new LancamentoFinanceiro
        {
            Id = "lan-" + Guid.NewGuid().ToString("N")[..8],
            Data = hoje,
            Competencia = hoje.ToString("yyyy-MM"),
            Descricao = "Manutenção " + ManutTipo + (string.IsNullOrWhiteSpace(nomeMaq) ? "" : " · " + nomeMaq),
            Categoria = "gastoFixo",
            Quantidade = 1,
            Valor = ManutValor,
            Origem = "manutencao"
        });
        await _db.SaveChangesAsync();
        TempData["Ok"] = "Despesa de manutenção lançada no Financeiro.";
        return RedirectToPage(new { aba = "manutencao" });
    }

    public string RotuloMaquina(string maquinaId)
    {
        var lista = TodosJobs.Where(x => x.MaquinaId == maquinaId).ToList();
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

    public string SeloStatus(ImpressaoJob item)
    {
        if (item.Status == "concluida" && item.Sucesso == false) return "imp-selo alerta";
        if (item.Status == "concluida") return "imp-selo ok";
        if (item.Status == "imprimindo") return "imp-selo ok";
        if (item.Status is "pausada" or "cancelada") return "imp-selo mute";
        if (item.Status == "erro") return "imp-selo alerta";
        return "imp-selo";
    }

    public string TextoStatus(ImpressaoJob item) =>
        item.Status == "concluida"
            ? (item.Sucesso == false ? "falha" : "sucesso")
            : item.Status;

    public static (int Progresso, string Restante) Andamento(ImpressaoJob item)
    {
        if (item.Inicio == null || item.Fim == null || item.Fim <= item.Inicio)
            return (0, "sem previsão");
        var ini = item.Inicio.Value;
        var fim = item.Fim.Value;
        var agora = DateTime.UtcNow;
        var progresso = agora <= ini ? 0 : agora >= fim ? 100 : (int)Math.Round((agora - ini).TotalMinutes / (fim - ini).TotalMinutes * 100);
        var minutos = Math.Max(0, (int)Math.Round((fim - agora).TotalMinutes));
        return (progresso, minutos > 0 ? minutos + " min" : "no prazo");
    }

    public static string FmtQuando(DateTime? utc)
    {
        if (utc == null) return "—";
        var local = utc.Value.Kind == DateTimeKind.Utc ? utc.Value.ToLocalTime() : utc.Value;
        return local.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    public static bool TemChoque(IEnumerable<ImpressaoJob> impressoes, ImpressaoJob atual)
    {
        if (atual.Inicio == null || atual.Fim == null || atual.Status is "cancelada" or "concluida")
            return false;
        return impressoes.Any(item =>
            item.Id != atual.Id
            && item.MaquinaId == atual.MaquinaId
            && item.Status is not ("cancelada" or "concluida")
            && item.Inicio != null
            && item.Fim != null
            && item.Inicio < atual.Fim
            && atual.Inicio < item.Fim);
    }

    public IList<ImpressaoJob> JobsDaMaquinaMesa(string maquinaId) =>
        TodosJobs
            .Where(x => x.MaquinaId == maquinaId && x.Status is "imprimindo" or "pausada" or "erro" or "na fila")
            .OrderBy(x => x.Status switch
            {
                "imprimindo" => 0,
                "pausada" => 1,
                "erro" => 2,
                "na fila" => 3,
                _ => 9
            })
            .ThenBy(x => x.Inicio)
            .ToList();

    public (decimal Horas, decimal Gramas, int Qtd) StatsMaquina(string maquinaId)
    {
        var lista = TodosJobs.Where(x => x.MaquinaId == maquinaId && x.Status == "concluida").ToList();
        decimal h = 0, g = 0;
        foreach (var item in lista)
        {
            if (item.Inicio != null && item.Fim != null && item.Fim > item.Inicio)
                h += (decimal)(item.Fim.Value - item.Inicio.Value).TotalHours;
            g += item.PesoReal > 0 ? item.PesoReal : 0;
        }
        return (h, g, lista.Count);
    }

    private async Task CarregarAsync()
    {
        Maquinas = await _db.Maquinas.AsNoTracking().Where(x => x.ControlaHora).OrderBy(x => x.Nome).ToListAsync();
        MaquinasSelect = new SelectList(Maquinas, "Id", "Nome", NovoMaquinaId);
        Produtos = await _db.Produtos.AsNoTracking().OrderBy(x => x.Titulo).Take(80).ToListAsync();

        TodosJobs = await _db.ImpressaoJobs.AsNoTracking().Include(x => x.Maquina)
            .OrderByDescending(x => x.Inicio)
            .ToListAsync();

        NaFila = TodosJobs.Count(x => x.Status == "na fila");
        Imprimindo = TodosJobs.Count(x => x.Status == "imprimindo");
        TotalTrabalhos = TodosJobs.Count;
        ConcluidasTotal = TodosJobs.Count(x => x.Status == "concluida");
        FalhasTotal = TodosJobs.Count(x => x.Status == "concluida" && x.Sucesso == false);
        TaxaFalha = ConcluidasTotal == 0 ? 0 : (decimal)FalhasTotal / ConcluidasTotal;

        JobsEmChoque = TodosJobs
            .Where(x => x.Status is not ("concluida" or "cancelada") && TemChoque(TodosJobs, x))
            .Select(x => x.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Choques = JobsEmChoque.Count;

        HorasConcluidas = 0;
        GramasConcluidas = 0;
        foreach (var item in TodosJobs.Where(x => x.Status == "concluida"))
        {
            if (item.Inicio != null && item.Fim != null && item.Fim > item.Inicio)
                HorasConcluidas += (decimal)(item.Fim.Value - item.Inicio.Value).TotalHours;
            GramasConcluidas += item.PesoReal > 0 ? item.PesoReal : item.PesoPrevisto;
        }

        QualidadePorMaquina = Maquinas.ToDictionary(
            m => m.Id,
            m =>
            {
                var lista = TodosJobs.Where(x => x.MaquinaId == m.Id && x.Status == "concluida").ToList();
                var ok = lista.Count(x => x.Sucesso != false);
                var falha = lista.Count(x => x.Sucesso == false);
                var motivos = lista.Where(x => x.Sucesso == false && !string.IsNullOrWhiteSpace(x.Motivo)).Select(x => x.Motivo).ToList();
                return new QualidadeMaquinaVm(ok, falha, motivos);
            });

        IEnumerable<ImpressaoJob> query = TodosJobs;
        if (!string.IsNullOrWhiteSpace(MaquinaFiltro))
            query = query.Where(x => x.MaquinaId == MaquinaFiltro);
        if (!string.IsNullOrWhiteSpace(StatusFiltro))
            query = query.Where(x => x.Status == StatusFiltro);
        if (!string.IsNullOrWhiteSpace(Q))
        {
            var t = Q.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Titulo.ToLower().Contains(t) ||
                (x.Maquina?.Nome ?? "").ToLower().Contains(t) ||
                x.Status.ToLower().Contains(t));
        }

        Jobs = Aba switch
        {
            "historico" => query.Where(x => x.Status == "concluida").OrderByDescending(x => x.Fim).ToList(),
            "qualidade" => new List<ImpressaoJob>(),
            "manutencao" => new List<ImpressaoJob>(),
            "mesa" => new List<ImpressaoJob>(),
            "fila" => query.OrderByDescending(x => x.Inicio).ToList(),
            _ => query.ToList()
        };

        if (string.IsNullOrWhiteSpace(NovoInicio))
        {
            var local = DateTime.Now;
            NovoInicio = local.ToString("yyyy-MM-ddTHH:mm");
            NovoFim = local.AddHours(3).ToString("yyyy-MM-ddTHH:mm");
        }
    }

    private static bool TryParseLocal(string? valor, out DateTime utc)
    {
        utc = default;
        if (string.IsNullOrWhiteSpace(valor)) return false;
        if (!DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var local) &&
            !DateTime.TryParse(valor, out local))
            return false;
        utc = DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();
        return true;
    }

    public sealed record QualidadeMaquinaVm(int Ok, int Falha, IList<string> Motivos)
    {
        public int Total => Ok + Falha;
        public int OkPct => Total == 0 ? 0 : (int)Math.Round(Ok * 100.0 / Total);
    }
}
