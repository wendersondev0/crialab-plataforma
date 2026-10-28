# Cria Lab DM — plataforma

Monorepo da Cria Lab com duas pastas principais e documentação de continuidade para o agente/humano.

| Pasta | Conteúdo |
| --- | --- |
| [`estatico/`](estatico/) | Loja HTML/CSS/JS (referência de paridade; Cloudflare / `localStorage`) |
| [`crialab-dotnet/`](crialab-dotnet/) | ASP.NET Core 10 Razor Pages + PostgreSQL + Identity (loja + `/Admin`) |
| [`docs/historico/`](docs/historico/) | Histórico versionado de atividades (skill Cursor) |

## Estado atual

- Destino operacional: app **Razor** em `crialab-dotnet` (paridade progressiva com `estatico/`).
- Admin: Pedidos unificado com Fiscal (abas); Financeiro com KPIs e tooltips (Receita, Operacional, Caixa, Lucro…).
- Favoritos: cookie anônimo + banco para usuário autenticado.
- Parceiros (Vila Verde, GoFit) com URL de Instagram no rodapé/Sobre.
- Seed aplica migrations e carga demo (planilha financeira, catálogo, oficina).

## Stack

| Camada | Tecnologia |
| --- | --- |
| Estático | HTML, CSS (Tailwind build), JS vanilla |
| Backend / UI | ASP.NET Core 10, Razor Pages |
| Dados | PostgreSQL, EF Core (snake_case), Identity |
| Auth | Papéis `Usuario` e `Administrador` |

## Estático

```powershell
cd estatico
npm install
npm run build
python -m http.server 8080
```

Abra `http://localhost:8080`. Deploy Cloudflare: `estatico/wrangler.toml` e [`estatico/README.md`](estatico/README.md).

## ASP.NET (Razor + Postgres)

```powershell
cd crialab-dotnet
# opcional: docker compose up -d
cd src/CriaLab.Web
dotnet watch run
```

Guia completo (rotas, seed, migrações, checklist): [`crialab-dotnet/README.md`](crialab-dotnet/README.md).

## Continuidade (Cursor)

- Skill: [`.cursor/skills/crialab-plataforma/SKILL.md`](.cursor/skills/crialab-plataforma/SKILL.md)
- Regra: [`.cursor/rules/crialab-plataforma.mdc`](.cursor/rules/crialab-plataforma.mdc) — aciona a skill em qualquer tarefa deste repositório
- **Sempre ler este README antes de alterar o projeto**
- Após mudanças relevantes: atualizar este README (e/ou o do Dotnet) e registrar em [`docs/historico/`](docs/historico/) (ver `INDEX.md`)
