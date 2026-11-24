# Pastas da conversão dentro do repositório

- **Data:** 2026-11-24 00:36 (America/Sao_Paulo)
- **Tipo:** chore

## Resumo

Frontend, backend e documentação da conversão passaram para dentro do repositório Git. O app Razor foi preservado em `crialab-plataforma-old`.

## Decisões

- O repositório continua no caminho `crialab-plataforma`. O `.git` foi para essa pasta. O Razor, que a conversão ainda lê, ficou em `crialab-plataforma-old` e deixou de ser um repositório.
- O histórico Git ainda contém o Razor. A árvore de trabalho passa a mostrar esses arquivos como removidos até um commit. O corte da seção 9 continua sendo o momento de tirar `crialab-plataforma-old` da pasta de trabalho.
- Skill, regra, plano e contexto de agentes apontam para os caminhos novos. A cópia versionada da skill fica em `crialab-plataforma/.cursor/`.

## O que foi feito

- `crialab-frontend`, `crialab-backend` e `crialab-docs` estão em `crialab-plataforma/`.
- O app Razor está em `crialab-plataforma-old/`, com `src/CriaLab.Web`.
