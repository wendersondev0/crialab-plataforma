# Criação da skill e pasta de histórico

- **Versão:** v001
- **Data:** 2026-10-28 16:09 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

Criada a skill Cursor `crialab-plataforma` para obrigar leitura do README antes de qualquer tarefa, manter o README atualizado e registrar histórico versionado em Markdown.

## Decisões técnicas

- Skill canônica versionada em `.cursor/skills/crialab-plataforma/` neste repositório; espelho no workspace `ctgi` para descoberta do agente.
- Regra `alwaysApply` em `.cursor/rules/crialab-plataforma.mdc` (repo + espelho no workspace).
- Histórico em `docs/historico/` com nome `vNNN-YYYY-MM-DD-HHmm-titulo.md` e índice `INDEX.md`.

## Arquivos afetados

- `.cursor/skills/crialab-plataforma/SKILL.md` — skill canônica
- `.cursor/rules/crialab-plataforma.mdc` — regra de acionamento
- `README.md` — visão do monorepo + links skill/histórico
- `docs/historico/INDEX.md` — índice
- `docs/historico/v001-2026-10-28-1609-cria-skill-e-historico.md` — esta entrada
- Espelho workspace: `ctgi/.cursor/skills/crialab-plataforma/` e `ctgi/.cursor/rules/crialab-plataforma.mdc`

## Funcionalidades / comportamento

- Qualquer alteração futura em `crialab-plataforma` deve seguir o checklist da skill (README → implementação → README → histórico).

## README

- [x] README raiz atualizado
- [x] README crialab-dotnet sem mudança necessária

## Commits (se houver)

- `f7fa205` docs: skill Cursor, README e histórico versionado
