# Skill e regras saem do workspace ctgi

- **Versão:** v013
- **Data:** 2026-11-04 14:55 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

O Cria Lab passou a viver na pasta `CriaLab`, fora dos workspaces Vue e do `ctgi`. A skill versionada ainda citava o espelho no `ctgi` e misturava caminhos da pasta pai com caminhos do repositório. O espelho da pasta pai estava defasado.

## Decisões técnicas

- A cópia canônica continua em `.cursor/` deste repositório, com caminhos relativos à raiz do Git.
- O Cursor aberto em `CriaLab` usa o espelho `../.cursor/`, que prefixa os caminhos com `crialab-plataforma/`. Esse espelho fica fora do Git porque a pasta pai não é um repositório.
- Não há agente de projeto. O fluxo cabe na skill e nas duas regras; um subagente repetiria o mesmo texto e só seria descoberto na raiz da pasta aberta.
- Não há skill extra. Execução, seed e rotas continuam no README.
- A regra `crialab-razor` cobre só arquivos do app: Razor Pages, menu admin, EF snake_case e favoritos.

## Arquivos afetados

- `.cursor/skills/crialab-plataforma/SKILL.md` — tira o `ctgi` e separa os dois níveis de caminho
- `.cursor/rules/crialab-plataforma.mdc` — aponta o espelho da pasta pai
- `.cursor/rules/crialab-razor.mdc` — convenções ao editar o Razor
- `README.md` — seção de continuidade
- `docs/historico/INDEX.md` — esta entrada
- Espelho local: `CriaLab/.cursor/skills/crialab-plataforma/SKILL.md`, `CriaLab/.cursor/rules/crialab-plataforma.mdc`, `CriaLab/.cursor/rules/crialab-razor.mdc`

## Funcionalidades / comportamento

- Tarefas neste repositório não procuram mais skill nem regra no workspace `ctgi`.
- `Arquivos_CriaLabDm/` fica marcado como referência (fotos e planilhas), não como código.

## README

- [x] README raiz atualizado

## Commits (se houver)

- `e8f35c4` docs: alinha skill e regras com a pasta CriaLab
