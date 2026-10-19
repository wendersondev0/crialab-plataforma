(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var calc = CriaCalculos;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  var competencias = ["2026-03", "2026-04", "2026-05", "2026-06", "2026-07", "2026-08", "2026-09", "2026-10"];
  function estado() { return Cria.estado; }

  function mesAtual() { return dinheiro.hojeISO().slice(0, 7); }
  function metaDe(comp) {
    if (estado().metas[comp] != null) return Number(estado().metas[comp]);
    var indice = competencias.indexOf(comp);
    if (indice <= 0) return 0;
    return calc.resumirMes(estado().lancamentos, competencias[indice - 1]).receita;
  }
  function categoriasOptions(selecionada) {
    return ["gastoFixo", "filamento", "outroInsumo", "receita", "investimento", "energia"].map(function (categoria) {
      var nomes = { gastoFixo: "Gasto fixo", filamento: "Filamento", outroInsumo: "Outro insumo", receita: "Receita de venda", investimento: "Investimento em patrimônio", energia: "Energia" };
      return '<option value="' + categoria + '"' + (selecionada === categoria ? " selected" : "") + ">" + nomes[categoria] + "</option>";
    }).join("");
  }
  function tiposOptions() {
    var tipos = {};
    estado().configuracao.tiposInsumo.forEach(function (tipo) { tipos[tipo] = true; });
    estado().lancamentos.forEach(function (item) { if (item.tipoInsumo) tipos[item.tipoInsumo] = true; });
    return Object.keys(tipos).map(function (tipo) { return '<option value="' + ui.esc(tipo) + '">' + ui.esc(tipo) + "</option>"; }).join("");
  }
  function barras(valores, cor, formato) {
    var escrever = formato || function (valor) { return dinheiro.real(valor); };
    var max = Math.max.apply(null, valores.map(function (item) { return Math.abs(item); }).concat([1]));
    return '<div class="barras">' + competencias.filter(function (comp) { return comp <= "2026-09" || calc.resumirMes(estado().lancamentos, comp).receita || calc.resumirMes(estado().lancamentos, comp).resultadoCaixa; }).map(function (comp) {
      var valor = valores[competencias.indexOf(comp)] || 0;
      var altura = Math.max(4, Math.round(Math.abs(valor) / max * 140));
      return '<div class="barra-mes"><span style="height:' + altura + "px;background:" + cor + '"></span><small>' + comp.slice(5) + "</small><small>" + escrever(valor) + "</small></div>";
    }).join("") + "</div>";
  }

  function painel() {
    document.title = "Financeiro · Administração";
    var atual = mesAtual();
    var resumo = calc.resumirMes(estado().lancamentos, atual);
    var anterior = competencias[competencias.indexOf(atual) - 1] || "2026-09";
    var resumoAnterior = calc.resumirMes(estado().lancamentos, anterior);
    var metaAtual = metaDe(atual);
    var metaPassada = metaDe(anterior);
    var dia = Number(dinheiro.hojeISO().slice(8, 10));
    var dias = dinheiro.diasNoMes(atual);
    var percentual = metaAtual ? resumo.receita / metaAtual * 100 : 0;
    var projecao = resumo.receita / dia * dias;
    var energiaReais = competencias.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).energia; });
    var energiaKwh = competencias.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).kwh; });
    var operacao = competencias.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).resultadoOperacional; });
    var caixa = competencias.map(function (comp) { return calc.resumirMes(estado().lancamentos, comp).resultadoCaixa; });
    var pla = calc.mediaFilamento(estado().lancamentos, "", "PLA");
    var petg = calc.mediaFilamento(estado().lancamentos, "", "PETG");
    raiz.innerHTML = '<h1>Financeiro</h1><div class="kpis"><article class="kpi"><span>Receita de ' + ui.esc(dinheiro.competenciaBR(atual)) + "</span><strong>" + dinheiro.real(resumo.receita) + "</strong></article><article class=\"kpi\"><span>Resultado operacional</span><strong>" + dinheiro.real(resumo.resultadoOperacional) + "</strong></article><article class=\"kpi\"><span>Resultado de caixa</span><strong>" + dinheiro.real(resumo.resultadoCaixa) + "</strong></article><article class=\"kpi\"><span>Lucro</span><strong>" + dinheiro.real(resumo.lucro) + "</strong><small>Receita menos operação e gastos fixos</small></article></div><section class=\"cartao\"><h2>Meta</h2><p>Meta de " + ui.esc(dinheiro.competenciaBR(atual)) + ": " + dinheiro.real(metaAtual) + " · " + percentual.toLocaleString("pt-BR", { maximumFractionDigits: 1 }) + "% do mês · projeção " + dinheiro.real(projecao) + "</p><p>Meta de " + ui.esc(dinheiro.competenciaBR(anterior)) + ": " + dinheiro.real(metaPassada) + ". Receita: " + dinheiro.real(resumoAnterior.receita) + ". " + (resumoAnterior.receita >= metaPassada ? "A meta passada foi batida." : "A meta passada não foi batida.") + '</p><form id="meta" class="linha"><input name="valor" type="number" step="0.01" value="' + metaAtual + '"><button class="botao-linha" type="submit">Salvar meta do mês</button></form></section><section class="cartao"><h2>Energia em reais</h2>' + barras(energiaReais, "#e8772a") + "<h2>Energia em kWh</h2>" + barras(energiaKwh, "#148f93", function (valor) { return (valor ? valor.toLocaleString("pt-BR", { maximumFractionDigits: 2 }) : "0") + " kWh"; }) + "<h2>Lucro operacional</h2>" + barras(operacao, "#148f93") + "<h2>Resultado de caixa</h2>" + barras(caixa, "#1b2430") + '</section><section class="cartao"><h2>Preço de referência do insumo</h2><p>PLA média ponderada ' + dinheiro.realKg(pla.media) + " · mediana mensal " + dinheiro.realKg(calc.medianaMensal(estado().lancamentos, "PLA", competencias.slice(0, 7))) + "</p><p>PETG média ponderada " + dinheiro.realKg(petg.media) + " · mediana mensal " + dinheiro.realKg(calc.medianaMensal(estado().lancamentos, "PETG", competencias.slice(0, 7))) + '</p></section><section class="cartao" id="lancar"><h2>Venda de balcão</h2><form id="balcao" class="formulario"><label>Data<input name="data" type="date" value="' + dinheiro.hojeISO() + '"></label><label>Item<input name="item" required></label><label>Quantidade<input name="quantidade" type="number" step="0.01" value="1"></label><label>Valor<input name="valor" type="number" step="0.01" required></label><label>Mês<select name="competencia">' + competencias.map(function (comp) { return '<option value="' + comp + '"' + (comp === "2026-09" ? "" : "") + ">" + dinheiro.competenciaBR(comp) + "</option>"; }).join("") + '</select></label><button class="botao" type="submit">Lançar receita</button></form><h2>Novo lançamento</h2><form id="lancamento" class="formulario"><label>Data<input name="data" type="date" value="' + dinheiro.hojeISO() + '"></label><label>Descrição<input name="descricao" required></label><label>Categoria<select name="categoria">' + categoriasOptions("gastoFixo") + '</select></label><label>Tipo de insumo<select name="tipoInsumo"><option value=""></option>' + tiposOptions() + '</select></label><label>Tipo novo<input name="tipoNovo" placeholder="Crie um tipo se faltar"></label><label>Quantidade<input name="quantidade" type="number" step="0.01" value="1"></label><label>Valor<input name="valor" type="number" step="0.01" required></label><label>Link<input name="link"></label><label>Competência<input name="competencia" value="' + atual + '"></label><label>Comprovante<input name="comprovante" type="file"></label><button class="botao-teal" type="submit">Salvar lançamento</button></form></section><section class="cartao"><h2>Filtros</h2><form id="filtros" class="formulario"><div class="grade-3"><label>De<input name="de" type="date"></label><label>Até<input name="ate" type="date"></label><label>Categoria<select name="categoria"><option value="">Todas</option>' + categoriasOptions("") + '</select></label><label>Tipo<input name="tipo"></label><label>Texto<input name="texto"></label><label>Link<input name="link"></label></div><button class="botao-linha" type="submit">Filtrar</button><button class="botao-linha" type="button" id="csv">Exportar CSV</button></form><div class="tabela-rolagem" id="tabela"></div></section><section class="cartao"><h2>Contas</h2>' + estado().contas.map(function (conta) {
      return '<article class="linha"><span>' + ui.esc(conta.tipo) + " · " + ui.esc(conta.descricao) + " · " + dinheiro.real(conta.valor) + " · " + (conta.baixada ? "baixada" : "aberta") + '</span><button class="botao-linha" type="button" data-baixa="' + ui.esc(conta.id) + '">Dar baixa</button></article>';
    }).join("") + '</section><p><a class="botao-linha" href="' + ui.url("paginas/admin/conciliacao.html") + '">Conciliação da aba Total</a> <a class="botao-linha" href="' + ui.url("paginas/admin/energia.html") + '">Fechamento de energia</a></p><div class="chips">' + competencias.map(function (comp) { return '<a class="chip" href="' + ui.url("paginas/admin/mes.html?mes=" + comp) + '">' + dinheiro.competenciaBR(comp) + "</a>"; }).join("") + "</div>";
    document.querySelector('[name=competencia]').value = atual;
    document.getElementById("meta").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var valor = Number(new FormData(evento.target).get("valor"));
      Cria.alterar(function (atualEstado) { atualEstado.metas[atual] = valor; });
      painel();
    });
    document.getElementById("balcao").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atualEstado) {
        atualEstado.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: String(dados.get("data")), competencia: String(dados.get("competencia")), descricao: String(dados.get("item")), categoria: "receita", tipoInsumo: "", quantidade: Number(dados.get("quantidade")) || 1, valor: Number(dados.get("valor")) || 0, link: "", kwh: null, tarifa: null, estornado: false, comprovante: "", origem: "balcao" });
      });
      painel();
    });
    document.getElementById("lancamento").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      var arquivo = evento.target.comprovante.files[0];
      var tipo = String(dados.get("tipoNovo") || dados.get("tipoInsumo") || "");
      Cria.alterar(function (atualEstado) {
        if (dados.get("tipoNovo")) atualEstado.configuracao.tiposInsumo.push(String(dados.get("tipoNovo")));
        atualEstado.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: String(dados.get("data")), competencia: String(dados.get("competencia")), descricao: String(dados.get("descricao")), categoria: String(dados.get("categoria")), tipoInsumo: tipo, quantidade: Number(dados.get("quantidade")) || 0, valor: Number(dados.get("valor")) || 0, link: String(dados.get("link") || ""), kwh: null, tarifa: null, estornado: false, comprovante: arquivo ? arquivo.name : "", origem: "manual" });
      });
      painel();
    });
    var filtrados = estado().lancamentos.slice();
    function pintarTabela(lista) {
      document.getElementById("tabela").innerHTML = '<table><thead><tr><th>Data</th><th>Descrição</th><th>Categoria</th><th>Tipo</th><th>Qtd</th><th>Valor</th><th>Link</th></tr></thead><tbody>' + lista.map(function (item) {
        return "<tr><td>" + ui.esc(dinheiro.dataBR(item.data)) + "</td><td>" + ui.esc(item.descricao) + "</td><td>" + ui.esc(item.categoria) + "</td><td>" + ui.esc(item.tipoInsumo) + "</td><td>" + item.quantidade + "</td><td>" + dinheiro.real(item.valor) + "</td><td>" + ui.esc(item.link) + "</td></tr>";
      }).join("") + "</tbody></table>";
    }
    pintarTabela(filtrados);
    document.getElementById("filtros").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      filtrados = estado().lancamentos.filter(function (item) {
        if (dados.get("de") && String(item.data) < String(dados.get("de"))) return false;
        if (dados.get("ate") && String(item.data) > String(dados.get("ate"))) return false;
        if (dados.get("categoria") && item.categoria !== dados.get("categoria")) return false;
        if (dados.get("tipo") && String(item.tipoInsumo).toLowerCase().indexOf(String(dados.get("tipo")).toLowerCase()) < 0) return false;
        if (dados.get("texto") && String(item.descricao).toLowerCase().indexOf(String(dados.get("texto")).toLowerCase()) < 0) return false;
        if (dados.get("link") && String(item.link).toLowerCase().indexOf(String(dados.get("link")).toLowerCase()) < 0) return false;
        return true;
      });
      pintarTabela(filtrados);
    });
    document.getElementById("csv").addEventListener("click", function () {
      var csv = "data;descricao;categoria;tipo;quantidade;valor;link;competencia\n" + filtrados.map(function (item) {
        return [item.data, item.descricao, item.categoria, item.tipoInsumo, item.quantidade, item.valor, item.link, item.competencia].map(function (valor) {
          return '"' + String(valor == null ? "" : valor).replace(/"/g, '""') + '"';
        }).join(";");
      }).join("\n");
      var blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8" });
      var link = document.createElement("a");
      link.href = URL.createObjectURL(blob);
      link.download = "lancamentos.csv";
      link.click();
    });
    raiz.addEventListener("click", function (evento) {
      var id = evento.target.getAttribute && evento.target.getAttribute("data-baixa");
      if (!id) return;
      var arquivo = prompt("Nome do comprovante da baixa", "comprovante.pdf");
      if (arquivo == null) return;
      Cria.alterar(function (atualEstado) {
        var conta = atualEstado.contas.find(function (item) { return item.id === id; });
        if (conta) { conta.baixada = true; conta.comprovante = arquivo; }
      });
      painel();
    });
  }

  function mes() {
    var comp = new URLSearchParams(location.search).get("mes") || mesAtual();
    document.title = dinheiro.competenciaBR(comp) + " · Financeiro";
    var lista = estado().lancamentos.filter(function (item) { return item.competencia === comp && !item.estornado; });
    function bloco(titulo, categoria) {
      var itens = lista.filter(function (item) { return item.categoria === categoria; });
      var linhas = itens.map(function (item) {
        var extra = item.kwh ? " · " + item.kwh + " kWh × " + item.tarifa : "";
        return "<tr><td>" + ui.esc(dinheiro.dataBR(item.data)) + "</td><td>" + ui.esc(item.descricao) + extra + "</td><td>" + ui.esc(item.tipoInsumo) + "</td><td>" + item.quantidade + "</td><td>" + dinheiro.real(item.valor) + "</td></tr>";
      }).join("");
      var media = "";
      if (categoria === "filamento") {
        ["PLA", "PETG", "TPU"].forEach(function (tipo) {
          var conta = calc.mediaFilamento(estado().lancamentos, comp, tipo);
          media += "<p>" + tipo + " " + dinheiro.realKg(conta.media) + " por kg (" + dinheiro.real(conta.valor) + " em " + conta.quantidade + " kg)</p>";
        });
      }
      return '<section class="bloco"><h2>' + titulo + "</h2>" + media + '<div class="tabela-rolagem"><table><tbody>' + linhas + "</tbody></table></div></section>";
    }
    var resumo = calc.resumirMes(estado().lancamentos, comp);
    var compra = new URLSearchParams(location.search).get("compra") || "";
    raiz.innerHTML = "<h1>" + ui.esc(dinheiro.competenciaBR(comp)) + "</h1>" + bloco("Gastos fixos", "gastoFixo") + bloco("Energia", "energia") + bloco("Filamentos", "filamento") + bloco("Outros insumos", "outroInsumo") + bloco("Vendas", "receita") + '<section class="cartao"><h2>Resultados</h2><p>Quantidade vendida ' + resumo.quantidade + "</p><p>Receita " + dinheiro.real(resumo.receita) + "</p><p>Despesa operacional " + dinheiro.real(resumo.despesaOperacional) + "</p><p>Gastos fixos " + dinheiro.real(resumo.gastosFixos) + "</p><p>Investimento " + dinheiro.real(resumo.investimento) + "</p><p><strong>Resultado operacional " + dinheiro.real(resumo.resultadoOperacional) + "</strong></p><p><strong>Resultado de caixa " + dinheiro.real(resumo.resultadoCaixa) + "</strong></p><p>Lucro " + dinheiro.real(resumo.lucro) + "</p></section>" + '<form id="compra" class="cartao formulario"><h2>Lançar compra de insumo</h2><label>Descrição<input name="descricao" value="' + ui.esc(compra) + '" required></label><label>Tipo<input name="tipo" value="' + ui.esc(compra) + '"></label><label>Quantidade<input name="quantidade" type="number" step="0.01" value="1"></label><label>Valor<input name="valor" type="number" step="0.01" required></label><button class="botao" type="submit">Confirmar compra</button></form>';
    document.getElementById("compra").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: dinheiro.hojeISO(), competencia: comp, descricao: String(dados.get("descricao")), categoria: "filamento", tipoInsumo: String(dados.get("tipo") || ""), quantidade: Number(dados.get("quantidade")) || 0, valor: Number(dados.get("valor")) || 0, link: "", kwh: null, tarifa: null, estornado: false, comprovante: "", origem: "compra" });
        var insumo = atual.estoqueInsumos.find(function (item) { return item.nome.toLowerCase() === String(dados.get("tipo")).toLowerCase(); });
        if (insumo) insumo.saldo += Number(dados.get("quantidade")) || 0;
      });
      location.href = ui.url("paginas/admin/mes.html?mes=" + comp);
    });
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
