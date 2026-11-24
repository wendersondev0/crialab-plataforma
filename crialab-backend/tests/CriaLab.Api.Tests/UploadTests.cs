using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CriaLab.Api.Tests;

public class UploadTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private readonly WebApplicationFactory<Program> _fabrica;

    public UploadTests(WebApplicationFactory<Program> fabrica)
    {
        _fabrica = fabrica.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
            builder.UseEnvironment(Environments.Development);
        });
    }

    [Fact]
    public async Task Foto_e_logo_validas_ficam_no_storage()
    {
        var cliente = await ClienteAdminAsync();

        using var foto = Formulario(Png, "peca.png", "image/png");
        var respostaFoto = await cliente.PostAsync("/api/admin/uploads/foto", foto);
        Assert.Equal(HttpStatusCode.OK, respostaFoto.StatusCode);
        var corpoFoto = await respostaFoto.Content.ReadFromJsonAsync<JsonElement>();
        var caminhoFoto = corpoFoto.GetProperty("arquivo").GetString();
        Assert.StartsWith("fotos/", caminhoFoto, StringComparison.Ordinal);
        var servida = await cliente.GetAsync("/" + caminhoFoto);
        Assert.Equal(HttpStatusCode.OK, servida.StatusCode);
        Assert.Equal("image/png", servida.Content.Headers.ContentType?.MediaType);

        using var logo = Formulario(Png, "logo.WEBP", "image/webp");
        logo.Add(new StringContent("par-testeup"), "id");
        var respostaLogo = await cliente.PostAsync("/api/admin/uploads/logo", logo);
        Assert.Equal(HttpStatusCode.OK, respostaLogo.StatusCode);
        var corpoLogo = await respostaLogo.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("parceiros/par-testeup.webp", corpoLogo.GetProperty("arquivo").GetString());
        var servidaLogo = await cliente.GetAsync("/parceiros/par-testeup.webp");
        Assert.Equal(HttpStatusCode.OK, servidaLogo.StatusCode);
    }

    [Fact]
    public async Task Recusa_extensao_content_type_e_tamanho()
    {
        var cliente = await ClienteAdminAsync();

        using var extensao = Formulario(Png, "peca.txt", "image/png");
        var respostaExtensao = await cliente.PostAsync("/api/admin/uploads/foto", extensao);
        Assert.Equal(HttpStatusCode.BadRequest, respostaExtensao.StatusCode);
        Assert.Contains("Use JPG, PNG, WEBP ou GIF.", await respostaExtensao.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        using var tipo = Formulario(Png, "peca.jpg", "application/pdf");
        var respostaTipo = await cliente.PostAsync("/api/admin/uploads/foto", tipo);
        Assert.Equal(HttpStatusCode.BadRequest, respostaTipo.StatusCode);
        Assert.Contains("Tipo de arquivo não permitido.", await respostaTipo.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var grande = new byte[(5 * 1024 * 1024) + 1];
        grande[0] = 1;
        using var tamanho = Formulario(grande, "peca.jpg", "image/jpeg");
        var respostaTamanho = await cliente.PostAsync("/api/admin/uploads/logo", tamanho);
        Assert.Equal(HttpStatusCode.BadRequest, respostaTamanho.StatusCode);
        Assert.Contains("Imagem acima de 5 MB.", await respostaTamanho.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Usuario_comum_recebe_403()
    {
        var cliente = _fabrica.CreateClient();
        var email = $"upload.{Guid.NewGuid():N}@teste.crialab.local";
        var senha = "Senha@123!";
        var cadastro = await cliente.PostAsJsonAsync("/api/conta/cadastrar", new
        {
            nomeCompleto = "Cliente",
            email,
            senha,
            confirmarSenha = senha
        });
        Assert.Equal(HttpStatusCode.OK, cadastro.StatusCode);
        var sessao = await cadastro.Content.ReadFromJsonAsync<JsonElement>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessao.GetProperty("accessToken").GetString());

        using var foto = Formulario(Png, "peca.png", "image/png");
        var resposta = await cliente.PostAsync("/api/admin/uploads/foto", foto);
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    private async Task<HttpClient> ClienteAdminAsync()
    {
        var cliente = _fabrica.CreateClient();
        var login = await cliente.PostAsJsonAsync("/api/conta/entrar", new
        {
            email = "crialab.dm@gmail.com",
            senha = "@123Mudar"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var sessao = await login.Content.ReadFromJsonAsync<JsonElement>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessao.GetProperty("accessToken").GetString());
        return cliente;
    }

    private static MultipartFormDataContent Formulario(byte[] bytes, string nome, string contentType)
    {
        var conteudo = new MultipartFormDataContent();
        var parte = new ByteArrayContent(bytes);
        parte.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        conteudo.Add(parte, "arquivo", nome);
        return conteudo;
    }
}
