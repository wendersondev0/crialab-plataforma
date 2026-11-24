# README da subida da API

- **Data:** 2026-11-20 12:03 (America/Sao_Paulo)
- **Tipo:** docs

## Resumo

O README do backend passou a explicar como subir o banco `crialab_api` e a API, reutilizando o usuário de desenvolvimento do Razor por segredo de usuário ou variável de ambiente, sem inventar senha nova.

## Decisões

- A senha não aparece no README. O texto manda copiar a do Razor.
- `UserSecretsId` entrou em `CriaLab.Api` para o comando `dotnet user-secrets` funcionar.
- `appsettings.Development.json` continua como fallback local; segredo e variável prevalecem.

## O que foi feito

- README do backend com pré-requisitos, Docker, SQL, connection string, `dotnet run`, build e testes.
- Checkbox da Fase 1 no plano marcado.
