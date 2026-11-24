namespace CriaLab.Application.Features.Catalogo.ObterProduto;

public sealed record ProdutoFichaResposta(
    string Id,
    string Titulo,
    string Sessao,
    string Descricao,
    decimal Preco,
    decimal PrecoComparativo,
    bool Oferta,
    bool AVista,
    decimal DescontoAVista,
    int Parcelas,
    int Estoque,
    string Medidas,
    decimal Peso,
    string Situacao,
    bool Personalizavel,
    string ModalidadeId,
    string ModalidadeNome,
    string CategoriaId,
    string CategoriaNome,
    string PrazoTipo,
    bool FreteGratis,
    int PrazoDias,
    DateOnly? PrazoInicio,
    DateOnly? PrazoFim,
    string? Whatsapp,
    string? Shopee,
    string? MercadoLivre,
    IReadOnlyList<ProdutoFotoResposta> Fotos,
    IReadOnlyList<VariacaoResposta> Variacoes,
    IReadOnlyList<CorLegadoResposta> Cores,
    IReadOnlyList<TamanhoLegadoResposta> Tamanhos,
    IReadOnlyList<EtapaResposta> Etapas,
    IReadOnlyList<AvaliacaoResposta> Avaliacoes,
    IReadOnlyList<PerguntaFichaResposta> Perguntas,
    IReadOnlyList<ProdutoRelacionadoResposta> Relacionados);

public sealed record ProdutoFotoResposta(int Id, string Arquivo, string Legenda, int Ordem);

public sealed record VariacaoResposta(string Id, string Nome, string Tipo, IReadOnlyList<VariacaoOpcaoResposta> Opcoes);

public sealed record VariacaoOpcaoResposta(string Id, string Nome, string Hex);

public sealed record CorLegadoResposta(int Id, string Nome, string Hex);

public sealed record TamanhoLegadoResposta(int Id, string Nome);

public sealed record EtapaResposta(string Id, string Nome, string Tipo, decimal Acresce);

public sealed record AvaliacaoResposta(string Id, string Autor, int Nota, string Texto);

public sealed record PerguntaFichaResposta(string Id, string Autor, string Texto, string Resposta, bool Respondida);

public sealed record ProdutoRelacionadoResposta(
    string Id,
    string Titulo,
    decimal Preco,
    string ModalidadeId,
    string ModalidadeNome,
    string? Foto,
    string PrazoTipo,
    bool FreteGratis,
    int PrazoDias,
    DateOnly? PrazoInicio,
    DateOnly? PrazoFim);
