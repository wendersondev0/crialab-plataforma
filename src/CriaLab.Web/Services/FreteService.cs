using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Services;

public record FreteOpcao(string Id, string Nome, decimal Preco, int Dias, string? Descricao = null);

public class FreteService
{
    private readonly ApplicationDbContext _db;

    public FreteService(ApplicationDbContext db) => _db = db;

    public async Task<(string? Erro, IList<FreteOpcao> Opcoes, decimal FreteGratisAPartir)> CotarAsync(string? cep, decimal subtotal)
    {
        var cfg = await _db.ConfiguracoesLoja.AsNoTracking().FirstOrDefaultAsync();
        var freteGratis = cfg?.FreteGratisAPartir ?? 200m;
        var digitos = new string((cep ?? "").Where(char.IsDigit).ToArray());
        if (digitos.Length < 8)
            return ("Informe um CEP válido com 8 dígitos.", new List<FreteOpcao>(), freteGratis);

        var prefixo = Prefixo(digitos);
        if (prefixo < 0)
            return ("CEP inválido.", new List<FreteOpcao>(), freteGratis);

        var faixas = await _db.FaixasCep.AsNoTracking()
            .Where(x => x.De <= prefixo && x.Ate >= prefixo)
            .OrderBy(x => x.Ate - x.De)
            .ToListAsync();

        if (faixas.Count == 0)
            return ("Não entregamos neste CEP ainda.", new List<FreteOpcao>(), freteGratis);

        var faixa = faixas[0];
        var gratis = subtotal >= freteGratis;
        var opcoes = new List<FreteOpcao>
        {
            new("pac", "PAC", gratis ? 0 : faixa.Pac, faixa.PacDias),
            new("sedex", "SEDEX", gratis ? 0 : faixa.Sedex, faixa.SedexDias)
        };

        var local = faixas.FirstOrDefault(x => x.EntregaLocal);
        if (local != null)
        {
            opcoes.Add(new FreteOpcao(
                "entrega-local",
                "Entrega Local",
                gratis ? 0 : local.EntregaLocalPreco,
                local.EntregaLocalDias <= 0 ? 1 : local.EntregaLocalDias,
                string.IsNullOrWhiteSpace(local.EntregaLocalDescricao) ? null : local.EntregaLocalDescricao.Trim()));
        }

        return (null, opcoes, freteGratis);
    }

    public static int Prefixo(string digitos)
    {
        if (!int.TryParse(digitos.Length > 8 ? digitos[..8] : digitos, out var n))
            return -1;
        return n >= 1_000_000 ? n / 1000 : n;
    }

    public async Task<Cupom?> CupomPorCodigoAsync(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        var c = codigo.Trim().ToUpperInvariant();
        return await _db.Cupons.AsNoTracking().FirstOrDefaultAsync(x => x.Ativo && x.Codigo.ToUpper() == c);
    }
}
