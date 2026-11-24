# Health check da API

- **Data:** 2026-11-06 21:27 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

`GET /api/saude` responde JSON com a situação `disponivel`. O Razor não tem essa rota e não foi alterado.

## Decisões

- A rota não consulta o banco. A connection string da API ainda não está no arquivo de configuração.
- O corpo é campo, não HTML.

## O que foi feito

- Módulo Carter `ModuloSaude`.
- Teste da API confirma status de sucesso, `application/json` e o texto `disponivel`.
