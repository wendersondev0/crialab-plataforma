# Favorito da ficha

- **Data:** 2026-11-23 04:44 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A ficha alterna o favorito. Sem sessão, a lista fica no navegador. Com access token, a lista fica na tabela `favoritos`.

## Decisões

- Anônimo usa `localStorage` na chave `crialab-favoritos`. Autenticado usa `GET` e `POST /api/loja/favoritos/{produtoId}`. Os dois modos continuam separados. A entrada dos ids anônimos no login fica para o item próprio.
- A ficha lê `crialab-access-token`. Sem esse valor, o botão não chama a API. Com ele, o rótulo segue o banco, mesmo que a lista local tenha o mesmo id.
- O cliente de exemplo não tem favorito no seed. Ana Silva tem `sessao-2026-03-14`. O teste do cliente grava e apaga a própria linha para deixar o banco como estava.
- Peça inexistente responde 404. Anônimo recebe 401. A peça personalizada também mostra o botão.

## O que foi feito

- No navegador: salvar, recarregar e tirar no modo anônimo; com o cliente autenticado, o rótulo ignora a lista local, grava, recarrega e apaga; personalizado mostra o botão; 390px sem rolagem horizontal; com a API parada, a sessão autenticada mostra “Não foi possível salvar o favorito.” e o anônimo continua gravando no navegador.
