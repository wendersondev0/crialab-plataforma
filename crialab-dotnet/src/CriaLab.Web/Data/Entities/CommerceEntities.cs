namespace CriaLab.Web.Data.Entities;

public class Favorito
{
    public int Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class EnderecoCliente
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string Rotulo { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Linha { get; set; } = string.Empty;
}

public class Pedido
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescontoCupom { get; set; }
    public decimal DescontoAVista { get; set; }
    public decimal FreteCobrado { get; set; }
    public decimal Total { get; set; }
    public string Forma { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string FreteNome { get; set; } = string.Empty;
    public string Cupom { get; set; } = string.Empty;
    public bool RepassarFrete { get; set; }
    public string Nota { get; set; } = string.Empty;
    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
    public PedidoOs? Os { get; set; }
}

public class PedidoItem
{
    public string Id { get; set; } = string.Empty;
    public string PedidoId { get; set; } = string.Empty;
    public Pedido? Pedido { get; set; }
    public string ProdutoId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool Fisico { get; set; }
    public bool Arquivo { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public string? CorEscolhida { get; set; }
    public string? TamanhoEscolhido { get; set; }
}

public class PedidoOs
{
    public string PedidoId { get; set; } = string.Empty;
    public Pedido? Pedido { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool InsumoBaixado { get; set; }
}

public class OrcamentoCliente
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Detalhes { get; set; } = string.Empty;
    public string ArquivoNome { get; set; } = string.Empty;
    public decimal? Valor { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class Avaliacao
{
    public string Id { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string? PedidoId { get; set; }
    public string Autor { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool FotoDoProduto { get; set; }
}

public class PerguntaProduto
{
    public string Id { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string Autor { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string Resposta { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class MensagemContato
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
