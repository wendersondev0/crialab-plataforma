# Banco da API no mesmo PostgreSQL

- **Versão:** v018
- **Data:** 2026-11-06 21:19 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

O README registra que o compose do backend cria só `crialab_api`. O banco `crialab` deste serviço PostgreSQL não foi alterado.

## Decisões técnicas

- A criação do banco ficou no compose de `crialab-backend`, fora deste Git. Nenhuma migration do Razor rodou.

## Arquivos afetados

- `README.md` — a seção de conversão cita o banco `crialab_api`

## Funcionalidades / comportamento

- Nenhuma tela, rota ou regra do Razor mudou. `crialab` segue com as mesmas 51 tabelas públicas.

## README

- [x] README raiz atualizado

## Commits (se houver)

- nenhum neste registro
