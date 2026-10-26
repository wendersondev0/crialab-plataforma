# Cria Lab DM — plataforma

Monorepo da Cria Lab com duas pastas:

| Pasta | Conteúdo |
| --- | --- |
| [`estatico/`](estatico/) | Loja HTML/CSS/JS (Cloudflare / `localStorage`) |
| [`crialab-dotnet/`](crialab-dotnet/) | ASP.NET Core 10 Razor Pages + PostgreSQL + Identity |

## Estático

```powershell
cd estatico
npm install
npm run build
python -m http.server 8080
```

Abra `http://localhost:8080`. Detalhes de deploy Cloudflare: [`estatico/README.md`](estatico/README.md) (se existir) ou o README antigo no histórico; `wrangler.toml` está em `estatico/`.

## ASP.NET (Razor + Postgres)

```powershell
cd crialab-dotnet
# opcional: docker compose up -d
cd src/CriaLab.Web
dotnet run
```

Guia completo: [`crialab-dotnet/README.md`](crialab-dotnet/README.md).
