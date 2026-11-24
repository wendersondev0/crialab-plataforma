# Constantes de status no Domain

- **Data:** 2026-11-23 02:56 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

Os textos de status que o Razor grava e mostra passaram para um único arquivo no Domain, com os mesmos valores.

## Decisões

- Pedido em produção fica `em produção`. OS fica `em producao`, como a tela já faz.
- Impressão inclui `pausada` e `erro`, que o filtro da fila já oferece além da lista da seção 7.
- Mensagem inclui `fechado`, que a tela de atendimento já trata como encerrada. O botão continua gravando `resolvido`.
- Máquina usa `ativa`, `manutencao`, `parada` e `baixada`.

## O que foi feito

- `Status/Status.cs` com pedido, OS, orçamento, mensagem, impressão, produto e máquina.
- Seis testes verdes comparando cada lista com o texto da tela.
