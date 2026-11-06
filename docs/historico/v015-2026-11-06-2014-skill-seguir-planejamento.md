# Skill seguir-planejamento-crialab

- **Versão:** v015
- **Data:** 2026-11-06 20:14 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

A conversão passa a ter skill própria. Ela segue os checkboxes do plano e encerra a resposta com quantas tarefas já estão marcadas, no formato `2 tarefas feitas de 50`.

## Decisões técnicas

- A cópia versionada fica em `.cursor/skills/seguir-planejamento-crialab/`. O espelho da pasta pai repete o fluxo com caminhos sem `../`.
- A contagem lê o plano no disco e inclui smoke e corte.

## Arquivos afetados

- `.cursor/skills/seguir-planejamento-crialab/SKILL.md` — skill nova
- `.cursor/skills/crialab-plataforma/SKILL.md` — aponta a skill da conversão
- `.cursor/rules/crialab-plataforma.mdc` — tarefa de conversão usa essa skill
- `README.md` — cita a skill na seção de conversão
- `../.cursor/skills/seguir-planejamento-crialab/SKILL.md` — espelho
- `../crialab-docs/historico/2026/11/2026-11-06-2014-skill-seguir-planejamento.md` — histórico da pasta pai

## Funcionalidades / comportamento

- Nenhuma tela do Razor mudou.

## README

- [x] README raiz atualizado

## Commits (se houver)

- nenhum neste registro
