using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CriaLab.Domain.Calculos;

public static class ContatoLoja
{
    private static readonly EmailAddressAttribute EmailValido = new();

    public sealed record Opcao(string Valor, string Rotulo);

    public static readonly Opcao[] Opcoes =
    [
        new("duvida", "Dúvida"),
        new("sugestao", "Sugestão"),
        new("elogio", "Elogio"),
        new("reclamacao", "Reclamação"),
        new("orcamento", "Orçamento"),
        new("outro", "Outro")
    ];

    public sealed record Preparada(string Nome, string Email, string Tipo, string Texto, IReadOnlyList<string> Erros);

    public static Preparada Preparar(string? nome, string? email, string? tipo, string? texto)
    {
        var nomeLimpo = (nome ?? "").Trim();
        var emailLimpo = (email ?? "").Trim();
        var textoLimpo = (texto ?? "").Trim().Replace("\r\n", "\n");
        var tipoLimpo = (tipo ?? "").Trim().ToLowerInvariant();
        var erros = new List<string>();

        if (nomeLimpo.Length < 2)
            erros.Add("Diga como podemos te chamar.");
        else if (nomeLimpo.Length > 120)
            erros.Add("Use um nome mais curto.");

        if (string.IsNullOrWhiteSpace(emailLimpo) || emailLimpo.Length > 180 || !EmailValido.IsValid(emailLimpo))
            erros.Add("Informe um e-mail válido.");

        var opcao = Opcoes.FirstOrDefault(item => item.Valor == tipoLimpo);
        if (opcao is null)
            erros.Add("Escolha o tipo da mensagem.");

        if (textoLimpo.Length < 8)
            erros.Add("Escreva a mensagem com um pouco mais de detalhe.");
        else if (textoLimpo.Length > 4000)
            erros.Add("A mensagem passou de 4000 caracteres.");

        return new Preparada(nomeLimpo, emailLimpo, opcao?.Rotulo ?? "", textoLimpo, erros);
    }

    public static string? Ligacao(string? telefone)
    {
        var digitos = Digitos(telefone);
        if (digitos.Length < 8)
            return null;
        return "tel:+" + ComPais(digitos);
    }

    public static string? Whatsapp(string? valor)
    {
        var texto = (valor ?? "").Trim();
        if (texto.Length == 0)
            return null;
        if (texto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || texto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return texto;
        var digitos = Digitos(texto);
        if (digitos.Length < 8)
            return null;
        return "https://wa.me/" + ComPais(digitos);
    }

    public static string? Visivel(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor;

    private static string Digitos(string? valor)
    {
        if (string.IsNullOrEmpty(valor))
            return "";
        var sb = new StringBuilder(valor.Length);
        foreach (var c in valor)
        {
            if (char.IsDigit(c))
                sb.Append(c);
        }
        return sb.ToString();
    }

    private static string ComPais(string digitos)
    {
        if (!digitos.StartsWith("55", StringComparison.Ordinal) && digitos.Length <= 11)
            return "55" + digitos;
        return digitos;
    }
}
