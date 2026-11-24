# Esqueleto da solução da API

- **Data:** 2026-11-06 20:38 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

O Razor foi conferido compilando, com os três testes aprovados e sem mudança de comportamento. A solução da API passou a existir em `crialab-backend`, fora desse repositório.

## Decisões

- O checkbox de manter o Razor fecha com essa conferência: build, testes e árvore Git limpa. As fases seguintes continuam sem alterar o comportamento descrito na seção 7 do plano.
- A solução usa o formato `CriaLab.sln`. Carter, EF Core, Identity, Dapper, banco e endpoints ficam para os próximos itens.
- `storage` existe vazio. A cópia dos arquivos de `wwwroot` continua no item que serve `/fotos`, `/parceiros` e `/marca`.

## O que foi feito

- `crialab-backend/CriaLab.sln` com Domain, Application, Infrastructure, Api, Domain.Tests e Api.Tests.
- Pastas de casos de uso em `CriaLab.Application/Features` e `CriaLab.Api/Features`.
- `dotnet test` da solução da API compilou os seis projetos. Os projetos de teste ainda não têm casos.
- README do backend, contexto para agentes, lacunas e o plano atualizados.
