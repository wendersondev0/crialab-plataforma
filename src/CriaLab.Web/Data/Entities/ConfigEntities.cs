namespace CriaLab.Web.Data.Entities;

public class ConfiguracaoLoja
{
    public string Id { get; set; } = "default";
    public decimal FreteGratisAPartir { get; set; }
    public decimal DescontoAVista { get; set; }
    public int Parcelas { get; set; }
    public bool RepassarFrete { get; set; }
    public decimal TaxaFalha { get; set; }
    public decimal FreteCanal { get; set; }
    public decimal PrecoKwhAtual { get; set; }
}

public class Cupom
{
    public string Id { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public bool Ativo { get; set; }
}

public class FaixaCep
{
    public int Id { get; set; }
    public int De { get; set; }
    public int Ate { get; set; }
    public decimal Pac { get; set; }
    public int PacDias { get; set; }
    public decimal Sedex { get; set; }
    public int SedexDias { get; set; }
    public bool EntregaLocal { get; set; }
    public decimal EntregaLocalPreco { get; set; }
    public int EntregaLocalDias { get; set; }
    public string EntregaLocalDescricao { get; set; } = string.Empty;
}

public class TipoInsumoConfig
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
}
