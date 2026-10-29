# Entrega local na faixa de CEP da cidade

- **Versão:** v012
- **Data:** 2026-10-28 21:35 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo
As faixas de CEP em Sistema voltaram a aparecer preenchidas. A faixa 29260–29279 ganhou Entrega Local a R$ 5, com texto para o cliente.

## Decisões técnicas
- A faixa guarda os 5 primeiros dígitos do CEP. A cotação converte o CEP completo para esse prefixo antes de comparar.
- A tela lê o JSON da faixa tanto em camelCase quanto em PascalCase, para os valores não sumirem na grade.
- Entrega Local é um campo da faixa: preço, prazo e descrição. Na faixa da cidade o prazo inicial é 1 dia útil e o preço é R$ 5.
- Bancos já semeados recebem essa faixa na subida, sem apagar as faixas existentes.

## Arquivos afetados
- `src/CriaLab.Web/Data/Entities/ConfigEntities.cs` — campos de Entrega Local na faixa
- `src/CriaLab.Web/Data/Migrations/20261029002620_EntregaLocalCep.cs` — colunas no banco
- `src/CriaLab.Web/Data/Seeding/DbSeeder.cs` — faixa 29260–29279
- `src/CriaLab.Web/Services/FreteService.cs` — opção `entrega-local` na cotação
- `src/CriaLab.Web/Pages/Admin/Sistema.cshtml` — grade com preço, prazo e descrição
- `src/CriaLab.Web/Pages/Produto.cshtml` e `Checkout.cshtml` — texto da entrega para o cliente
- `README.md` — estado do frete

## Funcionalidades / comportamento
- Sistema → Frete lista as faixas com CEP, PAC e SEDEX preenchidos.
- A faixa 29260–29279 (29260-000 a 29279-999) marca Entrega Local a R$ 5, em 1 dia útil, com a descrição da loja.
- No produto, um CEP dessa faixa mostra PAC, SEDEX e Entrega Local. Fora dela, só PAC e SEDEX.

## README
- [x] README raiz atualizado

## Commits (se houver)
- `e85bb84` feat: adiciona entrega local na faixa de CEP da cidade
