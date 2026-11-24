namespace CriaLab.Application.Comercio;

public sealed record ReciboItem(
    string Titulo,
    string? CorEscolhida,
    string? TamanhoEscolhido,
    int Quantidade,
    decimal PrecoUnitario);

public sealed record ReciboLido(
    string Id,
    string UsuarioId,
    DateTime CriadoEm,
    string Endereco,
    string Cep,
    string Forma,
    string FreteNome,
    decimal DescontoCupom,
    decimal DescontoAVista,
    decimal FreteCobrado,
    decimal Total,
    string? NomeLoja,
    string? Cnpj,
    IReadOnlyList<ReciboItem> Itens);

public interface IRecibos
{
    Task<ReciboLido?> ObterAsync(string id, CancellationToken cancelamento = default);
}
