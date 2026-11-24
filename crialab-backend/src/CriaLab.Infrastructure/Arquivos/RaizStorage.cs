namespace CriaLab.Infrastructure.Arquivos;

public static class RaizStorage
{
    public static string Resolver(string contentRoot)
    {
        var candidatos = new[]
        {
            Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "storage")),
            Path.GetFullPath(Path.Combine(contentRoot, "storage")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "storage"))
        };

        foreach (var candidato in candidatos)
        {
            if (Directory.Exists(candidato))
            {
                return candidato;
            }
        }

        var criada = candidatos[0];
        Directory.CreateDirectory(criada);
        return criada;
    }
}
