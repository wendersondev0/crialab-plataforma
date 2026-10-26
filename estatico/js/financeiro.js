(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var calc = CriaCalculos;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  var competencias = ["2026-03", "2026-04", "2026-05", "2026-06", "2026-07", "2026-08", "2026-09", "2026-10"];
  function estado() { return Cria.estado; }

  function mesAtual() { return dinheiro.hojeISO().slice(0, 7); }
  function anosDisponiveis() {
    var mapa = {};
    competencias.forEach(function (comp) { mapa[comp.slice(0, 4)] = true; });
    (estado().lancamentos || []).forEach(function (item) {
      if (item.competencia) mapa[String(item.competencia).slice(0, 4)] = true;
    });
    return Object.keys(mapa).sort();
  }
  function mesesDoAno(ano) {
    var mapa = {};
    competencias.forEach(function (comp) {
      if (String(comp).indexOf(ano) === 0) mapa[comp] = true;
    });
    (estado().lancamentos || []).forEach(function (item) {
      var comp = String(item.competencia || "");
      if (comp.indexOf(ano) === 0) mapa[comp] = true;
    });
    var lista = Object.keys(mapa).sort();
    if (lista.length) return lista;
    var todos = [];
    for (var i = 1; i <= 12; i++) todos.push(ano + "-" + (i < 10 ? "0" + i : String(i)));
    return todos;
  }
  function todasCompetencias() {
    var mapa = {};
    competencias.forEach(function (comp) { mapa[comp] = true; });
    (estado().lancamentos || []).forEach(function (item) {
      if (item.competencia) mapa[String(item.competencia)] = true;
    });
    return Object.keys(mapa).sort();
  }
  function ultimoMesComMovimento() {
    var lista = todasCompetencias().slice().reverse();
    for (var i = 0; i < lista.length; i++) {
      var r = calc.resumirMes(estado().lancamentos, lista[i]);
      if (r.receita || r.despesaOperacional || r.gastosFixos || r.energia || r.investimento) return lista[i];
    }
    return mesAtual();
  }
  function metaDe(comp) {
    if (estado().metas[comp] != null) return Number(estado().metas[comp]);
    var lista = todasCompetencias();
    var indice = lista.indexOf(comp);
    if (indice <= 0) return 0;
    return calc.resumirMes(estado().lancamentos, lista[indice - 1]).receita;
  }
  function categoriasOptions(selecionada) {
    return ["gastoFixo", "filamento", "outroInsumo", "receita", "investimento", "energia"].map(function (categoria) {
      var nomes = { gastoFixo: "Gasto fixo", filamento: "Filamento", outroInsumo: "Outro insumo", receita: "Receita de venda", investimento: "Investimento em patrimônio", energia: "Energia" };
      return '<option value="' + categoria + '"' + (selecionada === categoria ? " selected" : "") + ">" + nomes[categoria] + "</option>";
    }).join("");
  }
  function nomeCategoria(categoria) {
    var nomes = { gastoFixo: "Gasto fixo", filamento: "Filamento", outroInsumo: "Outro insumo", receita: "Receita", investimento: "Investimento", energia: "Energia" };
    return nomes[categoria] || categoria;
  }
  function tiposOptions() {
    var tipos = {};
    (estado().configuracao.tiposInsumo || []).forEach(function (tipo) { tipos[tipo] = true; });
    estado().lancamentos.forEach(function (item) { if (item.tipoInsumo) tipos[item.tipoInsumo] = true; });
    return Object.keys(tipos).map(function (tipo) { return '<option value="' + ui.esc(tipo) + '">' + ui.esc(tipo) + "</option>"; }).join("");
  }
  function ico(nome) {
    return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
  }
  function barras(meses, valores, cor, formato) {
    var escrever = formato || function (valor) { return dinheiro.real(valor); };
    var max = Math.max.apply(null, valores.map(function (item) { return Math.abs(item); }).concat([1]));
    return '<div class="barras fin-barras">' + meses.map(function (comp, indice) {
      var valor = valores[indice] || 0;
      var altura = Math.max(4, Math.round(Math.abs(valor) / max * 120));
      var neg = valor < 0 ? " neg" : "";
      return '<div class="barra-mes' + neg + '"><span style="height:' + altura + "px;background:" + cor + '"></span><small>' + comp.slice(5) + "</small><small>" + escrever(valor) + "</small></div>";
    }).join("") + "</div>";
  }

  var blocosCaderno = [
    { id: "gastoFixo", titulo: "Gastos fixos" },
    { id: "energia", titulo: "Energia" },
    { id: "filamento", titulo: "Filamentos" },
    { id: "outroInsumo", titulo: "Outros insumos" },
    { id: "receita", titulo: "Vendas" },
    { id: "investimento", titulo: "Investimento em patrimônio" }
  ];

  function filtrarDoMes(comp, categoria, texto, ordem) {
    var lista = (estado().lancamentos || []).filter(function (item) {
      if (item.competencia !== comp || item.estornado) return false;
      if (categoria && item.categoria !== categoria) return false;
      if (texto) {
        var alvo = String(item.descricao + " " + (item.tipoInsumo || "") + " " + (item.link || "")).toLowerCase();
        if (alvo.indexOf(texto.toLowerCase()) < 0) return false;
      }
      return true;
    }).slice();
    if (ordem === "valor") lista.sort(function (a, b) { return (Number(b.valor) || 0) - (Number(a.valor) || 0); });
    else if (ordem === "dataAsc") lista.sort(function (a, b) { return String(a.data).localeCompare(String(b.data)); });
    else lista.sort(function (a, b) { return String(b.data).localeCompare(String(a.data)); });
    return lista;
  }

  function htmlFiltroPeriodo(vista) {
    var mesesAno = mesesDoAno(vista.ano);
    var anos = anosDisponiveis();
    var linha = todasCompetencias();
    var idx = linha.indexOf(vista.mes);
    var ant = idx > 0 ? linha[idx - 1] : "";
    var prox = idx >= 0 && idx < linha.length - 1 ? linha[idx + 1] : "";
    return '<div class="fin-filtros">' +
      '<label class="fin-campo"><span>Ano</span><select id="fin-ano">' + anos.map(function (ano) {
        return '<option value="' + ano + '"' + (ano === vista.ano ? " selected" : "") + ">" + ano + "</option>";
      }).join("") + "</select></label>" +
      '<label class="fin-campo"><span>Mês</span><select id="fin-mes">' + mesesAno.map(function (comp) {
        return '<option value="' + comp + '"' + (comp === vista.mes ? " selected" : "") + ">" + dinheiro.competenciaBR(comp) + "</option>";
      }).join("") + "</select></label>" +
      '<div class="fin-nav-mes">' +
      '<button class="botao-linha" type="button" id="fin-ant"' + (ant ? ' data-mes="' + ant + '"' : " disabled") + "><span>‹ Anterior</span></button>" +
      '<button class="botao-linha" type="button" id="fin-prox"' + (prox ? ' data-mes="' + prox + '"' : " disabled") + "><span>Próximo ›</span></button>" +
      "</div>" +
      '<div class="fin-meses-chips">' + mesesAno.map(function (comp) {
        var r = calc.resumirMes(estado().lancamentos, comp);
        var tem = r.receita || r.despesaOperacional || r.gastosFixos || r.energia || r.investimento;
        return '<button class="fin-chip' + (comp === vista.mes ? " ativo" : "") + (tem ? "" : " vazio") + '" type="button" data-mes="' + comp + '" title="' + ui.esc(dinheiro.competenciaBR(comp)) + '">' + comp.slice(5) + "</button>";
      }).join("") + "</div></div>";
  }

  function htmlBlocoCaderno(titulo, categoria, itens, comp) {
    var total = itens.reduce(function (s, item) { return s + (Number(item.valor) || 0); }, 0);
    var media = "";
    if (categoria === "filamento") {
      media = '<div class="fin-fil-medias">' + ["PLA", "PETG", "TPU"].map(function (tipo) {
        var conta = calc.mediaFilamento(estado().lancamentos, comp, tipo);
        return '<span><strong>' + tipo + "</strong> " + dinheiro.realKg(conta.media) + " / kg · " + dinheiro.real(conta.valor) + " em " + String(conta.quantidade).replace(".", ",") + " kg</span>";
      }).join("") + "</div>";
    }
    var linhas = itens.length ? itens.map(function (item) {
      var extra = item.kwh ? " · " + item.kwh + " kWh × " + item.tarifa : "";
      var link = item.link ? ' <a class="fin-link-ext" href="' + ui.esc(item.link) + '" target="_blank" rel="noopener">link</a>' : "";
      return "<tr><td>" + ui.esc(dinheiro.dataBR(item.data)) + "</td><td>" + ui.esc(item.descricao) + extra + link + "</td><td>" + ui.esc(item.tipoInsumo || "—") + "</td><td>" + String(item.quantidade).replace(".", ",") + "</td><td>" + dinheiro.real(item.valor) + '</td><td><button class="botao-linha fin-estorno" type="button" data-estorno="' + ui.esc(item.id) + '">Estornar</button></td></tr>';
    }).join("") : '<tr><td colspan="6" class="fin-vazio">Nenhum lançamento neste bloco.</td></tr>';
    return '<section class="fin-card fin-bloco" id="bloco-' + categoria + '">' +
      '<div class="fin-card-topo"><h2 class="fin-titulo">' + ui.esc(titulo) + '</h2><span class="fin-selo">' + itens.length + " · " + dinheiro.real(total) + "</span></div>" +
      media +
      '<div class="tabela-rolagem"><table class="fin-tabela"><thead><tr><th>Data</th><th>Descrição</th><th>Tipo</th><th>Qtd</th><th>Valor</th><th></th></tr></thead><tbody>' + linhas + "</tbody></table></div></section>";
  }

  function htmlCaderno(vista, compraPrefill) {
    var atual = vista.mes;
    var resumo = calc.resumirMes(estado().lancamentos, atual);
    var lista = filtrarDoMes(atual, vista.categoria, vista.texto, vista.ordem || "data");
    var chips = '<div class="fin-cat-chips">' +
      '<button class="fin-cat' + (!vista.categoria ? " ativo" : "") + '" type="button" data-cat="">Todos</button>' +
      blocosCaderno.map(function (bloco) {
        return '<button class="fin-cat' + (vista.categoria === bloco.id ? " ativo" : "") + '" type="button" data-cat="' + bloco.id + '">' + ui.esc(bloco.titulo) + "</button>";
      }).join("") +
      "</div>";
    var ferramentas = '<div class="fin-ferramentas">' +
      '<label class="fin-campo fin-busca"><span>Buscar no mês</span><input id="fin-q" type="search" value="' + ui.esc(vista.texto || "") + '" placeholder="Descrição, tipo ou link"></label>' +
      '<label class="fin-campo"><span>Ordenar</span><select id="fin-ordem">' +
      '<option value="data"' + (vista.ordem === "data" || !vista.ordem ? " selected" : "") + ">Data (mais recente)</option>" +
      '<option value="dataAsc"' + (vista.ordem === "dataAsc" ? " selected" : "") + ">Data (mais antiga)</option>" +
      '<option value="valor"' + (vista.ordem === "valor" ? " selected" : "") + ">Maior valor</option>" +
      "</select></label>" +
      '<button class="botao-linha" type="button" id="csv">' + ico("documento") + "<span>CSV do mês</span></button>" +
      "</div>";
    var totais = '<div class="fin-detalhe-grid fin-caderno-totais">' +
      '<article class="fin-mini"><span>Receita</span><strong>' + dinheiro.real(resumo.receita) + "</strong></article>" +
      '<article class="fin-mini"><span>Operação</span><strong>' + dinheiro.real(resumo.despesaOperacional) + "</strong></article>" +
      '<article class="fin-mini"><span>Gastos fixos</span><strong>' + dinheiro.real(resumo.gastosFixos) + "</strong></article>" +
      '<article class="fin-mini"><span>Investimento</span><strong>' + dinheiro.real(resumo.investimento) + "</strong></article>" +
      '<article class="fin-mini"><span>Operacional</span><strong class="' + (resumo.resultadoOperacional < 0 ? "neg" : "") + '">' + dinheiro.real(resumo.resultadoOperacional) + "</strong></article>" +
      '<article class="fin-mini"><span>Caixa</span><strong class="' + (resumo.resultadoCaixa < 0 ? "neg" : "") + '">' + dinheiro.real(resumo.resultadoCaixa) + "</strong></article>" +
      "</div>";
    var blocos = blocosCaderno.filter(function (bloco) {
      return !vista.categoria || vista.categoria === bloco.id;
    }).map(function (bloco) {
      var itens = lista.filter(function (item) { return item.categoria === bloco.id; });
      if (vista.texto && !itens.length && vista.categoria !== bloco.id) return "";
      return htmlBlocoCaderno(bloco.titulo, bloco.id, itens, atual);
    }).join("");
    if (!blocos) blocos = '<section class="fin-card"><p class="fin-vazio m-0">Nada encontrado com esses filtros em ' + ui.esc(dinheiro.competenciaBR(atual)) + ".</p></section>";
    var compra = compraPrefill || "";
    var formCompra = '<section class="fin-card" id="compra-rapida"><h2 class="fin-titulo">Compra rápida de insumo</h2>' +
      '<form id="compra" class="fin-meta-form fin-compra-form">' +
      '<label class="fin-campo"><span>Descrição</span><input name="descricao" value="' + ui.esc(compra) + '" required></label>' +
      '<label class="fin-campo"><span>Tipo</span><input name="tipo" value="' + ui.esc(compra) + '" list="fin-tipos"></label>' +
      '<datalist id="fin-tipos">' + (estado().configuracao.tiposInsumo || []).map(function (t) { return '<option value="' + ui.esc(t) + '">'; }).join("") + "</datalist>" +
      '<label class="fin-campo"><span>Quantidade</span><input name="quantidade" type="number" step="0.01" value="1"></label>' +
      '<label class="fin-campo"><span>Valor</span><input name="valor" type="number" step="0.01" required></label>' +
      '<button class="botao" type="submit">' + ico("carrinho") + "<span>Confirmar compra</span></button>" +
      "</form></section>";
    return '<div class="fin-caderno-cab"><div><h2 class="fin-titulo m-0">Caderno · ' + ui.esc(dinheiro.competenciaBR(atual)) + '</h2><p class="fin-meta-txt m-0">Blocos no formato da planilha: filtre por categoria, busque e exporte o mês.</p></div><span class="fin-selo">' + lista.length + " lançamentos</span></div>" +
      chips + ferramentas + totais + '<div class="fin-caderno">' + blocos + "</div>" + formCompra;
  }

  function exportarCsvMes(comp) {
    var lista = (estado().lancamentos || []).filter(function (item) { return item.competencia === comp && !item.estornado; });
    var textoCsv = "data;descricao;categoria;tipo;quantidade;valor;link;competencia\n" + lista.map(function (item) {
      return [item.data, item.descricao, item.categoria, item.tipoInsumo, item.quantidade, item.valor, item.link, item.competencia].map(function (valor) {
        return '"' + String(valor == null ? "" : valor).replace(/"/g, '""') + '"';
      }).join(";");
    }).join("\n");
    var blob = new Blob(["\uFEFF" + textoCsv], { type: "text/csv;charset=utf-8" });
    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "caderno-" + comp + ".csv";
    link.click();
  }

  function ligarPeriodo(vista, pintar) {
    var ano = document.getElementById("fin-ano");
    var mesSel = document.getElementById("fin-mes");
    if (ano) ano.addEventListener("change", function (evento) {
      vista.ano = evento.target.value;
      var meses = mesesDoAno(vista.ano);
      vista.mes = meses.indexOf(vista.mes) >= 0 ? vista.mes : (meses[meses.length - 1] || vista.mes);
      pintar();
    });
    if (mesSel) mesSel.addEventListener("change", function (evento) {
      vista.mes = evento.target.value;
      vista.ano = vista.mes.slice(0, 4);
      pintar();
    });
    raiz.querySelectorAll("[data-mes]").forEach(function (botao) {
      botao.addEventListener("click", function () {
        var m = botao.getAttribute("data-mes");
        if (!m) return;
        vista.mes = m;
        vista.ano = vista.mes.slice(0, 4);
        pintar();
      });
    });
  }

  function ligarCaderno(vista, pintar) {
    raiz.querySelectorAll(".fin-cat[data-cat]").forEach(function (botao) {
      botao.addEventListener("click", function () {
        vista.categoria = botao.getAttribute("data-cat") || "";
        pintar();
      });
    });
    var q = document.getElementById("fin-q");
    if (q) q.addEventListener("input", function (evento) {
      vista.texto = evento.target.value;
      window.clearTimeout(q._t);
      q._t = window.setTimeout(pintar, 180);
    });
    var ordem = document.getElementById("fin-ordem");
    if (ordem) ordem.addEventListener("change", function (evento) {
      vista.ordem = evento.target.value;
      pintar();
    });
    var csv = document.getElementById("csv");
    if (csv) csv.addEventListener("click", function () { exportarCsvMes(vista.mes); });
    raiz.querySelectorAll("[data-estorno]").forEach(function (botao) {
      botao.addEventListener("click", function () {
        var id = botao.getAttribute("data-estorno");
        if (!confirm("Estornar este lançamento?")) return;
        Cria.alterar(function (atual) {
          var item = atual.lancamentos.find(function (lan) { return lan.id === id; });
          if (item) item.estornado = true;
        });
        pintar();
      });
    });
    var compra = document.getElementById("compra");
    if (compra) compra.addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: dinheiro.hojeISO(), competencia: vista.mes, descricao: String(dados.get("descricao")), categoria: "filamento", tipoInsumo: String(dados.get("tipo") || ""), quantidade: Number(dados.get("quantidade")) || 0, valor: Number(dados.get("valor")) || 0, link: "", kwh: null, tarifa: null, estornado: false, comprovante: "", origem: "compra" });
        var insumo = atual.estoqueInsumos.find(function (item) { return item.nome.toLowerCase() === String(dados.get("tipo")).toLowerCase(); });
        if (insumo) insumo.saldo += Number(dados.get("quantidade")) || 0;
      });
      vista.categoria = "filamento";
      vista.aba = "caderno";
      pintar();
    });
  }

  function painel() {
    document.title = "Financeiro · Administração";
    var params = new URLSearchParams(location.search);
    var padrao = params.get("mes") || ultimoMesComMovimento();
    var vista = {
      ano: params.get("ano") || padrao.slice(0, 4),
      mes: padrao,
      aba: params.get("aba") || "resumo",
      categoria: params.get("categoria") || "",
      texto: params.get("q") || "",
      ordem: params.get("ordem") || "data",
      compra: params.get("compra") || ""
    };
    if (vista.aba === "lancamentos") vista.aba = "caderno";
    if (vista.compra) vista.aba = "caderno";
    if (vista.mes.indexOf(vista.ano) !== 0) {
      var mesesAno = mesesDoAno(vista.ano);
      vista.mes = mesesAno[mesesAno.length - 1] || vista.mes;
    }
    function sincronizarUrl() {
      var url = new URL(location.href);
      url.searchParams.set("ano", vista.ano);
      url.searchParams.set("mes", vista.mes);
      url.searchParams.set("aba", vista.aba);
      if (vista.categoria) url.searchParams.set("categoria", vista.categoria);
      else url.searchParams.delete("categoria");
      if (vista.texto) url.searchParams.set("q", vista.texto);
      else url.searchParams.delete("q");
      if (vista.ordem && vista.ordem !== "data") url.searchParams.set("ordem", vista.ordem);
      else url.searchParams.delete("ordem");
      history.replaceState(null, "", url.pathname + url.search);
    }
    function pintar() {
      sincronizarUrl();
      var atual = vista.mes;
      var mesesAno = mesesDoAno(vista.ano);
      var resumo = calc.resumirMes(estado().lancamentos, atual);
      var linhaTempo = todasCompetencias();
      var indice = linhaTempo.indexOf(atual);
      var anterior = indice > 0 ? linhaTempo[indice - 1] : atual;
      var resumoAnterior = calc.resumirMes(estado().lancamentos, anterior);
      var metaAtual = metaDe(atual);
      var metaPassada = metaDe(anterior);
      var dia = Number(dinheiro.hojeISO().slice(8, 10)) || 1;
      var dias = dinheiro.diasNoMes(atual);
      var percentual = metaAtual ? resumo.receita / metaAtual * 100 : 0;
      var projecao = dia > 0 ? resumo.receita / dia * dias : 0;
      var batida = metaAtual > 0 && resumo.receita >= metaAtual;
      var energiaReais = mesesAno.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).energia; });
      var energiaKwh = mesesAno.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).kwh; });
      var operacao = mesesAno.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).resultadoOperacional; });
      var caixa = mesesAno.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).resultadoCaixa; });
      var receitasAno = mesesAno.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).receita; });
      var pla = calc.mediaFilamento(estado().lancamentos, "", "PLA");
      var petg = calc.mediaFilamento(estado().lancamentos, "", "PETG");
      var filtroAno = htmlFiltroPeriodo(vista);
      var hintReceita = "Soma das vendas (categoria receita) no período selecionado.";
      var hintOperacional = "Resultado operacional: receita menos filamento, outros insumos e energia. Não desconta gastos fixos nem investimento.";
      var hintCaixa = "Resultado de caixa: receita menos operação, gastos fixos e investimentos em patrimônio.";
      var hintLucro = "Lucro: receita menos operação e gastos fixos (investimentos ficam de fora).";
      var kpis = '<div class="fin-kpis">' +
        '<button class="fin-kpi" type="button" data-ir="caderno" data-cat="receita" title="' + ui.esc(hintReceita) + '"><span>Receita</span><strong>' + dinheiro.real(resumo.receita) + '</strong><small>' + ui.esc(dinheiro.competenciaBR(atual)) + "</small></button>" +
        '<button class="fin-kpi" type="button" data-ir="caderno" data-cat="" title="' + ui.esc(hintOperacional) + '"><span>Operacional</span><strong class="' + (resumo.resultadoOperacional < 0 ? "neg" : "") + '">' + dinheiro.real(resumo.resultadoOperacional) + "</strong><small>Receita − operação</small></button>" +
        '<button class="fin-kpi" type="button" data-ir="caderno" data-cat="investimento" title="' + ui.esc(hintCaixa) + '"><span>Caixa</span><strong class="' + (resumo.resultadoCaixa < 0 ? "neg" : "") + '">' + dinheiro.real(resumo.resultadoCaixa) + "</strong><small>Com investimentos</small></button>" +
        '<button class="fin-kpi" type="button" data-ir="resumo" data-cat="" title="' + ui.esc(hintLucro) + '"><span>Lucro</span><strong class="' + (resumo.lucro < 0 ? "neg" : "") + '">' + dinheiro.real(resumo.lucro) + "</strong><small>Receita − operação − fixos</small></button>" +
        "</div>";
      var detalheMes = '<div class="fin-detalhe-grid">' +
        '<button class="fin-mini" type="button" data-ir="caderno" data-cat="gastoFixo" title="Despesas recorrentes do período (aluguel, mensalidades, assinaturas etc.)."><span>Gastos fixos</span><strong>' + dinheiro.real(resumo.gastosFixos) + "</strong></button>" +
        '<button class="fin-mini" type="button" data-ir="caderno" data-cat="filamento" title="Compras de filamento lançadas no período."><span>Filamentos</span><strong>' + dinheiro.real(resumo.filamentos) + "</strong></button>" +
        '<button class="fin-mini" type="button" data-ir="caderno" data-cat="energia" title="Custo de energia (e kWh, quando informado) no período."><span>Energia</span><strong>' + dinheiro.real(resumo.energia) + "</strong></button>" +
        '<button class="fin-mini" type="button" data-ir="caderno" data-cat="investimento" title="Compras de patrimônio e equipamentos (máquinas, ferramentas etc.)."><span>Investimento</span><strong>' + dinheiro.real(resumo.investimento) + "</strong></button>" +
        '<button class="fin-mini" type="button" data-ir="caderno" data-cat="receita" title="Soma das quantidades nas vendas (lançamentos de receita)."><span>Qtd. vendida</span><strong>' + String(resumo.quantidade).replace(".", ",") + "</strong></button>" +
        '<button class="fin-mini" type="button" data-ir="caderno" data-cat="outroInsumo" title="Embalagens, argolas, tags e demais insumos que não são filamento."><span>Outros insumos</span><strong>' + dinheiro.real(resumo.outros) + "</strong></button>" +
        "</div>";
      var metaPct = Math.min(100, Math.round(percentual));
      var metaHtml = '<section class="fin-card">' +
        '<div class="fin-card-topo"><h2 class="fin-titulo">Meta do mês</h2><span class="fin-selo' + (batida ? " ok" : "") + '">' + (batida ? "Batida" : (metaAtual ? Math.round(percentual) + "%" : "Sem meta")) + "</span></div>" +
        '<div class="fin-meta-barra"><span style="width:' + metaPct + '%"></span></div>' +
        '<p class="fin-meta-txt">Meta ' + dinheiro.real(metaAtual) + " · realizado " + dinheiro.real(resumo.receita) + " · projeção " + dinheiro.real(projecao) + "</p>" +
        '<p class="fin-meta-ant">Mês anterior (' + ui.esc(dinheiro.competenciaBR(anterior)) + "): meta " + dinheiro.real(metaPassada) + " · receita " + dinheiro.real(resumoAnterior.receita) + " · " + (resumoAnterior.receita >= metaPassada && metaPassada > 0 ? "batida" : "não batida") + "</p>" +
        '<form id="meta" class="fin-meta-form"><label class="fin-campo"><span>Nova meta</span><input name="valor" type="number" step="0.01" value="' + metaAtual + '"></label><button class="botao" type="submit">' + ico("documento") + "<span>Salvar meta</span></button></form>" +
        "</section>";
      var graficos = '<section class="fin-card"><h2 class="fin-titulo">Ano ' + ui.esc(vista.ano) + '</h2><div class="fin-graficos">' +
        '<div class="fin-graf"><h3>Receita</h3>' + barras(mesesAno, receitasAno, "#e8772a") + "</div>" +
        '<div class="fin-graf"><h3>Energia (R$)</h3>' + barras(mesesAno, energiaReais, "#e8772a") + "</div>" +
        '<div class="fin-graf"><h3>Energia (kWh)</h3>' + barras(mesesAno, energiaKwh, "#148f93", function (valor) { return (valor ? valor.toLocaleString("pt-BR", { maximumFractionDigits: 1 }) : "0"); }) + "</div>" +
        '<div class="fin-graf"><h3>Resultado operacional</h3>' + barras(mesesAno, operacao, "#148f93") + "</div>" +
        '<div class="fin-graf"><h3>Resultado de caixa</h3>' + barras(mesesAno, caixa, "#1b2430") + "</div>" +
        "</div></section>";
      var refs = '<section class="fin-card fin-refs"><h2 class="fin-titulo">Referência de filamento</h2><div class="fin-ref-grade">' +
        '<article><span>PLA</span><strong>' + dinheiro.realKg(pla.media) + '</strong><small>média ponderada</small></article>' +
        '<article><span>PLA mediana</span><strong>' + dinheiro.realKg(calc.medianaMensal(estado().lancamentos, "PLA", mesesAno)) + "</strong><small>mensal no ano</small></article>" +
        '<article><span>PETG</span><strong>' + dinheiro.realKg(petg.media) + '</strong><small>média ponderada</small></article>' +
        '<article><span>PETG mediana</span><strong>' + dinheiro.realKg(calc.medianaMensal(estado().lancamentos, "PETG", mesesAno)) + "</strong><small>mensal no ano</small></article>" +
        "</div></section>";
      var abas = '<div class="fin-abas">' +
        '<button class="fin-aba' + (vista.aba === "resumo" ? " ativo" : "") + '" type="button" data-aba="resumo">Resumo</button>' +
        '<button class="fin-aba' + (vista.aba === "caderno" ? " ativo" : "") + '" type="button" data-aba="caderno">Caderno do mês</button>' +
        '<button class="fin-aba' + (vista.aba === "lancar" ? " ativo" : "") + '" type="button" data-aba="lancar">Lançar</button>' +
        '<button class="fin-aba' + (vista.aba === "contas" ? " ativo" : "") + '" type="button" data-aba="contas">Contas</button>' +
        "</div>";
      var corpo = "";
      if (vista.aba === "resumo") {
        corpo = metaHtml + graficos + refs;
      } else if (vista.aba === "caderno") {
        corpo = htmlCaderno(vista, vista.compra);
      } else if (vista.aba === "lancar") {
        corpo = '<section class="fin-card" id="lancar"><div class="fin-lancar-grade">' +
          '<form id="balcao" class="fin-form"><h2 class="fin-titulo">Venda de balcão</h2><label class="fin-campo"><span>Data</span><input name="data" type="date" value="' + dinheiro.hojeISO() + '"></label><label class="fin-campo"><span>Item</span><input name="item" required></label><label class="fin-campo"><span>Quantidade</span><input name="quantidade" type="number" step="0.01" value="1"></label><label class="fin-campo"><span>Valor</span><input name="valor" type="number" step="0.01" required></label><label class="fin-campo"><span>Competência</span><select name="competencia">' + competencias.map(function (comp) { return '<option value="' + comp + '"' + (comp === atual ? " selected" : "") + ">" + dinheiro.competenciaBR(comp) + "</option>"; }).join("") + '</select></label><button class="botao" type="submit">' + ico("carrinho") + "<span>Lançar receita</span></button></form>" +
          '<form id="lancamento" class="fin-form"><h2 class="fin-titulo">Novo lançamento</h2><label class="fin-campo"><span>Data</span><input name="data" type="date" value="' + dinheiro.hojeISO() + '"></label><label class="fin-campo"><span>Descrição</span><input name="descricao" required></label><label class="fin-campo"><span>Categoria</span><select name="categoria">' + categoriasOptions("gastoFixo") + '</select></label><label class="fin-campo"><span>Tipo de insumo</span><select name="tipoInsumo"><option value=""></option>' + tiposOptions() + '</select></label><label class="fin-campo"><span>Tipo novo</span><input name="tipoNovo" placeholder="Se faltar na lista"></label><label class="fin-campo"><span>Quantidade</span><input name="quantidade" type="number" step="0.01" value="1"></label><label class="fin-campo"><span>Valor</span><input name="valor" type="number" step="0.01" required></label><label class="fin-campo"><span>Link</span><input name="link"></label><label class="fin-campo"><span>Competência</span><input name="competencia" value="' + atual + '"></label><label class="fin-campo"><span>Comprovante</span><input name="comprovante" type="file"></label><button class="botao-teal" type="submit">' + ico("documento") + "<span>Salvar lançamento</span></button></form>" +
          "</div></section>";
      } else {
        corpo = '<section class="fin-card"><h2 class="fin-titulo">Contas a pagar / receber</h2><div class="fin-contas">' +
          (estado().contas || []).map(function (conta) {
            return '<article class="fin-conta' + (conta.baixada ? " baixa" : "") + '"><div><strong>' + ui.esc(conta.descricao) + '</strong><span>' + ui.esc(conta.tipo) + " · venc. " + ui.esc(dinheiro.dataBR(conta.vencimento)) + " · " + (conta.baixada ? "baixada" : "aberta") + "</span></div><strong>" + dinheiro.real(conta.valor) + "</strong>" +
              (conta.baixada ? "" : '<button class="botao-linha" type="button" data-baixa="' + ui.esc(conta.id) + '">Dar baixa</button>') +
              "</article>";
          }).join("") +
          "</div></section>";
      }
      var atalhos = '<div class="admin-prod-fluxo">' +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/energia.html") + '">' + ico("admin") + "<span>Fechamento de energia</span></a>" +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/conciliacao.html") + '">' + ico("estoque") + "<span>Conciliação</span></a>" +
        "</div>";
      raiz.innerHTML = '<div class="admin-prod-cab"><div><h1 class="m-0">Financeiro</h1><p class="admin-prod-intro m-0 mt-1">Ano e mês no topo. No Caderno do mês você organiza por bloco da planilha, busca, ordena e exporta.</p></div></div>' +
        filtroAno + kpis + detalheMes + atalhos + abas + corpo;
      ligar();
      ui.revelar(raiz);
    }
    function ligar() {
      ligarPeriodo(vista, pintar);
      raiz.querySelectorAll("[data-aba]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-aba") || "resumo";
          pintar();
        });
      });
      raiz.querySelectorAll("[data-ir]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-ir") || "resumo";
          vista.categoria = botao.getAttribute("data-cat") || "";
          vista.texto = "";
          pintar();
        });
      });
      var formMeta = document.getElementById("meta");
      if (formMeta) {
        formMeta.addEventListener("submit", function (evento) {
          evento.preventDefault();
          var valor = Number(new FormData(evento.target).get("valor"));
          Cria.alterar(function (atualEstado) { atualEstado.metas[vista.mes] = valor; });
          pintar();
        });
      }
      if (vista.aba === "caderno") ligarCaderno(vista, function () {
        vista.compra = "";
        pintar();
      });
      var balcao = document.getElementById("balcao");
      if (balcao) {
        balcao.addEventListener("submit", function (evento) {
          evento.preventDefault();
          var dados = new FormData(evento.target);
          Cria.alterar(function (atualEstado) {
            atualEstado.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: String(dados.get("data")), competencia: String(dados.get("competencia")), descricao: String(dados.get("item")), categoria: "receita", tipoInsumo: "", quantidade: Number(dados.get("quantidade")) || 1, valor: Number(dados.get("valor")) || 0, link: "", kwh: null, tarifa: null, estornado: false, comprovante: "", origem: "balcao" });
          });
          vista.aba = "caderno";
          vista.categoria = "receita";
          vista.mes = String(dados.get("competencia"));
          vista.ano = vista.mes.slice(0, 4);
          pintar();
        });
      }
      var lancamento = document.getElementById("lancamento");
      if (lancamento) {
        lancamento.addEventListener("submit", function (evento) {
          evento.preventDefault();
          var dados = new FormData(evento.target);
          var arquivo = evento.target.comprovante.files[0];
          var tipo = String(dados.get("tipoNovo") || dados.get("tipoInsumo") || "");
          var comp = String(dados.get("competencia") || vista.mes);
          Cria.alterar(function (atualEstado) {
            if (dados.get("tipoNovo")) atualEstado.configuracao.tiposInsumo.push(String(dados.get("tipoNovo")));
            atualEstado.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: String(dados.get("data")), competencia: comp, descricao: String(dados.get("descricao")), categoria: String(dados.get("categoria")), tipoInsumo: tipo, quantidade: Number(dados.get("quantidade")) || 0, valor: Number(dados.get("valor")) || 0, link: String(dados.get("link") || ""), kwh: null, tarifa: null, estornado: false, comprovante: arquivo ? arquivo.name : "", origem: "manual" });
          });
          vista.aba = "caderno";
          vista.categoria = String(dados.get("categoria") || "");
          vista.mes = comp;
          vista.ano = vista.mes.slice(0, 4);
          pintar();
        });
      }
      raiz.querySelectorAll("[data-baixa]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-baixa");
          var arquivo = prompt("Nome do comprovante da baixa", "comprovante.pdf");
          if (arquivo == null) return;
          Cria.alterar(function (atualEstado) {
            var conta = atualEstado.contas.find(function (item) { return item.id === id; });
            if (conta) { conta.baixada = true; conta.comprovante = arquivo; }
          });
          pintar();
        });
      });
    }
    pintar();
  }

  function mes() {
    var params = new URLSearchParams(location.search);
    var padrao = params.get("mes") || ultimoMesComMovimento();
    var vista = {
      ano: params.get("ano") || padrao.slice(0, 4),
      mes: padrao,
      aba: "caderno",
      categoria: params.get("categoria") || "",
      texto: params.get("q") || "",
      ordem: params.get("ordem") || "data"
    };
    if (vista.mes.indexOf(vista.ano) !== 0) {
      var mesesAno = mesesDoAno(vista.ano);
      vista.mes = mesesAno[mesesAno.length - 1] || vista.mes;
    }
    var compraPrefill = params.get("compra") || "";
    function sincronizarUrl() {
      var url = new URL(location.href);
      url.searchParams.set("ano", vista.ano);
      url.searchParams.set("mes", vista.mes);
      if (vista.categoria) url.searchParams.set("categoria", vista.categoria);
      else url.searchParams.delete("categoria");
      if (vista.texto) url.searchParams.set("q", vista.texto);
      else url.searchParams.delete("q");
      if (vista.ordem && vista.ordem !== "data") url.searchParams.set("ordem", vista.ordem);
      else url.searchParams.delete("ordem");
      if (compraPrefill) url.searchParams.set("compra", compraPrefill);
      history.replaceState(null, "", url.pathname + url.search);
    }
    function pintar() {
      sincronizarUrl();
      document.title = "Caderno · " + dinheiro.competenciaBR(vista.mes);
      raiz.innerHTML = '<div class="admin-prod-cab"><div><h1 class="m-0">Caderno do mês</h1><p class="admin-prod-intro m-0 mt-1">Mesmo caderno da planilha, com filtro de ano, mês e categoria.</p></div>' +
        '<a class="botao-linha" href="' + ui.url("paginas/admin/financeiro.html?mes=" + encodeURIComponent(vista.mes) + "&aba=resumo") + '">' + ico("carrinho") + "<span>Voltar ao Financeiro</span></a></div>" +
        htmlFiltroPeriodo(vista) + htmlCaderno(vista, compraPrefill);
      ligarPeriodo(vista, pintar);
      ligarCaderno(vista, function () {
        compraPrefill = "";
        pintar();
      });
      ui.revelar(raiz);
    }
    pintar();
  }

  function energia() {
    document.title = "Energia · Administração";
    var confirmado = false;
    var preco = estado().precoKwhAtual;
    function desenhar() {
      var maquinas = estado().maquinas.filter(function (item) { return item.controlaHora; });
      raiz.innerHTML = '<h1>Fechamento de energia</h1><form id="preco" class="cartao formulario"><label>Preço atual do kWh<input name="preco" type="number" step="0.0001" value="' + preco + '"></label><p class="discreto">O valor aberto é só o último preço, como ponto de partida.</p><button class="botao" type="submit">Confirmar preço do kWh</button></form><form id="fechar" class="formulario"' + (confirmado ? "" : " hidden") + ">" + maquinas.map(function (maquina) {
        var custo = (Number(maquina.kwhInformado) || 0) * preco;
        var sugerido = (maquina.watts / 1000) * preco / 60;
        return '<section class="cartao"><h2>' + ui.esc(maquina.nome) + '</h2><label>kWh<input data-id="' + ui.esc(maquina.id) + '" data-campo="kwh" type="number" step="0.01" value="' + (maquina.kwhInformado || 0) + '"></label><label>Potência (W)<input data-id="' + ui.esc(maquina.id) + '" data-campo="watts" type="number" step="1" value="' + maquina.watts + '"></label><label>Custo<input data-id="' + ui.esc(maquina.id) + '" data-campo="custo" type="number" step="0.01" value="' + custo + '"></label><label>R$/min<input data-id="' + ui.esc(maquina.id) + '" data-campo="minuto" type="number" step="0.000001" value="' + sugerido + '"></label></section>';
      }).join("") + '<button class="botao" type="submit">Salvar fechamento</button> <a class="botao-linha" href="' + ui.url("paginas/admin/financeiro.html") + '">Cancelar</a></form>';
      document.getElementById("preco").addEventListener("submit", function (evento) {
        evento.preventDefault();
        preco = Number(new FormData(evento.target).get("preco"));
        confirmado = true;
        desenhar();
      });
      var form = document.getElementById("fechar");
      if (!form) return;
      form.addEventListener("input", function (evento) {
        var campo = evento.target.getAttribute("data-campo");
        var id = evento.target.getAttribute("data-id");
        if (campo !== "kwh" && campo !== "watts") return;
        var kwh = Number(form.querySelector('[data-id="' + id + '"][data-campo="kwh"]').value) || 0;
        var watts = Number(form.querySelector('[data-id="' + id + '"][data-campo="watts"]').value) || 0;
        var custo = form.querySelector('[data-id="' + id + '"][data-campo="custo"]');
        var minuto = form.querySelector('[data-id="' + id + '"][data-campo="minuto"]');
        if (document.activeElement !== custo) custo.value = String(kwh * preco);
        if (!minuto.dataset.manual) minuto.value = String((watts / 1000) * preco / 60);
      });
      form.querySelectorAll('[data-campo="minuto"]').forEach(function (campo) {
        campo.addEventListener("input", function () { campo.dataset.manual = "1"; });
      });
      form.addEventListener("submit", function (evento) {
        evento.preventDefault();
        if (!confirmado) return;
        var registros = estado().maquinas.filter(function (item) { return item.controlaHora; }).map(function (maquina) {
          return {
            maquinaId: maquina.id,
            kwh: Number(form.querySelector('[data-id="' + maquina.id + '"][data-campo="kwh"]').value) || 0,
            watts: Number(form.querySelector('[data-id="' + maquina.id + '"][data-campo="watts"]').value) || 0,
            custo: Number(form.querySelector('[data-id="' + maquina.id + '"][data-campo="custo"]').value) || 0,
            energiaPorMinuto: Number(form.querySelector('[data-id="' + maquina.id + '"][data-campo="minuto"]').value) || 0
          };
        });
        Cria.alterar(function (atual) {
          atual.precoKwhAtual = preco;
          atual.fechamentosEnergia.push({ id: CriaCarrinho.novoId("energia"), quando: dinheiro.agoraSP(), precoKwh: preco, maquinas: registros });
          registros.forEach(function (registro) {
            var maquina = atual.maquinas.find(function (item) { return item.id === registro.maquinaId; });
            if (!maquina) return;
            maquina.watts = registro.watts;
            maquina.energiaPorMinuto = registro.energiaPorMinuto;
            maquina.kwhInformado = registro.kwh;
          });
          registros.forEach(function (registro) {
            var maquina = atual.maquinas.find(function (item) { return item.id === registro.maquinaId; });
            atual.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: dinheiro.hojeISO(), competencia: mesAtual(), descricao: "Energia " + (maquina ? maquina.nome : ""), categoria: "energia", tipoInsumo: "", quantidade: 1, valor: registro.custo, link: "", kwh: registro.kwh, tarifa: preco, estornado: false, comprovante: "", origem: "fechamento" });
          });
        });
        location.href = ui.url("paginas/admin/financeiro.html");
      });
    }
    desenhar();
  }

  function conciliacao() {
    document.title = "Conciliação · Administração";
    var aba = estado().abaTotal;
    var meses = competencias.slice(0, 7);
    var receita = meses.reduce(function (total, comp) { return total + calc.resumirMes(estado().lancamentos, comp).receita; }, 0);
    var quantidade = meses.reduce(function (total, comp) { return total + calc.resumirMes(estado().lancamentos, comp).quantidade; }, 0);
    var despesa = estado().lancamentos.filter(function (item) { return item.categoria !== "receita" && !item.estornado && meses.indexOf(item.competencia) >= 0; }).reduce(function (total, item) { return total + item.valor; }, 0);
    raiz.innerHTML = '<h1>Conciliação</h1><section class="cartao"><p>A aba Total soma a mediana só de março a julho, inclui o zero de junho no PETG, e deixa agosto e setembro de fora da quantidade, da receita e da despesa. O valor total dos gastos soma os sete meses. O painel usa o histórico completo.</p><div class="tabela-rolagem"><table><thead><tr><th></th><th>Aba Total</th><th>Histórico completo</th></tr></thead><tbody><tr><td>PLA mediana</td><td>' + dinheiro.realKg(aba.medianaPla) + "</td><td>" + dinheiro.realKg(calc.medianaMensal(estado().lancamentos, "PLA", meses)) + "</td></tr><tr><td>PETG mediana</td><td>" + dinheiro.realKg(aba.medianaPetg) + "</td><td>" + dinheiro.realKg(calc.medianaMensal(estado().lancamentos, "PETG", meses)) + "</td></tr><tr><td>Quantidade</td><td>" + aba.quantidade + "</td><td>" + quantidade + "</td></tr><tr><td>Receita</td><td>" + dinheiro.real(aba.receita) + "</td><td>" + dinheiro.real(receita) + "</td></tr><tr><td>Despesa</td><td>" + dinheiro.real(aba.despesa) + "</td><td>" + dinheiro.real(despesa) + "</td></tr><tr><td>Gastos</td><td>" + dinheiro.real(aba.valorTotal) + "</td><td>" + dinheiro.real(despesa) + "</td></tr></tbody></table></div></section>";
  }

  if (pagina === "admin-financeiro") painel();
  if (pagina === "admin-mes") mes();
  if (pagina === "admin-energia") energia();
  if (pagina === "admin-conciliacao") conciliacao();
})();
