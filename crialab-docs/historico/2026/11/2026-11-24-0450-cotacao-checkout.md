# Cotação do checkout

- **Data:** 2026-11-24 04:50 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

O checkout autenticado recarrega o preço, aplica cupom e desconto à vista e cota o frete. Sem endereço ou sem frete cotado, a confirmação fica bloqueada. O pedido ainda não é gravado.

## Decisões

- Visitante recebe 401. A tela pede para entrar e aponta para `/conta/entrar?returnUrl=/checkout`. A tela de login continua no próximo bloco da conta.
- O servidor ignora preço e total enviados pelo navegador. O cupom desconhecido fica com desconto zero, como no Razor.
- O desconto à vista entra só em PIX e boleto, sobre o subtotal já sem cupom. Cartão não recebe esse desconto.
- O frete grátis usa o subtotal, antes do cupom. “Atualizar frete / cupom” não exige endereço. “Confirmar pedido” exige endereço e uma cotação válida.
- Confirmar com dados válidos permanece no checkout. Gravar o pedido, a OS e limpar o carrinho é o próximo item.

## O que foi feito

- No navegador, sem sessão, a página pede para entrar. Com a conta do cliente e o carrinho vazio, pede para adicionar itens. Uma peça com preço falso de R$ 1 voltou a R$ 42,00. Sem CEP, o desconto à vista no PIX ficou R$ 4,20 e o total R$ 37,80, com o aviso de CEP. Endereço em branco e CEP vazio bloquearam a confirmação. CEP `00000-000` respondeu que ainda não entregamos. Com `29101-000` e `oficina10`, o PIX ficou cupom R$ 4,20, desconto à vista R$ 3,78, PAC R$ 18,90 e total R$ 52,92. No cartão o desconto à vista zerou e o total foi R$ 56,70. No boleto com SEDEX, o frete foi R$ 32,40 e o total R$ 66,42, sem erro e sem gravar pedido. O F5 limpou o formulário e manteve a peça. Cinco peças (R$ 210) em `29260-000` deixaram PAC, SEDEX e entrega local grátis, total R$ 170,10. Com a API parada, a peça continuou e apareceu “Não foi possível carregar o checkout.” Em 390px a página não teve rolagem horizontal.
