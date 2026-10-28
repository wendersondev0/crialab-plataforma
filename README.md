# Cria Lab DM — plataforma

Loja estática de referência e app Razor no mesmo repositório.

| Pasta | Conteúdo |
| --- | --- |
| [`estatico/`](estatico/) | Loja HTML/CSS/JS (paridade; Cloudflare / `localStorage`) |
| [`src/CriaLab.Web`](src/CriaLab.Web) | ASP.NET Core 10 Razor Pages + PostgreSQL + Identity (loja + `/Admin`) |
| [`tests/CriaLab.Web.Tests`](tests/CriaLab.Web.Tests) | Testes do cálculo de orçamento |
| [`docs/historico/`](docs/historico/) | Histórico versionado de atividades (skill Cursor) |

## Estado atual

- Destino operacional: app **Razor** em `src/CriaLab.Web` (paridade com `estatico/`).
- Busca da loja (`/Busca`): painel de filtros, sugestões, chips para remover filtro e vitrine completa quando não há filtro.
- Contato da loja (`/Contato`): WhatsApp, e-mail e telefone clicáveis, e formulário com validação e confirmação.
- Admin: menu na mesma barra laranja da loja. Papéis em `/Admin/Papeis` e usuários em `/Admin/Usuarios`. Pedidos segue unificado com Fiscal (abas); Financeiro com KPIs e tooltips.
- Favoritos: cookie anônimo + banco para usuário autenticado.
- Parceiros (Vila Verde, GoFit) com URL de Instagram no rodapé/Sobre.
- Seed aplica migrations e carga demo (planilha financeira, catálogo, oficina).
- Orçamento no admin calcula o preço da Shopee. O cartão de Mercado Livre do estático ainda não tem faixa no seed (a lista estática também está vazia) e não foi portado.

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

Pré-requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download) e PostgreSQL 14+ em `localhost:5432`.

```sql
CREATE DATABASE crialab;
```

Connection string em `src/CriaLab.Web/appsettings.json`:

`Host=localhost;Port=5432;Database=crialab;Username=postgres;Password=@123Mudar`

```powershell
# opcional, na raiz do repositório:
docker compose up -d

cd src/CriaLab.Web
dotnet watch run
```

`dotnet watch` recompila sem deixar o exe travado entre edits. Se ainda houver lock:

```powershell
Get-Process -Name CriaLab.Web -ErrorAction SilentlyContinue | Stop-Process -Force
```

Na subida a aplicação aplica a migration inicial e o seed (papéis, usuários, catálogo, config, financeiro e operações de exemplo).

### Logins padrão (seed)

| Perfil | E-mail | Senha |
|--------|--------|-------|
| Administrador | crialab.dm@gmail.com | @123Mudar |
| Cliente | cliente@crialab.local | Cliente@123! |
| Teste | ana.silva@teste.crialab.local | Teste@123! |
| Teste | bruno.costa@teste.crialab.local | Teste@123! |
| Teste | camila.souza@teste.crialab.local | Teste@123! |
| Teste | diego.ferreira@teste.crialab.local | Teste@123! |
| Teste | elena.ramos@teste.crialab.local | Teste@123! |

Cadastro público em `/Conta/Register` recebe o papel **Usuario**. `/Admin` exige política **AdminOnly** (papel `Administrador`).

O seed financeiro/orçamentos carrega `src/CriaLab.Web/Data/Seeding/planilha/*.json`.

### Mapa de rotas

#### Loja

| Rota | Página |
| --- | --- |
| `/` | Home |
| `/Busca` | Busca com filtros, sugestões e ordenação |
| `/Produtos?ModalidadeFiltro=…` | Listas por modalidade |
| `/pronta-entrega` · `/sob-demanda` · `/personalizados` | Redirects amigáveis → Produtos |
| `/Produto/{id}` | Ficha |
| `/Carrinho` | Carrinho + CEP/cupom |
| `/Checkout` | Checkout |
| `/Recibo/{id}` | Recibo do pedido |
| `/Conta` | Conta (abas) |
| `/Conta/Login` · `/Conta/Register` · `/Conta/Logout` | Auth |
| `/Contato` · `/Sobre` | Contato com canais e formulário · Sobre |
| `/Favorito` | Toggle favorito |

#### Admin (`[Authorize(Policy = "AdminOnly")]`)

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
| `/Admin/Sistema` · `/Admin/Papeis` · `/Admin/Usuarios` | Loja, frete, site · papéis · contas |

### Restaurar carga inicial (Development)

No Painel (`/Admin`) ou em Sistema → Avançado, use **Restaurar carga inicial**. Isso apaga o banco Postgres da app e roda o seed de novo (somente `ASPNETCORE_ENVIRONMENT=Development`).

### Upload de imagens

Produtos (`wwwroot/fotos/…`) e logos de parceiros (`wwwroot/parceiros/…`) aceitam JPG, PNG, WEBP ou GIF até **5 MB**. Extensão e content-type são validados em `ImageUploadHelper`.

### Testes e migrações

```powershell
dotnet test tests/CriaLab.Web.Tests/CriaLab.Web.Tests.csproj

cd src/CriaLab.Web
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

### O que está no banco

Catálogo (produtos, fotos, etapas, variações), identidade da loja, categorias, modalidades, cupons, faixas de CEP, depoimentos, parceiros, pedidos, orçamentos, perguntas, avaliações, mensagens, favoritos, endereços, máquinas, insumos, impressões, lançamentos financeiros, metas, energia, parâmetros de custo — além das tabelas do Identity.

Tabelas e colunas usam **snake_case** no PostgreSQL via `UseSnakeCaseNamingConvention()`.

### Checklist manual (aceite)

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

## Continuidade (Cursor)

- Skill: [`.cursor/skills/crialab-plataforma/SKILL.md`](.cursor/skills/crialab-plataforma/SKILL.md)
- Regra: [`.cursor/rules/crialab-plataforma.mdc`](.cursor/rules/crialab-plataforma.mdc) — aciona a skill em qualquer tarefa deste repositório
- **Sempre ler este README antes de alterar o projeto**
- Após mudanças relevantes: atualizar este README e registrar em [`docs/historico/`](docs/historico/) (ver `INDEX.md`)
