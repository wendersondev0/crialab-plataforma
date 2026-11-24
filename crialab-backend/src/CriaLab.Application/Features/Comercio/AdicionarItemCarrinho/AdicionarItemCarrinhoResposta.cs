namespace CriaLab.Application.Features.Comercio.AdicionarItemCarrinho;

public sealed record AdicionarItemCarrinhoPedido(
    string? ProdutoId,
    int Quantidade,
    string? Cor,
    string? Tamanho,
    string? Detalhes,
    string? Destino);

public sealed record AdicionarItemCarrinhoResposta(
    string ProdutoId,
    string Titulo,
    decimal PrecoUnitario,
    int Quantidade,
    string? Cor,
    string? Tamanho,
    string? Detalhes,
    string Destino);
