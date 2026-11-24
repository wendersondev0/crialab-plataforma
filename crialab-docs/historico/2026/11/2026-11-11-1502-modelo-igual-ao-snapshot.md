# Modelo igual ao snapshot

- **Data:** 2026-11-11 15:02 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

As entidades e a precisão de dinheiro da API repetem o modelo do Razor. A comparação de colunas com `ApplicationDbContextModelSnapshot` ficou zerada, antes de qualquer seed.

## Decisões

- Diferença de coluna seria lacuna. O teste não achou falta, sobra nem tipo diferente, inclusive `asp_net_users.id` e `produtos.preco`.
- A migration `Inicial` foi aplicada só em `crialab_api`. O banco `crialab` continuou com 51 tabelas públicas.

## O que foi feito

- Entidades de catálogo, comércio, configuração, financeiro e oficina no Domain.
- `ContextoAplicacao` com as mesmas chaves, precisões e relacionamentos do `OnModelCreating` do Razor.
- Teste que lê o snapshot e compara tabela, coluna e tipo.
