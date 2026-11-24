# Listagem por modalidade

- **Data:** 2026-11-23 03:36 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A vitrine por modalidade passou para `GET /api/loja/produtos` e para a tela `/produtos`. Os três atalhos amigáveis redirecionam no Vue como no Razor.

## Decisões

- Sem filtro, a lista traz as peças publicadas ordenadas pelo título e o texto “Todas as peças publicadas na vitrine.”
- Filtro de modalidade inexistente deixa o título “Produtos” e a lista vazia, como a página Razor.
- `/pronta-entrega`, `/sob-demanda` e `/personalizados` apontam para `/produtos` com `ModalidadeFiltro`. O último usa o id `personalizado`.

## O que foi feito

- Endpoint público, com o mesmo conteúdo para anônimo, usuário e administrador.
- Tela com carregando, vazio “Nada publicado nesta lista.”, erro e uma coluna no celular.
- Os cards e os chips de modalidade da home abrem essa lista.
