# Contexto para agentes

Leia isto antes de implementar a conversão. Em seguida leia o plano inteiro. A skill `seguir-planejamento-crialab` manda a ordem dos checkboxes e o fecho `N tarefas feitas de T`.

## Onde está cada coisa

| Pasta | Papel |
| --- | --- |
| `crialab-plataforma-old/` | App atual em Razor Pages. É a fonte do comportamento até o corte. |
| `crialab-plataforma/crialab-frontend/` | Vue 3 + TypeScript + Vite + PrimeVue + Pinia + Vue Router. A home, a busca, a listagem e a ficha do produto já leem a API. A ficha grava o carrinho e o favorito anônimo no navegador. Com `crialab-access-token`, o favorito e o pedido de orçamento vão para a API. PWA ainda não entrou. |
| `crialab-plataforma/crialab-backend/` | API com seed em `crialab_api`, Auth, `AdminOnly`, `storage` público, upload de imagem, `GET /api/loja/home`, `GET /api/loja/busca`, `GET /api/loja/produtos`, `GET /api/loja/produtos/{id}`, `POST /api/loja/carrinho/itens`, `GET`/`POST /api/loja/favoritos/{produtoId}` e `POST /api/loja/produtos/{id}/orcamentos`. No Domain: cálculos da fase 2, a busca da loja, o item do carrinho, o pedido de orçamento e as constantes de status. |
| `crialab-plataforma/crialab-docs/` | Plano, histórico e este contexto. |
| `arquivos-crialab-dm/` | Fotos, planilhas, PDFs e material de redes sociais. Referência humana. Nenhum projeto lê esta pasta. |
| `.cursor/` | Skill e regras ativas. A cópia versionada está em `crialab-plataforma/.cursor/`. |

A pasta pai `CriaLab` não é um repositório Git. O Git está em `crialab-plataforma/`. O Razor que a conversão lê está em `crialab-plataforma-old/`.

## Regras da conversão

- O plano manda: `crialab-plataforma/crialab-docs/planejamento/conversao-razor-para-vue-api.md`.
- Recriar o comportamento que o Razor já tem. Uma melhoria de regra de negócio fica fora da conversão e entra como lacuna, para decisão humana.
- Banco PostgreSQL, tabelas e colunas em snake_case, sem renomear o que já existe.
- Durante a construção, a API usa o banco `crialab_api`. O Razor continua em `crialab`.
- EF Core grava, migra e cuida do Identity. Dapper só lê listas, busca e indicadores.
- Carrinho e favorito anônimo ficam no navegador. Preço, frete, cupom e total são recalculados no servidor.
- `arquivos-crialab-dm` não entra em código, seed nem static files.
- As fotos e a marca que a loja já usa estão em `crialab-plataforma-old/src/CriaLab.Web/wwwroot` (`fotos/`, `marca/`). Essas sim migram para o storage da API.
- Fiscal continua aba de Pedidos. Papéis continua aba de Usuários.
- Texto visível em português.
- Ao terminar um bloco, marcar o item no plano, reler o PageModel Razor correspondente e registrar o que faltou em `planejamento/lacunas.md`.
- Registrar o trabalho em `crialab-plataforma/crialab-docs/historico/`, na pasta do ano e do mês, e atualizar `historico/INDEX.md`.

## O que não fazer nesta etapa

- Não apagar `crialab-plataforma-old`.
- Não apontar a API para o banco `crialab` antes do corte descrito no plano.
- Não colocar o frontend de volta na pasta do Razor. Frontend, backend e docs ficam no repositório `crialab-plataforma`.
- Não commitar nem dar push sem pedido explícito.
