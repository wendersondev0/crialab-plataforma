using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CriaLab.Api.Tests;

public class ArmazenamentoEstaticoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _cliente;

    public ArmazenamentoEstaticoTests(WebApplicationFactory<Program> fabrica)
    {
        _cliente = fabrica.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Seed:Executar", "false");
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=@123Mudar");
        }).CreateClient();
    }

    [Fact]
    public async Task Serve_marca_mascote_e_logo_de_parceiro_no_caminho_do_seed()
    {
        var mascote = await _cliente.GetAsync("/marca/mascote.png");
        Assert.Equal(HttpStatusCode.OK, mascote.StatusCode);
        Assert.Equal("image/png", mascote.Content.Headers.ContentType?.MediaType);
        Assert.True((await mascote.Content.ReadAsByteArrayAsync()).Length > 0);

        var parceiro = await _cliente.GetAsync("/marca/parceiro-vila-verde.png");
        Assert.Equal(HttpStatusCode.OK, parceiro.StatusCode);
        Assert.Equal("image/png", parceiro.Content.Headers.ContentType?.MediaType);

        var patrimonio = await _cliente.GetAsync("/marca/patrimonio/pat-pla.jpg");
        Assert.Equal(HttpStatusCode.OK, patrimonio.StatusCode);
        Assert.Equal("image/jpeg", patrimonio.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Serve_foto_de_produto_e_pasta_parceiros()
    {
        var foto = await _cliente.GetAsync("/fotos/2026-03-14/IMG_20260314_223533.jpg");
        Assert.Equal(HttpStatusCode.OK, foto.StatusCode);
        Assert.Equal("image/jpeg", foto.Content.Headers.ContentType?.MediaType);

        var pastaParceiros = await _cliente.GetAsync("/parceiros/placeholder.png");
        Assert.Equal(HttpStatusCode.OK, pastaParceiros.StatusCode);
        Assert.Equal("image/png", pastaParceiros.Content.Headers.ContentType?.MediaType);

        var inexistente = await _cliente.GetAsync("/fotos/nao-existe.jpg");
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
    }
}
