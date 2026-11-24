using CriaLab.Application.Features.Catalogo.Buscar;
using CriaLab.Application.Features.Comercio.AdicionarItemCarrinho;
using CriaLab.Application.Features.Comercio.AlternarFavorito;
using CriaLab.Application.Features.Comercio.ConsultarFavorito;
using CriaLab.Application.Features.Comercio.CotarCheckout;
using CriaLab.Application.Features.Comercio.CotarFreteFicha;
using CriaLab.Application.Features.Comercio.EnviarPergunta;
using CriaLab.Application.Features.Comercio.PedirOrcamento;
using CriaLab.Application.Features.Comercio.ResumirCarrinho;
using CriaLab.Application.Features.Catalogo.ListarProdutos;
using CriaLab.Application.Features.Catalogo.ObterHome;
using CriaLab.Application.Features.Catalogo.ObterProduto;
using CriaLab.Application.Features.Identidade.Cadastrar;
using CriaLab.Application.Features.Identidade.Entrar;
using CriaLab.Application.Features.Identidade.Eu;
using CriaLab.Application.Features.Identidade.Renovar;
using CriaLab.Application.Features.Identidade.Sair;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CriaLab.Application;

public static class InjecaoAplicacao
{
    public static IServiceCollection AddAplicacao(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(InjecaoAplicacao).Assembly);
        services.AddScoped<CadastrarHandler>();
        services.AddScoped<EntrarHandler>();
        services.AddScoped<RenovarHandler>();
        services.AddScoped<SairHandler>();
        services.AddScoped<EuHandler>();
        services.AddScoped<ObterHomeHandler>();
        services.AddScoped<BuscarHandler>();
        services.AddScoped<ListarProdutosHandler>();
        services.AddScoped<ObterProdutoHandler>();
        services.AddScoped<AdicionarItemCarrinhoHandler>();
        services.AddScoped<ResumirCarrinhoHandler>();
        services.AddScoped<ConsultarFavoritoHandler>();
        services.AddScoped<AlternarFavoritoHandler>();
        services.AddScoped<PedirOrcamentoHandler>();
        services.AddScoped<EnviarPerguntaHandler>();
        services.AddScoped<CotarFreteFichaHandler>();
        services.AddScoped<CotarCheckoutHandler>();
        return services;
    }
}
