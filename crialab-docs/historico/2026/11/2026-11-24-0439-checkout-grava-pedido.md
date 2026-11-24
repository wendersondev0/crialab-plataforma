# Checkout grava o pedido e abre o recibo

- **Data:** 2026-11-24 04:39 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

Confirmar o checkout grava o pedido `aguardando pagamento`, os itens e a OS `aguardando` sem baixa de insumo. O carrinho local é esvaziado e a tela abre o recibo. O recibo só aparece para o dono ou para o administrador.

## Decisões

- O preço, o cupom, o desconto à vista, o frete e o total continuam recalculados no servidor. O total enviado pelo navegador é ignorado.
- Cada item nasce com `Fisico = true` e `Tipo = pronto`. A cor e o tamanho escolhidos são gravados. Os detalhes do carrinho não vão para o item, como no Razor.
- O cupom gravado fica em maiúsculas. Endereço em branco ou frete sem cotação não grava e devolve as mesmas frases do Razor.
- O id do pedido é `ped-` mais 8 hex. O id do item é 32 hex. A OS nasce `aguardando` com `InsumoBaixado = false`. Nota vazia e frete não repassado.
- Visitante recebe 401. Outra pessoa recebe 404, sem distinguir pedido inexistente de pedido alheio. O administrador vê qualquer recibo, como a página Razor.
- A API devolve campos. O Vue monta a data, a moeda e a frase do recibo. Os atalhos Meus pedidos e Minha conta apontam para as rotas da conta, que continuam no próximo bloco.

## O que foi feito

- No navegador, sem sessão, o checkout pede para entrar. Com a conta do cliente e o carrinho vazio, pede para adicionar itens. Uma peça com preço falso de R$ 1 voltou a R$ 42,00. Sem endereço e sem CEP, a confirmação mostrou as duas mensagens e manteve o carrinho. CEP `00000-000` respondeu que ainda não entregamos, aplicou `oficina10` e não gravou. Com `29101-000`, o PIX ficou cupom R$ 4,20, desconto à vista R$ 3,78, PAC R$ 18,90 e total R$ 52,92. Confirmar abriu `/recibo/ped-56a67bd9`, limpou o carrinho e, no F5, o recibo continuou com CNPJ, endereço, peça, totais e a nota fiscal pendente. O recibo da Ana respondeu que não foi encontrado. Em 390px, recibo e checkout não tiveram rolagem horizontal. Com a API parada, a confirmação manteve a peça e mostrou “Não foi possível confirmar o pedido.”; o recibo mostrou “Não foi possível carregar o recibo.” O administrador foi conferido pelo teste da API: grava o próprio pedido e lê o recibo de outra pessoa.
