# Banco crialab_api

- **Data:** 2026-11-06 21:19 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

O compose do backend sobe PostgreSQL e cria somente `crialab_api`. No PostgreSQL que o Razor já usa, esse banco foi criado vazio e `crialab` continuou com 51 tabelas públicas.

## Decisões

- Usuário e senha são os mesmos do compose do Razor. Nenhuma senha nova foi criada.
- O volume do compose da API é separado do volume do Razor. O script só executa `CREATE DATABASE crialab_api` quando o banco ainda não existe.
- O Docker CLI não está instalado nesta máquina. O `docker compose up` não foi executado. A criação foi conferida no serviço PostgreSQL 18 que já escuta em `localhost:5432`.

## O que foi feito

- `crialab-backend/docker-compose.yml` com o serviço do Postgres e um serviço que garante o banco.
- `crialab-backend/docker/init/01-criar-crialab-api.sh` para a primeira inicialização do volume.
