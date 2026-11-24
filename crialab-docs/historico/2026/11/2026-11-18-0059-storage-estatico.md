# Storage estático da API

- **Data:** 2026-11-18 00:59 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A API serve `/fotos`, `/parceiros` e `/marca` a partir de `crialab-backend/storage`. A cópia inicial veio do `wwwroot` do Razor.

## Decisões

- Caminhos do seed (`marca/parceiro-vila-verde.png`, `marca/patrimonio/pat-pla.jpg`, fotos de produto) seguem válidos.
- `parceiros/` no wwwroot não existia; a pasta foi criada no storage com um `placeholder.png` para a rota pública.
- Logos de parceiros do seed continuam em `marca/`, como no Razor.

## O que foi feito

- Cópia de `fotos/` e `marca/` do wwwroot para `storage/`.
- `ArmazenamentoEstatico` no pipeline da API.
- Testes cobrindo marca, patrimônio, foto de produto e `/parceiros`.
