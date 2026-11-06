---
name: crialab-plataforma
description: >-
  Contexto e fluxo de trabalho do app Cria Lab (crialab-plataforma):
  ASP.NET Core Razor + PostgreSQL. Use always when
  the user asks to change, create, fix, review, document, or work on anything in
  crialab-plataforma, CriaLab.Web, Financeiro,
  Patrimonio, Pedidos, Fiscal, favoritos, seed, or Cria Lab DM.
---

# Cria Lab — crialab-plataforma

Skill obrigatória para qualquer tarefa neste projeto. Objetivo: continuidade entre sessões, README como fonte de verdade e histórico consultável.

Cópia canônica versionada neste repositório. Os caminhos abaixo são relativos à raiz do repositório.

A pasta de trabalho do Cursor é a pasta pai `CriaLab`, que contém este repositório. O espelho ativo fica em `../.cursor/skills/crialab-plataforma/SKILL.md` e `../.cursor/rules/`, fora do Git, com os mesmos caminhos prefixados por `crialab-plataforma/`. O workspace `ctgi` e os projetos Vue não espelham mais este app. Se o espelho divergir desta skill, vale esta skill: atualize o espelho no mesmo trabalho.

## Fluxo obrigatório

Copiar e acompanhar:

```
Crialab task:
- [ ] 1. Ler README.md (contexto principal)
- [ ] 2. Se a tarefa for no Razor/Postgres, ler a seção ASP.NET do README.md
- [ ] 3. Consultar docs/historico/INDEX.md se precisar de histórico recente
- [ ] 4. Implementar a tarefa
- [ ] 5. Atualizar README.md se arquitetura, stack, estrutura ou estado mudou
- [ ] 6. Registrar entrada em docs/historico/ (ver modelo abaixo)
- [ ] 7. Atualizar docs/historico/INDEX.md com o link da nova entrada
```

**Não iniciar implementação sem o passo 1.** Se o README estiver incompleto para a tarefa, completar o README no mesmo trabalho (passo 5).

Na pasta pai `CriaLab`, prefixe cada caminho deste fluxo com `crialab-plataforma/`.

## Escopo do projeto

Raiz do repositório: esta pasta (`crialab-plataforma` quando vista da pasta pai).

| Pasta | Papel |
| --- | --- |
| `src/CriaLab.Web/` | ASP.NET Core 10 Razor Pages + EF Core + Identity + PostgreSQL |
| `tests/CriaLab.Web.Tests/` | Testes do cálculo de orçamento |
| `docs/historico/` | Histórico versionado de atividades |
| `.cursor/skills/crialab-plataforma/` | Esta skill |
| `.cursor/rules/` | Regra de fluxo e regra de Razor/EF |
| `../arquivos-crialab-dm/` | Fotos, planilhas, PDFs e redes sociais de referência. Os projetos não leem esta pasta |
| `../crialab-docs/` | Plano da conversão para API + PWA, histórico novo e contexto para agentes |
| `../crialab-frontend/` | PWA reservado. Sem código de produto até a fase de esqueleto do plano |
| `../crialab-backend/` | API reservada. Sem código de produto até a fase de esqueleto do plano |

Detalhes de execução, seed, rotas e checklist: seção ASP.NET de `README.md`.

## Manutenção do README

Arquivo: `README.md` (raiz do repositório).

Manter sempre atualizado com:

- Visão do app e pastas
- Stack e como rodar o Razor
- Links para a skill, as regras e `docs/historico/`
- Estado atual relevante (módulos prontos, avisos operacionais)

O mapa de rotas, seed e checklist ficam no `README.md` da raiz.

## Histórico de atividades

Pasta: `docs/historico/`

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

Manter `docs/historico/INDEX.md` com lista do mais recente para o mais antigo:

```markdown
| Versão | Data | Título | Arquivo |
| --- | --- | --- | --- |
| v001 | 2026-10-28 16:09 | … | [arquivo](./arquivo.md) |
```

## Convenções do projeto

- Commits locais em pt-BR (Conventional Commits); não fazer push a menos que o usuário peça.
- Preferir `dotnet watch run` em `src/CriaLab.Web`; matar `CriaLab.Web` se MSB3026 travar o build.
- A loja e a administração em execução vivem no Razor. O PWA e a API nascem nas pastas irmãs `../crialab-frontend` e `../crialab-backend`, fora deste repositório. O contrato está em `../crialab-docs/planejamento/conversao-razor-para-vue-api.md`. Quem for executar esse contrato segue `.cursor/skills/seguir-planejamento-crialab/SKILL.md`.
- Fiscal fica sob o fluxo de Pedidos (abas); não recriar item Fiscal separado no menu topo.
- Favoritos: cookie anônimo + DB autenticado (`FavoritosService`).
- Parceiros podem ter `Url` (Instagram/site) clicável no rodapé/Sobre.
- Forma do código Razor/EF: regra `.cursor/rules/crialab-razor.mdc`.

## Quando atualizar esta skill

Atualizar esta `SKILL.md`, a regra de fluxo e o espelho em `../.cursor/` se mudar: pasta de trabalho, pastas do repositório, fluxo README/histórico, convenções de nome do histórico, ou regras operacionais listadas acima.
