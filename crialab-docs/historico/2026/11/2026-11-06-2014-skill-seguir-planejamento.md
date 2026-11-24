# Skill que segue o plano e conta as tarefas

- **Data:** 2026-11-06 20:14 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

A skill `seguir-planejamento-crialab` passa a executar a conversão na ordem dos checkboxes do plano e a fechar cada resposta com a contagem real, no formato `2 tarefas feitas de 50`.

## Decisões

- A contagem inclui todo checkbox do plano: fases, smoke e corte.
- A linha de progresso é a última da resposta, com os números lidos do arquivo depois de gravar.

## O que foi feito

- Skill em `.cursor/skills/seguir-planejamento-crialab/` e cópia versionada em `crialab-plataforma/.cursor/skills/seguir-planejamento-crialab/`.
- Regra e skill do Razor apontam para ela quando a tarefa é a conversão.
