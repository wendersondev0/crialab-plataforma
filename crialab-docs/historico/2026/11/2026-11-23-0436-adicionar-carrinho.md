# Adicionar ao carrinho

- **Data:** 2026-11-23 04:36 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A ficha passa a montar o item e gravá-lo no carrinho do navegador.

## Decisões

- `POST /api/loja/carrinho/itens` só normaliza quantidade, cor, tamanho, detalhes e destino, e devolve o preço gravado. O carrinho não entra no banco neste passo.
- A junção continua produto + cor + tamanho + detalhes. Quantidades do mesmo item se somam, inclusive acima do estoque já guardado.
- Peça personalizada não mostra os botões. Pronta entrega com estoque limita o passo da quantidade. Destino `checkout` abre `/checkout`; qualquer outro destino mostra o aviso por 4,2 s, com links para `/carrinho` e `/checkout`. Essas duas telas ainda não existem.
- A carga inicial não tem peça publicada esgotada. O botão desabilita nesse caso, mas o navegador não teve uma peça assim para clicar.

## O que foi feito

- Endpoint público, com a mesma resposta para anônimo, usuário e administrador.
- No navegador: cor PLA branco, quantidade presa em 2, soma no segundo clique, recarga mantém o `localStorage`, comprar agora abre `/checkout` sem aviso, personalizado sem botões, 390px sem rolagem horizontal e erro com a API parada.
