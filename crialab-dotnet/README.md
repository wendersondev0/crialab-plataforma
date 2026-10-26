# Cria Lab — ASP.NET Core (Razor Pages)

Pasta `crialab-plataforma/crialab-dotnet`. Loja e administração com **ASP.NET Core 10**, **Razor Pages**, **PostgreSQL**, **Entity Framework Core** e **ASP.NET Identity** (papéis `Usuario` e `Administrador`). A vitrine estática fica em `../estatico/`.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ em `localhost:5432`

## Banco de dados

Crie o banco (se ainda não existir):

```sql
CREATE DATABASE crialab;
```

Connection string em `src/CriaLab.Web/appsettings.json`:

`Host=localhost;Port=5432;Database=crialab;Username=postgres;Password=@123Mudar`

## Executar

```powershell
cd src/CriaLab.Web
dotnet run
```

Na subida a aplicação:

1. aplica a migration `Inicial`
2. cria papéis, usuários seed, catálogo, config, financeiro e operações de exemplo

## Logins padrão (seed)

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | crialab.dm@gmail.com | Admin@CriaLab1! |
| Cliente | cliente@crialab.local | Cliente@123! |

Cadastro público em `/Conta/Register` recebe o papel **Usuario**. Painel em `/Admin` exige **Administrador**.

## Migrações

```powershell
cd src/CriaLab.Web
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

## O que está no banco

Catálogo (produtos, fotos, etapas, variações), identidade da loja, categorias, modalidades, cupons, faixas de CEP, depoimentos, parceiros, pedidos, orçamentos do cliente, perguntas, avaliações, mensagens, favoritos, endereços, máquinas, insumos, impressões, lançamentos financeiros, metas, energia, parâmetros de custo/orçamento interno — além das tabelas do Identity.

Tabelas e colunas usam **snake_case** no PostgreSQL (`produtos`, `preco_comparativo`, `asp_net_users`, `categoria_id`, etc.), via `UseSnakeCaseNamingConvention()` no EF Core e mapeamento explícito das tabelas do Identity.

## Estrutura

- `src/CriaLab.Web` — app Razor Pages (loja + `/Admin`)
- `wwwroot/css`, `wwwroot/marca`, `wwwroot/fotos` — assets da vitrine estática
- `Data/Entities` — domínio EF
- `Data/Migrations` — schema PostgreSQL
- `Data/Seeding` — carga inicial
