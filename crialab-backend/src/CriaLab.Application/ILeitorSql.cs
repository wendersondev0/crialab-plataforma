namespace CriaLab.Application;

public interface ILeitorSql
{
    Task<IReadOnlyList<T>> ConsultarAsync<T>(string sql, object? parametros = null);
}
