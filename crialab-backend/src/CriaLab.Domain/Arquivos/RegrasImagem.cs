namespace CriaLab.Domain.Arquivos;

public static class RegrasImagem
{
    public const long MaxBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> Extensoes = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private static readonly HashSet<string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public static bool EhValida(string? nomeArquivo, string? contentType, long tamanho, out string? erro)
    {
        erro = null;
        if (tamanho <= 0)
        {
            erro = "Arquivo vazio.";
            return false;
        }

        if (tamanho > MaxBytes)
        {
            erro = "Imagem acima de 5 MB.";
            return false;
        }

        var ext = Path.GetExtension(nomeArquivo ?? string.Empty);
        if (string.IsNullOrWhiteSpace(ext) || !Extensoes.Contains(ext))
        {
            erro = "Use JPG, PNG, WEBP ou GIF.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(contentType) && !ContentTypes.Contains(contentType))
        {
            erro = "Tipo de arquivo não permitido.";
            return false;
        }

        return true;
    }

    public static string ExtensaoSegura(string? nomeArquivo, string padrao = ".jpg")
    {
        var ext = Path.GetExtension(nomeArquivo ?? string.Empty);
        if (string.IsNullOrWhiteSpace(ext) || !Extensoes.Contains(ext))
        {
            return padrao;
        }

        return ext.ToLowerInvariant();
    }
}
