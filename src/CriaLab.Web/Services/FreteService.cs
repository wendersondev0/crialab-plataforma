using CriaLab.Web.Data;
using CriaLab.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Services;

public record FreteOpcao(string Id, string Nome, decimal Preco, int Dias);

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

        if (!int.TryParse(digitos[..8], out var n))
            return ("CEP inválido.", new List<FreteOpcao>(), freteGratis);

        var faixa = await _db.FaixasCep.AsNoTracking()
            .Where(x => x.De <= n && x.Ate >= n)
            .OrderBy(x => x.Ate - x.De)
            .FirstOrDefaultAsync();

        if (faixa == null)
            return ("Não entregamos neste CEP ainda.", new List<FreteOpcao>(), freteGratis);

        var gratis = subtotal >= freteGratis;
        var opcoes = new List<FreteOpcao>
        {
            new("pac", "PAC", gratis ? 0 : faixa.Pac, faixa.PacDias),
            new("sedex", "SEDEX", gratis ? 0 : faixa.Sedex, faixa.SedexDias)
        };
        return (null, opcoes, freteGratis);
    }

    public async Task<Cupom?> CupomPorCodigoAsync(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        var c = codigo.Trim().ToUpperInvariant();
        return await _db.Cupons.AsNoTracking().FirstOrDefaultAsync(x => x.Ativo && x.Codigo.ToUpper() == c);
    }
}
