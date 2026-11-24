using System.Globalization;
using System.Text;

namespace CriaLab.Domain.Calculos;

public static class BuscaLoja
{
    private static readonly CultureInfo Brasil = CultureInfo.GetCultureInfo("pt-BR");

    public sealed record Item(
        string Id,
        string Titulo,
        string Descricao,
        string Material,
        string CategoriaId,
        string CategoriaNome,
        string ModalidadeId,
        string ModalidadeNome,
        decimal Preco,
        bool AVista,
        bool Personalizavel,
        decimal Nota,
        int Vendas,
        DateOnly CriadoEm,
        int PrazoDias,
        string? Foto);

    public sealed record Normalizada(
        string? Termo,
        string? CategoriaId,
        decimal? PrecoMin,
        decimal? PrecoMax,
        string PrecoMinCampo,
        string PrecoMaxCampo,
        string Ordem,
        string? Nota,
        bool FreteGratis,
        bool AVista,
        bool Personalizavel,
        bool TemFiltro,
        string? Mensagem);

    public sealed record FiltroAtivo(string Rotulo, IReadOnlyList<string> Remover);

    public static Normalizada Normalizar(
        string? termo,
        string? categoria,
        string? precoMinTexto,
        string? precoMaxTexto,
        string? ordem,
        string? nota,
        string? freteGratis,
        string? aVista,
        string? personalizavel,
        IEnumerable<(string Id, string Nome)> categorias)
    {
        termo = string.IsNullOrWhiteSpace(termo) ? null : termo.Trim();
        string? mensagem = null;

        var cat = categorias.FirstOrDefault(c => string.Equals(c.Id, categoria?.Trim(), StringComparison.OrdinalIgnoreCase));
        string? categoriaId;
        if (!string.IsNullOrWhiteSpace(categoria) && string.IsNullOrEmpty(cat.Id))
        {
            mensagem = "A categoria desse link não existe mais. Mostrando todas.";
            categoriaId = null;
        }
        else
        {
            categoriaId = string.IsNullOrEmpty(cat.Id) ? null : cat.Id;
        }

        var okMin = TentarPreco(precoMinTexto, out var min);
        var okMax = TentarPreco(precoMaxTexto, out var max);
        if ((!string.IsNullOrWhiteSpace(precoMinTexto) && !okMin) || (!string.IsNullOrWhiteSpace(precoMaxTexto) && !okMax))
            mensagem = "Use apenas números no preço. Exemplo: 25 ou 25,90.";

        decimal? precoMin = okMin ? min : null;
        decimal? precoMax = okMax ? max : null;
        if (precoMin.HasValue && precoMax.HasValue && precoMin > precoMax)
        {
            (precoMin, precoMax) = (precoMax, precoMin);
            mensagem = "O preço mínimo estava acima do máximo. A faixa foi invertida.";
        }

        var ordemNorm = (ordem ?? "").Trim().ToLowerInvariant();
        ordemNorm = ordemNorm is "preco" or "preco-desc" or "vendas" or "novidade" ? ordemNorm : "relevancia";

        var notaTexto = (nota ?? "").Trim().Replace(',', '.');
        var notaNorm = notaTexto is "4" or "4.0" or "5" or "5.0" ? notaTexto[..1] : null;

        var gratis = Flag(freteGratis);
        var vista = Flag(aVista);
        var perso = Flag(personalizavel);
        var temFiltro = termo != null
            || categoriaId != null
            || precoMin.HasValue
            || precoMax.HasValue
            || gratis
            || vista
            || perso
            || notaNorm != null
            || ordemNorm != "relevancia";

        return new Normalizada(
            termo,
            categoriaId,
            precoMin,
            precoMax,
            CampoPreco(precoMin),
            CampoPreco(precoMax),
            ordemNorm,
            notaNorm,
            gratis,
            vista,
            perso,
            temFiltro,
            mensagem);
    }

    public static List<Item> Aplicar(IEnumerable<Item> publicados, Normalizada filtro, decimal freteGratisAPartir)
    {
        var lista = publicados.Where(x => PassaEstrutura(x, filtro, freteGratisAPartir)).ToList();
        if (!string.IsNullOrWhiteSpace(filtro.Termo))
            lista = FiltrarTexto(lista, filtro.Termo);
        return Ordenar(lista, filtro.Ordem, filtro.Termo);
    }

    public static List<Item> Sugestoes(IEnumerable<Item> publicados, Normalizada filtro, decimal freteGratisAPartir, string? texto)
    {
        var q = (texto ?? "").Trim();
        if (q.Length < 2)
            return [];

        return FiltrarTexto(publicados.Where(x => PassaEstrutura(x, filtro, freteGratisAPartir)), q)
            .OrderByDescending(x => SemAcento(x.Titulo).Contains(SemAcento(q), StringComparison.Ordinal))
            .ThenBy(x => x.Titulo, StringComparer.Ordinal)
            .Take(6)
            .ToList();
    }

    public static IReadOnlyList<FiltroAtivo> Ativos(Normalizada filtro, IReadOnlyDictionary<string, string> nomesCategoria)
    {
        var itens = new List<FiltroAtivo>();
        if (!string.IsNullOrWhiteSpace(filtro.Termo))
            itens.Add(new FiltroAtivo($"Texto: {filtro.Termo}", ["q"]));
        if (!string.IsNullOrWhiteSpace(filtro.CategoriaId))
        {
            var nome = nomesCategoria.TryGetValue(filtro.CategoriaId, out var n) ? n : filtro.CategoriaId;
            itens.Add(new FiltroAtivo(nome, ["categoria"]));
        }
        if (filtro.PrecoMin.HasValue && filtro.PrecoMax.HasValue)
            itens.Add(new FiltroAtivo($"{Moeda(filtro.PrecoMin.Value)} – {Moeda(filtro.PrecoMax.Value)}", ["precoMin", "precoMax"]));
        else if (filtro.PrecoMin.HasValue)
            itens.Add(new FiltroAtivo($"A partir de {Moeda(filtro.PrecoMin.Value)}", ["precoMin"]));
        else if (filtro.PrecoMax.HasValue)
            itens.Add(new FiltroAtivo($"Até {Moeda(filtro.PrecoMax.Value)}", ["precoMax"]));
        if (filtro.FreteGratis)
            itens.Add(new FiltroAtivo("Frete grátis", ["freteGratis"]));
        if (filtro.AVista)
            itens.Add(new FiltroAtivo("À vista", ["aVista"]));
        if (filtro.Personalizavel)
            itens.Add(new FiltroAtivo("Personalizável", ["personalizavel"]));
        if (filtro.Nota == "4")
            itens.Add(new FiltroAtivo("Nota 4 ou mais", ["nota"]));
        else if (filtro.Nota == "5")
            itens.Add(new FiltroAtivo("Nota 5", ["nota"]));
        if (filtro.Ordem != "relevancia")
            itens.Add(new FiltroAtivo(RotuloOrdem(filtro.Ordem), ["ordem"]));
        return itens;
    }

    public static string Resumo(int quantidade) => quantidade == 1 ? "1 peça" : $"{quantidade} peças";

    public static string SemAcento(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var form = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(form.Length);
        foreach (var c in form)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    private static bool PassaEstrutura(Item item, Normalizada filtro, decimal freteGratisAPartir)
    {
        if (!string.IsNullOrWhiteSpace(filtro.CategoriaId) && !string.Equals(item.CategoriaId, filtro.CategoriaId, StringComparison.OrdinalIgnoreCase))
            return false;
        if (filtro.PrecoMin.HasValue && item.Preco < filtro.PrecoMin.Value)
            return false;
        if (filtro.PrecoMax.HasValue && item.Preco > filtro.PrecoMax.Value)
            return false;
        if (filtro.FreteGratis && item.Preco < freteGratisAPartir)
            return false;
        if (filtro.AVista && !item.AVista)
            return false;
        if (filtro.Personalizavel && !item.Personalizavel)
            return false;
        if (filtro.Nota is "4" or "5" && decimal.TryParse(filtro.Nota, NumberStyles.Number, CultureInfo.InvariantCulture, out var notaMin) && item.Nota < notaMin)
            return false;
        return true;
    }

    private static List<Item> FiltrarTexto(IEnumerable<Item> lista, string termo)
    {
        var t = SemAcento(termo);
        return lista.Where(x =>
            SemAcento(x.Titulo).Contains(t, StringComparison.Ordinal) ||
            SemAcento(x.Descricao).Contains(t, StringComparison.Ordinal) ||
            SemAcento(x.Material).Contains(t, StringComparison.Ordinal) ||
            SemAcento(x.CategoriaNome).Contains(t, StringComparison.Ordinal)).ToList();
    }

    private static List<Item> Ordenar(List<Item> lista, string ordem, string? termo)
    {
        var texto = SemAcento(termo);
        return ordem switch
        {
            "preco" => lista.OrderBy(x => x.Preco).ThenBy(x => x.Titulo, StringComparer.Ordinal).ToList(),
            "preco-desc" => lista.OrderByDescending(x => x.Preco).ThenBy(x => x.Titulo, StringComparer.Ordinal).ToList(),
            "vendas" => lista.OrderByDescending(x => x.Vendas).ThenBy(x => x.Titulo, StringComparer.Ordinal).ToList(),
            "novidade" => lista.OrderByDescending(x => x.CriadoEm).ThenBy(x => x.Titulo, StringComparer.Ordinal).ToList(),
            _ => lista
                .OrderByDescending(x => texto.Length > 0 && SemAcento(x.Titulo).Contains(texto, StringComparison.Ordinal) ? 2 : 1)
                .ThenByDescending(x => x.Vendas)
                .ThenBy(x => x.Titulo, StringComparer.Ordinal)
                .ToList()
        };
    }

    private static bool Flag(string? valor)
    {
        var v = (valor ?? "").Trim().ToLowerInvariant();
        return v is "1" or "true" or "on" or "sim";
    }

    private static bool TentarPreco(string? texto, out decimal valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        var t = texto.Trim().Replace("R$", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "");
        if (t.Contains(',') && t.Contains('.'))
            t = t.Replace(".", "").Replace(',', '.');
        else
            t = t.Replace(',', '.');
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out valor) && valor >= 0;
    }

    private static string CampoPreco(decimal? valor) => valor?.ToString("0.00", Brasil) ?? "";

    private static string Moeda(decimal valor) => valor.ToString("C", Brasil);

    private static string RotuloOrdem(string ordem) => ordem switch
    {
        "preco" => "Menor preço",
        "preco-desc" => "Maior preço",
        "vendas" => "Mais vendidos",
        "novidade" => "Novidades",
        _ => "Relevância"
    };
}
