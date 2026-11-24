# Seed e restauração em crialab_api

- **Data:** 2026-11-11 15:11 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

O seed do Razor foi portado para a API. A carga demo e a restauração só atuam no banco `crialab_api`, e a restauração só em Development.

## Decisões

- O seed recusa connection string cujo `Database` não seja `crialab_api`.
- Fora de Development, `RestaurarDemoAsync` lança a mesma regra do Painel/Sistema do Razor.
- Os JSON de produtos e planilha ficam em `Infrastructure/Seeding` e saem no output da API.
- `Seed:Executar` desliga a carga automática nos testes de composição.

## O que foi feito

- `DbSeeder`, `PlanilhaSeedLoader` e os arquivos `seed-produtos.json`, `lancamentos.json`, `orcamentos.json` e `energia.json`.
- Testes: recusa fora de Development, recusa no banco `crialab`, e seed que preenche produtos, pedidos, lançamentos, máquinas e usuários.
