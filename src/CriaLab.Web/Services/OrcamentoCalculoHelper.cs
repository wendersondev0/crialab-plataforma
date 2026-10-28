using CriaLab.Web.Data.Entities;

namespace CriaLab.Web.Services;

public static class OrcamentoCalculoHelper
{
    public sealed class ItemEntrada
    {
        public decimal Quantidade { get; set; } = 1;
        public decimal Preco { get; set; }
    }

    public sealed class ResultadoOrcamento
    {
        public decimal Filamento { get; set; }
        public decimal Tempo { get; set; }
        public decimal TaxaMin { get; set; }
        public decimal Parametros { get; set; }
        public decimal CustoBase { get; set; }
        public decimal Custo { get; set; }
        public decimal Lucro { get; set; }
        public decimal Reserva { get; set; }
        public decimal Original { get; set; }
        public decimal Direto { get; set; }
        public int Horas { get; set; }
        public int Dias { get; set; }
    }

    public sealed class ResultadoCanal
    {
        public decimal? Preco { get; set; }
        public decimal Comissao { get; set; }
        public decimal? Liquido { get; set; }
        public string Aviso { get; set; } = "";
        public bool FaixasVazias { get; set; }
    }

    public static decimal Centavos(decimal n) =>
        Math.Round(n, 2, MidpointRounding.AwayFromZero);

    public static decimal ArredondarCimaReal(decimal n)
    {
        var ajustado = Math.Round(n * 1000m, MidpointRounding.AwayFromZero) / 1000m;
        return Math.Ceiling(ajustado - 0.000000001m);
    }

    public static ResultadoOrcamento Orcar(
        decimal precoKg,
        decimal pesoG,
        decimal lixoG,
        decimal energiaPorMinuto,
        decimal depreciacao,
        decimal manutencao,
        int minutos,
        decimal servico,
        decimal margem,
        decimal taxaFalha,
        decimal desconto,
        IEnumerable<ItemEntrada>? itens)
    {
        var peso = pesoG + lixoG;
        var filamento = (precoKg / 1000m) * peso;
        var taxaMin = energiaPorMinuto + depreciacao + manutencao;
        var tempo = minutos * taxaMin;
        var parametros = (itens ?? Enumerable.Empty<ItemEntrada>()).Sum(i => i.Quantidade * i.Preco);
        var custoBase = servico + filamento + tempo + parametros;
        var custo = custoBase;
        var lucro = custo * margem;
        var reserva = custo * taxaFalha;
        var original = custo + lucro + reserva;
        var horas = (int)Math.Ceiling(minutos / 60m - 0.000000001m);
        var dias = (int)Math.Ceiling(horas / 24m - 0.000000001m);
        return new ResultadoOrcamento
        {
            Filamento = filamento,
            Tempo = tempo,
            TaxaMin = taxaMin,
            Parametros = parametros,
            CustoBase = custoBase,
            Custo = custo,
            Lucro = lucro,
            Reserva = reserva,
            Original = original,
            Direto = ArredondarCimaReal(original * (1 - desconto)),
            Horas = Math.Max(0, horas),
            Dias = Math.Max(0, dias)
        };
    }

    public static ResultadoCanal PrecoCanal(decimal valorOriginal, IList<FaixaMarketplace> faixas, decimal freteVendedor)
    {
        if (faixas == null || faixas.Count == 0)
            return new ResultadoCanal { FaixasVazias = true };

        var candidatos = faixas.Select(f =>
        {
            var preco = CandidatoFaixa(valorOriginal, f);
            return new { Faixa = f, Preco = preco, Dentro = preco >= f.De && preco <= f.Ate + 0.001m };
        }).ToList();

        var escolhido = candidatos.Where(c => c.Dentro).OrderBy(c => c.Preco).FirstOrDefault();
        var aviso = "";
        var liquido7999 = Centavos(79.99m - ComissaoNoPreco(79.99m, faixas));
        var liquido80 = Centavos(80m - ComissaoNoPreco(80m, faixas));
        var cru = candidatos.FirstOrDefault(c => c.Faixa.Ate <= 79.99m);
        if (cru != null && cru.Preco >= 80m && liquido7999 > liquido80)
            aviso = "De R$ 79,99 para R$ 80,00 o líquido cai.";

        if (escolhido == null)
            return new ResultadoCanal { Aviso = string.IsNullOrEmpty(aviso) ? "Nenhum candidato caiu dentro da própria faixa." : aviso };

        var comissao = ComissaoNoPreco(escolhido.Preco, faixas);
        return new ResultadoCanal
        {
            Preco = escolhido.Preco,
            Comissao = comissao,
            Liquido = Centavos(escolhido.Preco - comissao - freteVendedor),
            Aviso = aviso
        };
    }

    private static decimal CandidatoFaixa(decimal valorOriginal, FaixaMarketplace faixa)
    {
        var preco = (valorOriginal + faixa.Fixo) / Math.Max(0.01m, 1 - faixa.Percentual);
        if (preco < 9m)
        {
            var denom = 1 - faixa.Percentual - 0.5m;
            if (denom > 0) preco = valorOriginal / denom;
        }
        return Centavos(preco);
    }

    private static decimal ComissaoNoPreco(decimal preco, IList<FaixaMarketplace> faixas)
    {
        var faixa = faixas.FirstOrDefault(f => preco >= f.De && preco <= f.Ate + 0.001m);
        if (faixa == null) return 0;
        var fixo = preco < 9m ? preco / 2m : faixa.Fixo;
        return Centavos(preco * faixa.Percentual + fixo);
    }
}
