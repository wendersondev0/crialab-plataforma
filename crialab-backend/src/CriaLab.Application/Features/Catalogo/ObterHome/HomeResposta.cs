namespace CriaLab.Application.Features.Catalogo.ObterHome;

public sealed record HomeResposta(
    IdentidadeHomeResposta Identidade,
    decimal FreteGratisAPartir,
    string Intro,
    IReadOnlyList<ModalidadeHomeResposta> Modalidades,
    IReadOnlyList<CategoriaHomeResposta> Categorias,
    IReadOnlyList<DestaqueHomeResposta> Destaques,
    IReadOnlyList<DepoimentoHomeResposta> Depoimentos,
    IReadOnlyList<ParceiroHomeResposta> Parceiros);

public sealed record IdentidadeHomeResposta(
    string Nome,
    string Site,
    string Cnpj,
    string Email,
    string Telefone,
    string Whatsapp,
    string Instagram,
    string Tiktok,
    string Youtube,
    string Shopee,
    string MercadoLivre,
    string Endereco,
    string Horario);

public sealed record ModalidadeHomeResposta(string Id, string Nome, string Resumo, string Explicacao);

public sealed record CategoriaHomeResposta(string Id, string Nome);

public sealed record DestaqueHomeResposta(
    string Id,
    string Titulo,
    decimal Preco,
    string ModalidadeId,
    string ModalidadeNome,
    string CategoriaNome,
    string? Foto,
    bool MaisVendido,
    string PrazoTipo,
    bool FreteGratis,
    int PrazoDias,
    DateOnly? PrazoInicio,
    DateOnly? PrazoFim);

public sealed record DepoimentoHomeResposta(string Id, string Autor, string Texto);

public sealed record ParceiroHomeResposta(string Id, string Nome, string Logo, string Url);
