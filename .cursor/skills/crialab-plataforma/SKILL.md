---
name: crialab-plataforma
description: >-
  Contexto e fluxo de trabalho do monorepo Cria Lab (crialab-plataforma):
  loja estática HTML/JS e app ASP.NET Core Razor + PostgreSQL. Use always when
  the user asks to change, create, fix, review, document, or work on anything in
  crialab-plataforma, estatico, CriaLab.Web, Financeiro,
  Patrimonio, Pedidos, Fiscal, favoritos, seed, or Cria Lab DM.
---

# Cria Lab — crialab-plataforma

Skill obrigatória para qualquer tarefa neste projeto. Objetivo: continuidade entre sessões, README como fonte de verdade e histórico consultável.

Cópia canônica versionada neste repositório. O workspace `ctgi` pode espelhar em `.cursor/skills/crialab-plataforma/` e `.cursor/rules/crialab-plataforma.mdc` para descoberta pelo agente.

## Fluxo obrigatório

Copiar e acompanhar:

```
Crialab task:
- [ ] 1. Ler crialab-plataforma/README.md (contexto principal)
- [ ] 2. Se a tarefa for no Razor/Postgres, ler a seção ASP.NET do README.md
- [ ] 3. Consultar docs/historico/INDEX.md se precisar de histórico recente
- [ ] 4. Implementar a tarefa
- [ ] 5. Atualizar README.md se arquitetura, stack, estrutura ou estado mudou
- [ ] 6. Registrar entrada em docs/historico/ (ver modelo abaixo)
- [ ] 7. Atualizar docs/historico/INDEX.md com o link da nova entrada
```

**Não iniciar implementação sem o passo 1.** Se o README estiver incompleto para a tarefa, completar o README no mesmo trabalho (passo 5).

## Escopo do projeto

Raiz: `crialab-plataforma/` (raiz deste repositório)

| Pasta | Papel |
| --- | --- |
| `estatico/` | Loja HTML/CSS/JS (referência de paridade / Cloudflare) |
| `src/CriaLab.Web/` | ASP.NET Core 10 Razor Pages + EF Core + Identity + PostgreSQL |
| `tests/CriaLab.Web.Tests/` | Testes do cálculo de orçamento |
| `docs/historico/` | Histórico versionado de atividades |
| `.cursor/skills/crialab-plataforma/` | Esta skill |

Detalhes de execução, seed, rotas e checklist: seção ASP.NET de `README.md`.

## Manutenção do README

Arquivo: `README.md` (raiz do repositório).

Manter sempre atualizado com:

- Visão do monorepo e pastas
- Stack e como rodar (estático + Razor)
- Links para README detalhado e para `docs/historico/`
- Estado atual relevante (migração estático → Razor, módulos prontos, avisos operacionais)

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
- Paridade com `estatico/` quando a tarefa for UI/loja/admin espelhada.
- Fiscal fica sob o fluxo de Pedidos (abas); não recriar item Fiscal separado no menu topo.
- Favoritos: cookie anônimo + DB autenticado (`FavoritosService`).
- Parceiros podem ter `Url` (Instagram/site) clicável no rodapé/Sobre.

## Quando atualizar esta skill

Atualizar `SKILL.md` (e o espelho no workspace `ctgi`, se existir) se mudar: pastas do monorepo, fluxo README/histórico, convenções de nome do histórico, ou regras operacionais listadas acima.
