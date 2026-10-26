namespace CriaLab.Web.Data.Entities;

public class Maquina
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Imagem { get; set; } = string.Empty;
    public decimal Watts { get; set; }
    public decimal DepreciacaoPorMinuto { get; set; }
    public decimal ManutencaoPorMinuto { get; set; }
    public decimal EnergiaPorMinuto { get; set; }
    public bool ControlaHora { get; set; }
    public string CompraCompetencia { get; set; } = string.Empty;
    public DateOnly? AdquiridoEm { get; set; }
    public decimal CompraValor { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}

public class EstoqueInsumo
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public decimal Minimo { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Imagem { get; set; } = string.Empty;
    public DateOnly? AdquiridoEm { get; set; }
    public string Fornecedor { get; set; } = string.Empty;
    public string OrigemPlanilha { get; set; } = string.Empty;
    public ICollection<InsumoCor> Cores { get; set; } = new List<InsumoCor>();
    public ICollection<InsumoVariante> Variantes { get; set; } = new List<InsumoVariante>();
}

public class InsumoCor
{
    public string Id { get; set; } = string.Empty;
    public string EstoqueInsumoId { get; set; } = string.Empty;
    public EstoqueInsumo? EstoqueInsumo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public int Bobinas { get; set; }
}

public class InsumoVariante
{
    public string Id { get; set; } = string.Empty;
    public string EstoqueInsumoId { get; set; } = string.Empty;
    public EstoqueInsumo? EstoqueInsumo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public string Unidade { get; set; } = string.Empty;
}

public class ImpressaoJob
{
    public string Id { get; set; } = string.Empty;
    public string MaquinaId { get; set; } = string.Empty;
    public Maquina? Maquina { get; set; }
    public string ProdutoId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public decimal PesoPrevisto { get; set; }
    public decimal PesoReal { get; set; }
    public bool? Sucesso { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Kwh { get; set; }
    public DateTime? Inicio { get; set; }
    public DateTime? Fim { get; set; }
    public string Status { get; set; } = string.Empty;
}
