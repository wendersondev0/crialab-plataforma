---
name: seguir-planejamento-crialab
description: >-
  Executa a conversão do Cria Lab seguindo o plano em
  crialab-docs/planejamento/conversao-razor-para-vue-api.md, na ordem dos
  checkboxes, e fecha a resposta com o progresso. Use quando a tarefa tocar
  crialab-frontend, crialab-backend, crialab-docs, o plano de conversão, a API,
  o PWA, Vue, Carter, ou quando o usuário pedir para seguir o planejamento.
---

# Seguir planejamento Cria Lab

Esta cópia é a versionada, dentro do repositório `crialab-plataforma`. O espelho ativo, com caminhos a partir da pasta pai `CriaLab`, fica em `../.cursor/skills/seguir-planejamento-crialab/SKILL.md`. Se divergirem, vale esta cópia: alinhe o espelho no mesmo trabalho.

Da pasta pai, o contrato é `crialab-plataforma/crialab-docs/planejamento/conversao-razor-para-vue-api.md`. Da raiz deste repositório, o mesmo arquivo é `crialab-docs/planejamento/conversao-razor-para-vue-api.md`.

O Razor em `../crialab-plataforma-old/` continua o app em execução até o corte do plano. Trabalho só no Razor, sem avançar a conversão, segue a skill `crialab-plataforma`.

## Fluxo

```
Seguir planejamento:
- [ ] 1. Ler o plano e crialab-docs/planejamento/lacunas.md
- [ ] 2. Ler crialab-docs/ia/contexto-para-agentes.md se esta sessão ainda não leu
- [ ] 3. Fazer o primeiro checkbox aberto, na ordem do arquivo
- [ ] 4. Marcar [x] só quando o item estiver feito de ponta a ponta
- [ ] 5. Se faltar comportamento do Razor, registrar em lacunas.md e deixar o checkbox aberto
- [ ] 6. Se o plano, a lacuna ou o código da conversão mudou, registrar em crialab-docs/historico/ e atualizar o INDEX
- [ ] 7. Contar de novo e fechar a resposta com a linha de progresso
```

Na pasta pai `CriaLab`, prefixe `crialab-docs/` com `crialab-plataforma/`.

Não começar pelo meio da lista. Não reabrir as decisões da seção 3. Não alterar os comportamentos da seção 7. Não ler `../arquivos-crialab-dm` a partir do código.

Um checkbox fecha quando a API e a tela pedidas existem, ou quando o próprio item é só documentação e ela está escrita. Revisão da seção 8 do plano continua valendo.

## Contagem

Contar todo checkbox do plano, inclusive smoke e corte. Não contar checkbox de outro arquivo.

Na pasta pai `CriaLab`:

```powershell
$p = "crialab-plataforma/crialab-docs/planejamento/conversao-razor-para-vue-api.md"
$lines = Select-String -Path $p -Pattern '^- \[[ xX]\] '
$feitas = @($lines | Where-Object { $_.Line -match '^- \[[xX]\] ' }).Count
$total = @($lines).Count
"$feitas tarefas feitas de $total"
```

Rodar o comando depois de gravar o plano. Usar o número que o comando imprimir.

## Fecho da resposta

A última linha da resposta ao usuário é só o progresso, com estas palavras e a ordem do exemplo `2 tarefas feitas de 50`:

```
2 tarefas feitas de 50
```

Trocar os números pelos contados. Manter "tarefas feitas de" também quando o primeiro número for 0 ou 1. Sem negrito, sem frase antes nessa linha.
