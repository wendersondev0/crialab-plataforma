# Orçamento da ficha

- **Data:** 2026-11-24 00:20 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A peça personalizada ou personalizável pede orçamento. O registro nasce com status `aguardando`.

## Decisões

- Sem `crialab-access-token`, o botão abre `/conta/entrar` com `returnUrl` da ficha. Com token, `POST /api/loja/produtos/{id}/orcamentos` grava o orçamento e a tela segue para `/conta/orcamentos`. Essa lista ainda não existe.
- Detalhe em branco vira `Sem detalhes`. O nome da primeira etapa de arquivo vai para `arquivo_nome` e também entra no texto dos detalhes, no mesmo formato da ficha Razor.
- O POST do Razor deixa `ArquivoNome` vazio. O item do plano pede o nome do arquivo, então a coluna passa a receber esse nome.
- Peça de pronta entrega sem personalização não mostra o botão. Anônimo recebe 401. Peça inexistente responde 404. O teste apaga a linha que criou.

## O que foi feito

- No navegador: pronta entrega sem o botão; personalizado sem sessão vai ao entrar; com o cliente, medida `8 cm` e arquivo `referencia.png` gravam `Cor: Branco · Arquivo de referência: referencia.png · Medida: 8 cm` e abrem `/conta/orcamentos`; 390px sem rolagem horizontal; com a API parada, “Não foi possível pedir o orçamento.”
