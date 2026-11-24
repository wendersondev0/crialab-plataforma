# Contagens do seed conferidas

- **Data:** 2026-11-12 00:03 (America/Sao_Paulo)
- **Tipo:** test

## Resumo

Um banco Razor `crialab` e o banco `crialab_api` foram restaurados com a carga demo. As contagens pedidas pelo plano ficaram iguais.

## Decisões

- Contagens conferidas: produtos 8, pedidos 5, lançamentos 229, máquinas 13, usuários 7.
- O teste `ContagensSeedTests` restaura a API e compara com o banco Razor; também trava esses números.

## O que foi feito

- Restauração do Razor via `tools/RestaurarCargaInicial`.
- Restauração da API via `DbSeeder.RestaurarDemoAsync`.
- Teste de regressão das contagens.
