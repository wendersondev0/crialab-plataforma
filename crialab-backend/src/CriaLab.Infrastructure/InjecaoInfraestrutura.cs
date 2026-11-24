using System.Text;
using CriaLab.Application;
using CriaLab.Application.Comercio;
using CriaLab.Application.Identidade;
using CriaLab.Domain.Identidade;
using CriaLab.Infrastructure.Arquivos;
using CriaLab.Infrastructure.Comercio;
using CriaLab.Infrastructure.Identidade;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CriaLab.Infrastructure;

public static class InjecaoInfraestrutura
{
    public static IServiceCollection AddInfraestrutura(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataProtection();
        services.Configure<OpcoesJwt>(configuration.GetSection(OpcoesJwt.Secao));

        services.AddDbContext<ContextoAplicacao>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention());

        services.AddIdentityCore<UsuarioAplicacao>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ContextoAplicacao>()
            .AddErrorDescriber<DescritorErrosIdentidade>()
            .AddDefaultTokenProviders();

        var jwt = configuration.GetSection(OpcoesJwt.Secao).Get<OpcoesJwt>() ?? new OpcoesJwt();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Emissor,
                    ValidAudience = jwt.Publico,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Chave)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddScoped<IEmissorToken, EmissorToken>();
        services.AddScoped<ISessaoRefresh, SessaoRefresh>();
        services.AddScoped<ILeitorSql, LeitorSql>();
        services.AddScoped<IFavoritosUsuario, FavoritosUsuario>();
        services.AddScoped<IOrcamentosCliente, OrcamentosCliente>();
        services.AddScoped<IPerguntasProduto, PerguntasProduto>();
        services.AddScoped<IFreteProduto, FreteProduto>();
        services.AddScoped<ICheckoutLoja, CheckoutLoja>();
        services.AddScoped<IRecibos, Recibos>();
        services.AddScoped<GravadorImagem>();
        return services;
    }
}
