# Plano de paridade: estático → Razor (Cria Lab DM)

Objetivo: a loja e a administração no `crialab-dotnet` devem ter **mesma casca visual, mesmos fluxos e mesma cobertura funcional** do `estatico/`, com dados em Postgres + Identity (não `localStorage`).

Referência de verdade do front: HTML/CSS/JS em `estatico/` (`casca.js`, `vitrine.js`, `admin.js`, `oficina.js`, `financeiro.js`, `conta.js`, `checkout.js`, `orcamento.js`, `pedidos-admin.js`, `css/base.css`, `css/vitrine.css`, `css/admin.css`, `css/tailwind.css`).

---

## 0. Princípios

1. **Paridade visual primeiro na casca** (`topo-loja` / `topo-admin`, gaveta, rodapé, tipografia, `admin.css`).
2. **Portar tela a tela** com checklist de aceite = screenshot + fluxo clicável iguais ao estático.
3. **Não inventar UX nova** no admin: reutilizar classes e estrutura do estático (`admin-prod-*`, `fin-*`, `pat-*`, `sys-*`, etc.).
4. **Backend já modelado** (EF + seed): o gap principal é UI/UX + handlers CRUD, não schema.
5. **CSS admin**: layout do estático (topo horizontal + menu + gaveta).
6. Critério de “pronto” por módulo: (a) layout, (b) listagem/filtros, (c) criar/editar/ações, (d) persistência real, (e) empty states.

---

## 1. Mapa estático × Razor (estado atual)

### 1.1 Loja (storefront)

| Estático | Razor | Status |
| --- | --- | --- |
| Home | `Pages/Index` | OK |
| Busca | `Pages/Busca` | OK |
| Pronta / Sob demanda / Personalizados | `/Produtos?ModalidadeFiltro=` + redirects `/pronta-entrega`, `/sob-demanda`, `/personalizados` | OK |
| Ficha produto | `Pages/Produto` | OK |
| Carrinho | `Pages/Carrinho` | OK |
| Checkout | `Pages/Checkout` | OK |
| Conta | `Pages/Conta/Index` (abas) | OK |
| Contato / Sobre | `Pages/Contato`, `Sobre` | OK |
| Recibo | `Pages/Recibo` | OK |
| Notificações topo | `_LayoutLoja` (pedidos/orçamentos/perguntas) | OK |

### 1.2 Administração

| Estático | Razor | Status |
| --- | --- | --- |
| Casca admin | `_LayoutAdmin` | OK |
| Painel (+ guia + restaurar Dev) | `Admin/Index` | OK |
| Produtos / Produto | `Admin/Produtos`, `ProdutoEdit` (+ preview) | OK |
| Personalizados | `Admin/Personalizados` → filtro | OK |
| Pedidos / OS | `Admin/Pedidos`, `Os` | OK |
| Orçamentos / Orçamento | `Admin/Orcamentos`, `OrcamentoEdit` (fórmula + Site/Shopee) | OK |
| Marketing | `Admin/Marketing` | OK |
| Contato / Perguntas / Depoimentos | `Admin/Atendimento` (+ aliases) | OK |
| Patrimônio / Estoque | `Admin/Patrimonio`, `Estoque` → aba | OK |
| Impressão / Impressões | `Admin/Impressao` (+ choques), `Impressoes` → fila | OK |
| Financeiro / Mês / Energia / Conciliação | páginas dedicadas | OK |
| Fiscal | `Admin/Fiscal` | OK |
| Sistema / Configuração | `Admin/Sistema` (+ restaurar Dev) | OK |

---

## 2. Fase A — Fundação visual admin

- [x] Topo sticky, gaveta, Tailwind + base + admin.css, ícones, AdminOnly

---

## 3. Fase B — Storefront

- [x] B1 Ficha · B2 Busca · B3 Carrinho/Checkout/Recibo · B4 Conta · B5 Polish

---

## 4. Fase C — Admin por módulo

- [x] C1 Painel (KPIs, atalhos, guia 1ª visita, restaurar seed em Dev)
- [x] C2 Produtos + ProdutoEdit + preview
- [x] C3 Pedidos / OS / Orçamentos (fórmula `calculos.js` + cartões Site/Shopee)
- [x] C4 Atendimento (mensagens/perguntas/depoimentos; status **resolvido**)
- [x] C5 Marketing
- [x] C6 Patrimônio / Estoque / Impressão (KPI choques de agenda)
- [x] C7 Financeiro
- [x] C8 Fiscal
- [x] C9 Sistema

---

## 5. Fase D — Infra e qualidade

- [x] Upload validado (`ImageUploadHelper`)
- [x] Autorização AdminOnly
- [x] Anti-forgery (Razor Pages)
- [x] TempData `cta-ok` / `cta-erro`
- [x] `dotnet watch` no README
- [x] Checklist manual no README + §7
- [x] Testes unitários do cálculo de orçamento (`tests/CriaLab.Web.Tests`)
- [ ] (Opcional) testes de integração E2E (checkout/fiscal HTTP)
- [x] README com rotas e credenciais
- [x] Aliases de URL loja/admin iguais aos HTML estáticos
- [x] Restaurar carga inicial (Dev) no Painel e no Sistema

---

## 6. Sprints (histórico)

| Sprint | Foco | Resultado |
| --- | --- | --- |
| S0–S7 | Casca → Fiscal/Sistema | Paridade funcional |
| Polish | Orçamento, seed restore, URLs, notificações, choques | Fechamento de gaps pós-auditoria |

---

## 7. Checklist de aceite global

1. Abrir estático e Razor lado a lado.
2. Mesmos blocos/seções e ações principais.
3. Empty state não quebra layout.
4. Mobile: gaveta e scroll de menus ok.
5. Dados persistem após F5 (Postgres).
6. Sem regressão de `.revela` / CSS.

Inventário estático coberto pelas rotas da §1.

---

## 8. Riscos

| Risco | Mitigação |
| --- | --- |
| Lock `CriaLab.Web.exe` | `dotnet watch run` / matar processo |
| Restaurar seed | Só Development; `EnsureDeleted` + reseed |
| Orçamento vs marketplace | Faixas Shopee no seed; ML não tem faixas seed |

---

## 9. Próximo passo

**Paridade do plano concluída.** Opcional restante:

1. Testes E2E HTTP (checkout, fiscal CSV, restore)
2. Faixas Mercado Livre no seed (cartão ML no orçamento)
3. Checklist §7 visual lado a lado com `estatico/`
