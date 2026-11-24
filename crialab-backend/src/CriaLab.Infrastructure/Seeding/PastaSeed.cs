namespace CriaLab.Infrastructure.Seeding;

public static class PastaSeed
{
    public static string Raiz()
    {
        var candidatos = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Seeding"),
            Path.Combine(Path.GetDirectoryName(typeof(PastaSeed).Assembly.Location)!, "Seeding")
        };

        foreach (var pasta in candidatos)
        {
            if (Directory.Exists(pasta) && File.Exists(Path.Combine(pasta, "seed-produtos.json")))
            {
                return pasta;
            }
        }

        throw new InvalidOperationException("Pasta de seed não encontrada.");
    }
}
