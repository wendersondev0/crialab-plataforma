# Plano de paridade: estático → Razor (Cria Lab DM)

Objetivo: a loja e a administração no `crialab-dotnet` devem ter **mesma casca visual, mesmos fluxos e mesma cobertura funcional** do `estatico/`, com dados em Postgres + Identity (não `localStorage`).

Referência de verdade do front: HTML/CSS/JS em `estatico/` (`casca.js`, `vitrine.js`, `admin.js`, `oficina.js`, `financeiro.js`, `conta.js`, `checkout.js`, `orcamento.js`, `pedidos-admin.js`, `css/base.css`, `css/vitrine.css`, `css/admin.css`, `css/tailwind.css`).

---

## 0. Princípios

1. **Paridade visual primeiro na casca** (`topo-loja` / `topo-admin`, gaveta, rodapé, tipografia, `admin.css`).
2. **Portar tela a tela** com checklist de aceite = screenshot + fluxo clicável iguais ao estático.
3. **Não inventar UX nova** no admin: reutilizar classes e estrutura do estático (`admin-prod-*`, `fin-*`, `pat-*`, `sys-*`, etc.).
4. **Backend já modelado** (EF + seed): o gap principal é UI/UX + handlers CRUD, não schema.
5. **CSS admin**: o Razor hoje usa shell lateral (`admin-shell`) diferente do estático (topo horizontal + menu). **Padronizar no layout do estático**.
6. Critério de “pronto” por módulo: (a) layout, (b) listagem/filtros, (c) criar/editar/ações, (d) persistência real, (e) empty states.

---

## 1. Mapa estático × Razor (estado atual)

### 1.1 Loja (storefront)

| Estático | Razor | Status |
| --- | --- | --- |
| Home (carrossel, chips, grade) | `Pages/Index` | Parcial — casca/home ok; falta polish fino |
| Busca (filtros, sugestões, ordem) | `Pages/Busca` | Parcial — só `q` + categoria |
| Pronta / Sob demanda / Personalizados | `Pages/Produtos?ModalidadeFiltro=` | Parcial — falta intro rica e URL dedicada opcional |
| Ficha produto | `Pages/Produto` | Parcial — falta frete CEP, Shopee/ML, favorito, orçamento, variações interativas, toast |
| Carrinho | `Pages/Carrinho` | Parcial — sem cupom/frete na UI |
| Checkout | `Pages/Checkout` | Stub — sem PAC/SEDEX, cupom real, formas |
| Conta (abas) | `Pages/Conta/*` | Parcial — faltam favoritos, perguntas, editar dados/endereços, pagar |
| Contato | `Pages/Contato` | Parcial — formulário ok, UI fraca |
| Sobre | `Pages/Sobre` | Quase — revisar contra estático |
| Recibo | *(não existe)* | Ausente |

### 1.2 Administração

| Estático | JS | Razor | Status |
| --- | --- | --- | --- |
| Casca admin (topo + menu + gaveta) | `casca.js` | `_LayoutAdmin` shell lateral | **Diferente** |
| Painel | `admin.js` → `painel` | `Admin/Index` | Stub KPIs |
| Produtos (lista rica) | `admin.js` → `produtos` | `Admin/Produtos` | Tabela crua |
| Produto (form completo) | `admin.js` → `formularioProduto` | `Admin/ProdutoEdit` | Stub |
| Personalizados (atalho/lista) | `admin.js` | *(faltando página)* | Ausente |
| Marketing | `admin.js` → `marketing` | `Admin/Marketing` | Stub |
| Atendimento / Contato | `admin.js` → `contatoAdmin` | `Admin/Atendimento` | Stub |
| Perguntas | `admin.js` → `perguntas` | *(dentro Atendimento ou página)* | Ausente |
| Depoimentos | `admin.js` → `depoimentos` | *(faltando)* | Ausente |
| Pedidos | `pedidos-admin.js` | `Admin/Pedidos` | Stub |
| OS | `pedidos-admin.js` | *(faltando)* | Ausente |
| Orçamentos / Orçamento | `orcamento.js` | *(faltando)* | Ausente |
| Patrimônio | `oficina.js` | `Admin/Patrimonio` | Stub |
| Estoque | `oficina.js` | *(faltando / misturado)* | Ausente |
| Impressão / Impressões | `oficina.js` | `Admin/Impressao` | Stub |
| Financeiro | `financeiro.js` | `Admin/Financeiro` | Stub |
| Mês | `financeiro.js` | *(faltando)* | Ausente |
| Energia | `financeiro.js` | *(faltando)* | Ausente |
| Conciliação | `financeiro.js` | *(faltando)* | Ausente |
| Fiscal | `admin.js` → `fiscal` | `Admin/Fiscal` | Stub |
| Sistema / Configuração | `admin.js` → `configuracao` | `Admin/Sistema` | Stub leitura |

---

## 2. Fase A — Fundação visual admin (obrigatória antes das telas)

**Entrega:** `_LayoutAdmin.cshtml` = casca do estático.

- [x] Remover `admin-shell` lateral como padrão visual.
- [x] Topo branco sticky: hambúrguer, logo, “Ver loja”, nav `menu-admin` com os 9 itens do estático.
- [x] Menu gaveta admin (`menu-gaveta-admin`) no mobile.
- [x] Incluir `tailwind.css` + `base.css` + `admin.css` (hoje falta Tailwind no admin).
- [x] Ícones SVG iguais (`_SvgIcon` / portar mapa do `casca.js`).
- [x] Item ativo com as mesmas classes do estático.
- [x] Auth: só role `Administrador`; redirect loja se não autorizado.

**Aceite:** screenshot do topo admin Razor ≈ estático em desktop e mobile.

**Feito em:** casca admin + painel gerencial (KPIs/atalhos).

---

## 3. Fase B — Storefront (fechar gaps da loja)

Ordem sugerida (impacto no cliente):

### B1. Ficha de produto (alta)
- [x] Galeria + zoom + miniaturas ativas
- [x] Variações clicáveis (cor/tamanho) persistindo escolha no add-to-cart
- [x] Stepper quantidade
- [x] Preço à vista / parcelas / comparativo / oferta
- [x] Simulador frete CEP (PAC/SEDEX) via faixas seed
- [x] Canais Shopee / Mercado Livre
- [x] Favoritar (Identity ou cookie autenticado)
- [x] Pedir orçamento → `OrcamentoCliente`
- [x] Perguntas/avaliações UI completa
- [x] Relacionados da mesma categoria
- [x] Toast “adicionado ao carrinho”

### B2. Busca
- [x] Filtros: categoria, preço min/max, frete grátis, à vista, personalizável, nota, ordem
- [x] Sugestões ao digitar
- [x] Querystring compartilhável (paridade com estático)

### B3. Carrinho + Checkout + Recibo
- [x] Carrinho visual tipo cards (já parcialmente)
- [x] Cupom, CEP, escolha PAC/SEDEX
- [x] Checkout com endereço, forma de pagamento simulada, totais
- [x] Página Recibo (`/Recibo/{id}`) espelhando `recibo.html`

### B4. Conta
- [x] Abas: Pedidos, Orçamentos, Favoritos, Endereços, Perguntas, Meus dados
- [x] CRUD endereço, editar perfil
- [x] Ação “pagar” / status pedido
- [x] Login/Register já existem — alinhar visual `entrada`/cartão

### B5. Polish loja
- [x] Cards: favorito no card, selo oferta, entrega
- [x] Intro modalidades iguais ao estático
- [x] Contato/Sobre revisão final de classes

---

## 4. Fase C — Admin por módulo (ordem de negócio)

### C1. Painel (`Admin/Index`)
Espelhar `painel()`: KPIs por modalidade, mensagens abertas, guia primeira visita (opcional), atalhos para áreas, restaurar seed (somente Dev).

### C2. Produtos + ProdutoEdit (núcleo)
Lista (`produtos()`):
- [x] KPIs filtro modalidade
- [x] Filtros q/modalidade/situação/ordem
- [x] Cards `admin-prod-card` (thumb, selos, preço, estoque, vendas)
- [x] Ações: editar, duplicar, pausar/publicar, excluir
- [x] Atalhos fluxo: Pedidos, Atendimento (Orçamentos/OS na próxima leva)

Form (`formularioProduto()`):
- [x] Campos: título, categoria, modalidade, tipo, preços, estoque, medidas, material, prazo, descrição, situação, flags (oferta, mais vendido, personalizável, a vista, com arquivo)
- [x] Fotos (upload para `wwwroot/fotos/...`)
- [x] Variações (eixos/opções cor-hex ou texto)
- [x] Etapas personalização
- [ ] Preview card

Página opcional **Personalizados** = atalho filtrado da lista (KPI já filtra).

### C3. Pedidos + OS + Orçamentos
- [x] `Admin/Pedidos`: lista, status, nota, recibo, copiar ID
- [x] `Admin/Os`: ordens de serviço ligadas a pedidos
- [x] `Admin/Orcamentos` + `Admin/OrcamentoEdit/{id}`: lista e editor (cálculo simplificado; paridade total do `orcamento.js` fica para polish)

### C4. Atendimento
- [x] Mensagens contato (abrir/responder/fechar)
- [x] Perguntas de produto
- [x] Depoimentos (CRUD + `podeAparecer`) — página própria ou aba

### C5. Marketing
- [x] Parceiros (logo, nome, pode aparecer)
- [x] Conteúdos/menus ajuda se existirem no estático
- [x] O que o `marketing()` do estático gerencia (revisar e portar 1:1)

### C6. Patrimônio + Estoque + Impressão
- [x] Máquinas / patrimônio (`oficina.js` patrimônio)
- [x] Estoque insumos, cores, variantes
- [x] Jobs impressão + lista impressões

### C7. Financeiro
- [x] Painel financeiro (contas, lançamentos, metas)
- [x] Visão por mês (`mes.html`)
- [x] Energia (`energia.html`)
- [x] Conciliação (`conciliacao.html`)
- [x] Reusar classes `fin-*` do `admin.css`

### C8. Fiscal
- [ ] Lista documentos/pedidos com nota
- [ ] Export CSV
- [ ] Marcar pendente / recibo

### C9. Sistema (`configuracao`)
- [ ] Form editável: frete grátis, desconto à vista, parcelas, faixas CEP CRUD
- [ ] Cupons
- [ ] Tipos de insumo
- [ ] Identidade da loja (nome, redes, endereço, horário, CNPJ)
- [ ] Parâmetros custo / margens se presentes no estático

---

## 5. Fase D — Infra e qualidade

- [ ] Upload de imagens (produtos/parceiros) com validação
- [ ] Autorização por página (`[Authorize(Roles = "Administrador")]`)
- [ ] Anti-forgery em todos POSTs
- [ ] Mensagens TempData / flash iguais aos toasts do estático
- [ ] `dotnet watch` documentado para evitar lock de `CriaLab.Web.exe`
- [ ] Testes manuais checklist por módulo (abaixo)
- [ ] (Opcional) testes de integração API/handlers críticos
- [ ] Atualizar `crialab-dotnet/README.md` com mapa de rotas e credenciais

---

## 6. Ordem de execução recomendada (sprints)

| Sprint | Foco | Resultado |
| --- | --- | --- |
| **S0** | Fase A casca admin + Tailwind no admin | Admin “parece” o estático |
| **S1** | C2 Produtos/ProdutoEdit | Operação real da vitrine |
| **S2** | B1 ficha + B3 carrinho/checkout/recibo | Compra ponta a ponta |
| **S3** | C3 Pedidos/OS/Orçamentos + B4 Conta | Pós-venda |
| **S4** | C4 Atendimento + C5 Marketing | Conteúdo e suporte |
| **S5** | C6 Oficina (patrimônio/estoque/impressão) | Operação oficina |
| **S6** | C7 Financeiro (mês/energia/conciliação) | Gestão financeira |
| **S7** | C8 Fiscal + C9 Sistema + B2/B5 polish | Fechamento 100% |

Estimativa grossa: **S0–S2 = MVP comercial**; **S0–S7 = paridade completa**.

---

## 7. Checklist de aceite global

Para cada tela do estático:

1. Abrir estático e Razor lado a lado.
2. Mesmos blocos/seções (nomes e ordem).
3. Mesmas ações principais.
4. Empty state não quebra layout.
5. Mobile: menu gaveta e scroll horizontal de menus ok.
6. Dados persistem após F5 (Postgres).
7. Sem regressão de `.revela` invisível / CSS não carregado.

Inventário de páginas estáticas a bater:

**Loja:** `index`, `busca`, `pronta-entrega`, `sob-demanda`, `personalizados`, `produto`, `carrinho`, `checkout`, `conta`, `contato`, `sobre`, `recibo`.

**Admin:** `index`, `produtos`, `produto`, `personalizados`, `pedidos`, `os`, `orcamentos`, `orcamento`, `marketing`, `contato`, `perguntas`, `depoimentos`, `patrimonio`, `estoque`, `impressao`, `impressoes`, `financeiro`, `mes`, `energia`, `conciliacao`, `fiscal`, `configuracao`.

---

## 8. Riscos e decisões

| Risco | Mitigação |
| --- | --- |
| Portar JS monolítico (~80kb admin + 40kb financeiro…) | Portar por tela; não traduzir tudo de uma vez |
| Upload/fotos | Manter pasta `wwwroot/fotos` compatível com paths do seed |
| Shell admin lateral já existe | Substituir na S0; não manter dois layouts |
| Processo `dotnet run` travando build | Usar `dotnet watch run` ou matar `CriaLab.Web` antes |
| Escopo “100%” vs MVP | Congelar MVP em S2; backlog S3–S7 explícito |

---

## 9. Próximo passo imediato

**Concluído:** S0–S6 — inclui financeiro (C7).

**Seguir com (S7):**
1. Fiscal (C8) + Sistema/configuração (C9)
2. Preview card no ProdutoEdit (restante C2)
3. Fase D (README, watch, checklist)
