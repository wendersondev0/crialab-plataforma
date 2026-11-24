# Carrinho: listar, somar e remover

- **Data:** 2026-11-24 04:25 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A página `/carrinho` lista os itens guardados no navegador, mostra o subtotal e remove pela chave produto + cor + tamanho.

## Decisões

- O carrinho continua no `localStorage` `crialab-carrinho`. Esta página não grava o carrinho no banco.
- O resumo `POST /api/loja/carrinho/resumo` soma preço × quantidade e ignora o total enviado pelo cliente. Não recarrega preço, frete, cupom nem desconto à vista; isso fica no checkout.
- Remover não olha `detalhes`. Duas linhas da mesma peça, cor e tamanho saem juntas. Cor ou tamanho diferente permanece.
- O botão do Razor só envia o id da peça, então só apagaria linhas sem cor e sem tamanho. A tela Vue envia a cor e o tamanho da linha, que é o que `CarrinhoService.Remover` já compara.
- Sem rede, a lista e o subtotal local continuam visíveis e a página avisa que não atualizou.

## O que foi feito

- No navegador: carrinho vazio oferece “Ver produtos”. Uma peça de R$ 42 na cor areia aparece como `1 × R$ 42,00`, com subtotal R$ 42,00, e permanece no F5. Duas linhas brancas com detalhes diferentes saem no mesmo Remover; a linha teal fica. “Continuar comprando” abre `/produtos`. “Finalizar pedido” abre `/checkout`, que ainda não tem tela. Com a API parada, a linha continua e aparece “Não foi possível atualizar o carrinho.”; “Tentar de novo” limpa o aviso. Em 390px a página não teve rolagem horizontal.
