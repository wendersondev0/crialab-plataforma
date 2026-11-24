using Carter;
using CriaLab.Api.Arquivos;
using CriaLab.Api.Erros;
using CriaLab.Application;
using CriaLab.Domain.Identidade;
using CriaLab.Infrastructure;
using CriaLab.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAplicacao();
builder.Services.AddInfraestrutura(builder.Configuration);
builder.Services.AddCarter();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorExcecaoPortugues>();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Papeis.Administrador));
});

var app = builder.Build();

app.UseExceptionHandler();
ArmazenamentoEstatico.Usar(app);
app.UseAuthentication();
app.UseAuthorization();

if (app.Configuration.GetValue<bool>("ExporFalhaDeTeste"))
{
    app.MapGet("/api/teste/erro", (HttpContext _) => throw new InvalidOperationException("falha-interna-de-teste"));
}

app.MapCarter();
app.MapOpenApi();

if (app.Configuration.GetValue("Seed:Executar", true))
{
    await DbSeeder.SeedAsync(app.Services);
}

app.Run();

public partial class Program;
