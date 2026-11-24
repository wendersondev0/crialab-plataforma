# Cria Lab — backend

API em Clean Architecture. A solução `CriaLab.sln` tem quatro projetos de código e dois de teste.

| Projeto | Papel |
| --- | --- |
| `src/CriaLab.Domain` | Entidades, status e cálculos puros |
| `src/CriaLab.Application` | Casos de uso, um por pasta em `Features` |
| `src/CriaLab.Infrastructure` | Banco, Identity, arquivos e seed |
| `src/CriaLab.Api` | HTTP. Os módulos ficam em `Features` |
| `tests/CriaLab.Domain.Tests` | Testes de domínio |
| `tests/CriaLab.Api.Tests` | Testes da API |

A API registra Carter, EF Core com Npgsql e snake_case, Identity, Dapper, FluentValidation, Problem Details e OpenAPI. Arquivos da loja saem de `storage/` nas rotas `/fotos`, `/parceiros` e `/marca` (cópia inicial do `wwwroot` do Razor). Upload de foto e de logo, só com `Administrador`, aceita JPG, PNG, WEBP ou GIF até 5 MB. O documento OpenAPI fica em `/openapi/v1.json`. `GET /api/saude` responde JSON com `situacao` igual a `disponivel`, sem consultar o banco. Uma exceção não tratada vira `application/problem+json` com título `Erro interno` e detalhe `Ocorreu um erro inesperado.`, sem repetir a mensagem interna. A migration `Inicial` cria em `crialab_api` as mesmas colunas do snapshot do Razor. O seed da carga demo roda na subida quando `Seed:Executar` está ligado. A restauração apaga só `crialab_api` e só em Development; banco com outro nome é recusado. Depois de um Razor e uma API recém-semeados, as contagens batem: 8 produtos, 5 pedidos, 229 lançamentos, 13 máquinas e 7 usuários.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ em `localhost:5432`, ou Docker para subir o compose desta pasta

A API usa o banco `crialab_api`. O Razor continua em `crialab`. Os dois podem coexistir no mesmo PostgreSQL.

## Banco

O usuário e a senha de desenvolvimento são os mesmos do Razor. Não crie senha nova. Reutilize o `postgres` já usado em `crialab-plataforma-old`.

### Com Docker

Na raiz de `crialab-backend`:

```powershell
docker compose up -d
```

O compose sobe PostgreSQL 16 com o mesmo usuário do Razor e cria somente o banco `crialab_api`, sem tocar em `crialab`.

### Sem Docker

No PostgreSQL que o Razor já usa:

```sql
CREATE DATABASE crialab_api;
```

## Connection string

A chave é `ConnectionStrings:DefaultConnection`. O banco é `crialab_api`. Host, porta, usuário e senha são os do desenvolvimento do Razor.

Prefira segredo de usuário ou variável de ambiente. Assim a senha não precisa entrar em arquivo versionado novo.

### Segredo de usuário

Na pasta `src/CriaLab.Api`:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=SENHA_DO_RAZOR"
```

Troque `SENHA_DO_RAZOR` pela mesma senha que está na connection string do Razor (`crialab-plataforma-old/src/CriaLab.Web/appsettings.json`).

### Variável de ambiente

PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=crialab_api;Username=postgres;Password=SENHA_DO_RAZOR"
```

### Development local

`appsettings.Development.json` já aponta para `crialab_api` com o mesmo usuário do Razor. Se o segredo ou a variável estiverem definidos, eles prevalecem.

## Subir a API

```powershell
cd src/CriaLab.Api
dotnet run
```

Na subida a API aplica a migration e, com `Seed:Executar` ligado, carrega a demo. Confira `GET /api/saude` e o OpenAPI em `/openapi/v1.json`.

Auth de desenvolvimento: cadastrar, entrar, sair, renovar e `eu` em `/api/auth`. Rotas `/api/admin/*` exigem o papel `Administrador`.

## Build e testes

Na raiz de `crialab-backend`:

```powershell
dotnet build CriaLab.sln
dotnet test CriaLab.sln
```

Plano: [../crialab-docs/planejamento/conversao-razor-para-vue-api.md](../crialab-docs/planejamento/conversao-razor-para-vue-api.md).
