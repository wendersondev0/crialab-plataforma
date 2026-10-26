using System.Globalization;
using System.Text.Json;
using CriaLab.Web.Data.Entities;

namespace CriaLab.Web.Data.Seeding;

public static class PlanilhaSeedLoader
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public sealed class LancamentoDto
    {
        public string Id { get; set; } = "";
        public string Data { get; set; } = "";
        public string Competencia { get; set; } = "";
        public string Descricao { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string TipoInsumo { get; set; } = "";
        public decimal Quantidade { get; set; }
        public decimal Valor { get; set; }
        public string Origem { get; set; } = "planilha";
    }

    public sealed class OrcamentoDto
    {
        public string Id { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Material { get; set; } = "PLA";
        public int Minutos { get; set; }
        public decimal PesoG { get; set; }
        public decimal MargemSite { get; set; } = 0.4m;
        public decimal PrecoKg { get; set; } = 85;
        public bool Enviado { get; set; } = true;
        public List<ItemDto> Itens { get; set; } = new();
    }

    public sealed class ItemDto
    {
        public string Nome { get; set; } = "";
        public decimal Quantidade { get; set; }
        public decimal Preco { get; set; }
    }

    public sealed class EnergiaDto
    {
        public string Competencia { get; set; } = "";
        public string Descricao { get; set; } = "";
        public decimal Valor { get; set; }
    }

    public static string Pasta(IWebHostEnvironment env) =>
        Path.Combine(env.ContentRootPath, "Data", "Seeding", "planilha");

    public static List<LancamentoFinanceiro> CarregarLancamentos(IWebHostEnvironment env)
    {
        var path = Path.Combine(Pasta(env), "lancamentos.json");
        if (!File.Exists(path)) return new();
        var dtos = JsonSerializer.Deserialize<List<LancamentoDto>>(File.ReadAllText(path), JsonOpts) ?? new();
        return dtos.Select(d => new LancamentoFinanceiro
        {
            Id = d.Id,
            Data = DateOnly.TryParse(d.Data, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ? dt : null,
            Competencia = d.Competencia,
            Descricao = d.Descricao,
            Categoria = d.Categoria,
            TipoInsumo = d.TipoInsumo ?? "",
            Quantidade = d.Quantidade,
            Valor = d.Valor,
            Origem = string.IsNullOrWhiteSpace(d.Origem) ? "planilha" : d.Origem
        }).ToList();
    }

    public static List<OrcamentoInterno> CarregarOrcamentos(IWebHostEnvironment env)
    {
        var path = Path.Combine(Pasta(env), "orcamentos.json");
        if (!File.Exists(path)) return new();
        var dtos = JsonSerializer.Deserialize<List<OrcamentoDto>>(File.ReadAllText(path), JsonOpts) ?? new();
        return dtos.Select(d =>
        {
            var orc = new OrcamentoInterno
            {
                Id = d.Id,
                Nome = d.Nome,
                Material = string.IsNullOrWhiteSpace(d.Material) ? "PLA" : d.Material,
                Minutos = d.Minutos,
                PesoG = d.PesoG,
                MargemSite = d.MargemSite,
                PrecoKg = d.PrecoKg <= 0 ? 85 : d.PrecoKg,
                PrecoKgOrigem = "travado",
                EnergiaPorMinuto = 0.005m,
                Depreciacao = 0.02m,
                Manutencao = 0.01m,
                TaxaFalha = 0.05m,
                Enviado = d.Enviado,
                MaquinaId = "maq-bambu"
            };
            foreach (var i in d.Itens ?? new())
            {
                if (string.IsNullOrWhiteSpace(i.Nome) || i.Quantidade <= 0) continue;
                orc.Itens.Add(new OrcamentoItem
                {
                    Nome = i.Nome,
                    Quantidade = i.Quantidade,
                    Preco = i.Preco
                });
            }
            return orc;
        }).ToList();
    }

    public static List<FechamentoEnergia> CarregarEnergia(IWebHostEnvironment env)
    {
        var path = Path.Combine(Pasta(env), "energia.json");
        if (!File.Exists(path)) return new();
        var dtos = JsonSerializer.Deserialize<List<EnergiaDto>>(File.ReadAllText(path), JsonOpts) ?? new();
        return dtos
            .GroupBy(x => x.Competencia)
            .Select((g, i) =>
            {
                var valor = g.Sum(x => x.Valor);
                var tarifa = 0.95m;
                var kwh = tarifa > 0 ? Math.Round(valor / tarifa, 2) : 0;
                return new FechamentoEnergia
                {
                    Id = $"ene-{g.Key}",
                    Competencia = g.Key,
                    Valor = valor,
                    Tarifa = tarifa,
                    KwhTotal = kwh,
                    Notas = string.Join("; ", g.Select(x => x.Descricao).Distinct())
                };
            })
            .OrderBy(x => x.Competencia)
            .ToList();
    }
}
