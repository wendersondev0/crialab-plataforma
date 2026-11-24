using CriaLab.Domain.Arquivos;

namespace CriaLab.Domain.Tests;

public class RegrasImagemTests
{
    [Theory]
    [InlineData("foto.jpg")]
    [InlineData("foto.jpeg")]
    [InlineData("foto.PNG")]
    [InlineData("foto.webp")]
    [InlineData("foto.gif")]
    public void Aceita_extensoes_da_loja(string nome)
    {
        var ok = RegrasImagem.EhValida(nome, "image/jpeg", 10, out var erro);
        if (nome.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            ok = RegrasImagem.EhValida(nome, "image/png", 10, out erro);
        }
        else if (nome.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
        {
            ok = RegrasImagem.EhValida(nome, "image/webp", 10, out erro);
        }
        else if (nome.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
        {
            ok = RegrasImagem.EhValida(nome, "image/gif", 10, out erro);
        }

        Assert.True(ok);
        Assert.Null(erro);
    }

    [Fact]
    public void Recusa_extensao_fora_da_lista()
    {
        var ok = RegrasImagem.EhValida("foto.txt", "image/jpeg", 10, out var erro);
        Assert.False(ok);
        Assert.Equal("Use JPG, PNG, WEBP ou GIF.", erro);
    }

    [Fact]
    public void Recusa_content_type_fora_da_lista()
    {
        var ok = RegrasImagem.EhValida("foto.jpg", "application/pdf", 10, out var erro);
        Assert.False(ok);
        Assert.Equal("Tipo de arquivo não permitido.", erro);
    }

    [Fact]
    public void Recusa_acima_de_5_mb_e_arquivo_vazio()
    {
        var grande = RegrasImagem.EhValida("foto.jpg", "image/jpeg", RegrasImagem.MaxBytes + 1, out var erroGrande);
        Assert.False(grande);
        Assert.Equal("Imagem acima de 5 MB.", erroGrande);

        var vazio = RegrasImagem.EhValida("foto.jpg", "image/jpeg", 0, out var erroVazio);
        Assert.False(vazio);
        Assert.Equal("Arquivo vazio.", erroVazio);
    }

    [Fact]
    public void Content_type_vazio_passa_quando_a_extensao_e_valida()
    {
        var ok = RegrasImagem.EhValida("foto.jpg", "  ", 10, out var erro);
        Assert.True(ok);
        Assert.Null(erro);
    }
}
