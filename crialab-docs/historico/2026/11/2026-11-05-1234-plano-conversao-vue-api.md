# Plano da conversão Razor para API e PWA

- **Data:** 2026-11-05 12:34 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

O app em Razor Pages permanece como fonte do comportamento. A recriação em API separada e PWA Vue foi planejada, com a lista do que existe hoje e a ordem de construção e revisão. A pasta de fotos e planilhas de referência passou a se chamar `arquivos-crialab-dm`.

## Decisões

- A implementação não começa neste registro. O plano é o contrato da conversão.
- `crialab-plataforma` fica na pasta de trabalho até a paridade fechar. O ramo Git já guarda essa versão.
- A API, enquanto é construída, usa o banco `crialab_api`. O Razor continua no banco `crialab`.
- EF Core grava e migra. Dapper lê listas, busca e indicadores.
- Carrinho e favorito anônimo ficam no cliente. O servidor recalcula valores.
- Histórico novo mora em `crialab-docs/historico/`, agrupado por ano e mês.

## O que foi feito

- Plano em `crialab-docs/planejamento/conversao-razor-para-vue-api.md`.
- Arquivo de lacunas, índice desta pasta e contexto para agentes.
- Pastas `crialab-frontend` e `crialab-backend` reservadas, ainda sem código de produto.
- `Arquivos_CriaLabDm` renomeada para `arquivos-crialab-dm`.
- Skill, regra e README do Razor apontam para o plano e para o nome novo da pasta de referência.
