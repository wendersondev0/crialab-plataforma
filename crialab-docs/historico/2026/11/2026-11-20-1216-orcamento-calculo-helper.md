# OrcamentoCalculoHelper no Domain

- **Data:** 2026-11-20 12:16 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

O cálculo de orçamento interno foi portado para `CriaLab.Domain` com os mesmos números e avisos do Razor, inclusive faixa Shopee e lista vazia.

## Decisões

- Fórmula e `ArredondarCimaReal` iguais a `Services/OrcamentoCalculoHelper.cs` do Razor.
- `PrecoCanal` usa `FaixaMarketplace` do Domain; sem inventar Mercado Livre.

## O que foi feito

- `Calculos/OrcamentoCalculoHelper.cs` em `CriaLab.Domain`.
- Três testes em `CriaLab.Domain.Tests`, todos verdes.
