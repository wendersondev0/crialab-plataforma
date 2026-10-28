namespace CriaLab.Web.Data.Entities;

public class ContaFinanceira
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public bool Baixada { get; set; }
    public string Comprovante { get; set; } = string.Empty;
}

public class LancamentoFinanceiro
{
    public string Id { get; set; } = string.Empty;
    public DateOnly? Data { get; set; }
    public string Competencia { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string TipoInsumo { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Valor { get; set; }
    public string Link { get; set; } = string.Empty;
    public decimal? Kwh { get; set; }
    public decimal? Tarifa { get; set; }
    public bool Estornado { get; set; }
    public string Comprovante { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
}

public class MetaFinanceira
{
    public string Chave { get; set; } = string.Empty;
    public string ValorJson { get; set; } = "{}";
}

public class FechamentoEnergia
{
    public string Id { get; set; } = string.Empty;
    public string Competencia { get; set; } = string.Empty;
    public decimal KwhTotal { get; set; }
    public decimal Tarifa { get; set; }
    public decimal Valor { get; set; }
    public string Notas { get; set; } = string.Empty;
}

public class ParametroCusto
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Travado { get; set; }
}

public class MargemCanal
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public bool Ativo { get; set; }
}

public class DescontoOrcamento
{
    public string Id { get; set; } = "default";
    public decimal Recorrente { get; set; }
    public decimal Volume { get; set; }
}

public class ReferenciaMaquina
{
    public string Id { get; set; } = "default";
    public decimal EnergiaPorMinuto { get; set; }
    public decimal Depreciacao { get; set; }
    public decimal Manutencao { get; set; }
}

public class NivelPintura
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public string Tempo { get; set; } = string.Empty;
    public string Cores { get; set; } = string.Empty;
    public string Acabamento { get; set; } = string.Empty;
}

public class FaixaMarketplace
{
    public int Id { get; set; }
    public string Canal { get; set; } = string.Empty;
    public decimal De { get; set; }
    public decimal Ate { get; set; }
    public decimal Percentual { get; set; }
    public decimal Fixo { get; set; }
}

public class OrcamentoInterno
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public int Minutos { get; set; }
    public decimal PesoG { get; set; }
    public decimal LixoG { get; set; }
    public decimal Servico { get; set; }
    public decimal MargemSite { get; set; }
    public string DescontosMarcadosJson { get; set; } = "[]";
    public decimal PrecoKg { get; set; }
    public string PrecoKgOrigem { get; set; } = string.Empty;
    public decimal EnergiaPorMinuto { get; set; }
    public decimal Depreciacao { get; set; }
    public decimal Manutencao { get; set; }
    public decimal TaxaFalha { get; set; }
    public bool Enviado { get; set; }
    public string? MaquinaId { get; set; }
    public Maquina? Maquina { get; set; }
    public decimal FreteCanal { get; set; }
    public string? PinturaJson { get; set; }
    public string? RealizadoJson { get; set; }
    public ICollection<OrcamentoItem> Itens { get; set; } = new List<OrcamentoItem>();
}

public class OrcamentoItem
{
    public int Id { get; set; }
    public string OrcamentoInternoId { get; set; } = string.Empty;
    public OrcamentoInterno? OrcamentoInterno { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
}
