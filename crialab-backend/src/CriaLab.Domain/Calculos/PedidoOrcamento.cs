namespace CriaLab.Domain.Calculos;

public static class PedidoOrcamento
{
    public sealed record Normalizado(string Detalhes, string ArquivoNome);

    public static Normalizado Preparar(string? detalhes, string? arquivoNome) =>
        new(
            string.IsNullOrWhiteSpace(detalhes) ? "Sem detalhes" : detalhes.Trim(),
            string.IsNullOrWhiteSpace(arquivoNome) ? "" : arquivoNome.Trim());
}
