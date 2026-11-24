# FinanceiroResumo no Domain

- **Data:** 2026-11-20 14:56 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

`FinanceiroResumo` foi portado para `CriaLab.Domain` com as mesmas categorias, filtros de competência e rótulos `geral`, ano e `MMM/yyyy` do Razor.

## Decisões

- Cópia fiel de `Services/FinanceiroResumo.cs` do Razor, usando `LancamentoFinanceiro` do Domain.
- Testes cobrem as seis categorias, `geral`, filtro por ano e `CompetenciaBr` em `MMM/yyyy` pt-BR.

## O que foi feito

- `Calculos/FinanceiroResumo.cs` em `CriaLab.Domain`.
- Cinco testes em `CriaLab.Domain.Tests`, todos verdes.
- Ajuste colateral: `GravadorImagem` no DI, `ArmazenamentoEstatico` no pipeline, upload com `[FromServices]` e testes Admin/Upload em `/api/conta`.
