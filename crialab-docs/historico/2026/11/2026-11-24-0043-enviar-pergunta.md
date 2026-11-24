# Pergunta da ficha

- **Data:** 2026-11-24 00:43 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A ficha envia uma pergunta e permanece na peça. Sem sessão o autor é Visitante. Com sessão, o autor é o e-mail da conta, como o `Identity.Name` do Razor.

## Decisões

- Texto em branco não grava e a API responde 400 com “Escreva sua pergunta.” O texto enviado é aparado.
- A resposta nasce vazia. A lista mostra “Aguardando resposta”.
- Peça inexistente responde 404. A pergunta não exige login.
- O nome completo da conta não entra nessa gravação. A lista futura de perguntas da conta já aceita e-mail, nome de usuário e nome completo.

## O que foi feito

- No navegador, no chaveiro sem perguntas: campo vazio não envia; só espaços mostram “Escreva sua pergunta.”; visitante grava “Envia para o exterior?” e o F5 mantém; com o cliente, “Cabe na mochila?” aparece com `cliente@crialab.local`; com a API parada, “Não foi possível enviar a pergunta.”; 390px sem rolagem horizontal.
- As duas perguntas criadas na conferência foram apagadas.
