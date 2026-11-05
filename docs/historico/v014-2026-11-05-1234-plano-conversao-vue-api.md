# Plano da conversão para API e PWA

- **Versão:** v014
- **Data:** 2026-11-05 12:34 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

O comportamento deste Razor passa a ser a fonte de uma recriação em API ASP.NET Core e PWA Vue. O plano, a lista de trabalho e o histórico novo ficam na pasta pai, em `crialab-docs`. Este app continua em execução até o corte descrito nesse plano.

## Decisões técnicas

- A implementação da API e do PWA não começa neste registro.
- Enquanto a conversão não fecha a paridade, a API usará o banco `crialab_api` e este Razor permanece em `crialab`.
- A pasta de referência humana chama-se `arquivos-crialab-dm`.

## Arquivos afetados

- `README.md` — aponta o plano de conversão e o nome novo da pasta de referência
- `.cursor/skills/crialab-plataforma/SKILL.md` — pastas irmãs e caminho do plano
- `.cursor/rules/crialab-plataforma.mdc` — o Razor segue como app em execução até o corte
- `../crialab-docs/planejamento/conversao-razor-para-vue-api.md` — plano e lista de trabalho
- `../.cursor/` — espelho da skill e da regra

## Funcionalidades / comportamento

- Nenhuma tela, rota ou regra do Razor mudou.

## README

- [x] README raiz atualizado

## Commits (se houver)

- nenhum neste registro
