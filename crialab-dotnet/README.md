# Cria Lab — ASP.NET Core (Razor Pages)

Pasta `crialab-plataforma/crialab-dotnet`. Loja e administração com **ASP.NET Core 10**, **Razor Pages**, **PostgreSQL**, **Entity Framework Core** e **ASP.NET Identity** (papéis `Usuario` e `Administrador`). A vitrine estática de referência fica em `../estatico/`.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ em `localhost:5432`

## Banco de dados

```sql
CREATE DATABASE crialab;
```

Connection string em `src/CriaLab.Web/appsettings.json`:

`Host=localhost;Port=5432;Database=crialab;Username=postgres;Password=@123Mudar`

## Executar (recomendado: watch)

Evite `dotnet run` em segundo plano se for reconstruir: o processo trava `CriaLab.Web.exe` (erro MSB3026). Prefira:

```powershell
cd src/CriaLab.Web
dotnet watch run
```

`dotnet watch` recompila sem deixar o exe travado entre edits. Se ainda houver lock:

```powershell
Get-Process -Name CriaLab.Web -ErrorAction SilentlyContinue | Stop-Process -Force
```

Na subida a aplicação aplica a migration inicial e o seed (papéis, usuários, catálogo, config, financeiro e operações de exemplo).

## Logins padrão (seed)

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | crialab.dm@gmail.com | @123Mudar |
| Cliente | cliente@crialab.local | Cliente@123! |

Cadastro público em `/Conta/Register` recebe o papel **Usuario**. `/Admin` exige política **AdminOnly** (papel `Administrador`).

## Mapa de rotas

### Loja

| Rota | Página |
| --- | --- |
| `/` | Home |
| `/Busca` | Busca com filtros |
| `/Produtos?ModalidadeFiltro=…` | Listas por modalidade |
| `/pronta-entrega` · `/sob-demanda` · `/personalizados` | Redirects amigáveis → Produtos |
| `/Produto/{id}` | Ficha |
| `/Carrinho` | Carrinho + CEP/cupom |
| `/Checkout` | Checkout |
| `/Recibo/{id}` | Recibo do pedido |
| `/Conta` | Conta (abas) |
| `/Conta/Login` · `/Conta/Register` · `/Conta/Logout` | Auth |
| `/Contato` · `/Sobre` | Institucionais |
| `/Favorito` | Toggle favorito |

### Admin (`[Authorize(Policy = "AdminOnly")]`)

| Rota | Módulo |
| --- | --- |
| `/Admin` | Painel |
| `/Admin/Produtos` · `/Admin/ProdutoEdit/{id?}` · `/Admin/Personalizados` | Catálogo |
| `/Admin/Perguntas` · `/Admin/Depoimentos` | Aliases → Atendimento |
| `/Admin/Impressoes` · `/Admin/Estoque` | Aliases → Impressão fila / Patrimônio |
| `/Admin/Pedidos` · `/Admin/Os` | Pedidos / OS |
| `/Admin/Orcamentos` · `/Admin/OrcamentoEdit/{id}` | Orçamentos |
| `/Admin/Atendimento` | Mensagens, perguntas, depoimentos |
| `/Admin/Marketing` | Cupom, parceiros, menus ajuda |
| `/Admin/Patrimonio` · `/Admin/Estoque` · `/Admin/Impressao` | Oficina |
| `/Admin/Financeiro` · `/Admin/Mes` · `/Admin/Energia` · `/Admin/Conciliacao` | Financeiro |
| `/Admin/Fiscal` | Recibos / revisão de nota / CSV |
| `/Admin/Sistema` | Identidade, vendas, CEP, publicação, avançado |

## Restaurar carga inicial (Development)

No Painel (`/Admin`) ou em Sistema → Avançado, use **Restaurar carga inicial**. Isso apaga o banco Postgres da app e roda o seed de novo (somente `ASPNETCORE_ENVIRONMENT=Development`).

## Upload de imagens

Produtos (`wwwroot/fotos/…`) e logos de parceiros (`wwwroot/parceiros/…`) aceitam JPG, PNG, WEBP ou GIF até **5 MB**. Extensão e content-type são validados em `ImageUploadHelper`.

## Testes

```powershell
dotnet test tests/CriaLab.Web.Tests/CriaLab.Web.Tests.csproj
```

## Migrações

```powershell
cd src/CriaLab.Web
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

## O que está no banco

Catálogo (produtos, fotos, etapas, variações), identidade da loja, categorias, modalidades, cupons, faixas de CEP, depoimentos, parceiros, pedidos, orçamentos, perguntas, avaliações, mensagens, favoritos, endereços, máquinas, insumos, impressões, lançamentos financeiros, metas, energia, parâmetros de custo — além das tabelas do Identity.

Tabelas e colunas usam **snake_case** no PostgreSQL via `UseSnakeCaseNamingConvention()`.

## Estrutura

- `src/CriaLab.Web` — app Razor Pages (loja + `/Admin`)
- `wwwroot/css`, `wwwroot/marca`, `wwwroot/fotos` — assets
- `Data/Entities` — domínio EF
- `Data/Migrations` — schema PostgreSQL
- `Data/Seeding` — carga inicial
- `Services` — carrinho, frete, financeiro, upload

## Checklist manual (aceite)

Para cada tela do estático vs Razor:

1. Mesmos blocos/seções e ações principais
2. Empty state não quebra layout
3. Mobile: menu gaveta / scroll de menus ok
4. Dados persistem após F5 (Postgres)
5. Sem regressão de `.revela` invisível / CSS não carregado

Smoke sugerido após `dotnet watch run`:

- [ ] Login admin → Painel → Produtos → editar com preview → salvar foto válida
- [ ] Loja: busca → ficha → carrinho → checkout → recibo
- [ ] Conta: pedidos / favoritos / endereços
- [ ] Admin: Pedidos, Fiscal (CSV), Sistema (loja + faixa CEP), Marketing (cupom)
