using CriaLab.Domain.Entidades;

namespace CriaLab.Domain.Calculos;

public record FreteOpcao(string Id, string Nome, decimal Preco, int Dias, string? Descricao = null);

public static class FreteCalculo
{
    public static (string? Erro, IList<FreteOpcao> Opcoes, decimal FreteGratisAPartir) Cotar(
        string? cep,
        decimal subtotal,
        IEnumerable<FaixaCep> faixasCadastradas,
        decimal freteGratisAPartir = 200m)
    {
        var freteGratis = freteGratisAPartir;
        var digitos = new string((cep ?? "").Where(char.IsDigit).ToArray());
        if (digitos.Length < 8)
            return ("Informe um CEP válido com 8 dígitos.", new List<FreteOpcao>(), freteGratis);

        var prefixo = Prefixo(digitos);
        if (prefixo < 0)
            return ("CEP inválido.", new List<FreteOpcao>(), freteGratis);

        var faixas = faixasCadastradas
            .Where(x => x.De <= prefixo && x.Ate >= prefixo)
            .OrderBy(x => x.Ate - x.De)
            .ToList();

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

    public static Cupom? CupomPorCodigo(string? codigo, IEnumerable<Cupom> cupons)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        var c = codigo.Trim().ToUpperInvariant();
        return cupons.FirstOrDefault(x => x.Ativo && x.Codigo.ToUpperInvariant() == c);
    }

    public static decimal DescontoCupom(decimal subtotal, Cupom? cupom)
    {
        if (cupom == null) return 0;
        return Math.Round(subtotal * cupom.Percentual, 2, MidpointRounding.AwayFromZero);
    }
}
