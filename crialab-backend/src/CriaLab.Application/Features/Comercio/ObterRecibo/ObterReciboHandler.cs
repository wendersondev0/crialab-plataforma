using CriaLab.Application.Comercio;

namespace CriaLab.Application.Features.Comercio.ObterRecibo;

public sealed record ObterReciboItem(
    string Titulo,
    string? CorEscolhida,
    string? TamanhoEscolhido,
    int Quantidade,
    decimal PrecoUnitario,
    decimal TotalLinha);

public sealed record ObterReciboResposta(
    string Id,
    DateTime CriadoEm,
    string Endereco,
    string Cep,
    string Forma,
    string FreteNome,
    decimal DescontoCupom,
    decimal DescontoAVista,
    decimal FreteCobrado,
    decimal Total,
    string NomeLoja,
    string Cnpj,
    IReadOnlyList<ObterReciboItem> Itens);

public sealed class ObterReciboHandler(IRecibos recibos)
{
    public async Task<ObterReciboResposta?> ExecutarAsync(
        string? usuarioId,
        bool administrador,
        string? id,
        CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(usuarioId) || string.IsNullOrWhiteSpace(id))
            return null;

        var recibo = await recibos.ObterAsync(id.Trim(), cancelamento);
        if (recibo is null)
            return null;
        if (!administrador && !string.Equals(recibo.UsuarioId, usuarioId, StringComparison.Ordinal))
            return null;

        return new ObterReciboResposta(
            recibo.Id,
            recibo.CriadoEm,
            recibo.Endereco,
            recibo.Cep,
            recibo.Forma,
            recibo.FreteNome,
            recibo.DescontoCupom,
            recibo.DescontoAVista,
            recibo.FreteCobrado,
            recibo.Total,
            recibo.NomeLoja ?? "Cria Lab DM",
            recibo.Cnpj ?? "—",
            recibo.Itens.Select(item => new ObterReciboItem(
                item.Titulo,
                item.CorEscolhida,
                item.TamanhoEscolhido,
                item.Quantidade,
                item.PrecoUnitario,
                item.PrecoUnitario * item.Quantidade)).ToList());
    }
}
