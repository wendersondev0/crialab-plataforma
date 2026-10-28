# Minha conta separada da lista de pedidos

- **Versão:** v009
- **Data:** 2026-10-28 18:52 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo
O menu da loja mandava “Minha conta” e “Meus pedidos” para a mesma página com abas. A conta passou a cuidar dos dados do cliente, e os pedidos ficaram numa listagem própria.

## Decisões técnicas
- `/Conta` abre em dados pessoais. As abas são dados, endereços, favoritos, perguntas e orçamentos. `?aba=pedidos` redireciona para `/Conta/Pedidos`.
- Aceitar um orçamento cria o pedido e leva o cliente para a lista de pedidos.
- Notificações de pedido apontam para `/Conta/Pedidos`; orçamento e pergunta continuam em `/Conta` na aba correspondente.
- O item ativo do menu distingue `/Conta` de `/Conta/Pedidos`.

## Arquivos afetados
- `src/CriaLab.Web/Pages/Conta/Index.cshtml` e `Index.cshtml.cs` — conta sem a lista de pedidos
- `src/CriaLab.Web/Pages/Conta/Pedidos.cshtml` e `Pedidos.cshtml.cs` — listagem, filtros e ações do pedido
- `src/CriaLab.Web/Pages/Shared/_LayoutLoja.cshtml` — menu, gaveta e notificações
- `src/CriaLab.Web/Pages/Produto.cshtml.cs` — orçamento enviado volta para a aba de orçamentos
- `src/CriaLab.Web/Pages/Recibo.cshtml` — atalho para meus pedidos
- `src/CriaLab.Web/wwwroot/css/vitrine.css` — grade de quatro indicadores e itens do pedido
- `README.md` — rotas da conta

## Funcionalidades / comportamento
- Minha conta mostra dados, endereços, favoritos, perguntas e orçamentos, com atalho para a quantidade de pedidos.
- Meus pedidos lista só as compras do cliente, com filtros e ações de pagamento, cancelamento, devolução e recibo.

## README
- [x] README raiz atualizado

## Commits (se houver)
- `6dea338` feat: separa minha conta da lista de pedidos
