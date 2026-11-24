# Frete da ficha

- **Data:** 2026-11-24 01:41 (America/Sao_Paulo)
- **Tipo:** feat

## Resumo

A ficha cota o frete pelo preço da peça. CEP curto, CEP sem faixa, PAC, SEDEX e entrega local seguem o cálculo que já estava no Domain.

## Decisões

- A cotação não grava no banco e não pede sessão. Recarregar a página limpa o resultado, como o Razor.
- CEP inválido responde 200 com a frase do cálculo. Peça inexistente responde 404.
- O preço zera e a tela escreve “grátis” quando o preço da peça alcança `FreteGratisAPartir`. Nenhuma peça do seed chega a R$ 200, então essa frase não apareceu no navegador. O cálculo e a flag `gratis` já cobrem o caso.
- A frase do prazo e do preço é montada no Vue. A API devolve nome, preço, dias, descrição e se o frete é grátis.

## O que foi feito

- No navegador, na peça de R$ 42: CEP vazio pede 8 dígitos; `00000-000` diz que ainda não entregamos; `29101-000` mostra PAC R$ 18,90 em 5 dias úteis e SEDEX R$ 32,40 em 2 dias úteis; `29260-000` inclui Entrega Local a R$ 5,00 em 1 dia útil. O F5 limpa a cotação. Com a API parada, “Não foi possível cotar agora.” Em 390px a página não teve rolagem horizontal.
