(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var calc = CriaCalculos;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function descontoMarcado(orcamento) {
    var mapa = estado().descontosOrcamento;
    return (orcamento.descontosMarcados || []).reduce(function (total, chave) { return total + (Number(mapa[chave]) || 0); }, 0);
  }

  function entradaDe(orcamento, margem, pintura) {
    return {
      precoKg: orcamento.precoKg,
      pesoG: orcamento.pesoG,
      lixoG: orcamento.lixoG,
      minutos: orcamento.minutos,
      servico: orcamento.servico,
      itens: orcamento.itens,
      energiaPorMinuto: orcamento.energiaPorMinuto,
      depreciacao: orcamento.depreciacao,
      manutencao: orcamento.manutencao,
      taxaFalha: orcamento.taxaFalha,
      margem: margem,
      desconto: descontoMarcado(orcamento),
      pintura: pintura
    };
  }

  function precoTravado(material) {
    var mapa = { PLA: 85, PETG: 65, TPU: 80 };
    var parametro = estado().parametrosCusto.find(function (item) { return item.nome === material && item.travado; });
    return parametro ? parametro.preco : mapa[material] || 0;
  }

  function precoMedio(material) {
    var conta = calc.mediaFilamento(estado().lancamentos, "", material);
    return conta.media || precoTravado(material);
  }

  function lista() {
    document.title = "Orçamentos · Administração";
    raiz.innerHTML = '<h1>Orçamentos</h1><p><a class="botao" href="' + ui.url("paginas/admin/orcamento.html?novo=1") + '">Novo orçamento</a></p>' + estado().orcamentos.map(function (item) {
      return '<article class="cartao"><a href="' + ui.url("paginas/admin/orcamento.html?id=" + encodeURIComponent(item.id)) + '"><strong>' + ui.esc(item.nome) + "</strong></a><p>" + ui.esc(item.material) + " · " + item.minutos + " min · " + (item.enviado ? "enviado" : "rascunho") + "</p></article>";
    }).join("") + '<section class="cartao"><h2>Parâmetros</h2><form id="parametro" class="linha"><input name="nome" placeholder="Nome" required><input name="preco" type="number" step="0.0001" placeholder="Preço" required><button class="botao-linha" type="submit">Incluir</button></form><div id="parametros"></div><h2>Margens</h2><div id="margens"></div></section>';
    document.getElementById("parametros").innerHTML = estado().parametrosCusto.map(function (item) {
      return '<p class="linha">' + ui.esc(item.nome) + " · " + dinheiro.real(item.preco, 4) + ' <button class="botao-linha" type="button" data-remover="' + ui.esc(item.id) + '">Remover</button></p>';
    }).join("");
    document.getElementById("margens").innerHTML = estado().margens.map(function (item) {
      return '<label class="linha">' + ui.esc(item.nome) + ' <input data-margem="' + ui.esc(item.id) + '" type="number" step="0.01" value="' + item.percentual + '"></label>';
    }).join("");
    document.getElementById("parametro").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.parametrosCusto.push({ id: CriaCarrinho.novoId("par"), nome: String(dados.get("nome")), unidade: "un", preco: Number(dados.get("preco")) || 0 });
      });
      lista();
    });
    raiz.addEventListener("click", function (evento) {
      var id = evento.target.getAttribute && evento.target.getAttribute("data-remover");
      if (!id) return;
      Cria.alterar(function (atual) { atual.parametrosCusto = atual.parametrosCusto.filter(function (item) { return item.id !== id; }); });
      lista();
    });
    raiz.addEventListener("change", function (evento) {
      var id = evento.target.getAttribute && evento.target.getAttribute("data-margem");
      if (!id) return;
      Cria.alterar(function (atual) {
        var margem = atual.margens.find(function (item) { return item.id === id; });
        if (margem) margem.percentual = Number(evento.target.value) || 0;
      });
    });
  }

  function editor() {
    var params = new URLSearchParams(location.search);
    var id = params.get("id");
    var orcamento = estado().orcamentos.find(function (item) { return item.id === id; });
    if (!orcamento && params.get("novo")) {
      var referencia = estado().referenciaMaquina;
      var maquina = estado().maquinas.find(function (item) { return item.id === "maq-bambu"; });
      orcamento = {
        id: "",
        nome: "Orçamento novo",
        material: "PLA",
        minutos: 60,
        pesoG: 20,
        lixoG: 0,
        servico: 0,
        itens: [],
        margemSite: 0.4,
        descontosMarcados: [],
        precoKg: precoTravado("PLA"),
        precoKgOrigem: "travado",
        energiaPorMinuto: maquina ? maquina.energiaPorMinuto : referencia.energiaPorMinuto,
        depreciacao: maquina ? maquina.depreciacaoPorMinuto : referencia.depreciacao,
        manutencao: maquina ? maquina.manutencaoPorMinuto : referencia.manutencao,
        taxaFalha: estado().configuracao.taxaFalha,
        enviado: false,
        maquinaId: "maq-bambu",
        freteCanal: estado().configuracao.freteCanal || 0,
        pintura: null,
        realizado: null
      };
    }
    if (!orcamento) {
      raiz.innerHTML = ui.vazio("Orçamento não encontrado.");
      return;
    }
    document.title = orcamento.nome + " · Orçamento";
    function desenhar() {
      var site = calc.orcar(entradaDe(orcamento, orcamento.margemSite, null));
      var mercado = calc.orcar(entradaDe(orcamento, 0.25, null));
      var shopee = calc.precoCanal(mercado.original, estado().faixasShopee, orcamento.freteCanal);
      var ml = calc.precoCanal(mercado.original, estado().faixasMercadoLivre, orcamento.freteCanal);
      var pinturaSimples = calc.orcar(entradaDe(orcamento, orcamento.margemSite, { percentual: 0.2, itens: [{ quantidade: 1, preco: 2 }] }));
      var taxaMin = orcamento.energiaPorMinuto + orcamento.depreciacao + orcamento.manutencao;
      raiz.innerHTML = '<h1>' + ui.esc(orcamento.nome) + '</h1><form id="orc" class="cartao formulario"><div class="grade-3"><label>Nome<input name="nome" value="' + ui.esc(orcamento.nome) + '"></label><label>Material<select name="material"><option>PLA</option><option>PETG</option><option>TPU</option></select></label><label>Minutos<input name="minutos" type="number" step="1" value="' + orcamento.minutos + '"></label><label>Peso (g)<input name="pesoG" type="number" step="0.01" value="' + orcamento.pesoG + '"></label><label>Lixo (g)<input name="lixoG" type="number" step="0.01" value="' + orcamento.lixoG + '"></label><label>Origem do kg<select name="precoKgOrigem"><option value="travado">Preço travado</option><option value="medio">Preço médio atual</option></select></label></div><label><input type="checkbox" name="recorrente"' + (orcamento.descontosMarcados.indexOf("recorrente") >= 0 ? " checked" : "") + "> Cliente recorrente 5%</label><label><input type=\"checkbox\" name=\"volume\"" + (orcamento.descontosMarcados.indexOf("volume") >= 0 ? " checked" : "") + '> Volume 5%</label><label>Frete do canal<input name="freteCanal" type="number" step="0.01" value="' + orcamento.freteCanal + '"></label><button class="botao" type="submit">Salvar</button> <button class="botao-teal" type="button" id="enviar">Enviar</button></form><section class="cartao"><h2>Conta</h2><p>Filamento ' + dinheiro.real(site.filamento, 4) + "</p><p>Parâmetros " + dinheiro.real(site.parametros, 4) + "</p><p>Máquina " + orcamento.minutos + " min × " + dinheiro.real(taxaMin, 3) + " = " + dinheiro.real(site.tempo, 4) + "</p><p>Custo " + dinheiro.real(site.custo, 4) + " · mostrado " + dinheiro.real(site.custo) + "</p><p>Prazo " + site.horas + " horas, " + site.dias + " dia</p><p>Valor que precisa sobrar no marketplace " + dinheiro.real(mercado.original) + " · lucro " + dinheiro.real(calc.centavos(mercado.lucro)) + '</p></section><div class="cartoes-preco"><article class="cartao"><h2>Site</h2><p class="numero-grande">' + dinheiro.real(site.direto) + '</p></article><article class="cartao"><h2>Shopee</h2><p class="numero-grande">' + (shopee.preco == null ? "—" : dinheiro.real(shopee.preco)) + "</p><p>Comissão " + dinheiro.real(shopee.comissao) + "</p><p>Líquido " + (shopee.liquido == null ? "—" : dinheiro.real(shopee.liquido)) + "</p>" + (shopee.aviso ? '<p class="aviso">' + ui.esc(shopee.aviso) + " Líquido em R$ 79,99: " + dinheiro.real(shopee.liquido7999) + ". Líquido em R$ 80,00: " + dinheiro.real(shopee.liquido80) + ".</p>" : "") + '</article><article class="cartao"><h2>Mercado Livre</h2><p class="numero-grande">' + (ml.preco == null ? "Faixas vazias" : dinheiro.real(ml.preco)) + "</p><p>Margem de marketplace 25%.</p>" + (ml.faixasVazias ? "<p>As faixas ainda não foram preenchidas.</p>" : "") + "</article></div>" + (orcamento.nome === "Cachorro branco" ? '<section class="cartao"><h2>Pintura simples com R$ 2,00 de material</h2><p>Com pintura ' + dinheiro.real(pinturaSimples.direto) + " ao lado de " + dinheiro.real(site.direto) + " sem pintura.</p></section>" : "") + '<section class="cartao"><h2>Realizado</h2><form id="realizado" class="formulario"><label>Tempo real (min)<input name="tempoReal" type="number" step="1" value="' + ((orcamento.realizado && orcamento.realizado.tempoReal) || "") + '"></label><label>Peso na balança (g)<input name="pesoReal" type="number" step="0.01" value="' + ((orcamento.realizado && orcamento.realizado.pesoReal) || "") + '"></label><label>Extras<input name="extras" type="number" step="0.01" value="' + ((orcamento.realizado && orcamento.realizado.extras) || 0) + '"></label><label>Energia da tomada<input name="energiaTomada" type="number" step="0.01" value="' + ((orcamento.realizado && orcamento.realizado.energiaTomada) || 0) + '"></label><button class="botao-linha" type="submit">Guardar realizado</button></form><div id="diferenca"></div></section>';
      document.querySelector('[name=material]').value = orcamento.material;
      document.querySelector('[name=precoKgOrigem]').value = orcamento.precoKgOrigem;
      if (orcamento.realizado) mostrarDiferenca();
      document.getElementById("orc").addEventListener("submit", function (evento) {
        evento.preventDefault();
        gravar(false);
      });
      document.getElementById("enviar").addEventListener("click", function () { gravar(true); });
      document.getElementById("realizado").addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        orcamento.realizado = { tempoReal: Number(dados.get("tempoReal")) || 0, pesoReal: Number(dados.get("pesoReal")) || 0, extras: Number(dados.get("extras")) || 0, energiaTomada: Number(dados.get("energiaTomada")) || 0 };
        if (orcamento.id) Cria.alterar(function (atual) {
          var salvo = atual.orcamentos.find(function (item) { return item.id === orcamento.id; });
          if (salvo) salvo.realizado = orcamento.realizado;
        });
        mostrarDiferenca();
      });
    }
    function mostrarDiferenca() {
      var real = orcamento.realizado;
      if (!real) return;
      var previsto = calc.orcar(entradaDe(orcamento, orcamento.margemSite, null));
      var custoReal = calc.orcar(entradaDe(Object.assign({}, orcamento, { minutos: real.tempoReal || orcamento.minutos, pesoG: real.pesoReal || orcamento.pesoG, servico: (orcamento.servico || 0) + (real.extras || 0) + (real.energiaTomada || 0) }), orcamento.margemSite, null)).custo;
      document.getElementById("diferenca").innerHTML = "<p>Orçado " + dinheiro.real(previsto.custo) + " · realizado " + dinheiro.real(custoReal) + " · diferença " + dinheiro.real(custoReal - previsto.custo) + "</p>";
    }
    function gravar(enviar) {
      var dados = new FormData(document.getElementById("orc"));
      var material = String(dados.get("material"));
      var origem = String(dados.get("precoKgOrigem"));
      var descontos = [];
      if (dados.get("recorrente")) descontos.push("recorrente");
      if (dados.get("volume")) descontos.push("volume");
      var novoPreco = orcamento.enviado ? orcamento.precoKg : (origem === "medio" ? precoMedio(material) : precoTravado(material));
      var taxas = orcamento.enviado ? { energiaPorMinuto: orcamento.energiaPorMinuto, depreciacao: orcamento.depreciacao, manutencao: orcamento.manutencao, taxaFalha: orcamento.taxaFalha } : (function () {
        var maquina = estado().maquinas.find(function (item) { return item.id === orcamento.maquinaId; }) || {};
        return { energiaPorMinuto: maquina.energiaPorMinuto, depreciacao: maquina.depreciacaoPorMinuto, manutencao: maquina.manutencaoPorMinuto, taxaFalha: estado().configuracao.taxaFalha };
      })();
      Cria.alterar(function (atual) {
        var salvo = atual.orcamentos.find(function (item) { return item.id === orcamento.id; });
        if (!salvo) {
          salvo = Object.assign({}, orcamento, { id: CriaCarrinho.novoId("orc") });
          atual.orcamentos.push(salvo);
          orcamento.id = salvo.id;
        }
        salvo.nome = String(dados.get("nome"));
        salvo.material = material;
        salvo.minutos = Number(dados.get("minutos")) || 0;
        salvo.pesoG = Number(dados.get("pesoG")) || 0;
        salvo.lixoG = Number(dados.get("lixoG")) || 0;
        salvo.precoKgOrigem = origem;
        salvo.precoKg = novoPreco;
        salvo.descontosMarcados = descontos;
        salvo.freteCanal = Number(dados.get("freteCanal")) || 0;
        salvo.energiaPorMinuto = taxas.energiaPorMinuto;
        salvo.depreciacao = taxas.depreciacao;
        salvo.manutencao = taxas.manutencao;
        salvo.taxaFalha = taxas.taxaFalha;
        if (enviar) salvo.enviado = true;
        orcamento = salvo;
      });
      if (enviar && !params.get("id")) location.href = ui.url("paginas/admin/orcamento.html?id=" + encodeURIComponent(orcamento.id));
      else desenhar();
    }
    desenhar();
  }

  if (pagina === "admin-orcamentos") lista();
  if (pagina === "admin-orcamento") editor();
})();
