namespace CriaLab.Domain.Calculos;

public static class PerguntaFicha
{
    public static string? Texto(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    public static string Autor(bool autenticado, string? nome) =>
        autenticado
            ? string.IsNullOrEmpty(nome) ? "Cliente" : nome
            : "Visitante";
}
