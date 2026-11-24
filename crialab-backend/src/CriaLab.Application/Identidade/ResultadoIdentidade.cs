namespace CriaLab.Application.Identidade;

public sealed class ResultadoIdentidade<T>
{
    public bool Sucesso { get; private init; }
    public int Status { get; private init; }
    public T? Valor { get; private init; }
    public IReadOnlyList<string> Erros { get; private init; } = Array.Empty<string>();

    public static ResultadoIdentidade<T> Ok(T valor) => new()
    {
        Sucesso = true,
        Status = 200,
        Valor = valor
    };

    public static ResultadoIdentidade<T> Falha(int status, params string[] erros) => new()
    {
        Sucesso = false,
        Status = status,
        Erros = erros
    };
}
