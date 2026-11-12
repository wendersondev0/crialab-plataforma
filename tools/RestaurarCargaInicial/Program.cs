using CriaLab.Web.Data;
using CriaLab.Web.Data.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

var conteudo = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "CriaLab.Web"));
if (!Directory.Exists(conteudo))
{
    conteudo = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "CriaLab.Web"));
}
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = conteudo,
    EnvironmentName = Environments.Development
});

builder.Configuration.AddJsonFile(Path.Combine(conteudo, "appsettings.json"), optional: false);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();
await DbSeeder.RestaurarDemoAsync(app.Services);
Console.WriteLine("Razor crialab restaurado.");
