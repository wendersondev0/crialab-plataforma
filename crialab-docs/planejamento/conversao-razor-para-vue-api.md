# Conversão do Cria Lab: Razor Pages para API + PWA

- **Data do plano:** 2026-11-05 12:34 (America/Sao_Paulo)
- **Fonte do comportamento:** `crialab-plataforma-old` (ASP.NET Core 10, Razor Pages, EF Core, Identity, PostgreSQL)
- **Destino:** `crialab-plataforma/crialab-frontend` e `crialab-plataforma/crialab-backend`
- **Estado:** plano aprovado como contrato de trabalho. O Razor segue como app em execução. Fase 3 começou: home, busca, listagem, ficha, carrinho, favorito e orçamento da ficha já abrem no Vue.

Este arquivo é a lista de trabalho. Um item só sai de aberto quando o comportamento correspondente do Razor existe na API e na tela, com a revisão da seção 8 marcada.

## 1. Objetivo

Recriar a loja e a administração inteiras, com a API separada do cliente e um PWA instalável no celular e no desktop.

Entram na recriação: rotas, papéis, seed, cálculos, estoque, financeiro, fiscal, oficina, uploads, textos em português e os efeitos colaterais que o Razor já executa. A aparência segue a loja atual (faixa laranja, marca, gaveta no celular, estados vazios) usando PrimeVue, sem copiar jQuery nem Bootstrap.

## 2. Pasta de trabalho

A pasta pai `CriaLab` não é um repositório Git.

| Pasta | Papel |
| --- | --- |
| `.cursor/` | Skill e regras ativas, espelho da cópia versionada |
| `arquivos-crialab-dm/` | Referência humana: `Fotos_Crialab`, `Redes Sociais`, `Planilhas-…`, `Orçamentos PDFs-…`. Os projetos não leem esta pasta |
| `crialab-plataforma/` | Repositório Git. Contém `crialab-frontend/`, `crialab-backend/` e `crialab-docs/` |
| `crialab-plataforma-old/` | App Razor em execução. É a fonte que a lista abaixo compara, até o corte da seção 9 |

O Git está em `crialab-plataforma/`. O histórico desse repositório guarda o Razor. A pasta que a conversão lê é `crialab-plataforma-old/`.

`.qodo/` já existia na pasta pai e fica fora deste plano. Não faz parte do produto.

## 3. Decisões travadas

Estas decisões não voltam a ser discutidas em cada tela. Mudança aqui exige um arquivo novo no histórico.

1. **Dois processos.** O navegador fala só com a API. O Vue não acessa o PostgreSQL.
2. **Mesmo modelo físico.** Tabelas e colunas continuam em snake_case. Nomes atuais de tabela e coluna permanecem. A migration inicial da API é comparada com `ApplicationDbContextModelSnapshot` do Razor antes de qualquer carga. Diferença de coluna é lacuna, não ajuste silencioso.
3. **Banco separado enquanto os dois apps convivem.** O Razor segue em `crialab`. A API usa `crialab_api` no mesmo PostgreSQL. O seed e o “restaurar carga inicial” da API só apagam `crialab_api`, e só com `ASPNETCORE_ENVIRONMENT=Development`.
4. **EF Core e Dapper no mesmo modelo.** EF Core é o dono do schema, das migrations, do Identity e de toda gravação. Dapper lê busca, vitrine, listagens administrativas e os agregados do financeiro e do painel. Um caso de uso não grava pelas duas vias.
5. **Clean Architecture com fatia vertical.** Quatro projetos:
   - `CriaLab.Domain`: entidades, status, cálculos puros (`OrcamentoCalculoHelper`, `FinanceiroResumo`, regras puras de frete e de prazo de vitrine).
   - `CriaLab.Application`: uma pasta por caso de uso, com pedido, validador, handler e resposta.
   - `CriaLab.Infrastructure`: DbContext, configurações, migrations, Identity, Dapper, arquivos, seed.
   - `CriaLab.Api`: módulos Carter, um por área, autenticação, Problem Details e arquivos estáticos.
   Carter chama o handler direto. MediatR não entra.
   FluentValidation fica na Application. OpenAPI nativo do ASP.NET publica o contrato para o Vue.
6. **Identidade.** ASP.NET Identity permanece. Regras de senha copiadas: dígito, minúscula, maiúscula, não alfanumérico, mínimo 8, conta sem confirmação de e-mail. Papéis iniciais `Usuario` e `Administrador`. Cadastro público recebe `Usuario`. Política `AdminOnly` exige `Administrador`. A tela de papéis continua podendo criar outros papéis.
7. **Sessão do browser.** Access token JWT curto e refresh token em cookie `HttpOnly`. No desenvolvimento, o Vite faz proxy de `/api` para a API, então o browser enxerga uma origem só. Em produção, um proxy reverso publica o PWA e a API no mesmo site. Isso mantém o cookie e o service worker simples.
8. **Carrinho.** Pinia persistido em `localStorage`. A chave de junção de item continua produto + cor + tamanho + detalhes. Remover continua por produto + cor + tamanho. O cliente envia os itens; o servidor recarrega preço, frete, cupom, desconto à vista e total. O total enviado pelo cliente é ignorado.
9. **Favoritos.** Anônimo em `localStorage`. Autenticado na tabela `Favoritos`. No login, os ids anônimos entram no banco e o armazenamento local dessa lista é esvaziado. Os dois modos continuam separados até esse momento, como o cookie e o banco já fazem hoje.
10. **Arquivos da loja.** A API serve `/fotos`, `/parceiros` e `/marca` a partir de `crialab-backend/storage`. A cópia inicial vem de `crialab-plataforma-old/src/CriaLab.Web/wwwroot` (`fotos/`, `marca/`, inclusive patrimônio e logos de parceiros). Os caminhos gravados no seed (`marca/parceiro-vila-verde.png`, `marca/patrimonio/pat-pla.jpg`, fotos de produto) continuam válidos. Upload de produto e de logo: JPG, PNG, WEBP ou GIF, até 5 MB, com extensão e content-type validados.
11. **PWA.** `vite-plugin-pwa`. O shell do app entra no precache. Chamadas de API são network-first. Sem rede, o shell e a última vitrine em cache aparecem; carrinho local continua visível; checkout, login e gravações exigem rede. Instalável no celular e no desktop. Ícones a partir de `wwwroot/marca/logo-colorida.png`. Nome: Cria Lab DM.
12. **Interface.** PrimeVue com tema Aura ajustado à faixa laranja atual. Dois layouts: loja e administração. Gaveta no celular. Textos em português. Fiscal é aba de Pedidos, sem item próprio no topo. Papéis é aba de Usuários, sem item próprio no topo. Atalhos antigos viram redirect do Vue Router.
13. **Dinheiro.** `decimal`. Centavos com `MidpointRounding.AwayFromZero`. O arredondamento para cima do orçamento direto permanece o de `ArredondarCimaReal`. Os testes atuais de `OrcamentoCalculoHelper` são portados antes de qualquer edição da fórmula.
14. **Ids.** Os formatos atuais permanecem (`ped-` + 8 hex, `imp-` + 8 hex, ids de seed como `orc-cli-1`). O banco já populado no corte precisa continuar legível.
15. **O que a conversão não acrescenta.** Não há gateway de pagamento: o checkout grava `aguardando pagamento` e o cliente pode marcar `pago`. O card de preço do Mercado Livre permanece ausente, porque o seed não tem faixas desse canal; o cálculo da Shopee e o aviso de faixa vazia permanecem. jQuery, Bootstrap e o CSS de reveal (`.revela`) não são portados; o conteúdo da página nasce visível.
16. **HTML.** A API devolve campos (datas, frete grátis, prazo). O Vue monta a frase. A API não devolve HTML.
17. **Idioma e validação.** Mensagens de validação em português, no mesmo teor das mensagens atuais (“Informe um CEP válido com 8 dígitos.”, “Não entregamos neste CEP ainda.”, “Informe o endereço de entrega.”).

## 4. Forma do código

### Backend

```
crialab-backend/
  docker-compose.yml
  CriaLab.sln
  src/
    CriaLab.Domain/
    CriaLab.Application/
      Features/
        Identidade/
        Catalogo/
        Comercio/
        Conta/
        Atendimento/
        Oficina/
        Financeiro/
        Fiscal/
        Sistema/
    CriaLab.Infrastructure/
    CriaLab.Api/
      Features/
  tests/
    CriaLab.Domain.Tests/
    CriaLab.Api.Tests/
  storage/
    fotos/
    marca/
    parceiros/
```

Cada caso de uso em `Features/.../NomeDoCaso/` contém o pedido, o validador, o handler e a resposta. O módulo Carter correspondente fica em `CriaLab.Api/Features` e só traduz HTTP em chamada de handler.

Precisão de dinheiro e percentual segue `HasPrecision` já usado no `OnModelCreating` do Razor.

### Frontend

```
crialab-frontend/
  src/
    app/            router, layouts, guarda de rota, tema
    shared/         cliente HTTP, formato de moeda e data pt-BR
    stores/         sessão, carrinho, favoritos
    features/
      loja/
      conta/
      admin/
  public/
    icons/
```

Rotas do Vue repetem os caminhos atuais, para o endereço que a pessoa já usa continuar válido:

| Caminho | Tela |
| --- | --- |
| `/` | Home |
| `/busca` | Busca |
| `/produtos` | Listagem, com `modalidade` |
| `/pronta-entrega`, `/sob-demanda`, `/personalizados` | Redirect para a listagem |
| `/produto/:id` | Ficha |
| `/carrinho` | Carrinho |
| `/checkout` | Checkout, exige sessão |
| `/recibo/:id` | Recibo do próprio pedido |
| `/conta` | Dados e endereços |
| `/conta/pedidos`, `/conta/favoritos`, `/conta/orcamentos`, `/conta/perguntas` | Áreas da conta |
| `/conta/entrar`, `/conta/cadastrar`, `/conta/sair` | Auth |
| `/contato`, `/sobre`, `/privacidade` | Institucional |
| `/admin` e filhos | Administração, política AdminOnly |
| `/admin/pedidos`, `/admin/os`, `/admin/orcamentos`, `/admin/fiscal` | Pedidos e abas |
| `/admin/usuarios`, `/admin/papeis` | Usuários e aba de papéis |

Aliases que hoje só redirecionam (`/Admin/Perguntas`, `/Admin/Depoimentos`, `/Admin/Impressoes`, `/Admin/Estoque`, `/Admin/Personalizados`) viram redirects para a tela real.

A API fica sob `/api`. O mapa de cada caso de uso está na seção 6.

## 5. Inventário que a lista cobre

Entidades já existentes, todas recriadas no Domain e no EF:

- Catálogo: `LojaIdentidade`, `Categoria`, `Modalidade`, `MenuAjuda`, `Depoimento`, `Parceiro`, `Produto`, `ProdutoFoto`, `ProdutoEtapaPersonalizacao`, `VariacaoEixo`, `VariacaoOpcao`, `ProdutoCorLegado`, `ProdutoTamanhoLegado`.
- Comércio: `Favorito`, `EnderecoCliente`, `Pedido`, `PedidoItem`, `PedidoOs`, `OrcamentoCliente`, `Avaliacao`, `PerguntaProduto`, `MensagemContato`.
- Configuração: `ConfiguracaoLoja`, `Cupom`, `FaixaCep`, `TipoInsumoConfig`.
- Financeiro: `ContaFinanceira`, `LancamentoFinanceiro`, `MetaFinanceira`, `FechamentoEnergia`, `ParametroCusto`, `MargemCanal`, `DescontoOrcamento`, `ReferenciaMaquina`, `NivelPintura`, `FaixaMarketplace`, `OrcamentoInterno`, `OrcamentoItem`.
- Oficina: `Maquina`, `EstoqueInsumo`, `InsumoCor`, `InsumoVariante`, `ImpressaoJob`.
- Identity: `ApplicationUser` com `NomeCompleto`, `CriadoEm`, `TelefoneContato`, `Documento`, mais as tabelas padrão do Identity.

Serviços puros a portar com teste antes da tela:

- `OrcamentoCalculoHelper` (`Orcar`, `PrecoCanal`, comissão, aviso de R$ 79,99 para R$ 80,00, faixa vazia).
- `FinanceiroResumo` (categorias `gastoFixo`, `energia`, `filamento`, `outroInsumo`, `receita`, `investimento`; competência; totais do mês).
- `FreteService`: CEP com 8 dígitos, prefixo, faixa mais específica, PAC, SEDEX, entrega local, frete grátis a partir do valor da configuração (padrão 200), cupom ativo em maiúsculas.
- Prazo da vitrine hoje em `LojaVitrineHelper`, devolvido como dados, não como HTML.

Seed a portar: `DbSeeder`, `PlanilhaSeedLoader`, `planilha/orcamentos.json`, `planilha/lancamentos.json`, `planilha/energia.json`, `seed-produtos.json`. Logins de desenvolvimento continuam os descritos no README do Razor.

Menu da administração, na mesma ordem: Painel, Produtos, Pedidos, Marketing, Atendimento, Patrimônio, Financeiro, Sistema, Usuários, Impressão, e o atalho Ver loja.

Notificações do topo da loja, para usuário autenticado: orçamento `respondido`; pedido `aguardando pagamento` ou `pago`.

## 6. Lista de trabalho

Marcar aqui. Lacuna encontrada vai para [lacunas.md](lacunas.md), sem desmarcar o item do Razor até o comportamento existir.

### Fase 0 — Pasta e contrato

- [x] Renomear a referência humana para `arquivos-crialab-dm`
- [x] Reservar `crialab-frontend` e `crialab-backend`
- [x] Escrever este plano, o contexto de agentes, o índice de histórico e o arquivo de lacunas
- [x] Apontar skill, regra e README do Razor para este plano
- [x] Manter `crialab-plataforma-old` compilando e sem mudança de comportamento durante as fases seguintes

### Fase 1 — Esqueleto da API

- [x] Solução com Domain, Application, Infrastructure, Api e os dois projetos de teste
- [x] Carter, EF Core + Npgsql, snake_case, Identity, Dapper, FluentValidation, Problem Details, OpenAPI
- [x] `docker-compose` do backend sobe PostgreSQL; cria `crialab_api` sem tocar em `crialab`
- [x] Health check `GET /api/saude`
- [x] Pipeline: exceção vira Problem Details em português
- [x] Comparar o modelo EF com o snapshot do Razor e zerar diferença de coluna antes do seed
- [x] Portar o seed e a restauração de development só em `crialab_api`
- [x] Conferir contagens do seed (produtos, pedidos, lançamentos, máquinas, usuários) contra um banco Razor recém-semeado
- [x] Auth: cadastrar, entrar, sair, renovar, eu. Senha e papéis conforme a decisão 6
- [x] Guarda `AdminOnly` nos módulos administrativos
- [x] Servir `storage` em `/fotos`, `/parceiros`, `/marca` e copiar os arquivos atuais do `wwwroot`
- [x] Upload com as regras da decisão 10, coberto por teste de extensão, content-type e tamanho
- [x] Documentar no README do backend como subir a API e o banco, sem gravar senha nova: reutilizar o usuário de desenvolvimento já usado pelo Razor, via segredo de usuário ou variável de ambiente

### Fase 2 — Cálculos, antes de qualquer tela

- [x] Portar `OrcamentoCalculoHelper` e os testes existentes, com os mesmos números (filamento 10, tempo 0,6, custo 20,6, lucro 10,3, reserva 2,06, direto 33, 1 hora; faixa Shopee)
- [x] Portar `FinanceiroResumo` com testes de categoria, competência `geral`, ano e `MMM/yyyy`
- [x] Portar a cotação de frete e o cupom com testes: CEP curto, CEP sem faixa, PAC, SEDEX, entrega local 29260–29279 a R$ 5, frete grátis, cupom inativo
- [x] Portar o prazo de vitrine como dados (personalizado, sob demanda, pronta entrega, frete grátis pelo preço)
- [x] Constantes de status num único lugar do Domain, com os textos atuais

### Fase 3 — Loja, API e telas

Cada item inclui endpoint, tela Vue, estado vazio, loading, erro e celular estreito.

- [x] Home: identidade, destaques, modalidades, depoimentos e parceiros que podem aparecer
- [x] Busca: painel de filtros, sugestões, chips que removem um filtro, vitrine completa quando não há filtro, ordenação
- [x] Listagem por modalidade e os três redirects amigáveis
- [x] Ficha: fotos, variações, cores e tamanhos legados, etapas de personalização, avaliações, perguntas já respondidas
- [x] Ficha: adicionar ao carrinho (quantidade, cor, tamanho, detalhes, destino)
- [x] Ficha: alternar favorito
- [x] Ficha: pedir orçamento (status `aguardando`, detalhes, nome de arquivo)
- [ ] Ficha: enviar pergunta
- [ ] Ficha: cotar frete do produto
- [ ] Carrinho: listar, somar, remover
- [ ] Checkout, só autenticado: cotar, cupom, desconto à vista em pix e boleto sobre (subtotal − cupom), frete grátis, endereço obrigatório, bloqueio se o frete não foi cotado
- [ ] Checkout grava pedido `aguardando pagamento`, itens, OS `aguardando` com insumo não baixado, limpa o carrinho e abre o recibo
- [ ] Recibo só do dono do pedido
- [ ] Contato: WhatsApp, e-mail e telefone clicáveis; formulário válido grava mensagem `aberto`
- [ ] Sobre e rodapé com parceiros e URL
- [ ] Privacidade e página de erro
- [ ] Notificações do topo da loja
- [ ] Menu gaveta da loja no celular

Itens do checkout que a conversão copia como estão, mesmo parecendo simplificação:

- Cada item do pedido nasce com `Fisico = true` e `Tipo = "pronto"`.
- O pedido também cria `PedidoOs`.
- Formas da tela: PIX à vista, boleto à vista e cartão. O desconto à vista entra só em pix e boleto. Cartão é opção de forma, sem adquirente e sem desconto à vista.

### Fase 4 — Conta

- [ ] Entrar, cadastrar, sair, retorno à página pedida
- [ ] Meus dados
- [ ] Endereços: incluir e remover (rótulo, CEP, linha)
- [ ] Pedidos do cliente, filtros todas / em aberto / andamento / entregues / encerrados
- [ ] Pagar: só `aguardando pagamento` ou `novo` passa a `pago`
- [ ] Cancelar: só os mesmos status abertos passam a `cancelado`
- [ ] Devolver: só `entregue` passa a `devolvido`
- [ ] Favoritos da conta e remover
- [ ] Orçamentos: filtros, aceitar `respondido` com valor (vira `aceito` e cria pedido `aguardando pagamento`), recusar `respondido` (vira `recusado`)
- [ ] Perguntas do cliente: todas, aguardando resposta, respondidas
- [ ] Merge de favoritos anônimos no login
- [ ] Guarda: área da conta exige sessão; recibo e listas só mostram dados daquele usuário

### Fase 5 — Catálogo administrativo

- [ ] Listar produtos, pausar, publicar, excluir, duplicar
- [ ] Criar e editar produto com pré-visualização da foto
- [ ] Remover foto
- [ ] Personalizados como redirect da listagem filtrada
- [ ] Upload recusando tipo ou tamanho inválido, com mensagem clara

### Fase 6 — Pedidos, OS, orçamento interno e fiscal

- [ ] Pedidos: listar, trocar status (`novo`, `aguardando pagamento`, `pago`, `em produção`, `enviado`, `entregue`, `cancelado`, `devolvido`), gravar nota
- [ ] Responder orçamento de cliente com valor
- [ ] OS: lista do que tem OS ou item `personalizado`; detalhe; abrir OS `aguardando` se ainda não existe
- [ ] Concluir OS: pedido `entregue`, OS `concluida`, `InsumoBaixado = true`, e baixa de 0,05 no primeiro insumo ordenado por id, sem saldo negativo
- [ ] Orçamentos internos: listar, criar, editar
- [ ] Cálculo do preço direto e do preço Shopee na edição, com aviso de faixa e com faixa vazia sem inventar Mercado Livre
- [ ] Fiscal como aba de Pedidos: filtros, revisão de nota, exportação CSV
- [ ] Redirects de Perguntas e Depoimentos para Atendimento

### Fase 7 — Marketing e atendimento

- [ ] Cupom: código, percentual, ativo
- [ ] Parceiro: criar e editar, URL, logo, aparecer ou não, remover
- [ ] Textos do menu de ajuda
- [ ] Mensagens: filtrar e trocar status (`aberto`, `em atendimento`, `resolvido`)
- [ ] Responder pergunta de produto
- [ ] Depoimento: criar, editar, mostrar ou ocultar, remover
- [ ] Publicar ou ocultar depoimento e parceiro também a partir de Sistema, como a tela atual faz

### Fase 8 — Oficina

- [ ] Patrimônio: máquinas, status, busca
- [ ] Estoque na mesma área: ajuste de saldo, de cor e de variante, sem saldo negativo
- [ ] `/admin/estoque` redireciona para Patrimônio
- [ ] Impressão: fila, filtros por máquina, status e texto
- [ ] Mudar status; `imprimindo` preenche início se ainda estiver vazio; cancelada tem mensagem própria
- [ ] Concluir: sucesso ou falha, peso real, motivo, kWh, fim. Com peso, baixa PLA em kg (peso / 1000) no insumo de nome `PLA`. Sucesso com produto e estoque maior que zero desconta 1 do estoque do produto
- [ ] Novo trabalho na fila, com fim depois do início
- [ ] Copiar taxa de falha das concluídas para `ConfiguracaoLoja.TaxaFalha`
- [ ] Lançar manutenção
- [ ] `/admin/impressoes` redireciona para Impressão

### Fase 9 — Financeiro e sistema

- [ ] Financeiro: KPIs, tooltips, filtro por período, ano, mês, categoria, texto e ordem
- [ ] Baixar conta, nova conta, meta da competência
- [ ] Venda de balcão, lançamento e compra
- [ ] Estornar lançamento
- [ ] CSV do financeiro
- [ ] Mês: lançamento e estorno na competência
- [ ] Energia: confirmar preço do kWh e fechar competência
- [ ] Conciliação: a leitura que a página atual mostra
- [ ] Sistema: dados da loja, parâmetros de venda, faixas de CEP incluindo entrega local
- [ ] Tipos de insumo: incluir e remover
- [ ] Parâmetro de custo (preço e travado) e margem de canal (percentual e ativo)
- [ ] Cotar um CEP de teste dentro de Sistema
- [ ] Restaurar carga inicial no Painel e em Sistema, só em Development, só no banco da API
- [ ] Guia do painel: mostrar uma vez e guardar por um ano no `localStorage` (`crialab_guia_visto`)
- [ ] Contagens do painel: publicados por modalidade e mensagens `aberto`

### Fase 10 — Usuários e papéis

- [ ] Listar, buscar, criar, editar
- [ ] Bloquear e desbloquear. A própria conta não se bloqueia. O último administrador ativo não se bloqueia. Bloqueio usa lockout de 100 anos
- [ ] Apagar. A própria conta não se apaga. O último administrador não se apaga. Conta com pedido ou orçamento não se apaga: a mensagem pede para bloquear. Ao apagar, saem favoritos e endereços dessa pessoa
- [ ] Papéis: criar, excluir, atribuir. `Usuario` e `Administrador` não se excluem. Papel com pessoas vinculadas não se exclui. A própria conta não tira o próprio `Administrador`. O último administrador não perde o papel
- [ ] Papéis como aba de Usuários
- [ ] Usuário sem `Administrador` recebe 403 nas rotas `/admin` e é enviado ao login

### Fase 11 — PWA, visual e navegação

- [ ] Manifest, ícones, tema e nome Cria Lab DM
- [ ] Instalação no desktop e num viewport de celular
- [ ] Shell abre offline; checkout offline explica que precisa de rede
- [ ] Faixa laranja, logo, selo Administração, gaveta da loja e da administração
- [ ] Empty state não quebra o layout
- [ ] Conteúdo visível no primeiro paint, sem depender de classe de animação
- [ ] F5 mantém sessão (refresh cookie), carrinho e favoritos
- [ ] Atalho Ver loja na administração

### Fase 12 — Revisão de fechamento

A seção 8 é executada por completo, não por amostragem.

- [ ] Cada entidade da seção 5 existe no modelo e no snapshot comparado
- [ ] Cada `OnGet` / `OnPost` do Razor tem caso de uso marcado nesta lista
- [ ] Cada rota do README do Razor abre no Vue ou é redirect
- [ ] Testes de domínio verdes
- [ ] Testes de API dos fluxos que gravam: checkout, aceitar orçamento, concluir OS, concluir impressão, estorno, restaurar seed fora de Development recusado
- [ ] Smoke manual da seção 8 nos dois viewports
- [ ] `lacunas.md` sem linha `aberta`
- [ ] README dos dois projetos descrevem como subir, e este plano marca o corte como pronto

## 7. Comportamentos copiados como estão

A implementação não “corrige” estes pontos. Se alguém quiser mudá-los, o pedido é outro trabalho, registrado em lacuna.

| Comportamento | Onde está hoje |
| --- | --- |
| Item de checkout nasce físico e tipo `pronto` | `Checkout.cshtml.cs` |
| Todo checkout cria OS `aguardando` | `Checkout.cshtml.cs` |
| Aceitar orçamento de cliente cria outro pedido `aguardando pagamento` | `Conta/Orcamentos.cshtml.cs` |
| Concluir OS baixa 0,05 do primeiro insumo | `Admin/Os.cshtml.cs` |
| Concluir impressão baixa PLA pelo nome e 1 de estoque do produto se houve sucesso | `Admin/Impressao.cshtml.cs` |
| Taxa de falha = falhas / concluídas, gravada com 4 casas | `OnPostCopiarTaxaAsync` |
| Frete grátis zera PAC, SEDEX e entrega local | `FreteService` |
| Entrega local na faixa que tiver a flag; seed da cidade usa 29260–29279 a R$ 5 | migration `EntregaLocalCep` e seed |
| Desconto à vista só em pix e boleto, sobre o subtotal já sem cupom. Cartão existe como forma e não recebe esse desconto | `Checkout.cshtml` e `Checkout.cshtml.cs` |
| Remover do carrinho não olha o campo detalhes | `CarrinhoService.Remover` |
| Guia do painel grava o cookie na primeira abertura | `Admin/Index.cshtml.cs` |
| Card de Mercado Livre ausente | README e `PrecoCanal` com lista vazia |
| Pagamento é troca de status, sem adquirente | `Conta/Pedidos` `OnPostPagarAsync` |
| Produto pausado usa a string `pausado`; duplicata nasce pausada | `Admin/Produtos.cshtml.cs` |
| Própria conta não se bloqueia nem se apaga; último administrador permanece; conta com pedido ou orçamento é bloqueada em vez de apagada | `Admin/Usuarios.cshtml.cs` |
| Papéis `Usuario` e `Administrador` não se excluem; papel em uso não se exclui; a pessoa não tira o próprio Administrador | `Admin/Papeis.cshtml.cs` |

Status que a interface já usa e a API deve aceitar e devolver iguais:

- Pedido: `novo`, `aguardando pagamento`, `pago`, `em produção`, `enviado`, `entregue`, `cancelado`, `devolvido`.
- OS: `aguardando`, `em producao`, `concluida`.
- Orçamento de cliente: `aguardando`, `respondido`, `aceito`, `recusado`.
- Mensagem: `aberto`, `em atendimento`, `resolvido`.
- Impressão: `na fila`, `imprimindo`, `concluida`, `cancelada`.
- Produto: `publicado` e `pausado`. Duplicar grava o clone como `pausado`.

## 8. Como revisar, em toda fase e no fim

Depois de cada fase da seção 6, e de novo na fase 12:

1. Abrir o PageModel Razor citado no item e conferir status, efeitos no banco e mensagens.
2. Chamar o endpoint com usuário anônimo, com `Usuario` e com `Administrador`, conforme a tela.
3. Repetir o fluxo na tela: caminho feliz, lista vazia, validação, e o erro de rede quando a fase já tiver PWA.
4. Recarregar a página e ver se o dado continua no PostgreSQL.
5. Olhar a gaveta e o scroll do menu num viewport estreito quando a tela tiver navegação.
6. Marcar o checkbox só se API e tela passaram. Se algo do Razor não couber ainda, escrever a linha em `lacunas.md` e deixar o checkbox aberto.
7. Registrar a fase em `crialab-docs/historico/`, na pasta do ano e do mês, e atualizar o índice.

Smoke mínimo, o mesmo espírito do README atual:

- [ ] Login administrador, painel, produtos, editar com prévia, salvar foto válida, recusar arquivo inválido
- [ ] Loja: busca, ficha, carrinho, checkout, recibo
- [ ] Conta: pedidos, favoritos, endereços, orçamento aceito e recusado, perguntas
- [ ] Admin: pedidos, OS concluída, fiscal com CSV, sistema com loja e faixa de CEP, marketing com cupom
- [ ] Financeiro: lançamento, estorno, CSV, energia
- [ ] Impressão concluída com baixa de PLA
- [ ] Restaurar carga inicial em Development e recusa fora dele
- [ ] PWA instalado abre a home; offline mostra o shell

Dois viewports: largura de desktop e largura de celular. Um print da tela parada não fecha o item; o fluxo precisa ser executado.

## 9. Corte

Só depois da fase 12:

- [ ] Apontar a API para o banco que for o de uso, com backup antes
- [ ] Subir PWA e API atrás do mesmo site
- [ ] Percorrer o smoke mais uma vez nesse ambiente
- [ ] Aí sim tirar `crialab-plataforma-old` da pasta de trabalho. O histórico Git continua com o Razor
- [ ] Atualizar a skill do Cursor para a API e o PWA passarem a ser o app em execução
- [ ] Arquivo de histórico do corte, com a data e o que foi conferido

Até esse arquivo existir, o app em execução continua sendo o Razor.

## 10. Fora deste plano

- Gateway de pagamento, nota fiscal eletrônica de verdade, e card de Mercado Livre.
- Ler ou importar automaticamente `arquivos-crialab-dm`.
- Reescrever regra de negócio que a seção 7 manda copiar.
- Criar repositório Git novo. A pasta pai não é repositório; frontend e backend só ganham Git quando isso for pedido.
