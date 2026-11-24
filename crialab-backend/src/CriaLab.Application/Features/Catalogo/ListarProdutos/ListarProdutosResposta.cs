namespace CriaLab.Application.Features.Catalogo.ListarProdutos;

public sealed record ListarProdutosConsulta(string? ModalidadeFiltro);

public sealed record ListarProdutosResposta(
    string Titulo,
    string? Resumo,
    string Texto,
    decimal FreteGratisAPartir,
    IReadOnlyList<ProdutoListaResposta> Produtos);

public sealed record ProdutoListaResposta(
    string Id,
    string Titulo,
    decimal Preco,
    string ModalidadeId,
    string ModalidadeNome,
    string CategoriaNome,
    string? Foto,
    string PrazoTipo,
    bool FreteGratis,
    int PrazoDias,
    DateOnly? PrazoInicio,
    DateOnly? PrazoFim);
