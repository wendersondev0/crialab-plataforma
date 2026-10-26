# Planejamento de execução — Cria Lab DM

Site estático da loja da própria Cria Lab. Cada fase abaixo é uma entrega que abre no navegador. A fase seguinte só começa quando o critério da anterior estiver marcado.

| Campo | Valor |
| --- | --- |
| Produto | Cria Lab DM |
| Site | https://crialabdm.com.br |
| Pasta | `d:\projetos\ctgi\crialab-plataforma\estatico` |
| Nesta etapa | HTML, CSS e JavaScript, sem framework e sem backend |
| CNPJ | 68.914.190/0001-96 |
| E-mail | crialab.dm@gmail.com |
| Telefone e WhatsApp | (27) 99653-0954 · https://wa.me/5527996530954 |
| Instagram | https://www.instagram.com/crialab.dm |
| TikTok | https://www.tiktok.com/@crialab.dm |
| YouTube | https://www.youtube.com/@crialabdm |
| Shopee | https://shopee.com.br/shop/1648584425 |
| Mercado Livre | https://www.mercadolivre.com.br/ como exemplo de navegação, até existir a loja real |
| Endereço e horário | vazios, editáveis na administração |

O back `dotnet-crialab-plataforma` e o front `vue-crialab-plataforma` foram apagados, com o git local deles. Este plano não recria Nuxt, PrimeVue, ASP.NET, Carter nem SQL Server.

## Direção vigente

A loja vende a peça. Não vende arquivo 3D (STL, OBJ, FBX, 3MF, STEP nem licença de download). O cliente manda uma imagem só como referência de um personalizado. As frases das fases 1 a 10 que ainda falam em “Comprar arquivo”, Downloads ou “peça com arquivo” descrevem o primeiro corte e deixam de valer.

“Na mesa agora” saiu da home. A fila, a máquina e o andamento ficam só na administração, em Gestão de impressão 3D. O cliente não vê o nome da impressora.

O cliente entra num marketplace simples, no tom da vitrine da Shopee, com a cara da Cria Lab:

1. Início. Carrossel logo abaixo do menu. Em seguida, os cards. Produto pronto: preço fixo e a tag Produto. Personalizado: preço médio, a tag Personalizado e o aviso de que o valor final sai depois do orçamento. Cada card e cada ficha têm link da loja na Shopee, link de exemplo do Mercado Livre e cálculo de frete por CEP.
2. Produtos. Só as peças prontas.
3. Personalizados. Só as encomendas. A carga traz “Chaveiro do seu Cachorro”, “Da imagem para o 3D” e “Action figure personalizada”.

Carrinho e busca ficam na barra laranja. Acima dela, uma faixa fina traz os ícones de WhatsApp, Instagram, TikTok, YouTube, Shopee e Mercado Livre, o aviso de notificações e a conta. Passar o mouse em Conta abre Minha conta, Meus pedidos e Sair.

A administração tem dez áreas:

1. Gestão de produtos. Peças prontas.
2. Gestão de personalizados. Orçamentos, pedidos, ordens de serviço e fabricação.
3. Marketing. Cupom da loja.
4. Atendimento ao cliente. Mensagens e perguntas.
5. Patrimônio e estoque. Impressoras, equipamentos, filamentos, ferramentas e outros insumos.
6. Gestão financeira. Despesas, receitas, vendas, custos, contas e lucratividade.
7. Gestão fiscal e documental. Recibo e nota ainda pendente de transmissão, sem chave e sem protocolo.
8. Administração do sistema. Usuários, permissões, configurações, cadastros e regras. Nesta etapa o acesso continua pelo link, sem senha.
9. Painel gerencial. Contagem da vitrine e atalhos das áreas.
10. Gestão de impressão 3D. Monitoramento (disponível, imprimindo, pausada, com erro, trabalho, progresso, tempo restante e previsão; temperatura só quando houver leitura), mesa (em andamento, próximos, peça, quantidade, material, cor, bico, perfil, ocupação e prioridade), qualidade (sucesso, falha, taxa por máquina, motivo e reimpressão) e histórico (horas, filamento, tempo parado, custo e produtividade). A API da impressora fica para depois; a tela lê a fila local.

## Como acompanhar

1. Fazer uma fase por vez, de cima para baixo.
2. Abrir as páginas no navegador antes de marcar o critério.
3. Servir a pasta com um servidor estático (`python -m http.server 8080` ou Live Server) e usar `http://localhost`. Assim o carrinho e o admin compartilham a mesma memória do navegador. Abrir o arquivo direto no disco separa as páginas.
4. Número de negócio nasce em `dados/` e aparece na configuração. Orçamento e fechamento de energia guardam o valor daquela hora. Mudança de configuração vale para o que vier depois.
5. Dinheiro é número decimal e aparece em real. Data e hora usam o fuso de São Paulo. Texto de tela em português.
6. Alvo de toque com pelo menos 44 px. A página cabe em 360 px, 768 px e 1280 px.

## O que o site é

A vitrine da Cria Lab e o painel do dono.

Quem compra vê home, busca, categorias, ficha, personalização, carrinho, checkout, conta, contato e redes. Quem administra publica produto, cuida do patrimônio das máquinas, do financeiro e do dia: pedido, orçamento, ordem de serviço e estoque. A entrada do dono é o link Administração.

Ficam de fora, em todas as fases: loja de terceiro, slug de outro vendedor, selo comparando lojas, plano, mensalidade, teto de faturamento, fatura de assinatura, revendedor, cupom de indicador, comissão de indicador, saque de parceiro, desconto chamado revendedor, papel de vendedor externo, taxa da plataforma, split e repasse.

## Pastas

```text
crialab-plataforma/
  estatico/
    index.html
    css/
    js/
    dados/
    paginas/
    paginas/admin/
    marca/
    fotos/
  crialab-dotnet/
```

`dados/*.js` é a carga inicial. O que o usuário muda na tela vai para `localStorage`. Recarregar mantém a alteração. Na home da administração, um botão discreto “Restaurar carga inicial” apaga essa memória e volta aos arquivos.

Pagamento, frete dos Correios, nota fiscal e e-mail não chamam serviço. A tela simula o passo e mostra o resultado na hora.

## Marca que entra em `marca/`

Copiar só isto, a partir de `Arquivos_CriaLabDm\Redes Sociais`:

| Arquivo de origem | Uso |
| --- | --- |
| `Imagens da Marca\Logo.png` | logo colorida |
| `Imagens da Marca\Logo Completa.jpeg` | marca completa, se o cabeçalho precisar do conjunto |
| `Imagens da Marca\crilab_logo_melhorado.svg` | vetor colorido |
| `Imagens da Marca\Logo Preto.svg` | logo preta, recibo e fundo claro |
| `Imagens da Marca\Letreiro Preto.svg` | letreiro |
| `Imagens da Marca\Logo e Letreiro Preto.svg` | conjunto preto |
| `Imagens da Marca\mascote.png` | pássaro laranja de óculos, mochila no formato de impressora |
| `template de postagem.png` | template de postagem |

Não copiar vídeo, rascunho de gerador de imagem, print de conversa nem arquivo com nome aleatório. Paleta: laranja `#e8772a`, teal `#148f93`, creme `#f6f1e7`, tinta `#1b2430`, branco `#ffffff`. O mascote aparece em estado vazio, carregamento e no selo da loja.

## Fotos de peça já vendida

Há 106 arquivos `.jpg` em `Arquivos_CriaLabDm\Fotos_Crialab\`, nomeados `IMG_data_hora.jpg`, de 14/03/2026 a 15/10/2026, em 41 dias de sessão. O nome não diz qual peça é.

Na fase 2, copiar todas para `fotos/`, uma pasta por dia (`fotos/2026-05-24/`). A legenda provisória é “Peça da oficina — DD/MM/AAAA”. O dono troca o texto depois; a data da sessão continua gravada no dado da foto.

Doze fichas nomeadas repartem as sessões maiores, para a vitrine ter os três caminhos de compra. O restante aparece na busca como “Peça da oficina”, com a data na legenda.

| Ficha provisória | Caminho | Categoria | Sessão | Fotos |
| --- | --- | --- | --- | --- |
| Peça 24/05 | peça pronta | decoração | 2026-05-24 | 13 |
| Peça 15/10 | peça pronta | chaveiros | 2026-10-15 | 7 |
| Peça 26/09 | peça pronta | utilidades | 2026-09-26 | 6 |
| Peça 13/05 | personalizada | moldes | 2026-05-13 | 4 |
| Peça 28/05 | peça pronta | pintura | 2026-05-28 | 4 |
| Peça 03/06 | peça pronta | utilidades | 2026-06-03 | 4 |
| Peça 08/07 | personalizada | peças sob encomenda | 2026-07-08 | 4 |
| Peça 23/09 | peça pronta | decoração | 2026-09-23 | 4 |
| Peça 10/07 | peça pronta | chaveiros | 2026-07-10 | 3 |
| Peça 31/07 | peça pronta | moldes | 2026-07-31 | 3 |
| Peça 31/08 | personalizada | peças sob encomenda | 2026-08-31 | 3 |
| Peça 12/10 | peça pronta | utilidades | 2026-10-12 | 3 |

Cada ficha vende a peça. Não há preço nem botão de arquivo 3D.

## Regras de venda que a configuração guarda

Valores iniciais:

- Frete grátis a partir de R$ 200 em produto físico.
- Desconto à vista de 10% no PIX e no boleto. Cartão não recebe esse desconto. Parcelas, 3 vezes sem juros, incidem sobre o preço cheio.
- PAC e SEDEX falsos por faixa de CEP, com prazo e preço. CEP inválido mostra erro. Sugestão de faixas: 01000–19999, 20000–28999, 29000–29999, 30000–39999, 40000–69999, 70000–79999, 80000–89999, 90000–99999.

```text
subtotalFisico = soma dos itens físicos depois do desconto de item e do cupom
freteCobrado = 0 quando subtotalFisico >= limite de frete grátis, senão a cotação simulada
descontoAVista = percentual do subtotal de produtos quando a forma for PIX ou boleto
total = subtotal - descontoAVista + freteCobrado
```

Cupom é desconto da própria loja. A carga traz um cupom, por exemplo `OFICINA10`, 10% nos produtos. O frete vale para a peça pronta e para o personalizado. Não existe item digital.

Líquido da venda no site = o que o cliente pagou, menos o frete que a loja repassa quando a configuração marcar esse repasse.

## Fase 1 — Casca e home

A home abre com a marca, o nome e as redes. Ainda não há preço, catálogo nem conta.

Arquivos: `index.html`, `paginas/sobre.html`, `css/base.css`, `js/casca.js`, `marca/`.

`js/casca.js` monta o mesmo cabeçalho e o mesmo rodapé em todas as páginas que vierem depois.

A home da primeira entrega mostrou a marca. A direção vigente troca essa abertura pelo marketplace: menu, busca no topo e carrossel. A fila de impressão não aparece para o cliente.

O rodapé e a página Sobre mostram CNPJ, e-mail, telefone, WhatsApp, Instagram, TikTok, YouTube, Shopee e o link de exemplo do Mercado Livre. Endereço e horário vazios não aparecem.

Critério:

- [x] `dotnet-crialab-plataforma` e `vue-crialab-plataforma` continuam ausentes.
- [x] `index.html` e `paginas/sobre.html` abrem em `http://localhost`.
- [x] Logo colorida, mascote e nome aparecem. As oito saídas (WhatsApp, Instagram, TikTok, YouTube, Shopee, Mercado Livre, e-mail, telefone) são links de verdade.
- [x] CNPJ 68.914.190/0001-96 está no rodapé e em Sobre.
- [x] Em 360 px, 768 px e 1280 px a página não estoura para o lado. Botão e link têm pelo menos 44 px.
- [x] Não existe tela de plano, mensalidade, outra loja nem orçamento.

## Fase 2 — Catálogo, busca e ficha

A vitrine passa a listar as peças das fotos reais.

Arquivos novos: `css/vitrine.css`, `js/vitrine.js`, `js/memoria.js`, `js/dinheiro.js`, `dados/loja.js`, `dados/catalogo.js`, `paginas/busca.html`, `paginas/produto.html`, `fotos/`.

A home vigente abre no carrossel e na grade de produtos. As categorias continuam como atalho. Não há trilho de arquivo nem bloco “Na mesa agora”.

A busca filtra por texto, categoria, faixa de preço, frete grátis, à vista, personalizável e avaliação mínima. Ordena por relevância, preço, vendas e novidade. O filtro fica na URL (`paginas/busca.html?q=&categoria=&precoMin=&precoMax=&freteGratis=&aVista=&personalizavel=&nota=&ordem=`).

A ficha tem galeria com zoom, variação, estoque, preço, preço à vista já com o desconto, parcelas sem esse desconto, frete simulado por CEP, prazo, descrição, medidas, materiais, personalização, perguntas, avaliações com foto e relacionados. A cor de filamento é uma fileira de amostras (cor e nome), não uma lista genérica.

A ficha vende a peça. O produto pronto tem a tag Produto e o preço fixo. O personalizado tem a tag Personalizado, o preço médio e o texto de que o valor final sai no orçamento. Os dois têm Shopee, Mercado Livre e cotação de frete.

Personalização lê as etapas que o dado já traz (medida, cor, texto gravado, arquivo de referência, quantidade, prazo). Cada etapa pode alterar o preço. O cliente escolhe uma imagem do computador; a tela guarda o nome do arquivo. O item segue como “aguardando orçamento”.

Na ficha, um botão abre o WhatsApp da loja com a mensagem “Olá, quero saber sobre [nome da peça]”. Avaliação e pergunta de exemplo vêm no dado, ligadas a pedido entregue falso. Favorito e visto recentemente ficam na memória do navegador.

Critério:

- [x] Card, home, busca e ficha usam jpg de `Fotos_Crialab`. Nenhuma imagem de banco no lugar delas.
- [x] A ficha da sessão 15/10 e a da sessão 24/05 oferecem a peça, sem compra de arquivo.
- [x] Amostra de filamento é botão colorido, com nome visível, e cabe no toque de 44 px.
- [x] `busca.html?categoria=chaveiros&ordem=preco` reabre com o mesmo filtro depois de recarregar.
- [x] Busca sem resultado mostra o mascote e uma frase curta.
- [x] Preço à vista de uma peça de R$ 100 aparece como R$ 90. As 3 parcelas usam R$ 100.
- [x] O HTML da ficha não contém o caminho do arquivo 3D.

## Fase 3 — Carrinho, frete e checkout

Uma página só de fechamento. O resumo fica visível o tempo todo, ao lado no desktop e acima do botão no celular.

Arquivos novos: `js/checkout.js`, `dados/frete.js`, `paginas/carrinho.html`, `paginas/checkout.html`.

Dá para mudar quantidade, tirar item e aplicar o cupom da loja. O checkout pede endereço, CEP, frete, pagamento e mostra o desconto à vista antes de confirmar. Um pedido mistura peça pronta e personalizado. Abaixo do limite, PAC e SEDEX aparecem com prazo e preço. No limite ou acima, o frete da peça vai a zero.

Confirmar não chama banco nem operadora. O pedido nasce com status “aguardando pagamento”. Um botão “Simular pagamento” passa para “pago”. Estoque de peça pronta baixa nesse momento. Personalizado não baixa unidade pronta. Não há download de arquivo 3D.

Critério:

- [x] Carrinho vazio mostra o mascote.
- [x] Peça pronta e personalizado entram no mesmo pedido. O frete soma os dois.
- [x] Com físico abaixo de R$ 200, PAC e SEDEX têm preço. Com físico de R$ 200 ou mais, frete R$ 0.
- [x] PIX e boleto aplicam 10% sobre o subtotal de produtos. Cartão mostra o total sem esse desconto e as parcelas sobre o preço cheio.
- [x] Cupom `OFICINA10` reduz os produtos e entra na conta do frete grátis. Cupom inexistente mostra erro e não altera o total.
- [x] CEP com menos de 8 dígitos mostra erro e não deixa confirmar.
- [x] Em 360 px o resumo e o botão de confirmar aparecem sem cortar o total.
- [x] Recarregar o checkout mantém carrinho, CEP e forma de pagamento.

## Fase 4 — Conta, pedidos e contato

Arquivos novos: `js/conta.js`, `dados/pedidos.js`, `dados/mensagens.js`, `paginas/conta.html`, `paginas/contato.html`.

A conta, sem senha de verdade, guarda um cliente de exemplo e o que a pessoa preencher: pedidos, orçamentos, endereços, favoritos, perguntas, devoluções e dados pessoais. Trocar de “cliente” na própria página basta para a demonstração. Não há seção de downloads.

Orçamento personalizado: a carga já traz um pedido respondido, para o cliente aceitar e virar pedido, e o fluxo novo fica “aguardando orçamento” até a fase 9.

Contato tem os tipos dúvida, sugestão, elogio, reclamação, orçamento e outro. A mensagem cai na lista com status aberto. A resposta do dono entra na fase 5.

Critério:

- [x] Conta sem pedido mostra o mascote.
- [x] O pedido pago na fase 3 aparece em Pedidos com o status e o total daquela hora.
- [x] A conta não oferece download de arquivo 3D.
- [x] Aceitar o orçamento de exemplo cria um pedido. Recusar deixa o orçamento encerrado, sem pedido.
- [x] Enviar o contato grava a mensagem, sobrevive ao recarregar e mostra confirmação na hora.
- [x] Devolução de um pedido entregue de exemplo muda o status para “devolvido”.

## Fase 5 — Administração da vitrine e da configuração

O dono entra por Administração no rodapé. Não há senha nem cobrança da plataforma.

Arquivos novos: `css/admin.css`, `js/admin.js`, `paginas/admin/index.html`, `paginas/admin/produtos.html`, `paginas/admin/produto.html`, `paginas/admin/configuracao.html`, `paginas/admin/perguntas.html`, `paginas/admin/contato.html`, `paginas/admin/depoimentos.html`.

Publicar, pausar e esgotar. Campos: tipo (pronto ou personalizado), título, descrição, preço, preço comparativo, estoque, medidas, peso, material, prazo, fotos, variações de cor, tamanho e filamento. A legenda da foto mostra a data da sessão e pode ser editada. O cadastro não oferece arquivo 3D.

Perguntas do anúncio são respondidas aqui. Avaliação nova só nasce ligada a um pedido entregue da carga. Parceiros e depoimentos têm o interruptor “pode aparecer no site”; desligado, somem da home e de Sobre.

A configuração edita frete grátis, desconto à vista, faixas de CEP, cupom, parcelas e “repassar frete no líquido”. O botão de restaurar a carga inicial fica nesta home, discreto.

Critério:

- [x] Pausar a peça 24/05 tira ela da busca e a ficha direta explica que está pausada.
- [x] Esgotar zera a compra da peça pronta e mantém a ficha visível.
- [x] Trocar o frete grátis para R$ 50 muda o checkout de um carrinho novo. O pedido já pago continua com o frete que tinha.
- [x] Depoimento com o interruptor desligado não aparece no site. Ligado, aparece.
- [x] Resposta de pergunta surge na ficha. Mensagem de contato vai de aberto para em atendimento e resolvido.
- [x] Restaurar a carga inicial desfaz pausa, depoimento e frete grátis alterados.

## Fase 6 — Financeiro no formato do caderno

A tela segue os blocos da planilha, não um cadastro genérico de ERP: gastos fixos, energia, filamentos, outros insumos e vendas. “Mensalidade” da planilha entra como energia.

Arquivos novos: `js/financeiro.js`, `dados/financeiro.js`, `paginas/admin/financeiro.html`, `paginas/admin/mes.html`, `paginas/admin/energia.html`, `paginas/admin/conciliacao.html`.

A carga reproduz `Despesas e Receitas Mensais.xlsx`, março a setembro de 2026. O mês vale pelo nome escrito na aba, não pelo texto cortado em 31 caracteres:

| Aba no arquivo | Mês |
| --- | --- |
| Vendas Mensal - Março de 2026 ( | março 2026 |
| Gastos Mensal - Março de 2026 ( | março 2026 |
| Vendas Mensal - Abril de 2026 ( | abril 2026 |
| Gastos Mensal - Abril de 2026 ( | abril 2026 |
| Vendas Mensal - Maio de 2026 (f | maio 2026 |
| Gastos Mensal - Maio de 2026 (f | maio 2026 |
| Vendas Mensal - Junho de 2026 ( | junho 2026 |
| Gastos Mensal - Junho de 2026 ( | junho 2026 |
| Vendas Mensal - Julho de 2026 ( | julho 2026 |
| Gastos Mensal - Julho de 2026 ( | julho 2026 |
| Vendas Mensal - Agosto de 2026 | agosto 2026 |
| Gastos Mensal - Agosto de 2026 | agosto 2026 |
| Vendas Mensal - Setembro de 202 | setembro 2026 |
| Gastos Mensal - Setembro de 202 | setembro 2026 |

Cada gasto fixo tem data, produto, tipo, link, quantidade e valor. Filamentos: PLA, PETG e TPU. Preço médio do mês = soma do valor ÷ soma da quantidade. Quantidade 0 deixa o preço médio em 0. Outros insumos aceitam tipo livre, já com NFC, papel fotográfico, barbante, copo, verniz, lâmpada LED, argola, ímã, caixa P, caixa M, caixa G, sacola kraft P e sacola kraft G. Dá para criar tipo novo.

Categorias: gasto fixo, filamento, outro insumo, receita de venda, investimento em patrimônio, energia.

Compras que saem do resultado operacional e continuam no caixa:

- Bambu Lab A1 com AMS Lite, março, R$ 4.437,66
- Anycubic Kobra S1, julho, R$ 5.359,93
- Epson EcoTank L3250, agosto, R$ 1.071,00

Caneta 3D, se estiver na planilha, permanece gasto. Não vira patrimônio.

```text
receita = soma das receitas do mês
despesaOperacional = energia + filamentos + outros insumos
investimento = compras marcadas como patrimônio
gastosFixos = o resto dos gastos fixos
resultadoOperacional = receita - despesaOperacional
resultadoCaixa = receita - tudo que saiu no mês, inclusive gastos fixos e investimento
```

Os dois resultados aparecem com esses rótulos. A compra da impressora não entra na conta de “prejuízo de produto”.

Preço de referência do insumo, os dois visíveis: média ponderada de todas as compras não estornadas, e mediana dos preços médios mensais.

Energia da carga, quando a planilha tem fórmula:

- setembro, Bambu: 1,1193 × 13,85 kWh
- setembro, Kobra: 1,1193 × 39,86 kWh
- agosto: 0,99 × 33,48 e 0,99 × 41,27
- julho: 1,0373 × 70,14
- junho: 54,25 × 1,0373

Março, abril e maio guardam o valor fechado de energia, sem kWh separado. O lançamento guarda a tarifa e o kWh da época.

A aba Total da planilha está incompleta. A mediana de PLA e de PETG só usa março a julho, e a de PETG inclui o zero de junho: PLA R$ 80,56625 e PETG R$ 60,18875. Quantidade 104,5, receita R$ 2.919,45 e despesa R$ 18.291,50 deixam agosto e setembro de fora. O valor total dos gastos, R$ 23.423,44, soma os sete meses. A conciliação mostra o número da aba ao lado do histórico completo e explica a diferença. O painel usa o histórico completo.

O mês mostra quantidade vendida, receita, despesa operacional, gastos fixos, investimento, resultado operacional, resultado de caixa e lucro. Gráfico de energia mês a mês, em reais e em kWh quando houver medição. Gráfico de lucro com série operacional e série de caixa. Meta de venda: o mês corrente compara com a receita do mês anterior, diz se a meta passada foi batida, mostra o percentual do mês e a projeção pelo ritmo diário. A meta é um campo editável.

Fechamento de energia: a primeira pergunta é o preço atual do kWh. O campo abre com o último preço só como ponto de partida e não grava sem confirmação explícita. Cada máquina tem kWh, potência em watts e custo, todos editáveis. Custo = kWh × preço informado. Ao confirmar, o R$/min sugerido é `(watts / 1000) × preço do kWh / 60`, e o dono pode alterar antes de salvar.

Lançamento avulso: data, descrição, categoria, tipo de insumo, quantidade, valor, link e competência. Filtros por período, categoria, tipo, texto e link. Exportar CSV. Contas a pagar e a receber com baixa. Anexo de comprovante simulado: fica o nome do arquivo no lançamento.

Venda de balcão: data, item, quantidade e valor. Ao salvar, já é receita do mês escolhido.

Critério:

- [x] Setembro de 2026 mostra PLA R$ 72,84142857 por kg (R$ 509,89 em 7 kg) e PETG R$ 64,776 por kg (R$ 971,64 em 15 kg).
- [x] Mês com quantidade 0 de um filamento mostra preço médio R$ 0.
- [x] No mês da Bambu, o resultado operacional não subtrai R$ 4.437,66. O resultado de caixa subtrai.
- [x] A conciliação mostra, lado a lado, PLA R$ 80,56625 e PETG R$ 60,18875 da aba Total, quantidade 104,5, receita R$ 2.919,45, despesa R$ 18.291,50, gastos R$ 23.423,44, e os totais dos sete meses calculados pela tela, com uma frase dizendo que a aba corta agosto, setembro e o zero de junho no PETG.
- [x] Gráfico de energia tem valor em reais nos sete meses e kWh em junho, julho, agosto e setembro.
- [x] Fechar a energia com preço novo pede confirmação. Cancelar deixa o preço anterior intacto. O R$/min sugerido segue a fórmula e aceita um valor digitado por cima.
- [x] Venda de balcão de R$ 40 em setembro aumenta a receita desse mês em R$ 40 e aparece no CSV.
- [x] Filtro por link e por texto acha um gasto fixo conhecido. Exportar CSV baixa o período filtrado.

## Fase 7 — Patrimônio, estoque e impressão

Arquivos novos: `js/oficina.js`, `dados/patrimonio.js`, `dados/estoque.js`, `paginas/admin/patrimonio.html`, `paginas/admin/estoque.html`, `paginas/admin/impressoes.html`.

Máquinas iniciais, nascidas das compras marcadas como patrimônio: Bambu Lab A1 com AMS Lite, Anycubic Kobra S1, secadores e Epson EcoTank L3250. Ficha com watts, depreciação por minuto, manutenção por minuto e energia por minuto do último fechamento.

Depreciação inicial de referência R$ 0,020 por minuto e manutenção R$ 0,010 por minuto, nas fichas da Bambu, da Kobra e dos secadores. São campos editáveis. Outros equipamentos (secador Creality, maçarico, retífica, pinça, alicate, tomada inteligente) entram como patrimônio sem hora de máquina.

Estoque de insumo e de produto pronto, com mínimo e alerta. Entrada pela compra. Saída pela impressão, pela ordem de serviço ou pela venda. Quando o saldo fica no mínimo, a tela oferece “Lançar compra”: abre o bloco de insumos do mês com o item preenchido e só grava se o dono confirmar.

Impressão: máquina, peça, peso previsto, peso real, sucesso ou falha, motivo e kWh. A taxa de falha realizada pode ser copiada para a configuração, só com confirmação. Fila simples por máquina, com aviso de choque de horário. Essa fila alimenta a Gestão de impressão 3D e não a home. Manutenção (bico, PTFE, placa, limpeza) pode virar despesa se o dono confirmar.

Critério:

- [x] As três compras de máquina e a Epson aparecem no patrimônio com os valores da fase 6. A caneta 3D não aparece nessa lista.
- [x] Bambu e Kobra mostram R$ 0,020 e R$ 0,010 por minuto até o dono editar.
- [x] Uma impressão com falha não baixa produto pronto e baixa o filamento usado. Uma com sucesso baixa os dois.
- [x] Dois trabalhos no mesmo horário, na mesma máquina, mostram o choque. Horários diferentes não mostram.
- [x] A home lista o trabalho que está na fila, no lugar dos dois exemplos da fase 1.
- [x] Copiar a taxa de falha realizada pede confirmação e não altera orçamento já fechado.
- [x] Confirmar uma manutenção de bico cria despesa no mês. Cancelar não cria.

## Fase 8 — Orçamento, pintura, Shopee e realizado

A tela e uma carga falsa dos casos de teste. Nesta etapa ninguém lê o Excel. Os arquivos reais ficam em `Planilhas/Orçamentos/2026`, `Base Orçamento.xlsx` e `Base pintura Orçamento.xlsx` só como referência de quem for conferir.

Arquivos novos: `js/orcamento.js`, `dados/orcamento.js`, `paginas/admin/orcamentos.html`, `paginas/admin/orcamento.html`.

Parâmetros iniciais, editáveis. O catálogo de custo ganha e perde item.

| Parâmetro | Valor inicial |
| --- | --- |
| PLA | R$ 85,00 / kg |
| PETG | R$ 65,00 / kg |
| TPU | R$ 80,00 / kg |
| Energia da planilha antiga | R$ 0,005 / min, só referência, substituída pelo fechamento |
| Depreciação | R$ 0,020 / min |
| Manutenção | R$ 0,010 / min |
| Tempo de máquina enquanto não houver fechamento | R$ 0,035 / min |
| Desconto de cliente recorrente | 5% |
| Desconto de volume (4 peças iguais ou pedido a partir de R$ 150) | 5% |
| Saco kraft pequeno | R$ 0,50 |
| Saco kraft grande | R$ 1,1286 |
| Taxa de falha | 5% |

Itens que a carga já traz e o dono pode apagar: cartão, ímã 8×2 mm, ímã 10×2 mm, argola, cola.

Margens, com alta e baixa de tipo:

| Margem | Percentual |
| --- | --- |
| Marketplace, para Shopee e Mercado Livre | 25% |
| Simples | 40% |
| Médio | 50% |
| Complexo | 60% |

Fórmula sem pintura. Cada linha entra uma vez. A planilha antiga soma filamento e embalagem e ainda soma o subtotal que já contém essas linhas; o Cachorro branco de outubro fica R$ 20,43 e preço R$ 30 nessa conta. O site não repete linha.

1. Filamento = (preço do kg / 1000) × (peso da peça + peso do lixo).
2. Tempo de máquina = minutos × (energia por minuto + depreciação + manutenção).
3. Parâmetros = soma de quantidade × preço de cada item escolhido.
4. Custo = serviço + filamento + tempo + parâmetros.
5. Lucro = custo × margem.
6. Reserva de falha = custo × taxa de falha.
7. Valor original = custo + lucro + reserva.
8. Desconto = soma dos percentuais marcados.
9. Preço direto = valor original × (1 − desconto), arredondado para cima no real.
10. Prazo em horas = minutos / 60, para cima. Prazo em dias = horas / 24, para cima.

O preço de anúncio do canal arredonda em centavos, não para o real cheio.

Comissão Shopee de vendedor CNPJ desde 1º de outubro de 2026, só sobre o produto, editável:

| Preço do item | Comissão |
| --- | --- |
| Até R$ 79,99 | 20% + R$ 4,50 |
| De R$ 80,00 a R$ 99,99 | 14% + R$ 16,00 |
| De R$ 100,00 a R$ 199,99 | 14% + R$ 20,00 |
| De R$ 200,00 a R$ 499,99 | 14% + R$ 26,00 |
| A partir de R$ 500,00 | 14% + R$ 26,00 |

Abaixo de R$ 9, o adicional é metade do preço. O subsídio de Pix da Shopee não aumenta o líquido. O preço candidato de cada faixa é `(valor original + fixo) / (1 − percentual)`. Vale o menor candidato que cai dentro da própria faixa. Se cruzar a faixa piorar o líquido, em especial de R$ 79,99 para R$ 80,00, a tela avisa no cartão da Shopee.

Mercado Livre usa a margem de 25% e a mesma tela de faixas, ainda sem faixas preenchidas. Frete pago pelo vendedor no canal começa em zero e é editável.

Na tela, três cartões lado a lado: preço do site, Shopee e Mercado Livre.

Pintura, níveis editáveis:

| Nível | Taxa | Tempo | Cores | Acabamento |
| --- | --- | --- | --- | --- |
| Simples | 20% | cerca de 30 min | até 2 | cor chapada |
| Médio | 40% | 1 h a 2 h | 3 a 5 | sombreamento leve, máscara, lixa e primer |
| Complexo | 60% | mais de 2 h | mais de 5 | degradê, textura, aerógrafo, montagem |

```text
custoBase = custo sem pintura, já com lixo e tempo extra
custoPintura = soma(quantidade × preço) de tinta, primer, verniz, lixa e máscara
taxaPintura = custoBase × percentual do nível
precoComPintura = fórmula padrão sobre (custoBase + custoPintura + taxaPintura)
```

Os dois preços ficam lado a lado. O pedido guarda a escolha.

Filamento do orçamento pode usar preço travado (85, 65 e 80) ou o preço médio atual. Orçamento já enviado não muda quando o preço médio muda. Orçamento novo, depois de um fechamento de energia, usa o R$/min salvo na fase 6 no lugar dos R$ 0,035.

O realizado guarda tempo real, peso na balança, extras, energia da tomada e a diferença contra o orçado.

Caso de teste Cachorro branco, PLA, 367 min, 41,66 g, 0,5 saco pequeno, sem desconto, tempo de máquina ainda em R$ 0,035:

| Etapa | Valor |
| --- | --- |
| Filamento 41,66 g × R$ 0,085 | R$ 3,5411 |
| Saco 0,5 × R$ 0,50 | R$ 0,25 |
| Máquina 367 min × R$ 0,035 | R$ 12,845 |
| Custo | R$ 16,6361, mostrado como R$ 16,64 |
| Margem simples 40%, preço direto | R$ 25 |
| Margem marketplace 25% + falha 5%, valor que precisa sobrar | R$ 21,63 |
| Anúncio Shopee | R$ 32,66 |
| Comissão | R$ 11,03 |
| Líquido | R$ 21,63 |
| Lucro | R$ 4,16 |
| Prazo | 7 horas, 1 dia |

Com recorrente e volume ligados, o preço direto do site vai a R$ 22.

Pintura simples com R$ 2,00 de material, sobre o mesmo custo base e margem simples: preço direto do site R$ 32. O cartão sem pintura continua em R$ 25.

Um segundo caso sintético, “faixa da Shopee”, usa um valor original que empurra o candidato para R$ 80,00 ou mais enquanto R$ 79,99 deixa líquido maior. O cartão da Shopee mostra o aviso e o líquido dos dois preços.

O mascote conduz o primeiro acesso do admin em três passos: lançar uma despesa, ver o mês, fazer um orçamento. Dá para pular. O guia não bloqueia o menu.

Critério:

- [x] Cachorro branco reproduz a tabela acima, inclusive Shopee R$ 32,66, comissão R$ 11,03, líquido R$ 21,63 e lucro R$ 4,16.
- [x] A tela não chega em R$ 20,43 nem em preço R$ 30 para esse caso.
- [x] Com os dois descontos de 5%, o preço direto é R$ 22.
- [x] Pintura simples com R$ 2,00 de material mostra R$ 32 ao lado de R$ 25.
- [x] Os três cartões aparecem juntos. O aviso de faixa fica no cartão da Shopee. O cartão do Mercado Livre usa a margem de 25% e mostra faixas vazias, editáveis.
- [x] Enviar o orçamento, mudar o PLA médio e reabrir: o Cachorro continua em R$ 85/kg. Um orçamento novo pode escolher o médio.
- [x] Depois de um fechamento com R$/min diferente de 0,035, um orçamento novo usa o valor fechado. O Cachorro já salvo permanece em 0,035.
- [x] Realizar com peso e tempo diferentes mostra a diferença em reais contra o orçado.
- [x] O guia de três passos abre uma vez e, pulado, não volta sozinho.

## Fase 9 — Ordem de serviço, recibo e nota pendente

Arquivos novos: `js/pedidos-admin.js`, `paginas/admin/pedidos.html`, `paginas/admin/os.html`, `paginas/recibo.html`.

Status: aguardando pagamento, pago, em produção, enviado, entregue, cancelado, devolvido.

Pedido de peça pronta gera recibo simulado. Personalizado gera ordem de serviço e, ao concluir, recibo. A ordem guarda a escolha de pintura e o preço aceito. Ao terminar, baixa o insumo. Nota fiscal fica “pendente de transmissão”, sem número de autorização fingido.

O recibo é uma página imprimível com a marca preta, CNPJ, itens, frete, desconto à vista e total. Não é PDF de servidor.

O dono responde o personalizado que ficou aguardando na fase 4. O cliente aceita na conta e isso vira pedido.

Critério:

- [x] Pagar uma peça pronta baixa o estoque e libera o recibo. Cancelar antes do pagamento não baixa.
- [x] Concluir a ordem de serviço baixa insumo e não baixa uma unidade de peça pronta.
- [x] O recibo abre a impressão do navegador com logo, CNPJ e o total daquele pedido.
- [x] A nota do pedido mostra “pendente de transmissão” e não mostra chave nem protocolo.
- [x] O dono informa o valor do personalizado pendente, o cliente aceita na conta, e o pedido nasce com esse valor.

## Fase 10 — Acabamento

Revisão visual e de texto. Sem regra nova de dinheiro.

Percorrer home, busca, ficha, carrinho, checkout, conta, contato, sobre, admin, mês, energia, orçamento, recibo e os estados vazios. O mascote entra onde a lista está vazia e no carregamento curto da busca. Conferir 360 px, 768 px e 1280 px. Conferir contraste da tinta no creme e do branco no laranja. Tirar texto em inglês que tenha sobrado. Endereço e horário só aparecem depois que o dono preencher na administração.

Critério:

- [x] Busca sem resultado, carrinho vazio e conta sem pedido mostram o mascote.
- [x] Nenhum número de frete, margem, comissão, kWh ou depreciação está escrito direto no HTML. Todos saem de `dados/` ou da memória.
- [x] As três larguras não cortam botão, total nem tabela principal. Onde a tabela não cabe, ela rola na horizontal dentro do bloco.
- [x] Não há menu, selo ou frase de outra loja, plano, mensalidade ou revendedor.

## Ideias novas adotadas

- **Mesa só na administração.** A fila de impressão fica em Gestão de impressão 3D. O cliente não vê a máquina.
- **Amostras de filamento.** A cor na ficha é uma fileira de amostras. A pessoa vê o tom antes de ler o nome.
- **Venda de balcão.** Data, item, quantidade e valor já nascem como receita do mês, no mesmo caderno das vendas da planilha.
- **Caderno, não ERP.** O financeiro repete os blocos que o dono já usa: gastos fixos, energia, filamentos e outros insumos.
- **Três cartões no orçamento.** Site, Shopee e Mercado Livre lado a lado, com o aviso de faixa no cartão da Shopee.
- **Checkout de uma página.** O resumo fica visível até confirmar, inclusive no celular.
- **Guia do mascote.** Três passos no primeiro acesso do admin: lançar uma despesa, ver o mês, fazer um orçamento. Dá para pular.
- **WhatsApp na ficha.** A oficina fecha muita venda na conversa. O botão abre o número da loja com o nome da peça na mensagem.
- **Data da sessão na foto.** A legenda provisória leva o dia do arquivo, e o admin continua mostrando essa data depois que o dono renomeia a peça.
- **Compra a partir do mínimo.** O alerta de estoque abre um lançamento preenchido no caderno do mês e só grava com confirmação.

## Ideias deixadas para depois

- Antes e depois na ficha pintada. As fotos não vieram em par marcado. Entra quando o dono indicar as duas imagens.
- Leitura automática das planilhas. A carga desta etapa é transcrita para os arquivos `dados/`.
- Faixas reais do Mercado Livre. A tela existe; as faixas esperam a loja e a tabela.
- Subsídio de Pix da Shopee como receita. Ele não aumenta o líquido e fica de fora da conta.
- Frete real dos Correios, pagamento, nota autorizada, e-mail e senha. Estão na lista simulada abaixo.

## O que é simulado e o que fica para um backend futuro

Simulado agora, no navegador:

- catálogo, pedidos, estoque, financeiro e orçamentos, a partir dos arquivos `dados/` e da memória local
- pagamento, que só muda o status
- PAC e SEDEX por faixa de CEP
- comprovante, que guarda o nome do arquivo
- nota fiscal, que permanece “pendente de transmissão”
- contato e e-mail, que só entram na lista
- acesso do dono, pelo link Administração, sem senha

Para um backend futuro, sem desenhar esse backend aqui: gravar de verdade pedido, estoque, lançamento e orçamento; cobrar; cotar frete; autorizar nota; enviar e-mail; entrar com senha; ler o estado da impressora por API.

## Fase 11 — Marketplace e áreas da administração

A vitrine e o menu passam a seguir a direção vigente. As fases anteriores continuam no histórico; o que elas diziam sobre venda de arquivo e sobre a mesa na home não se repete na tela.

Critério:

- [x] A home não mostra “Na mesa agora”, “Comprar arquivo” nem “Também vendem o arquivo”.
- [x] O topo da loja tem busca, carrinho e conta. O menu do cliente é Início, Produtos e Personalizados. O carrossel passa abaixo do menu.
- [x] O card de peça pronta tem a tag Produto e o preço fixo. O card personalizado tem a tag Personalizado, o preço médio e o aviso do orçamento.
- [x] Produtos lista só peça pronta. Personalizados lista só encomenda, incluindo Chaveiro do seu Cachorro, Da imagem para o 3D e Action figure personalizada.
- [x] A ficha tem Shopee, Mercado Livre e cotação de frete. Não há botão de arquivo 3D nem Downloads na conta.
- [x] O menu da administração tem as dez áreas. Gestão de impressão 3D mostra monitoramento, mesa, qualidade e histórico, sem prometer API ligada.
- [x] O painel gerencial mostra a contagem da vitrine. Marketing grava o cupom. Fiscal mostra a nota pendente, sem chave.

## Fase 12 — Publicação, visual e Cloudflare

As fases 1 a 11 entregam a loja completa no navegador local. Esta fase deixa o site pronto para subir no Cloudflare e reforça o visual da marca sem mudar regra de dinheiro.

Direção visual:

- Paleta fixa: laranja `#e8772a`, teal `#148f93`, creme `#f6f1e7`, tinta `#1b2430`, branco.
- Tailwind CLI gera `css/tailwind.css` no build. O HTML e o JS usam utilitários da marca (`bg-laranja`, `text-tinta`, `shadow-cria`).
- Microinterações: hover em card, chip e botão; carrossel com brilho e mascote flutuando; entrada em cascata ao rolar (respeita `prefers-reduced-motion`).
- Fundo da página com gradientes suaves da marca, sem flat único.

Publicação (Cloudflare):

- Site estático. Não há backend Nest/ASP.NET nesta etapa.
- Preferência: Cloudflare Pages com build `npm run build:css` e pasta de saída na raiz do repositório.
- Alternativa na tela Workers + Git: mesmo build, deploy com Wrangler e `wrangler.toml` apontando os assets estáticos, ignorando `node_modules`.
- Domínio alvo: `https://crialabdm.com.br` (DNS e domínio customizado ficam no painel da Cloudflare; o critério desta fase é o deploy da aplicação).

Arquivos desta fase: `wrangler.toml`, `.assetsignore`, `package.json` (scripts `build`, `deploy` e `otimizar-fotos`), `scripts/otimizar-fotos.mjs`, `README.md`, ajustes em `css/` e `js/` do visual. As fotos de `fotos/` passam por compressão web (lado máximo 1600 px) porque o Cloudflare Workers limita cada arquivo a 25 MiB.

Critério:

- [x] `npm run build` gera `css/tailwind.css` sem erro.
- [x] Existe `wrangler.toml` com assets na raiz e `.assetsignore` sem `node_modules`.
- [x] O README explica o que colocar na tela do Cloudflare (build, deploy e alternativa Pages).
- [x] `npx wrangler deploy --dry-run` conclui sem erro de tamanho de asset.
- [x] Home, ficha e painel admin abrem com a paleta da marca, hover e animação de entrada.
- [x] Com `prefers-reduced-motion: reduce`, as animações contínuas param.
- [x] Continua sem venda de arquivo 3D, sem “Na mesa agora” na home e sem Nuxt/ASP.NET.
