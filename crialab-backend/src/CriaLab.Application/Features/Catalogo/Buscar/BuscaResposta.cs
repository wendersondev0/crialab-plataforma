namespace CriaLab.Application.Features.Catalogo.Buscar;

public sealed record BuscaConsulta(
    string? Q,
    string? Categoria,
    string? PrecoMin,
    string? PrecoMax,
    string? Ordem,
    string? Nota,
    string? FreteGratis,
    string? AVista,
    string? Personalizavel);

public sealed record BuscaResposta(
    string Ajuda,
    string? Mensagem,
    bool TemFiltro,
    string Resumo,
    decimal FreteGratisAPartir,
    string FreteGratisTexto,
    BuscaFiltrosResposta Filtros,
    IReadOnlyList<BuscaCategoriaResposta> Categorias,
    IReadOnlyList<BuscaFiltroAtivoResposta> Ativos,
    IReadOnlyList<BuscaItemResposta> Resultados);

public sealed record BuscaFiltrosResposta(
    string? Q,
    string? Categoria,
    string PrecoMin,
    string PrecoMax,
    string Ordem,
    string? Nota,
    bool FreteGratis,
    bool AVista,
    bool Personalizavel);

public sealed record BuscaCategoriaResposta(string Id, string Nome);

public sealed record BuscaFiltroAtivoResposta(string Rotulo, IReadOnlyList<string> Remover);

public sealed record BuscaItemResposta(
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

public sealed record SugestaoResposta(string Id, string Titulo, decimal Preco, string Categoria, string? Foto);
