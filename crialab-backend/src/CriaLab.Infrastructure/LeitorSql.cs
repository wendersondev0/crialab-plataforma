using CriaLab.Application;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CriaLab.Infrastructure;

public sealed class LeitorSql : ILeitorSql
{
    private readonly string _conexao;

    static LeitorSql()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public LeitorSql(IConfiguration configuration)
    {
        _conexao = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A connection string DefaultConnection não está configurada.");
    }

    public async Task<IReadOnlyList<T>> ConsultarAsync<T>(string sql, object? parametros = null)
    {
        await using var conexao = new NpgsqlConnection(_conexao);
        var linhas = await conexao.QueryAsync<T>(sql, parametros);
        return linhas.AsList();
    }
}
