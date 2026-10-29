# Favoritos, orçamentos e perguntas com página própria

- **Versão:** v010
- **Data:** 2026-10-28 21:04 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo
O menu da conta ganhou três itens abaixo de Meus pedidos. Cada assunto ficou numa página só dele, e Minha conta ficou com dados e endereços.

## Decisões técnicas
- Rotas: `/Conta/Favoritos`, `/Conta/Orcamentos` e `/Conta/Perguntas`.
- Os links antigos `?aba=favoritos`, `?aba=orcamentos` e `?aba=perguntas` redirecionam para essas páginas.
- Aceitar um orçamento continua criando o pedido e levando o cliente para `/Conta/Pedidos`.
- Notificação de orçamento aponta para `/Conta/Orcamentos`; pergunta respondida aponta para `/Conta/Perguntas`.

## Arquivos afetados
- `src/CriaLab.Web/Pages/Conta/Favoritos.cshtml` e `.cs` — lista e remoção
- `src/CriaLab.Web/Pages/Conta/Orcamentos.cshtml` e `.cs` — filtros, aceitar e recusar
- `src/CriaLab.Web/Pages/Conta/Perguntas.cshtml` e `.cs` — filtros e resposta
- `src/CriaLab.Web/Pages/Conta/Index.cshtml` e `.cs` — só dados e endereços
- `src/CriaLab.Web/Pages/Shared/_LayoutLoja.cshtml` — menu, gaveta e notificações
- `src/CriaLab.Web/Pages/Shared/_SvgIcon.cshtml` — ícones de orçamento e pergunta
- `src/CriaLab.Web/Pages/Produto.cshtml.cs` — orçamento enviado abre a página de orçamentos
- `README.md` — rotas

## Funcionalidades / comportamento
- No menu da conta, a ordem é Minha conta, Meus pedidos, Favoritos, Orçamentos e Perguntas.
- Favoritos lista as peças guardadas e permite remover.
- Orçamentos lista as propostas, com filtros e ações de aceitar ou recusar.
- Perguntas lista o que o cliente perguntou, separando aguardando e respondidas.

## README
- [x] README raiz atualizado

## Commits (se houver)
- `42e4032` feat: separa favoritos, orcamentos e perguntas da conta
