(function (root) {
  function centavos(n) {
    return Math.round((Number(n) + Number.EPSILON) * 100) / 100;
  }

  function arredondarCimaReal(n) {
    var ajustado = Math.round(Number(n) * 1000) / 1000;
    return Math.ceil(ajustado - 1e-9);
  }

  function somar(lista, campo) {
    return lista.reduce(function (total, item) { return total + (Number(item[campo]) || 0); }, 0);
  }

  function cotarFrete(cep, faixas) {
    var digitos = String(cep || "").replace(/\D/g, "");
    if (digitos.length !== 8) return { erro: "Informe o CEP com 8 dígitos." };
    var prefixo = Number(digitos.slice(0, 5));
    var faixa = (faixas || []).find(function (item) { return prefixo >= item.de && prefixo <= item.ate; });
    if (!faixa) return { erro: "Não há cotação para este CEP." };
    return {
      opcoes: [
        { id: "pac", nome: "PAC", preco: faixa.pac, dias: faixa.pacDias },
        { id: "sedex", nome: "SEDEX", preco: faixa.sedex, dias: faixa.sedexDias }
      ]
    };
  }

  function fecharPedido(itens, opcoes) {
    var bruto = 0;
    var fisico = 0;
    (itens || []).forEach(function (item) {
      var parcial = (Number(item.precoUnitario) || 0) * (Number(item.quantidade) || 0);
      bruto += parcial;
      if (item.fisico) fisico += parcial;
    });
    var cupom = Number(opcoes.cupomPercentual) || 0;
    var descontoCupom = bruto * cupom;
    var subtotal = bruto - descontoCupom;
    var subtotalFisico = fisico - fisico * cupom;
    var freteTabela = Number(opcoes.frete) || 0;
    var freteCobrado = subtotalFisico <= 0 || subtotalFisico >= Number(opcoes.freteGratisAPartir) ? 0 : freteTabela;
    var aVista = opcoes.forma === "pix" || opcoes.forma === "boleto";
    var descontoAVista = aVista ? subtotal * (Number(opcoes.descontoAVista) || 0) : 0;
    var total = subtotal - descontoAVista + freteCobrado;
    var parcelas = (subtotal + freteCobrado) / (Number(opcoes.parcelas) || 3);
    return {
      bruto: bruto,
      subtotal: subtotal,
      subtotalFisico: subtotalFisico,
      descontoCupom: descontoCupom,
      freteCobrado: freteCobrado,
      descontoAVista: descontoAVista,
      total: total,
      parcelas: parcelas,
      aVista: aVista
    };
  }

  function orcar(entrada) {
    var precoKg = Number(entrada.precoKg) || 0;
    var peso = (Number(entrada.pesoG) || 0) + (Number(entrada.lixoG) || 0);
    var filamento = (precoKg / 1000) * peso;
    var energia = Number(entrada.energiaPorMinuto) || 0;
    var depreciacao = Number(entrada.depreciacao) || 0;
    var manutencao = Number(entrada.manutencao) || 0;
    var taxaMin = energia + depreciacao + manutencao;
    var tempo = (Number(entrada.minutos) || 0) * taxaMin;
    var parametros = 0;
    (entrada.itens || []).forEach(function (item) {
      parametros += (Number(item.quantidade) || 0) * (Number(item.preco) || 0);
    });
    var custoBase = (Number(entrada.servico) || 0) + filamento + tempo + parametros;
    var custoPintura = 0;
    var taxaPintura = 0;
    if (entrada.pintura && entrada.pintura.percentual) {
      (entrada.pintura.itens || []).forEach(function (item) {
        custoPintura += (Number(item.quantidade) || 0) * (Number(item.preco) || 0);
      });
      taxaPintura = custoBase * Number(entrada.pintura.percentual);
    }
    var custo = custoBase + custoPintura + taxaPintura;
    var margem = Number(entrada.margem) || 0;
    var falha = Number(entrada.taxaFalha) || 0;
    var lucro = custo * margem;
    var reserva = custo * falha;
    var original = custo + lucro + reserva;
    var desconto = Number(entrada.desconto) || 0;
    var minutos = Number(entrada.minutos) || 0;
    var horas = Math.ceil(minutos / 60 - 1e-9);
    var dias = Math.ceil(horas / 24 - 1e-9);
    return {
      filamento: filamento,
      tempo: tempo,
      taxaMin: taxaMin,
      parametros: parametros,
      custoBase: custoBase,
      custoPintura: custoPintura,
      taxaPintura: taxaPintura,
      custo: custo,
      lucro: lucro,
      reserva: reserva,
      original: original,
      direto: arredondarCimaReal(original * (1 - desconto)),
      horas: horas,
      dias: dias
    };
  }

  function faixaDoPreco(preco, faixas) {
    return (faixas || []).find(function (faixa) { return preco >= faixa.de && preco <= faixa.ate + 0.001; }) || null;
  }

  function candidatoFaixa(valorOriginal, faixa) {
    var preco = (valorOriginal + faixa.fixo) / (1 - faixa.percentual);
    if (preco < 9) {
      var denom = 1 - faixa.percentual - 0.5;
      if (denom > 0) preco = valorOriginal / denom;
    }
    return centavos(preco);
  }

  function comissaoNoPreco(preco, faixas) {
    var faixa = faixaDoPreco(preco, faixas);
    if (!faixa) return 0;
    var fixo = preco < 9 ? preco / 2 : faixa.fixo;
    return centavos(preco * faixa.percentual + fixo);
  }

  function precoCanal(valorOriginal, faixas, freteVendedor) {
    var lista = faixas || [];
    var frete = Number(freteVendedor) || 0;
    if (!lista.length) {
      return { preco: null, comissao: 0, liquido: null, aviso: "", liquido7999: null, liquido80: null, faixasVazias: true };
    }
    var candidatos = lista.map(function (faixa) {
      var preco = candidatoFaixa(valorOriginal, faixa);
      return { faixa: faixa, preco: preco, dentro: preco >= faixa.de && preco <= faixa.ate + 0.001 };
    });
    var validos = candidatos.filter(function (item) { return item.dentro; }).sort(function (a, b) { return a.preco - b.preco; });
    var escolhido = validos[0] || null;
    var liquido7999 = centavos(79.99 - comissaoNoPreco(79.99, lista));
    var liquido80 = centavos(80 - comissaoNoPreco(80, lista));
    var cru = candidatos.find(function (item) { return item.faixa.ate <= 79.99; });
    var aviso = "";
    if (cru && cru.preco >= 80 && liquido7999 > liquido80) {
      aviso = "De R$ 79,99 para R$ 80,00 o líquido cai.";
    }
    if (!escolhido) {
      return { preco: null, comissao: 0, liquido: null, aviso: aviso || "Nenhum candidato caiu dentro da própria faixa.", liquido7999: liquido7999, liquido80: liquido80, faixasVazias: false };
    }
    var comissao = comissaoNoPreco(escolhido.preco, lista);
    return {
      preco: escolhido.preco,
      comissao: comissao,
      liquido: centavos(escolhido.preco - comissao - frete),
      aviso: aviso,
      liquido7999: liquido7999,
      liquido80: liquido80,
      faixasVazias: false
    };
  }

  function resumirMes(lancamentos, competencia) {
    var doMes = (lancamentos || []).filter(function (item) { return item.competencia === competencia && !item.estornado; });
    function soma(pred) {
      return doMes.filter(pred).reduce(function (total, item) { return total + (Number(item.valor) || 0); }, 0);
    }
    var receita = soma(function (item) { return item.categoria === "receita"; });
    var quantidade = doMes.filter(function (item) { return item.categoria === "receita"; }).reduce(function (total, item) { return total + (Number(item.quantidade) || 0); }, 0);
    var energia = soma(function (item) { return item.categoria === "energia"; });
    var filamentos = soma(function (item) { return item.categoria === "filamento"; });
    var outros = soma(function (item) { return item.categoria === "outroInsumo"; });
    var gastosFixos = soma(function (item) { return item.categoria === "gastoFixo"; });
    var investimento = soma(function (item) { return item.categoria === "investimento"; });
    var despesaOperacional = energia + filamentos + outros;
    var kwh = doMes.filter(function (item) { return item.categoria === "energia" && item.kwh; }).reduce(function (total, item) { return total + Number(item.kwh); }, 0);
    return {
      receita: receita,
      quantidade: quantidade,
      energia: energia,
      filamentos: filamentos,
      outros: outros,
      gastosFixos: gastosFixos,
      investimento: investimento,
      despesaOperacional: despesaOperacional,
      resultadoOperacional: receita - despesaOperacional,
      resultadoCaixa: receita - (despesaOperacional + gastosFixos + investimento),
      lucro: receita - despesaOperacional - gastosFixos,
      kwh: kwh
    };
  }

  function mediaFilamento(lancamentos, competencia, tipo) {
    var lista = (lancamentos || []).filter(function (item) {
      return !item.estornado && item.categoria === "filamento" && item.tipoInsumo === tipo && (!competencia || item.competencia === competencia);
    });
    var quantidade = somar(lista, "quantidade");
    var valor = somar(lista, "valor");
    return { quantidade: quantidade, valor: valor, media: quantidade ? valor / quantidade : 0 };
  }

  function medianaMensal(lancamentos, tipo, competencias) {
    var medias = competencias.map(function (comp) { return mediaFilamento(lancamentos, comp, tipo).media; });
    var xs = medias.slice().sort(function (a, b) { return a - b; });
    var n = xs.length;
    if (!n) return 0;
    var meio = Math.floor(n / 2);
    if (n % 2) return xs[meio];
    return (xs[meio - 1] + xs[meio]) / 2;
  }

  root.CriaCalculos = {
    centavos: centavos,
    arredondarCimaReal: arredondarCimaReal,
    cotarFrete: cotarFrete,
    fecharPedido: fecharPedido,
    orcar: orcar,
    precoCanal: precoCanal,
    comissaoNoPreco: comissaoNoPreco,
    resumirMes: resumirMes,
    mediaFilamento: mediaFilamento,
    medianaMensal: medianaMensal
  };
})(typeof window !== "undefined" ? window : globalThis);
