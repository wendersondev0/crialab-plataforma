# Exceção em Problem Details

- **Data:** 2026-11-06 21:57 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

Uma exceção não tratada na API passa a responder Problem Details em português. O texto interno da exceção não sai na resposta. O Razor não foi alterado.

## Decisões

- O título é `Erro interno` e o detalhe é `Ocorreu um erro inesperado.`. O tipo é `about:blank` e o status é 500.
- A rota que força a falha só existe quando a configuração `ExporFalhaDeTeste` está ligada. Ela serve ao teste e não fica disponível na subida normal.

## O que foi feito

- `TratadorExcecaoPortugues` registrado com `UseExceptionHandler`.
- Teste confirma `application/problem+json`, os textos em português e a ausência da mensagem interna.
