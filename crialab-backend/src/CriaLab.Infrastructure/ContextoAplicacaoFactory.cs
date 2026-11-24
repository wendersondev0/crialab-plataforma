using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CriaLab.Infrastructure;

public sealed class ContextoAplicacaoFactory : IDesignTimeDbContextFactory<ContextoAplicacao>
{
    public ContextoAplicacao CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar";
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseNpgsql(conexao)
            .UseSnakeCaseNamingConvention()
            .Options;
        return new ContextoAplicacao(options);
    }
}
