---
name: crialab-plataforma
description: >-
  Contexto e fluxo de trabalho do app Cria Lab (crialab-plataforma-old):
  ASP.NET Core Razor + PostgreSQL. Use always when
  the user asks to change, create, fix, review, document, or work on anything in
  crialab-plataforma-old, CriaLab.Web, Financeiro,
  Patrimonio, Pedidos, Fiscal, favoritos, seed, or Cria Lab DM.
---

# Cria Lab — crialab-plataforma

Skill obrigatória para qualquer tarefa no app Razor. Objetivo: continuidade entre sessões, README como fonte de verdade e histórico consultável.

Cópia canônica versionada neste repositório. O app Razor está em `../crialab-plataforma-old/`. Este repositório contém `crialab-frontend/`, `crialab-backend/` e `crialab-docs/`.

A pasta de trabalho do Cursor é a pasta pai `CriaLab`. O espelho ativo fica em `../.cursor/skills/crialab-plataforma/SKILL.md` e `../.cursor/rules/`. Se o espelho divergir desta skill, vale esta skill: atualize o espelho no mesmo trabalho.

## Fluxo obrigatório

Copiar e acompanhar. Os caminhos são a partir deste repositório.

```
Crialab task:
- [ ] 1. Ler ../crialab-plataforma-old/README.md (contexto principal)
- [ ] 2. Se a tarefa for no Razor/Postgres, ler a seção ASP.NET do README.md
- [ ] 3. Consultar ../crialab-plataforma-old/docs/historico/INDEX.md se precisar de histórico recente
- [ ] 4. Implementar a tarefa
- [ ] 5. Atualizar ../crialab-plataforma-old/README.md se arquitetura, stack, estrutura ou estado mudou
- [ ] 6. Registrar entrada em ../crialab-plataforma-old/docs/historico/ (ver modelo abaixo)
- [ ] 7. Atualizar ../crialab-plataforma-old/docs/historico/INDEX.md com o link da nova entrada
```

**Não iniciar implementação sem o passo 1.** Se o README estiver incompleto para a tarefa, completar o README no mesmo trabalho (passo 5).

Na pasta pai `CriaLab`, troque `../crialab-plataforma-old/` por `crialab-plataforma-old/`.

## Escopo do projeto

| Pasta | Papel |
| --- | --- |
| `../crialab-plataforma-old/src/CriaLab.Web/` | ASP.NET Core 10 Razor Pages + EF Core + Identity + PostgreSQL |
| `../crialab-plataforma-old/tests/CriaLab.Web.Tests/` | Testes do cálculo de orçamento |
| `../crialab-plataforma-old/docs/historico/` | Histórico versionado de atividades do Razor |
| `.cursor/skills/crialab-plataforma/` | Esta skill |
| `.cursor/rules/` | Regra de fluxo e regra de Razor/EF |
| `../.cursor/skills/crialab-plataforma/` | Espelho da pasta pai |
| `../arquivos-crialab-dm/` | Fotos, planilhas, PDFs e redes sociais de referência. Os projetos não leem esta pasta |
| `crialab-docs/` | Plano da conversão para API + PWA, histórico novo e contexto para agentes |
| `crialab-frontend/` | PWA Vue. Home, busca, listagem e ficha já leem a API |
| `crialab-backend/` | API com Carter, EF Core, Identity, Dapper, FluentValidation, Problem Details e OpenAPI |

Detalhes de execução, seed, rotas e checklist: seção ASP.NET de `../crialab-plataforma-old/README.md`.

## Manutenção do README

Arquivo: `../crialab-plataforma-old/README.md`.

Manter sempre atualizado com:

- Visão do app e pastas
- Stack e como rodar o Razor
- Links para a skill, as regras e `docs/historico/`
- Estado atual relevante (módulos prontos, avisos operacionais)

O mapa de rotas, seed e checklist ficam no README do Razor.

## Histórico de atividades

Pasta: `../crialab-plataforma-old/docs/historico/`

### Nome do arquivo

Formato (UTC-3 / horário de Brasília):

```
vNNN-YYYY-MM-DD-HHmm-titulo-kebab.md
```

- `NNN`: sequência com 3 dígitos (`001`, `002`…). Ver o maior `vNNN` em `INDEX.md` ou na pasta e incrementar.
- `YYYY-MM-DD`: data local
- `HHmm`: hora local (24h)
- `titulo-kebab`: resumo curto em português, sem acentos no nome do arquivo

Exemplo: `v001-2026-10-28-1609-cria-skill-e-historico.md`

### Conteúdo mínimo de cada entrada

```markdown
# Título descritivo

- **Versão:** vNNN
- **Data:** YYYY-MM-DD HH:mm (America/Sao_Paulo)
- **Tipo:** feat | fix | refactor | docs | chore | chore+docs

## Resumo
1–3 frases do que foi feito e por quê.

## Decisões técnicas
- Decisão e motivo (se houver)

## Arquivos afetados
- `caminho/relativo` — o que mudou

## Funcionalidades / comportamento
- Comportamento novo ou corrigido visível ao usuário ou à operação

## README
- [ ] README raiz atualizado / sem mudança necessária

## Commits (se houver)
- `hash` mensagem
```

### INDEX.md

Manter `../crialab-plataforma-old/docs/historico/INDEX.md` com lista do mais recente para o mais antigo:

```markdown
| Versão | Data | Título | Arquivo |
| --- | --- | --- | --- |
| v001 | 2026-10-28 16:09 | … | [arquivo](./arquivo.md) |
```

## Convenções do projeto

- Commits locais em pt-BR (Conventional Commits); não fazer push a menos que o usuário peça.
- Preferir `dotnet watch run` em `../crialab-plataforma-old/src/CriaLab.Web`; matar `CriaLab.Web` se MSB3026 travar o build.
- A loja e a administração em execução vivem no Razor, em `../crialab-plataforma-old/`. O PWA e a API estão em `crialab-frontend/` e `crialab-backend/`. O contrato está em `crialab-docs/planejamento/conversao-razor-para-vue-api.md`. Quem for executar esse contrato segue `.cursor/skills/seguir-planejamento-crialab/SKILL.md`.
- Fiscal fica sob o fluxo de Pedidos (abas); não recriar item Fiscal separado no menu topo.
- Favoritos: cookie anônimo + DB autenticado (`FavoritosService`).
- Parceiros podem ter `Url` (Instagram/site) clicável no rodapé/Sobre.
- Forma do código Razor/EF: regra `.cursor/rules/crialab-razor.mdc`.

## Quando atualizar esta skill

Atualizar esta `SKILL.md` e o espelho em `../.cursor/skills/crialab-plataforma/SKILL.md` se mudar: pasta de trabalho, pastas do repositório, fluxo README/histórico, convenções de nome do histórico, ou regras operacionais listadas acima.
