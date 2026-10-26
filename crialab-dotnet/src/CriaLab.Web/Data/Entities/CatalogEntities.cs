namespace CriaLab.Web.Data.Entities;

public class LojaIdentidade
{
    public string Id { get; set; } = "principal";
    public string Nome { get; set; } = string.Empty;
    public string Site { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string Tiktok { get; set; } = string.Empty;
    public string Youtube { get; set; } = string.Empty;
    public string Shopee { get; set; } = string.Empty;
    public string MercadoLivre { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Horario { get; set; } = string.Empty;
}

public class Categoria
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}

public class Modalidade
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
    public string Explicacao { get; set; } = string.Empty;
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}

public class MenuAjuda
{
    public string Id { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
}

public class Depoimento
{
    public string Id { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public bool PodeAparecer { get; set; }
}

public class Parceiro
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool PodeAparecer { get; set; }
}

public class Produto
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Sessao { get; set; } = string.Empty;
    public string CategoriaId { get; set; } = string.Empty;
    public Categoria? Categoria { get; set; }
    public string ModalidadeId { get; set; } = string.Empty;
    public Modalidade? Modalidade { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal PrecoComparativo { get; set; }
    public decimal PrecoArquivo { get; set; }
    public int Estoque { get; set; }
    public int Minimo { get; set; }
    public string Medidas { get; set; } = string.Empty;
    public decimal Peso { get; set; }
    public string Material { get; set; } = string.Empty;
    public int PrazoDias { get; set; }
    public bool ComArquivo { get; set; }
    public string? Arquivo { get; set; }
    public bool Personalizavel { get; set; }
    public int Vendas { get; set; }
    public decimal Nota { get; set; }
    public bool Oferta { get; set; }
    public bool MaisVendido { get; set; }
    public string Situacao { get; set; } = "publicado";
    public bool AVista { get; set; }
    public DateOnly CriadoEm { get; set; }
    public ICollection<ProdutoFoto> Fotos { get; set; } = new List<ProdutoFoto>();
    public ICollection<ProdutoEtapaPersonalizacao> EtapasPersonalizacao { get; set; } = new List<ProdutoEtapaPersonalizacao>();
    public ICollection<VariacaoEixo> Variacoes { get; set; } = new List<VariacaoEixo>();
    public ICollection<ProdutoCorLegado> CoresLegado { get; set; } = new List<ProdutoCorLegado>();
    public ICollection<ProdutoTamanhoLegado> TamanhosLegado { get; set; } = new List<ProdutoTamanhoLegado>();
}

public class ProdutoFoto
{
    public int Id { get; set; }
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string Arquivo { get; set; } = string.Empty;
    public string Sessao { get; set; } = string.Empty;
    public string Legenda { get; set; } = string.Empty;
    public int Ordem { get; set; }
}

public class ProdutoEtapaPersonalizacao
{
    public string Id { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public decimal Acresce { get; set; }
}

public class VariacaoEixo
{
    public string Id { get; set; } = string.Empty;
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = "texto";
    public int Ordem { get; set; }
    public ICollection<VariacaoOpcao> Opcoes { get; set; } = new List<VariacaoOpcao>();
}

public class VariacaoOpcao
{
    public string Id { get; set; } = string.Empty;
    public string VariacaoEixoId { get; set; } = string.Empty;
    public VariacaoEixo? VariacaoEixo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Hex { get; set; } = "#cccccc";
    public int Ordem { get; set; }
}

public class ProdutoCorLegado
{
    public int Id { get; set; }
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Hex { get; set; } = "#cccccc";
}

public class ProdutoTamanhoLegado
{
    public int Id { get; set; }
    public string ProdutoId { get; set; } = string.Empty;
    public Produto? Produto { get; set; }
    public string Nome { get; set; } = string.Empty;
}
