namespace CriaLab.Web.Services;

public static class ImageUploadHelper
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

    public static bool EhImagemValida(IFormFile? arquivo, out string? erro)
    {
        erro = null;
        if (arquivo == null || arquivo.Length <= 0)
        {
            erro = "Arquivo vazio.";
            return false;
        }

        if (arquivo.Length > MaxBytes)
        {
            erro = "Imagem acima de 5 MB.";
            return false;
        }

        var ext = Path.GetExtension(arquivo.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !Extensoes.Contains(ext))
        {
            erro = "Use JPG, PNG, WEBP ou GIF.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(arquivo.ContentType) && !ContentTypes.Contains(arquivo.ContentType))
        {
            erro = "Tipo de arquivo não permitido.";
            return false;
        }

        return true;
    }

    public static string ExtensaoSegura(IFormFile arquivo, string padrao = ".jpg")
    {
        var ext = Path.GetExtension(arquivo.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !Extensoes.Contains(ext))
            return padrao;
        return ext.ToLowerInvariant();
    }
}
