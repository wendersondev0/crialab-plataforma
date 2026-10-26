(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function guia() {
    if (estado().guiaVisto) return "";
    return '<div class="guia" id="guia"><div class="cartao"><img src="' + ui.url("marca/mascote.png") + '" alt="" style="width:120px"><h2>Primeiro acesso</h2><ol><li><a href="' + ui.url("paginas/admin/financeiro.html?aba=lancar") + '">Lançar uma despesa</a></li><li><a href="' + ui.url("paginas/admin/financeiro.html?mes=2026-09&aba=caderno") + '">Ver o caderno do mês</a></li><li><a href="' + ui.url("paginas/admin/orcamento.html?novo=1") + '">Fazer um orçamento</a></li></ol><button class="botao-linha" type="button" id="pular-guia">Pular</button></div></div>';
  }

  function painel() {
    document.title = "Administração · Cria Lab DM";
    var mostrarGuia = !estado().guiaVisto;
    var htmlGuia = mostrarGuia ? guia() : "";
    if (mostrarGuia) Cria.alterar(function (atual) { atual.guiaVisto = true; });
    var areas = [
      ["paginas/admin/produtos.html", "Gestão de produtos", "Pronta entrega, sob demanda e personalizados."],
      ["paginas/admin/marketing.html", "Marketing", "Cupom da loja."],
      ["paginas/admin/contato.html", "Atendimento ao cliente", "Mensagens e perguntas."],
      ["paginas/admin/patrimonio.html", "Patrimônio e estoque", "Máquinas, filamento e peças."],
      ["paginas/admin/pedidos.html", "Pedidos, OS e fiscal", "Status, recibos, notas e ordens de serviço."],
      ["paginas/admin/financeiro.html", "Gestão financeira", "Despesas, receitas e o mês."],
      ["paginas/admin/configuracao.html", "Administração do sistema", "Loja, frete, site e restauração."],
      ["paginas/admin/impressao.html", "Gestão de impressão 3D", "Mesa, fila, qualidade e manutenção."]
    ];
    function contagem(mod) {
      return estado().produtos.filter(function (item) {
        var m = item.modalidade || (item.tipo === "personalizado" ? "personalizado" : "pronta-entrega");
        return m === mod && item.situacao === "publicado";
      }).length;
    }
    var pronta = contagem("pronta-entrega");
    var demanda = contagem("sob-demanda");
    var sobMedida = contagem("personalizado");
    var abertos = (estado().mensagens || []).filter(function (item) { return item.status === "aberto"; }).length;
    var resumo = '<a class="painel-kpi revela rounded-[18px] border-2 border-linha bg-white p-4 shadow-cria no-underline" href="' + ui.url("paginas/admin/produtos.html?modalidade=pronta-entrega") + '"><strong class="block text-3xl font-extrabold text-laranja">' + pronta + '</strong><span class="mt-1 block font-bold text-tinta/70">Pronta entrega</span></a><a class="painel-kpi revela rounded-[18px] border-2 border-linha bg-white p-4 shadow-cria no-underline" href="' + ui.url("paginas/admin/produtos.html?modalidade=sob-demanda") + '"><strong class="block text-3xl font-extrabold text-teal">' + demanda + '</strong><span class="mt-1 block font-bold text-tinta/70">Sob demanda</span></a><a class="painel-kpi revela rounded-[18px] border-2 border-linha bg-white p-4 shadow-cria no-underline" href="' + ui.url("paginas/admin/produtos.html?modalidade=personalizado") + '"><strong class="block text-3xl font-extrabold text-tinta">' + sobMedida + '</strong><span class="mt-1 block font-bold text-tinta/70">Personalizados</span></a><a class="painel-kpi revela rounded-[18px] border-2 border-linha bg-white p-4 shadow-cria no-underline" href="' + ui.url("paginas/admin/contato.html") + '"><strong class="block text-3xl font-extrabold text-tinta">' + abertos + '</strong><span class="mt-1 block font-bold text-tinta/70">Atendimentos abertos</span></a>';
    var atalhos = areas.map(function (area) {
      return '<a class="painel-area revela rounded-[18px] border-2 border-linha bg-white p-4 no-underline shadow-cria" href="' + ui.url(area[0]) + '"><h2 class="mt-0 text-lg font-extrabold text-tinta no-underline">' + ui.esc(area[1]) + '</h2><p class="m-0 mt-1 text-sm text-tinta/65 no-underline">' + ui.esc(area[2]) + "</p></a>";
    }).join("");
    raiz.innerHTML = htmlGuia + '<h1 class="mt-0 mb-4 text-2xl font-extrabold text-tinta no-underline">Painel gerencial</h1><div class="painel-grade grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">' + resumo + atalhos + '</div><p class="painel-acoes mt-5"><button class="botao-linha inline-flex min-h-11 items-center justify-center gap-2 rounded-full border-2 border-tinta bg-white px-4 font-bold text-tinta no-underline" type="button" id="restaurar">Restaurar carga inicial</button></p>';
    var pular = document.getElementById("pular-guia");
    if (pular) pular.addEventListener("click", function () {
      Cria.alterar(function (atual) { atual.guiaVisto = true; });
      document.getElementById("guia").remove();
    });
    document.getElementById("restaurar").addEventListener("click", function () {
      if (!confirm("Isso apaga as alterações deste navegador e volta à carga inicial.")) return;
      Cria.restaurar();
      location.reload();
    });
    ui.revelar(raiz);
  }

  function produtos() {
    document.title = "Gestão de produtos · Administração";
    var params = new URLSearchParams(location.search);
    var vista = {
      q: params.get("q") || "",
      situacao: params.get("situacao") || "",
      modalidade: params.get("modalidade") || "",
      ordem: params.get("ordem") || "titulo",
      dir: params.get("dir") === "desc" ? "desc" : "asc"
    };
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function modalidadeDe(produto) {
      return produto.modalidade || (produto.tipo === "personalizado" ? "personalizado" : "pronta-entrega");
    }
    function base() {
      return estado().produtos.slice();
    }
    function contagemMod(mod) {
      return base().filter(function (item) { return modalidadeDe(item) === mod; }).length;
    }
    function pctDesconto(produto) {
      var anterior = Number(produto.precoComparativo) || 0;
      var atual = Number(produto.preco) || 0;
      if (anterior <= atual || atual <= 0) return 0;
      return Math.round((1 - atual / anterior) * 100);
    }
    function seloSit(situacao) {
      var classe = situacao === "publicado" ? "selo-sit selo-publicado" : (situacao === "pausado" ? "selo-sit selo-pausado" : "selo-sit selo-esgotado");
      return '<span class="' + classe + '">' + ui.esc(situacao || "—") + "</span>";
    }
    function seloMod(mod) {
      var nome = ui.nomeModalidade ? ui.nomeModalidade(mod) : mod;
      var classe = mod === "personalizado" ? "selo-mod selo-pers" : (mod === "sob-demanda" ? "selo-mod selo-demanda" : "selo-mod selo-pronta");
      return '<span class="' + classe + '">' + ui.esc(nome) + "</span>";
    }
    function cabOrdena(chave, rotulo) {
      var ativo = vista.ordem === chave;
      var seta = !ativo ? "" : (vista.dir === "asc" ? " ↑" : " ↓");
      return '<button class="th-ordena' + (ativo ? " ativo" : "") + '" type="button" data-ordenar="' + chave + '">' + ui.esc(rotulo) + seta + "</button>";
    }
    function filtrar() {
      var texto = vista.q.trim().toLowerCase();
      var lista = base().filter(function (produto) {
        if (vista.situacao === "esgotado") {
          if (produto.situacao !== "esgotado" && Number(produto.estoque) > 0) return false;
        } else if (vista.situacao && produto.situacao !== vista.situacao) return false;
        if (vista.modalidade && modalidadeDe(produto) !== vista.modalidade) return false;
        if (texto && (produto.titulo + " " + (produto.material || "") + " " + (produto.id || "") + " " + ui.nomeCategoria(produto.categoria)).toLowerCase().indexOf(texto) < 0) return false;
        return true;
      });
      var sinal = vista.dir === "desc" ? -1 : 1;
      lista.sort(function (a, b) {
        var r = 0;
        if (vista.ordem === "preco") r = (Number(a.preco) || 0) - (Number(b.preco) || 0);
        else if (vista.ordem === "estoque") r = (Number(a.estoque) || 0) - (Number(b.estoque) || 0);
        else if (vista.ordem === "vendas") r = (Number(a.vendas) || 0) - (Number(b.vendas) || 0);
        else if (vista.ordem === "criadoEm") r = String(a.criadoEm || "").localeCompare(String(b.criadoEm || ""));
        else if (vista.ordem === "situacao") r = String(a.situacao || "").localeCompare(String(b.situacao || ""));
        else r = String(a.titulo || "").localeCompare(String(b.titulo || ""), "pt-BR");
        return r * sinal || String(a.titulo || "").localeCompare(String(b.titulo || ""), "pt-BR");
      });
      return lista;
    }
    function cardLinha(produto) {
      var foto = (produto.fotos && produto.fotos[0]) ? produto.fotos[0] : null;
      var thumb = foto
        ? '<img class="admin-prod-thumb" src="' + ui.url(foto.arquivo) + '" alt="">'
        : '<span class="admin-prod-thumb vazia">' + ico("caixa") + "</span>";
      var pct = pctDesconto(produto);
      var desconto = pct ? '<span class="selo-desc">-' + pct + "%</span>" : "";
      var precoAnt = pct ? '<s class="preco-ant">' + dinheiro.real(produto.precoComparativo) + "</s>" : "";
      var mod = modalidadeDe(produto);
      var personalizado = mod === "personalizado";
      var estoqueBaixo = !personalizado && Number(produto.estoque) > 0 && Number(produto.estoque) <= 2;
      var estoqueClasse = !personalizado && Number(produto.estoque) <= 0 ? "estoque-zero" : (estoqueBaixo ? "estoque-baixo" : "");
      var href = ui.url("paginas/admin/produto.html?id=" + encodeURIComponent(produto.id));
      var estoqueTxt = personalizado
        ? '<span class="admin-prod-estoque">Preço médio</span>'
        : '<span class="admin-prod-estoque ' + estoqueClasse + '">Estoque ' + (Number(produto.estoque) || 0) + "</span>";
      return '<article class="admin-prod-card" data-id="' + ui.esc(produto.id) + '">' +
        '<div class="admin-prod-foto">' + thumb + desconto + "</div>" +
        '<div class="admin-prod-info">' +
          '<div class="admin-prod-topo">' + seloMod(mod) + seloSit(produto.situacao) + "</div>" +
          '<h3 class="admin-prod-titulo">' + ui.esc(produto.titulo) + "</h3>" +
          '<p class="admin-prod-meta">' + ui.esc(ui.nomeCategoria(produto.categoria)) + " · " + ui.esc(dinheiro.dataBR(produto.sessao || produto.criadoEm)) + "</p>" +
          '<div class="admin-prod-nums"><span class="admin-prod-preco">' + dinheiro.real(produto.preco) + "</span>" + precoAnt + estoqueTxt + '<span class="admin-prod-vendas">' + (Number(produto.vendas) || 0) + " vendas</span></div>" +
        "</div>" +
        '<div class="admin-prod-acoes">' +
          '<a class="botao-linha admin-acao" href="' + href + '">' + ico("documento") + "<span>Editar</span></a>" +
          '<button class="botao-linha admin-acao" type="button" data-acao="duplicar">' + ico("caixa") + "<span>Duplicar</span></button>" +
          (produto.situacao === "publicado"
            ? '<button class="botao-linha admin-acao" type="button" data-acao="pausar">' + ico("fechar") + "<span>Pausar</span></button>"
            : '<button class="botao-linha admin-acao" type="button" data-acao="publicar">' + ico("sobre") + "<span>Publicar</span></button>") +
          '<button class="botao-linha admin-acao perigo" type="button" data-acao="excluir">' + ico("fechar") + "<span>Excluir</span></button>" +
        "</div></article>";
    }
    function pintar() {
      var lista = filtrar();
      var todos = base();
      var kpis = '<div class="admin-prod-kpis">' +
        '<button class="admin-prod-kpi' + (!vista.modalidade ? " ativo" : "") + '" type="button" data-filtro-mod=""><strong>' + todos.length + "</strong><span>Todas</span></button>" +
        '<button class="admin-prod-kpi' + (vista.modalidade === "pronta-entrega" ? " ativo" : "") + '" type="button" data-filtro-mod="pronta-entrega"><strong>' + contagemMod("pronta-entrega") + "</strong><span>Pronta Entrega</span></button>" +
        '<button class="admin-prod-kpi' + (vista.modalidade === "sob-demanda" ? " ativo" : "") + '" type="button" data-filtro-mod="sob-demanda"><strong>' + contagemMod("sob-demanda") + "</strong><span>Sob Demanda</span></button>" +
        '<button class="admin-prod-kpi' + (vista.modalidade === "personalizado" ? " ativo" : "") + '" type="button" data-filtro-mod="personalizado"><strong>' + contagemMod("personalizado") + "</strong><span>Personalizados</span></button>" +
        "</div>";
      function opcao(valor, rotulo, atual) {
        return '<option value="' + valor + '"' + (atual === valor ? " selected" : "") + ">" + rotulo + "</option>";
      }
      var filtros = [
        '<form class="admin-prod-filtros" id="filtros-produtos">',
        '<label class="campo-busca"><span class="visually-hidden">Buscar</span>' + ico("busca") + '<input name="q" type="search" placeholder="Buscar peça, material ou categoria" value="' + ui.esc(vista.q) + '"></label>',
        '<label>Modalidade<select name="modalidade">' + opcao("", "Todas", vista.modalidade) + opcao("pronta-entrega", "Pronta Entrega", vista.modalidade) + opcao("sob-demanda", "Sob Demanda", vista.modalidade) + opcao("personalizado", "Personalizados", vista.modalidade) + "</select></label>",
        '<label>Situação<select name="situacao">' + opcao("", "Todas", vista.situacao) + opcao("publicado", "Publicado", vista.situacao) + opcao("pausado", "Pausado", vista.situacao) + opcao("esgotado", "Esgotado / sem estoque", vista.situacao) + "</select></label>",
        '<label>Ordenar<select name="ordem">' + opcao("titulo", "Nome", vista.ordem) + opcao("preco", "Preço", vista.ordem) + opcao("estoque", "Estoque", vista.ordem) + opcao("vendas", "Vendas", vista.ordem) + opcao("criadoEm", "Data", vista.ordem) + opcao("situacao", "Situação", vista.ordem) + "</select></label>",
        '<button class="botao-linha" type="button" id="inverter-ordem" title="Inverter ordem">' + (vista.dir === "asc" ? "A→Z" : "Z→A") + "</button>",
        "</form>"
      ].join("");
      var fluxo = '<div class="admin-prod-fluxo">' +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/orcamentos.html") + '">' + ico("documento") + "<span>Orçamentos</span></a>" +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/pedidos.html") + '">' + ico("pedidos") + "<span>Pedidos</span></a>" +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/os.html") + '">' + ico("estoque") + "<span>Ordens de serviço</span></a>" +
        "</div>";
      var barraOrd = '<div class="admin-prod-ordena">' + cabOrdena("titulo", "Peça") + cabOrdena("situacao", "Situação") + cabOrdena("preco", "Preço") + cabOrdena("estoque", "Estoque") + cabOrdena("vendas", "Vendas") + "</div>";
      var corpo = lista.length
        ? '<div class="admin-prod-lista">' + lista.map(cardLinha).join("") + "</div>"
        : ui.vazio("Nenhuma peça com esses filtros.");
      raiz.innerHTML = '<div class="admin-prod-cab"><div><h1 class="m-0">Gestão de produtos</h1><p class="admin-prod-intro m-0 mt-1">Pronta entrega, sob demanda e personalizados no mesmo lugar. Filtre por modalidade, ordene e gerencie a peça sem sair da lista.</p></div><a class="botao" href="' + ui.url("paginas/admin/produto.html") + '">' + ico("caixa") + "<span>Novo produto</span></a></div>" + kpis + fluxo + filtros + '<div class="admin-prod-painel cartao"><div class="admin-prod-resumo"><strong>' + lista.length + "</strong> peça" + (lista.length === 1 ? "" : "s") + " nesta vista</div>" + barraOrd + corpo + "</div>";
      document.querySelector('[name="ordem"]').value = vista.ordem;
      ligar();
      ui.revelar(raiz);
    }
    function ligar() {
      var form = document.getElementById("filtros-produtos");
      function lerForm() {
        var dados = new FormData(form);
        vista.q = String(dados.get("q") || "");
        vista.modalidade = String(dados.get("modalidade") || "");
        vista.situacao = String(dados.get("situacao") || "");
        vista.ordem = String(dados.get("ordem") || "titulo");
      }
      form.addEventListener("submit", function (evento) { evento.preventDefault(); });
      form.querySelector('[name="q"]').addEventListener("input", function () {
        vista.q = form.querySelector('[name="q"]').value;
        window.clearTimeout(form._t);
        form._t = window.setTimeout(pintar, 180);
      });
      ["modalidade", "situacao", "ordem"].forEach(function (nome) {
        form.querySelector('[name="' + nome + '"]').addEventListener("change", function () {
          lerForm();
          pintar();
        });
      });
      document.getElementById("inverter-ordem").addEventListener("click", function () {
        vista.dir = vista.dir === "asc" ? "desc" : "asc";
        pintar();
      });
      raiz.querySelectorAll("[data-filtro-mod]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.modalidade = botao.getAttribute("data-filtro-mod") || "";
          pintar();
        });
      });
      raiz.querySelectorAll("[data-ordenar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var chave = botao.getAttribute("data-ordenar");
          if (vista.ordem === chave) vista.dir = vista.dir === "asc" ? "desc" : "asc";
          else { vista.ordem = chave; vista.dir = "asc"; }
          pintar();
        });
      });
      raiz.querySelectorAll(".admin-prod-card").forEach(function (card) {
        card.querySelectorAll("[data-acao]").forEach(function (botao) {
          botao.addEventListener("click", function () {
            var id = card.getAttribute("data-id");
            var acao = botao.getAttribute("data-acao");
            if (acao === "excluir") {
              var peca = estado().produtos.find(function (item) { return item.id === id; });
              if (!confirm('Excluir "' + (peca ? peca.titulo : "esta peça") + '"? Essa ação não desfaz.')) return;
              Cria.alterar(function (atual) {
                atual.produtos = atual.produtos.filter(function (item) { return item.id !== id; });
                atual.favoritos = (atual.favoritos || []).filter(function (item) { return item !== id; });
                atual.vistos = (atual.vistos || []).filter(function (item) { return item !== id; });
              });
              pintar();
              return;
            }
            if (acao === "duplicar") {
              Cria.alterar(function (atual) {
                var origem = atual.produtos.find(function (item) { return item.id === id; });
                if (!origem) return;
                var copia = JSON.parse(JSON.stringify(origem));
                copia.id = CriaCarrinho.novoId("peca");
                copia.titulo = origem.titulo + " (cópia)";
                copia.situacao = "pausado";
                copia.vendas = 0;
                copia.criadoEm = dinheiro.hojeISO();
                atual.produtos.unshift(copia);
              });
              pintar();
              return;
            }
            if (acao === "pausar" || acao === "publicar") {
              Cria.alterar(function (atual) {
                var alvo = atual.produtos.find(function (item) { return item.id === id; });
                if (alvo) alvo.situacao = acao === "pausar" ? "pausado" : "publicado";
              });
              pintar();
            }
          });
        });
      });
    }
    pintar();
  }

  function formularioProduto() {
    var id = new URLSearchParams(location.search).get("id");
    var produto = estado().produtos.find(function (item) { return item.id === id; }) || {
      id: "", titulo: "", descricao: "", tipo: "pronto", modalidade: "pronta-entrega", comArquivo: false, categoria: "decoracao", preco: 0, precoComparativo: 0, precoArquivo: 0, estoque: 0, minimo: 1, medidas: "", peso: 0, material: "PLA", prazoDias: 5, situacao: "publicado", fotos: [], cores: [{ nome: "Laranja", hex: "#e8772a" }], tamanhos: ["Único"], variacoes: [{ id: "var-cor", nome: "Cor", tipo: "cor", opcoes: [{ id: "opc-1", nome: "Laranja", hex: "#e8772a" }] }], personalizavel: false, etapas: [], sessao: "", oferta: false, maisVendido: false, aVista: true, vendas: 0, nota: 0, criadoEm: dinheiro.hojeISO(), arquivo: null
    };
    if (Cria.normalizarVariacoesProduto) Cria.normalizarVariacoesProduto(produto);
    var rascunho = JSON.parse(JSON.stringify(produto.variacoes || []));
    document.title = (produto.id ? "Editar" : "Novo") + " produto";
    var categorias = estado().categorias.map(function (categoria) {
      return '<option value="' + ui.esc(categoria.id) + '"' + (produto.categoria === categoria.id ? " selected" : "") + ">" + ui.esc(categoria.nome) + "</option>";
    }).join("");
    var fotos = (produto.fotos || []).map(function (foto, indice) {
      return '<label>Legenda da sessão ' + ui.esc(dinheiro.dataBR(foto.sessao || produto.sessao)) + '<input data-legenda="' + indice + '" value="' + ui.esc(foto.legenda) + '"></label><img src="' + ui.url(foto.arquivo) + '" alt="" style="width:120px;height:120px;object-fit:cover;border-radius:12px">';
    }).join("");
    raiz.innerHTML = '<h1>' + ui.esc(produto.titulo || "Novo produto") + '</h1><form id="produto" class="cartao formulario"><label>Título<input name="titulo" required value="' + ui.esc(produto.titulo) + '"></label><label>Descrição<textarea name="descricao">' + ui.esc(produto.descricao) + '</textarea></label><label>Modalidade<select name="modalidade"><option value="pronta-entrega">Pronta Entrega</option><option value="sob-demanda">Sob Demanda</option><option value="personalizado">Personalizado</option></select></label><label>Tipo legado<select name="tipo"><option value="pronto">pronto</option><option value="personalizado">personalizado</option></select></label><label>Categoria<select name="categoria">' + categorias + '</select></label><div class="grade-3"><label>Preço<input name="preco" type="number" step="0.01" value="' + produto.preco + '"></label><label>Preço comparativo<input name="precoComparativo" type="number" step="0.01" value="' + (produto.precoComparativo || 0) + '"></label><label>Estoque<input name="estoque" type="number" value="' + produto.estoque + '"></label><label>Peso (g)<input name="peso" type="number" step="0.01" value="' + produto.peso + '"></label><label>Prazo (dias)<input name="prazoDias" type="number" value="' + produto.prazoDias + '"></label></div><label>Medidas<input name="medidas" value="' + ui.esc(produto.medidas) + '"></label><label>Material<input name="material" value="' + ui.esc(produto.material) + '"></label><section class="var-admin" id="variacoes-admin"><div class="var-admin-topo"><div><h2 class="m-0">Variações</h2><p class="var-admin-ajuda m-0">Cadastre eixos como Cor, Tamanho ou Acabamento. O cliente escolhe um de cada no detalhe do produto.</p></div><button class="botao-linha" type="button" id="var-add-eixo">+ Variação</button></div><div id="var-lista"></div></section><h2>Fotos</h2>' + fotos + '<div class="linha"><button class="botao" type="submit">Salvar</button><button class="botao-teal" type="button" id="publicar">Publicar</button><button class="botao-linha" type="button" id="pausar">Pausar</button><button class="botao-linha" type="button" id="esgotar">Esgotar</button></div></form>';
    var modAtual = produto.modalidade || (produto.tipo === "personalizado" ? "personalizado" : "pronta-entrega");
    document.querySelector("[name=modalidade]").value = modAtual;
    document.querySelector("[name=tipo]").value = modAtual === "personalizado" ? "personalizado" : "pronto";
    function novoIdLocal(prefixo) {
      return prefixo + "-" + Date.now().toString(36) + Math.random().toString(36).slice(2, 6);
    }
    function coletarCamposVariacao() {
      var lista = document.getElementById("var-lista");
      if (!lista) return;
      lista.querySelectorAll("[data-eixo-i]").forEach(function (card) {
        var i = Number(card.getAttribute("data-eixo-i"));
        if (!rascunho[i]) return;
        var nome = card.querySelector("[data-campo=nome]");
        var tipo = card.querySelector("[data-campo=tipo]");
        if (nome) rascunho[i].nome = String(nome.value || "").trim() || "Variação";
        if (tipo) rascunho[i].tipo = tipo.value === "cor" ? "cor" : "texto";
        card.querySelectorAll("[data-opcao-i]").forEach(function (linha) {
          var j = Number(linha.getAttribute("data-opcao-i"));
          if (!rascunho[i].opcoes[j]) return;
          var opNome = linha.querySelector("[data-campo=op-nome]");
          var opHex = linha.querySelector("[data-campo=op-hex]");
          if (opNome) rascunho[i].opcoes[j].nome = String(opNome.value || "").trim() || ("Opção " + (j + 1));
          if (opHex) rascunho[i].opcoes[j].hex = String(opHex.value || "#cccccc");
        });
      });
    }
    function pintarVariacoes() {
      var lista = document.getElementById("var-lista");
      if (!lista) return;
      if (!rascunho.length) {
        lista.innerHTML = '<p class="var-admin-vazia">Nenhuma variação ainda. Ex.: adicione <strong>Cor</strong> e depois <strong>Tamanho</strong>.</p>';
        return;
      }
      lista.innerHTML = rascunho.map(function (eixo, i) {
        var opcoes = (eixo.opcoes || []).map(function (opcao, j) {
          return '<div class="var-opcao" data-opcao-i="' + j + '">' +
            '<input data-campo="op-nome" type="text" value="' + ui.esc(opcao.nome) + '" placeholder="Nome da opção" aria-label="Nome da opção">' +
            (eixo.tipo === "cor" ? '<input data-campo="op-hex" type="color" value="' + ui.esc(opcao.hex || "#cccccc") + '" aria-label="Cor">' : "") +
            '<button class="botao-linha" type="button" data-rm-opcao="' + i + "-" + j + '"' + ((eixo.opcoes || []).length <= 1 ? " disabled" : "") + ">Remover</button>" +
            "</div>";
        }).join("");
        return '<article class="var-eixo" data-eixo-i="' + i + '">' +
          '<div class="var-eixo-cab">' +
          '<label class="fin-campo"><span>Nome da variação</span><input data-campo="nome" type="text" value="' + ui.esc(eixo.nome) + '" placeholder="Ex.: Cor, Tamanho"></label>' +
          '<label class="fin-campo"><span>Como mostrar</span><select data-campo="tipo"><option value="cor"' + (eixo.tipo === "cor" ? " selected" : "") + '>Amostra de cor</option><option value="texto"' + (eixo.tipo !== "cor" ? " selected" : "") + ">Botões de texto</option></select></label>" +
          '<button class="botao-linha" type="button" data-rm-eixo="' + i + '">Remover variação</button>' +
          "</div>" +
          '<div class="var-opcoes">' + opcoes + "</div>" +
          '<button class="botao-linha" type="button" data-add-opcao="' + i + '">+ Opção</button>' +
          "</article>";
      }).join("");
      lista.querySelectorAll("[data-campo=nome], [data-campo=tipo], [data-campo=op-nome], [data-campo=op-hex]").forEach(function (campo) {
        campo.addEventListener("change", function () {
          coletarCamposVariacao();
          if (campo.getAttribute("data-campo") === "tipo") pintarVariacoes();
        });
      });
      lista.querySelectorAll("[data-rm-eixo]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          coletarCamposVariacao();
          rascunho.splice(Number(botao.getAttribute("data-rm-eixo")), 1);
          pintarVariacoes();
        });
      });
      lista.querySelectorAll("[data-add-opcao]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          coletarCamposVariacao();
          var i = Number(botao.getAttribute("data-add-opcao"));
          if (!rascunho[i]) return;
          rascunho[i].opcoes.push({
            id: novoIdLocal("opc"),
            nome: rascunho[i].tipo === "cor" ? "Nova cor" : "Nova opção",
            hex: "#e8772a"
          });
          pintarVariacoes();
        });
      });
      lista.querySelectorAll("[data-rm-opcao]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          coletarCamposVariacao();
          var partes = String(botao.getAttribute("data-rm-opcao") || "").split("-");
          var i = Number(partes[0]);
          var j = Number(partes[1]);
          if (!rascunho[i] || !rascunho[i].opcoes) return;
          if (rascunho[i].opcoes.length <= 1) return;
          rascunho[i].opcoes.splice(j, 1);
          pintarVariacoes();
        });
      });
    }
    pintarVariacoes();
    document.getElementById("var-add-eixo").addEventListener("click", function () {
      coletarCamposVariacao();
      rascunho.push({
        id: novoIdLocal("var"),
        nome: rascunho.length ? "Tamanho" : "Cor",
        tipo: rascunho.length ? "texto" : "cor",
        opcoes: [{ id: novoIdLocal("opc"), nome: rascunho.length ? "M" : "Laranja", hex: "#e8772a" }]
      });
      pintarVariacoes();
    });
    document.getElementById("produto").addEventListener("submit", function (evento) {
      evento.preventDefault();
      salvar("manter");
    });
    document.getElementById("publicar").addEventListener("click", function () { salvar("publicado"); });
    document.getElementById("pausar").addEventListener("click", function () { salvar("pausado"); });
    document.getElementById("esgotar").addEventListener("click", function () { salvar("esgotado"); });
    function salvar(situacao) {
      coletarCamposVariacao();
      var dados = new FormData(document.getElementById("produto"));
      var variacoesSalvas = rascunho.map(function (eixo, indiceEixo) {
        return {
          id: eixo.id || novoIdLocal("var"),
          nome: String(eixo.nome || "Variação").trim() || "Variação",
          tipo: eixo.tipo === "cor" ? "cor" : "texto",
          opcoes: (eixo.opcoes || []).map(function (opcao, indiceOpcao) {
            return {
              id: opcao.id || novoIdLocal("opc"),
              nome: String(opcao.nome || "").trim() || ("Opção " + (indiceOpcao + 1)),
              hex: opcao.hex || "#cccccc"
            };
          }).filter(function (opcao) { return opcao.nome; })
        };
      }).filter(function (eixo) { return eixo.opcoes.length; });
      Cria.alterar(function (atual) {
        var alvo = atual.produtos.find(function (item) { return item.id === produto.id; });
        if (!alvo) {
          alvo = JSON.parse(JSON.stringify(produto));
          alvo.id = CriaCarrinho.novoId("peca");
          alvo.fotos = [];
          atual.produtos.push(alvo);
        }
        alvo.titulo = String(dados.get("titulo"));
        alvo.descricao = String(dados.get("descricao"));
        alvo.modalidade = String(dados.get("modalidade") || "pronta-entrega");
        alvo.tipo = alvo.modalidade === "personalizado" ? "personalizado" : "pronto";
        alvo.personalizavel = alvo.modalidade === "personalizado";
        alvo.comArquivo = false;
        alvo.precoArquivo = 0;
        alvo.arquivo = null;
        alvo.categoria = String(dados.get("categoria"));
        alvo.preco = Number(dados.get("preco")) || 0;
        alvo.precoComparativo = Number(dados.get("precoComparativo")) || 0;
        alvo.estoque = Number(dados.get("estoque")) || 0;
        alvo.peso = Number(dados.get("peso")) || 0;
        alvo.prazoDias = Number(dados.get("prazoDias")) || 0;
        alvo.medidas = String(dados.get("medidas"));
        alvo.material = String(dados.get("material"));
        alvo.variacoes = variacoesSalvas;
        if (Cria.sincronizarLegadoVariacoes) Cria.sincronizarLegadoVariacoes(alvo);
        document.querySelectorAll("[data-legenda]").forEach(function (campo) {
          var foto = alvo.fotos[Number(campo.getAttribute("data-legenda"))];
          if (foto) foto.legenda = campo.value;
        });
        if (situacao !== "manter") alvo.situacao = situacao;
        if (situacao === "esgotado") alvo.estoque = 0;
      });
      location.href = ui.url("paginas/admin/produtos.html");
    }
  }

  function configuracao() {
    document.title = "Sistema · Administração";
    var params = new URLSearchParams(location.search);
    var abasOk = { loja: 1, vendas: 1, frete: 1, site: 1, avancado: 1 };
    var vista = {
      aba: abasOk[params.get("aba")] ? params.get("aba") : "loja",
      cep: params.get("cep") || "",
      ok: ""
    };
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function pctDe(fracao) {
      return Math.round((Number(fracao) || 0) * 1000) / 10;
    }
    function fracaoDe(pct) {
      var n = Number(pct) || 0;
      if (n < 0) n = 0;
      if (n > 90) n = 90;
      return Math.round(n * 10) / 1000;
    }
    function sincronizarUrl() {
      var url = new URL(location.href);
      url.searchParams.set("aba", vista.aba);
      if (vista.cep) url.searchParams.set("cep", vista.cep);
      else url.searchParams.delete("cep");
      history.replaceState(null, "", url.pathname + url.search);
    }
    function flash(msg) {
      vista.ok = msg;
      pintar();
      window.setTimeout(function () {
        if (vista.ok === msg) { vista.ok = ""; pintar(); }
      }, 2400);
    }
    function pintar() {
      sincronizarUrl();
      var conf = estado().configuracao;
      var id = estado().identidade || {};
      var cupom = (conf.cupons || [])[0] || { codigo: "", percentual: 0, ativo: true };
      var depoimentosLista = estado().depoimentos || [];
      var parceirosLista = estado().parceiros || [];
      var faixas = conf.faixasCep || [];
      var tipos = conf.tiposInsumo || [];
      var avisos = [];
      if (!(id.endereco || "").trim()) avisos.push("Endereço da loja vazio (some do rodapé).");
      if (!(id.horario || "").trim()) avisos.push("Horário vazio (some do rodapé).");
      if (!cupom.ativo) avisos.push("Cupom pausado no Marketing.");

      var kpis = '<div class="sys-kpis">' +
        '<button class="sys-kpi' + (vista.aba === "loja" ? " ativo" : "") + '" type="button" data-aba="loja"><strong>' + ui.esc((id.nome || "Loja").split(" ")[0]) + '</strong><span>Identidade</span></button>' +
        '<button class="sys-kpi' + (vista.aba === "vendas" ? " ativo" : "") + '" type="button" data-aba="vendas"><strong>' + dinheiro.real(conf.freteGratisAPartir) + '</strong><span>Frete grátis</span></button>' +
        '<button class="sys-kpi' + (vista.aba === "frete" ? " ativo" : "") + '" type="button" data-aba="frete"><strong>' + faixas.length + '</strong><span>Faixas de CEP</span></button>' +
        '<button class="sys-kpi' + (vista.aba === "site" ? " ativo" : "") + '" type="button" data-aba="site"><strong>' + depoimentosLista.filter(function (d) { return d.podeAparecer; }).length + "/" + parceirosLista.filter(function (p) { return p.podeAparecer; }).length + '</strong><span>Depoim. / Parceiros</span></button>' +
        '<button class="sys-kpi' + (vista.aba === "avancado" ? " ativo" : "") + '" type="button" data-aba="avancado"><strong>' + tipos.length + '</strong><span>Tipos de insumo</span></button>' +
        "</div>";

      var abas = '<div class="fin-abas sys-abas">' +
        [["loja", "Loja"], ["vendas", "Vendas"], ["frete", "Frete"], ["site", "Publicação"], ["avancado", "Avançado"]].map(function (par) {
          return '<button class="fin-aba' + (vista.aba === par[0] ? " ativo" : "") + '" type="button" data-aba="' + par[0] + '">' + par[1] + "</button>";
        }).join("") +
        "</div>";

      var alerta = avisos.length
        ? '<div class="sys-alerta">' + avisos.map(function (txt) { return "<span>" + ui.esc(txt) + "</span>"; }).join("") + "</div>"
        : "";
      var okHtml = vista.ok ? '<p class="sys-ok">' + ui.esc(vista.ok) + "</p>" : "";

      var corpo = "";
      if (vista.aba === "loja") {
        corpo = '<form id="sys-loja" class="sys-card">' +
          '<div class="sys-card-topo"><h2 class="sys-titulo">Identidade da loja</h2><span class="sys-selo">Aparece no recibo e no rodapé</span></div>' +
          '<div class="sys-grade">' +
          '<label class="fin-campo"><span>Nome</span><input name="nome" value="' + ui.esc(id.nome || "") + '" required></label>' +
          '<label class="fin-campo"><span>CNPJ</span><input name="cnpj" value="' + ui.esc(id.cnpj || "") + '"></label>' +
          '<label class="fin-campo"><span>E-mail</span><input name="email" type="email" value="' + ui.esc(id.email || "") + '"></label>' +
          '<label class="fin-campo"><span>Telefone</span><input name="telefone" value="' + ui.esc(id.telefone || "") + '"></label>' +
          '<label class="fin-campo"><span>Site</span><input name="site" value="' + ui.esc(id.site || "") + '"></label>' +
          '<label class="fin-campo sys-span2"><span>Endereço</span><input name="endereco" value="' + ui.esc(id.endereco || "") + '" placeholder="Rua, número, cidade/UF"></label>' +
          '<label class="fin-campo sys-span2"><span>Horário</span><input name="horario" value="' + ui.esc(id.horario || "") + '" placeholder="Seg a sex, 9h–18h"></label>' +
          "</div>" +
          '<h3 class="sys-subtitulo">Redes e canais</h3>' +
          '<div class="sys-grade">' +
          '<label class="fin-campo"><span>WhatsApp</span><input name="whatsapp" value="' + ui.esc(id.whatsapp || "") + '"></label>' +
          '<label class="fin-campo"><span>Instagram</span><input name="instagram" value="' + ui.esc(id.instagram || "") + '"></label>' +
          '<label class="fin-campo"><span>TikTok</span><input name="tiktok" value="' + ui.esc(id.tiktok || "") + '"></label>' +
          '<label class="fin-campo"><span>YouTube</span><input name="youtube" value="' + ui.esc(id.youtube || "") + '"></label>' +
          '<label class="fin-campo"><span>Shopee</span><input name="shopee" value="' + ui.esc(id.shopee || "") + '"></label>' +
          '<label class="fin-campo"><span>Mercado Livre</span><input name="mercadoLivre" value="' + ui.esc(id.mercadoLivre || "") + '"></label>' +
          "</div>" +
          '<div class="sys-acoes"><button class="botao" type="submit">' + ico("documento") + "<span>Salvar loja</span></button></div>" +
          "</form>";
      } else if (vista.aba === "vendas") {
        corpo = '<form id="sys-vendas" class="sys-card">' +
          '<div class="sys-card-topo"><h2 class="sys-titulo">Regras de venda</h2><span class="sys-selo">Checkout e orçamento</span></div>' +
          '<div class="sys-grade">' +
          '<label class="fin-campo"><span>Frete grátis a partir de (R$)</span><input name="freteGratisAPartir" type="number" step="0.01" min="0" value="' + conf.freteGratisAPartir + '"></label>' +
          '<label class="fin-campo"><span>Desconto à vista (%)</span><input name="descontoAVista" type="number" step="0.5" min="0" max="90" value="' + pctDe(conf.descontoAVista) + '"><small>Ex.: 10 = 10% no PIX/boleto</small></label>' +
          '<label class="fin-campo"><span>Parcelas no cartão</span><input name="parcelas" type="number" min="1" max="12" value="' + conf.parcelas + '"></label>' +
          '<label class="fin-campo"><span>Taxa de falha (%)</span><input name="taxaFalha" type="number" step="0.1" min="0" max="50" value="' + pctDe(conf.taxaFalha) + '"><small>Usada no orçamento de impressão</small></label>' +
          '<label class="fin-campo"><span>Frete de canal (R$)</span><input name="freteCanal" type="number" step="0.01" min="0" value="' + (conf.freteCanal || 0) + '"><small>Custo extra no orçamento de marketplace</small></label>' +
          '<label class="sys-check"><input type="checkbox" name="repassarFrete"' + (conf.repassarFrete ? " checked" : "") + "><span>Repassar o frete no líquido da venda</span></label>" +
          "</div>" +
          '<div class="sys-cupom-box">' +
          '<div><strong>Cupom da loja</strong><p class="sys-meta m-0">' + ui.esc(cupom.codigo || "—") + " · " + pctDe(cupom.percentual) + "% · " + (cupom.ativo !== false ? "ativo" : "pausado") + "</p></div>" +
          '<a class="botao-linha" href="' + ui.url("paginas/admin/marketing.html") + '">' + ico("tiktok") + "<span>Editar no Marketing</span></a>" +
          "</div>" +
          '<div class="sys-acoes"><button class="botao" type="submit">' + ico("documento") + "<span>Salvar vendas</span></button></div>" +
          '<p class="sys-meta">Pedidos já feitos continuam com o frete e o desconto da hora da compra.</p>' +
          "</form>";
      } else if (vista.aba === "frete") {
        var filtroCep = String(vista.cep || "").replace(/\D/g, "");
        var faixasVis = faixas.map(function (faixa, indice) { return { faixa: faixa, i: indice }; }).filter(function (item) {
          if (!filtroCep) return true;
          var n = Number(filtroCep);
          if (!n) return true;
          return n >= item.faixa.de && n <= item.faixa.ate;
        });
        corpo = '<div class="sys-card">' +
          '<div class="sys-card-topo"><h2 class="sys-titulo">Faixas de CEP</h2><button class="botao-linha" type="button" id="sys-add-faixa">' + ico("caixa") + "<span>Nova faixa</span></button></div>" +
          '<div class="sys-frete-topo"><label class="fin-campo fis-busca"><span>Testar CEP</span><input id="sys-cep" type="search" inputmode="numeric" value="' + ui.esc(vista.cep) + '" placeholder="Ex.: 29010-000"></label>' +
          (filtroCep ? '<p class="sys-meta">' + (faixasVis.length ? "Faixa encontrada para o CEP informado." : "Nenhuma faixa cobre este CEP.") + "</p>" : '<p class="sys-meta">PAC e SEDEX por intervalo de CEP. Pedidos novos usam a tabela salva.</p>') +
          "</div>" +
          '<form id="sys-frete" class="sys-faixas">' +
          (faixasVis.length ? faixasVis.map(function (item) {
            var f = item.faixa;
            var i = item.i;
            return '<article class="sys-faixa' + (filtroCep && Number(filtroCep) >= f.de && Number(filtroCep) <= f.ate ? " destaque" : "") + '">' +
              '<div class="sys-faixa-cab"><strong>Faixa ' + (i + 1) + "</strong>" +
              '<button class="botao-linha" type="button" data-remover-faixa="' + i + '"' + (faixas.length <= 1 ? " disabled" : "") + ">Remover</button></div>" +
              '<div class="sys-faixa-grade">' +
              '<label class="fin-campo"><span>CEP de</span><input name="de-' + i + '" type="number" value="' + f.de + '"></label>' +
              '<label class="fin-campo"><span>CEP até</span><input name="ate-' + i + '" type="number" value="' + f.ate + '"></label>' +
              '<label class="fin-campo"><span>PAC (R$)</span><input name="pac-' + i + '" type="number" step="0.01" value="' + f.pac + '"></label>' +
              '<label class="fin-campo"><span>Dias PAC</span><input name="pacDias-' + i + '" type="number" value="' + f.pacDias + '"></label>' +
              '<label class="fin-campo"><span>SEDEX (R$)</span><input name="sedex-' + i + '" type="number" step="0.01" value="' + f.sedex + '"></label>' +
              '<label class="fin-campo"><span>Dias SEDEX</span><input name="sedexDias-' + i + '" type="number" value="' + f.sedexDias + '"></label>' +
              "</div></article>";
          }).join("") : '<p class="sys-meta">Nenhuma faixa para mostrar com esse CEP.</p>') +
          '<div class="sys-acoes"><button class="botao" type="submit">' + ico("documento") + "<span>Salvar faixas</span></button></div>" +
          "</form></div>";
      } else if (vista.aba === "site") {
        corpo = '<div class="sys-pub-grade">' +
          '<section class="sys-card"><div class="sys-card-topo"><h2 class="sys-titulo">Depoimentos</h2><span class="sys-selo">' + depoimentosLista.filter(function (d) { return d.podeAparecer; }).length + " no site</span></div>" +
          '<div class="sys-pub-lista">' + depoimentosLista.map(function (item) {
            return '<label class="sys-pub-item"><input type="checkbox" data-pub="depoimentos" data-id="' + ui.esc(item.id) + '"' + (item.podeAparecer ? " checked" : "") + ">" +
              '<span><strong>' + ui.esc(item.autor) + "</strong><small>" + ui.esc(item.texto) + "</small></span></label>";
          }).join("") + "</div></section>" +
          '<section class="sys-card"><div class="sys-card-topo"><h2 class="sys-titulo">Parceiros</h2><span class="sys-selo">' + parceirosLista.filter(function (p) { return p.podeAparecer; }).length + " no site</span></div>" +
          '<div class="sys-pub-lista">' + parceirosLista.map(function (item) {
            return '<label class="sys-pub-item"><input type="checkbox" data-pub="parceiros" data-id="' + ui.esc(item.id) + '"' + (item.podeAparecer ? " checked" : "") + ">" +
              (item.logo ? '<img class="sys-pub-logo" src="' + ui.url(item.logo) + '" alt="">' : "") +
              '<span><strong>' + ui.esc(item.nome) + "</strong><small>Pode aparecer no rodapé e em Sobre</small></span></label>";
          }).join("") + "</div></section></div>" +
          '<p class="sys-meta">Desligado some na hora da home, do Sobre e do rodapé. Não apaga o cadastro.</p>';
      } else {
        corpo = '<div class="sys-card">' +
          '<div class="sys-card-topo"><h2 class="sys-titulo">Tipos de insumo</h2><span class="sys-selo">Financeiro e estoque</span></div>' +
          '<div class="sys-chips" id="sys-tipos">' + tipos.map(function (tipo) {
            return '<span class="sys-chip">' + ui.esc(tipo) + '<button type="button" data-tipo-rm="' + ui.esc(tipo) + '" aria-label="Remover">×</button></span>';
          }).join("") + "</div>" +
          '<form id="sys-tipo" class="sys-meta-form"><label class="fin-campo"><span>Novo tipo</span><input name="tipo" required placeholder="Ex.: Verniz fosco"></label><button class="botao" type="submit">' + ico("caixa") + "<span>Adicionar</span></button></form>" +
          "</div>" +
          '<div class="sys-card sys-perigoso">' +
          '<div class="sys-card-topo"><h2 class="sys-titulo">Restaurar carga inicial</h2><span class="sys-selo alerta">Irreversível neste navegador</span></div>' +
          '<p class="sys-meta">Apaga alterações locais (produtos, pedidos, financeiro editado, etc.) e volta aos dados de demonstração.</p>' +
          '<button class="botao-linha" type="button" id="sys-restaurar">' + ico("sair") + "<span>Restaurar agora</span></button>" +
          "</div>";
      }

      raiz.innerHTML =
        '<div class="admin-prod-cab"><div><h1 class="m-0">Sistema</h1><p class="admin-prod-intro m-0 mt-1">Configure a loja, as regras de venda, o frete por CEP e o que aparece no site — sem misturar tudo num formulário só.</p></div>' +
        '<a class="botao-linha" href="' + ui.url("paginas/admin/marketing.html") + '">' + ico("tiktok") + "<span>Marketing</span></a></div>" +
        kpis + abas + alerta + okHtml + corpo;

      raiz.querySelectorAll("[data-aba]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-aba") || "loja";
          vista.ok = "";
          pintar();
        });
      });

      var formLoja = document.getElementById("sys-loja");
      if (formLoja) formLoja.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        Cria.alterar(function (atual) {
          var destino = atual.identidade;
          ["nome", "cnpj", "email", "telefone", "site", "endereco", "horario", "whatsapp", "instagram", "tiktok", "youtube", "shopee", "mercadoLivre"].forEach(function (campo) {
            destino[campo] = String(dados.get(campo) || "").trim();
          });
        });
        flash("Dados da loja salvos.");
      });

      var formVendas = document.getElementById("sys-vendas");
      if (formVendas) formVendas.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        Cria.alterar(function (atual) {
          atual.configuracao.freteGratisAPartir = Number(dados.get("freteGratisAPartir")) || 0;
          atual.configuracao.descontoAVista = fracaoDe(dados.get("descontoAVista"));
          atual.configuracao.parcelas = Math.max(1, Number(dados.get("parcelas")) || 1);
          atual.configuracao.taxaFalha = fracaoDe(dados.get("taxaFalha"));
          atual.configuracao.freteCanal = Number(dados.get("freteCanal")) || 0;
          atual.configuracao.repassarFrete = Boolean(dados.get("repassarFrete"));
        });
        flash("Regras de venda salvas. Pedidos antigos não mudam.");
      });

      var cep = document.getElementById("sys-cep");
      if (cep) cep.addEventListener("input", function (evento) {
        vista.cep = evento.target.value;
        window.clearTimeout(raiz._cepT);
        raiz._cepT = window.setTimeout(pintar, 180);
      });

      var formFrete = document.getElementById("sys-frete");
      if (formFrete) {
        formFrete.addEventListener("submit", function (evento) {
          evento.preventDefault();
          var dados = new FormData(evento.target);
          Cria.alterar(function (atual) {
            (atual.configuracao.faixasCep || []).forEach(function (faixa, i) {
              ["de", "ate", "pac", "pacDias", "sedex", "sedexDias"].forEach(function (campo) {
                var chave = campo + "-" + i;
                if (dados.has(chave)) faixa[campo] = Number(dados.get(chave));
              });
            });
            atual.configuracao.faixasCep.sort(function (a, b) { return a.de - b.de; });
          });
          flash("Faixas de CEP salvas.");
        });
        raiz.querySelectorAll("[data-remover-faixa]").forEach(function (botao) {
          botao.addEventListener("click", function () {
            var i = Number(botao.getAttribute("data-remover-faixa"));
            Cria.alterar(function (atual) {
              if ((atual.configuracao.faixasCep || []).length <= 1) return;
              atual.configuracao.faixasCep.splice(i, 1);
            });
            flash("Faixa removida.");
          });
        });
      }
      var addFaixa = document.getElementById("sys-add-faixa");
      if (addFaixa) addFaixa.addEventListener("click", function () {
        Cria.alterar(function (atual) {
          var lista = atual.configuracao.faixasCep || [];
          var ultimo = lista[lista.length - 1];
          var de = ultimo ? ultimo.ate + 1 : 1000;
          lista.push({ de: de, ate: de + 999, pac: 29.9, pacDias: 8, sedex: 49.9, sedexDias: 3 });
        });
        flash("Nova faixa adicionada. Ajuste e salve.");
      });

      raiz.querySelectorAll("[data-pub]").forEach(function (campo) {
        campo.addEventListener("change", function () {
          var chave = campo.getAttribute("data-pub");
          var idItem = campo.getAttribute("data-id");
          Cria.alterar(function (atual) {
            var item = (atual[chave] || []).find(function (cadastro) { return cadastro.id === idItem; });
            if (item) item.podeAparecer = campo.checked;
          });
          pintar();
        });
      });

      var formTipo = document.getElementById("sys-tipo");
      if (formTipo) formTipo.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var tipo = String(new FormData(evento.target).get("tipo") || "").trim();
        if (!tipo) return;
        Cria.alterar(function (atual) {
          atual.configuracao.tiposInsumo = atual.configuracao.tiposInsumo || [];
          if (atual.configuracao.tiposInsumo.indexOf(tipo) < 0) atual.configuracao.tiposInsumo.push(tipo);
        });
        flash("Tipo de insumo adicionado.");
      });
      raiz.querySelectorAll("[data-tipo-rm]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var tipo = botao.getAttribute("data-tipo-rm");
          Cria.alterar(function (atual) {
            atual.configuracao.tiposInsumo = (atual.configuracao.tiposInsumo || []).filter(function (item) { return item !== tipo; });
          });
          flash("Tipo removido da lista.");
        });
      });
      var restaurar = document.getElementById("sys-restaurar");
      if (restaurar) restaurar.addEventListener("click", function () {
        if (!confirm("Isso apaga as alterações deste navegador e volta à carga inicial.")) return;
        Cria.restaurar();
        location.reload();
      });

      ui.revelar(raiz);
    }
    pintar();
  }

  function perguntas() {
    location.replace(ui.url("paginas/admin/contato.html?aba=perguntas"));
  }

  function contatoAdmin() {
    document.title = "Atendimento · Administração";
    var params = new URLSearchParams(location.search);
    var vista = {
      aba: params.get("aba") === "perguntas" ? "perguntas" : "mensagens",
      status: params.get("status") || "",
      q: params.get("q") || ""
    };
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function seloStatus(status) {
      var mapa = {
        aberto: "at-selo at-aberto",
        "em atendimento": "at-selo at-andamento",
        resolvido: "at-selo at-resolvido",
        pendente: "at-selo at-aberto",
        respondida: "at-selo at-resolvido"
      };
      return '<span class="' + (mapa[status] || "at-selo") + '">' + ui.esc(status) + "</span>";
    }
    function seloTipo(tipo) {
      return '<span class="at-tipo">' + ui.esc(tipo || "contato") + "</span>";
    }
    function iniciais(nome) {
      var partes = String(nome || "?").trim().split(/\s+/);
      return ((partes[0] || "?").charAt(0) + (partes[1] ? partes[1].charAt(0) : "")).toUpperCase();
    }
    function cardMensagem(item) {
      return '<article class="at-card" data-id="' + ui.esc(item.id) + '">' +
        '<div class="at-avatar" aria-hidden="true">' + ui.esc(iniciais(item.nome)) + "</div>" +
        '<div class="at-corpo">' +
          '<div class="at-topo">' + seloTipo(item.tipo) + seloStatus(item.status) +
            '<span class="at-data">' + ui.esc(dinheiro.dataBR((item.criadoEm || "").slice(0, 10))) + "</span></div>" +
          '<h3 class="at-titulo">' + ui.esc(item.nome) + "</h3>" +
          '<p class="at-meta"><a href="mailto:' + ui.esc(item.email) + '">' + ui.esc(item.email) + "</a></p>" +
          '<p class="at-texto">' + ui.esc(item.texto) + "</p>" +
          '<div class="at-acoes">' +
            (item.status !== "em atendimento" ? '<button class="botao-linha at-btn" type="button" data-status="em atendimento">' + ico("contato") + "<span>Em atendimento</span></button>" : "") +
            (item.status !== "resolvido" ? '<button class="botao at-btn" type="button" data-status="resolvido">' + ico("sobre") + "<span>Resolvido</span></button>" : "") +
            (item.status === "resolvido" ? '<button class="botao-linha at-btn" type="button" data-status="aberto">' + ico("sino") + "<span>Reabrir</span></button>" : "") +
          "</div></div></article>";
    }
    function cardPergunta(item) {
      var produto = estado().produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
      var respondida = Boolean(String(item.resposta || "").trim());
      return '<article class="at-card at-pergunta">' +
        '<div class="at-avatar at-avatar-perg" aria-hidden="true">' + ico("documento") + "</div>" +
        '<div class="at-corpo">' +
          '<div class="at-topo">' + seloTipo("pergunta") + seloStatus(respondida ? "respondida" : "pendente") +
            '<span class="at-data">' + ui.esc(dinheiro.dataBR((item.criadoEm || "").slice(0, 10))) + "</span></div>" +
          '<h3 class="at-titulo">' + ui.esc(produto ? produto.titulo : "Peça") + "</h3>" +
          '<p class="at-meta">' + ui.esc(item.autor || "Cliente") + "</p>" +
          '<p class="at-texto">' + ui.esc(item.texto) + "</p>" +
          '<form class="at-resp" data-id="' + ui.esc(item.id) + '">' +
            '<label class="at-campo"><span>Resposta pública</span><textarea name="resposta" rows="3" placeholder="Escreva a resposta que aparece na ficha">' + ui.esc(item.resposta || "") + "</textarea></label>" +
            '<button class="botao at-btn" type="submit">' + ico("documento") + "<span>" + (respondida ? "Atualizar resposta" : "Publicar resposta") + "</span></button>" +
          "</form></div></article>";
    }
    function pintar() {
      var mensagens = estado().mensagens || [];
      var perguntasLista = estado().perguntas || [];
      var abertos = mensagens.filter(function (item) { return item.status === "aberto"; }).length;
      var andamento = mensagens.filter(function (item) { return item.status === "em atendimento"; }).length;
      var pendentes = perguntasLista.filter(function (item) { return !String(item.resposta || "").trim(); }).length;
      var texto = vista.q.trim().toLowerCase();
      var listaMsg = mensagens.filter(function (item) {
        if (vista.status && item.status !== vista.status) return false;
        if (texto && (item.nome + " " + item.email + " " + item.texto + " " + item.tipo).toLowerCase().indexOf(texto) < 0) return false;
        return true;
      }).slice().sort(function (a, b) {
        return String(b.criadoEm || "").localeCompare(String(a.criadoEm || ""));
      });
      var listaPerg = perguntasLista.filter(function (item) {
        var respondida = Boolean(String(item.resposta || "").trim());
        if (vista.status === "pendente" && respondida) return false;
        if (vista.status === "respondida" && !respondida) return false;
        var produto = estado().produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
        if (texto && ((produto ? produto.titulo : "") + " " + (item.autor || "") + " " + item.texto + " " + (item.resposta || "")).toLowerCase().indexOf(texto) < 0) return false;
        return true;
      }).slice().sort(function (a, b) {
        var pa = String(a.resposta || "").trim() ? 1 : 0;
        var pb = String(b.resposta || "").trim() ? 1 : 0;
        return pa - pb || String(b.criadoEm || "").localeCompare(String(a.criadoEm || ""));
      });
      var kpis = '<div class="admin-prod-kpis at-kpis">' +
        '<button class="admin-prod-kpi' + (vista.aba === "mensagens" && !vista.status ? " ativo" : "") + '" type="button" data-aba="mensagens" data-status=""><strong>' + mensagens.length + "</strong><span>Mensagens</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "mensagens" && vista.status === "aberto" ? " ativo" : "") + '" type="button" data-aba="mensagens" data-status="aberto"><strong>' + abertos + "</strong><span>Abertas</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "mensagens" && vista.status === "em atendimento" ? " ativo" : "") + '" type="button" data-aba="mensagens" data-status="em atendimento"><strong>' + andamento + "</strong><span>Em atendimento</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "perguntas" ? " ativo" : "") + '" type="button" data-aba="perguntas" data-status="pendente"><strong>' + pendentes + "</strong><span>Perguntas pendentes</span></button>" +
        "</div>";
      var abas = '<div class="at-abas">' +
        '<button class="at-aba' + (vista.aba === "mensagens" ? " ativo" : "") + '" type="button" data-aba="mensagens">Mensagens do contato</button>' +
        '<button class="at-aba' + (vista.aba === "perguntas" ? " ativo" : "") + '" type="button" data-aba="perguntas">Perguntas da vitrine</button>' +
        "</div>";
      var busca = '<form class="at-busca" id="at-busca"><label class="campo-busca"><span class="visually-hidden">Buscar</span>' + ico("busca") + '<input name="q" type="search" placeholder="' + (vista.aba === "perguntas" ? "Buscar pergunta, peça ou autor" : "Buscar mensagem, nome ou e-mail") + '" value="' + ui.esc(vista.q) + '"></label></form>';
      var corpo;
      if (vista.aba === "perguntas") {
        corpo = listaPerg.length
          ? '<div class="at-lista">' + listaPerg.map(cardPergunta).join("") + "</div>"
          : ui.vazio("Nenhuma pergunta com esses filtros.");
      } else {
        corpo = listaMsg.length
          ? '<div class="at-lista">' + listaMsg.map(cardMensagem).join("") + "</div>"
          : ui.vazio("Nenhuma mensagem com esses filtros.");
      }
      var resumo = vista.aba === "perguntas"
        ? "<strong>" + listaPerg.length + "</strong> pergunta" + (listaPerg.length === 1 ? "" : "s") + " · " + pendentes + " pendente" + (pendentes === 1 ? "" : "s")
        : "<strong>" + listaMsg.length + "</strong> mensagem" + (listaMsg.length === 1 ? "" : "ns") + " · " + abertos + " aberta" + (abertos === 1 ? "" : "s");
      raiz.innerHTML = '<div class="admin-prod-cab"><div><h1 class="m-0">Atendimento</h1><p class="admin-prod-intro m-0 mt-1">Mensagens do formulário de contato e perguntas das fichas de produto, no mesmo lugar.</p></div></div>' +
        kpis + abas + busca +
        '<div class="admin-prod-painel cartao at-painel"><div class="admin-prod-resumo">' + resumo + "</div>" + corpo + "</div>";
      ligar();
      ui.revelar(raiz);
    }
    function ligar() {
      raiz.querySelectorAll("[data-aba]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-aba") || "mensagens";
          vista.status = botao.hasAttribute("data-status") ? (botao.getAttribute("data-status") || "") : "";
          pintar();
        });
      });
      var form = document.getElementById("at-busca");
      form.addEventListener("submit", function (evento) { evento.preventDefault(); });
      form.querySelector('[name="q"]').addEventListener("input", function (evento) {
        vista.q = evento.target.value;
        window.clearTimeout(form._t);
        form._t = window.setTimeout(pintar, 180);
      });
      raiz.querySelectorAll(".at-card[data-id] .at-btn[data-status]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var card = botao.closest("[data-id]");
          var id = card.getAttribute("data-id");
          var status = botao.getAttribute("data-status");
          Cria.alterar(function (atual) {
            var mensagem = atual.mensagens.find(function (item) { return item.id === id; });
            if (mensagem) mensagem.status = status;
          });
          pintar();
        });
      });
      raiz.querySelectorAll("form.at-resp").forEach(function (formResp) {
        formResp.addEventListener("submit", function (evento) {
          evento.preventDefault();
          var id = formResp.getAttribute("data-id");
          var resposta = new FormData(formResp).get("resposta");
          Cria.alterar(function (atual) {
            var pergunta = atual.perguntas.find(function (item) { return item.id === id; });
            if (pergunta) pergunta.resposta = String(resposta || "");
          });
          pintar();
        });
      });
    }
    pintar();
  }

  function depoimentos() {
    location.replace(ui.url("paginas/admin/configuracao.html?aba=site"));
  }

  function personalizadosAdmin() {
    location.replace(ui.url("paginas/admin/produtos.html?modalidade=personalizado"));
  }

  function marketing() {
    document.title = "Marketing · Administração";
    var cupom = (estado().configuracao.cupons || [])[0] || { codigo: "", percentual: 0, ativo: true };
    var pct = Math.round((Number(cupom.percentual) || 0) * 1000) / 10;
    var ativo = cupom.ativo !== false;
    var codigo = cupom.codigo || "OFICINA10";
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function preview(cod, porcento, ligado) {
      var selo = porcento > 0 ? "-" + porcento + "%" : "0%";
      return '<div class="mkt-preview' + (ligado ? "" : " off") + '">' +
        '<div class="mkt-preview-selo">' + ui.esc(selo) + "</div>" +
        '<div class="mkt-preview-corpo">' +
          '<p class="mkt-preview-rotulo">Prévia no checkout</p>' +
          '<p class="mkt-preview-codigo">' + ui.esc(cod || "CUPOM") + "</p>" +
          '<p class="mkt-preview-txt">' + (ligado ? "Cupom ativo · desconto no fechamento do pedido" : "Cupom pausado · não vale no checkout") + "</p>" +
        "</div></div>";
    }
    raiz.innerHTML = [
      '<div class="admin-prod-cab"><div><h1 class="m-0">Marketing</h1><p class="admin-prod-intro m-0 mt-1">Cupom da própria loja. O cliente digita o código no fechamento e o desconto entra no total.</p></div></div>',
      '<div class="mkt-grade">',
      '<form id="cupom" class="mkt-card cartao">',
      '<div class="mkt-card-topo"><span class="mkt-badge">' + ico("tiktok") + "<span>Cupom da loja</span></span>" +
        '<label class="mkt-switch"><input type="checkbox" name="ativo" id="cupom-ativo"' + (ativo ? " checked" : "") + '><span>Cupom ativo</span></label></div>',
      '<div class="mkt-campos">',
      '<label class="mkt-campo"><span>Código</span><input name="codigo" id="cupom-codigo" maxlength="24" autocomplete="off" value="' + ui.esc(codigo) + '" placeholder="OFICINA10"></label>',
      '<label class="mkt-campo"><span>Desconto</span><span class="mkt-pct"><input name="percentual" id="cupom-pct" type="number" step="0.5" min="0" max="90" value="' + pct + '"><span class="mkt-pct-sufixo">%</span></span><small>Informe 10 para 10% de desconto</small></label>',
      "</div>",
      '<div class="mkt-atalhos"><button class="chip-pct" type="button" data-pct="5">5%</button><button class="chip-pct" type="button" data-pct="10">10%</button><button class="chip-pct" type="button" data-pct="15">15%</button><button class="chip-pct" type="button" data-pct="20">20%</button></div>',
      '<div class="mkt-acoes"><button class="botao" type="submit">' + ico("documento") + "<span>Salvar cupom</span></button></div>",
      '<p class="mkt-ok" id="mkt-ok" hidden>Cupom salvo.</p>',
      "</form>",
      '<aside class="mkt-lado">',
      '<div id="mkt-preview">' + preview(codigo, pct, ativo) + "</div>",
      '<div class="mkt-dica cartao"><h2 class="mkt-dica-titulo">Como funciona</h2><ul class="mkt-lista"><li>O código vale só na loja própria, no checkout.</li><li>Digite o percentual em números inteiros (ex.: <strong>10</strong> = 10%).</li><li>Desative o cupom sem apagar o código.</li></ul></div>',
      "</aside></div>"
    ].join("");
    function atualizarPreview() {
      var cod = String(document.getElementById("cupom-codigo").value || "").trim().toUpperCase();
      var porcento = Number(document.getElementById("cupom-pct").value) || 0;
      var ligado = document.getElementById("cupom-ativo").checked;
      document.getElementById("mkt-preview").innerHTML = preview(cod, porcento, ligado);
    }
    document.getElementById("cupom-codigo").addEventListener("input", function (evento) {
      evento.target.value = String(evento.target.value || "").toUpperCase().replace(/\s+/g, "");
      atualizarPreview();
    });
    document.getElementById("cupom-pct").addEventListener("input", atualizarPreview);
    document.getElementById("cupom-ativo").addEventListener("change", atualizarPreview);
    raiz.querySelectorAll(".chip-pct").forEach(function (botao) {
      botao.addEventListener("click", function () {
        document.getElementById("cupom-pct").value = botao.getAttribute("data-pct");
        atualizarPreview();
      });
    });
    document.getElementById("cupom").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      var porcento = Number(dados.get("percentual")) || 0;
      if (porcento < 0) porcento = 0;
      if (porcento > 90) porcento = 90;
      Cria.alterar(function (atual) {
        atual.configuracao.cupons = [{
          codigo: String(dados.get("codigo") || "").trim().toUpperCase(),
          percentual: Math.round(porcento * 10) / 1000,
          ativo: Boolean(dados.get("ativo"))
        }];
      });
      var ok = document.getElementById("mkt-ok");
      ok.hidden = false;
      window.setTimeout(function () { ok.hidden = true; }, 2200);
      atualizarPreview();
    });
    ui.revelar(raiz);
  }

  function fiscal() {
    document.title = "Gestão fiscal · Administração";
    var params = new URLSearchParams(location.search);
    var vista = {
      q: params.get("q") || "",
      status: params.get("status") || "",
      doc: params.get("doc") || "",
      nota: params.get("nota") || "",
      ordem: params.get("ordem") || "data"
    };
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function temRecibo(pedido) {
      return pedido.status !== "aguardando pagamento" && pedido.status !== "cancelado";
    }
    function estadoNota(pedido) {
      if (pedido.status === "cancelado" || pedido.status === "aguardando pagamento") return "nao_aplicavel";
      return pedido.nota === "revisada" ? "revisada" : "pendente";
    }
    function rotuloNota(estadoNf) {
      if (estadoNf === "revisada") return "Revisada · sem transmissão";
      if (estadoNf === "nao_aplicavel") return "Sem nota neste status";
      return "Pendente de transmissão";
    }
    function classeNota(estadoNf) {
      if (estadoNf === "revisada") return "fis-selo ok";
      if (estadoNf === "nao_aplicavel") return "fis-selo mute";
      return "fis-selo alerta";
    }
    function classeStatus(status) {
      if (status === "entregue" || status === "pago") return "fis-status ok";
      if (status === "cancelado" || status === "devolvido") return "fis-status mute";
      if (status === "aguardando pagamento") return "fis-status alerta";
      return "fis-status";
    }
    function sincronizarUrl() {
      var url = new URL(location.href);
      ["q", "status", "doc", "nota", "ordem"].forEach(function (chave) {
        if (vista[chave]) url.searchParams.set(chave, vista[chave]);
        else url.searchParams.delete(chave);
      });
      history.replaceState(null, "", url.pathname + url.search);
    }
    function pintar() {
      sincronizarUrl();
      var identidade = estado().identidade || {};
      var pedidos = (estado().pedidos || []).slice();
      var comRecibo = pedidos.filter(temRecibo);
      var pendentes = comRecibo.filter(function (pedido) { return estadoNota(pedido) === "pendente"; });
      var revisadas = comRecibo.filter(function (pedido) { return estadoNota(pedido) === "revisada"; });
      var totalDocs = comRecibo.reduce(function (soma, pedido) { return soma + (Number(pedido.total) || 0); }, 0);
      var statusUnicos = [];
      pedidos.forEach(function (pedido) {
        if (statusUnicos.indexOf(pedido.status) < 0) statusUnicos.push(pedido.status);
      });
      var lista = pedidos.filter(function (pedido) {
        if (vista.status && pedido.status !== vista.status) return false;
        if (vista.doc === "recibo" && !temRecibo(pedido)) return false;
        if (vista.doc === "sem-recibo" && temRecibo(pedido)) return false;
        if (vista.nota && estadoNota(pedido) !== vista.nota) return false;
        if (vista.q) {
          var texto = (pedido.id + " " + pedido.status + " " + (pedido.itens || []).map(function (item) { return item.titulo; }).join(" ") + " " + (pedido.endereco || "")).toLowerCase();
          if (texto.indexOf(vista.q.toLowerCase()) < 0) return false;
        }
        return true;
      });
      if (vista.ordem === "valor") lista.sort(function (a, b) { return (Number(b.total) || 0) - (Number(a.total) || 0); });
      else if (vista.ordem === "id") lista.sort(function (a, b) { return String(a.id).localeCompare(String(b.id)); });
      else lista.sort(function (a, b) { return String(b.criadoEm || "").localeCompare(String(a.criadoEm || "")); });

      var kpis = '<div class="fis-kpis">' +
        '<button class="fis-kpi" type="button" data-filtro-doc="" data-filtro-nota=""><strong>' + pedidos.length + '</strong><span>Pedidos</span></button>' +
        '<button class="fis-kpi" type="button" data-filtro-doc="recibo" data-filtro-nota=""><strong>' + comRecibo.length + '</strong><span>Com recibo</span></button>' +
        '<button class="fis-kpi alerta" type="button" data-filtro-doc="recibo" data-filtro-nota="pendente"><strong>' + pendentes.length + '</strong><span>Notas pendentes</span></button>' +
        '<button class="fis-kpi ok" type="button" data-filtro-doc="recibo" data-filtro-nota="revisada"><strong>' + revisadas.length + '</strong><span>Notas revisadas</span></button>' +
        '<article class="fis-kpi valor"><strong>' + dinheiro.real(totalDocs) + '</strong><span>Total com recibo</span></article>' +
        "</div>";

      var filtros = '<form id="fis-filtros" class="fis-filtros">' +
        '<label class="fin-campo fis-busca"><span>Buscar</span><input name="q" type="search" value="' + ui.esc(vista.q) + '" placeholder="Pedido, item ou endereço"></label>' +
        '<label class="fin-campo"><span>Status do pedido</span><select name="status"><option value="">Todos</option>' +
        statusUnicos.map(function (status) {
          return '<option value="' + ui.esc(status) + '"' + (vista.status === status ? " selected" : "") + ">" + ui.esc(status) + "</option>";
        }).join("") + "</select></label>" +
        '<label class="fin-campo"><span>Documento</span><select name="doc">' +
        '<option value=""' + (!vista.doc ? " selected" : "") + ">Todos</option>" +
        '<option value="recibo"' + (vista.doc === "recibo" ? " selected" : "") + ">Com recibo</option>" +
        '<option value="sem-recibo"' + (vista.doc === "sem-recibo" ? " selected" : "") + ">Sem recibo</option>" +
        "</select></label>" +
        '<label class="fin-campo"><span>Nota fiscal</span><select name="nota">' +
        '<option value=""' + (!vista.nota ? " selected" : "") + ">Todas</option>" +
        '<option value="pendente"' + (vista.nota === "pendente" ? " selected" : "") + ">Pendentes</option>" +
        '<option value="revisada"' + (vista.nota === "revisada" ? " selected" : "") + ">Revisadas</option>" +
        '<option value="nao_aplicavel"' + (vista.nota === "nao_aplicavel" ? " selected" : "") + ">Sem nota</option>" +
        "</select></label>" +
        '<label class="fin-campo"><span>Ordenar</span><select name="ordem">' +
        '<option value="data"' + (vista.ordem === "data" ? " selected" : "") + ">Mais recentes</option>" +
        '<option value="valor"' + (vista.ordem === "valor" ? " selected" : "") + ">Maior valor</option>" +
        '<option value="id"' + (vista.ordem === "id" ? " selected" : "") + ">Pedido</option>" +
        "</select></label>" +
        '<button class="botao-linha fis-csv" type="button" id="fis-csv">' + ico("documento") + "<span>CSV</span></button>" +
        "</form>";

      var cards = lista.length ? lista.map(function (pedido) {
        var nf = estadoNota(pedido);
        var itens = (pedido.itens || []).map(function (item) { return ui.esc(item.titulo) + " × " + item.quantidade; }).join(", ");
        var personalizado = (pedido.itens || []).some(function (item) { return item.tipo === "personalizado"; });
        var acoes = '<div class="fis-acoes">';
        if (temRecibo(pedido)) {
          acoes += '<a class="botao" href="' + ui.url("paginas/recibo.html?id=" + encodeURIComponent(pedido.id)) + '">' + ico("documento") + "<span>Abrir recibo</span></a>";
          if (nf === "pendente") acoes += '<button class="botao-linha" type="button" data-revisar="' + ui.esc(pedido.id) + '">' + ico("email") + "<span>Marcar revisada</span></button>";
          if (nf === "revisada") acoes += '<button class="botao-linha" type="button" data-pendente="' + ui.esc(pedido.id) + '"><span>Voltar para pendente</span></button>';
        } else {
          acoes += '<span class="fis-aviso">Recibo liberado após o pagamento</span>';
        }
        acoes += '<a class="botao-linha" href="' + ui.url("paginas/admin/pedidos.html") + '">' + ico("pedidos") + "<span>Pedidos</span></a>";
        if (personalizado) acoes += '<a class="botao-linha" href="' + ui.url("paginas/admin/os.html?id=" + encodeURIComponent(pedido.id)) + '">' + ico("admin") + "<span>Ordem de serviço</span></a>";
        acoes += '<button class="botao-linha" type="button" data-copiar="' + ui.esc(pedido.id) + '"><span>Copiar ID</span></button>';
        acoes += "</div>";
        return '<article class="fis-card">' +
          '<div class="fis-card-topo">' +
            '<div><strong class="fis-id">' + ui.esc(pedido.id) + "</strong>" +
            '<p class="fis-meta">' + ui.esc(dinheiro.dataBR(String(pedido.criadoEm || "").slice(0, 10))) +
            (pedido.forma ? " · " + ui.esc(pedido.forma) : "") +
            (pedido.freteNome ? " · " + ui.esc(pedido.freteNome) : "") + "</p></div>" +
            '<div class="fis-selos"><span class="' + classeStatus(pedido.status) + '">' + ui.esc(pedido.status) + '</span><span class="' + classeNota(nf) + '">' + ui.esc(rotuloNota(nf)) + "</span></div>" +
          "</div>" +
          '<p class="fis-itens">' + (itens || "Sem itens") + "</p>" +
          '<div class="fis-valores"><span>Total <strong>' + dinheiro.real(pedido.total) + "</strong></span>" +
          (pedido.descontoAVista ? "<span>À vista −" + dinheiro.real(pedido.descontoAVista) + "</span>" : "") +
          (pedido.freteCobrado ? "<span>Frete " + dinheiro.real(pedido.freteCobrado) + "</span>" : "") +
          "</div>" +
          '<p class="fis-nf-txt">Nota fiscal: ' + ui.esc(rotuloNota(nf)) + ". Sem chave e sem protocolo nesta etapa.</p>" +
          acoes +
          "</article>";
      }).join("") : '<div class="fis-vazio cartao"><img src="' + ui.url("marca/mascote.png") + '" alt="" width="96" height="96"><p>Nenhum documento com esses filtros.</p></div>';

      var empresa = '<aside class="fis-empresa cartao">' +
        '<div class="fis-empresa-topo">' + ico("email") + '<h2 class="fis-titulo m-0">Emitente</h2></div>' +
        '<p class="fis-empresa-nome">' + ui.esc(identidade.nome || "Cria Lab DM") + "</p>" +
        "<p>CNPJ " + ui.esc(identidade.cnpj || "—") + "</p>" +
        '<p class="fis-dica">O recibo usa a marca e o CNPJ da loja. A nota fica marcada como pendente de transmissão — não inventamos chave nem protocolo.</p>' +
        '<a class="botao-linha" href="' + ui.url("paginas/admin/configuracao.html") + '">' + ico("admin") + "<span>Dados da loja</span></a>" +
        "</aside>";

      raiz.innerHTML =
        '<div class="admin-prod-cab"><div><h1 class="m-0">Fiscal</h1><p class="admin-prod-intro m-0 mt-1">Organize recibos e o andamento das notas por pedido. Filtre, revise e exporte sem misturar status.</p></div>' +
        '<a class="botao-linha" href="' + ui.url("paginas/admin/pedidos.html") + '">' + ico("pedidos") + "<span>Ver pedidos</span></a></div>" +
        '<div class="fin-abas mb-4"><a class="fin-aba" href="' + ui.url("paginas/admin/pedidos.html") + '">Pedidos</a><a class="fin-aba ativo" href="' + ui.url("paginas/admin/fiscal.html") + '">Fiscal</a></div>' +
        kpis +
        '<div class="fis-grade"><div class="fis-principal">' + filtros +
        '<div class="fis-lista-cab"><h2 class="fis-titulo m-0">Documentos</h2><span class="fis-contador">' + lista.length + " de " + pedidos.length + "</span></div>" +
        '<div class="fis-lista">' + cards + "</div></div>" + empresa + "</div>";

      document.getElementById("fis-filtros").addEventListener("change", function (evento) {
        var form = evento.currentTarget;
        var dados = new FormData(form);
        vista.status = String(dados.get("status") || "");
        vista.doc = String(dados.get("doc") || "");
        vista.nota = String(dados.get("nota") || "");
        vista.ordem = String(dados.get("ordem") || "data");
        pintar();
      });
      document.getElementById("fis-filtros").querySelector('[name="q"]').addEventListener("input", function (evento) {
        vista.q = evento.target.value;
        window.clearTimeout(raiz._fisT);
        raiz._fisT = window.setTimeout(pintar, 180);
      });
      raiz.querySelectorAll("[data-filtro-doc]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.doc = botao.getAttribute("data-filtro-doc") || "";
          vista.nota = botao.getAttribute("data-filtro-nota") || "";
          vista.status = "";
          pintar();
        });
      });
      raiz.querySelectorAll("[data-revisar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-revisar");
          Cria.alterar(function (atual) {
            var pedido = atual.pedidos.find(function (item) { return item.id === id; });
            if (pedido && temRecibo(pedido)) pedido.nota = "revisada";
          });
          pintar();
        });
      });
      raiz.querySelectorAll("[data-pendente]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-pendente");
          Cria.alterar(function (atual) {
            var pedido = atual.pedidos.find(function (item) { return item.id === id; });
            if (pedido) pedido.nota = "pendente";
          });
          pintar();
        });
      });
      raiz.querySelectorAll("[data-copiar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-copiar") || "";
          if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(id).catch(function () {});
          }
          botao.querySelector("span").textContent = "Copiado";
          window.setTimeout(function () { botao.querySelector("span").textContent = "Copiar ID"; }, 1200);
        });
      });
      document.getElementById("fis-csv").addEventListener("click", function () {
        var linhas = [["pedido", "data", "status", "total", "forma", "nota", "recibo", "itens"].join(";")].concat(lista.map(function (pedido) {
          var nf = estadoNota(pedido);
          var itens = (pedido.itens || []).map(function (item) { return item.titulo + " x" + item.quantidade; }).join(" | ");
          return [pedido.id, String(pedido.criadoEm || "").slice(0, 10), pedido.status, pedido.total, pedido.forma || "", rotuloNota(nf), temRecibo(pedido) ? "sim" : "nao", itens].map(function (valor) {
            return '"' + String(valor == null ? "" : valor).replace(/"/g, '""') + '"';
          }).join(";");
        }));
        var blob = new Blob(["\uFEFF" + linhas.join("\n")], { type: "text/csv;charset=utf-8" });
        var link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.download = "fiscal-documentos.csv";
        link.click();
      });
      ui.revelar(raiz);
    }
    pintar();
  }

  if (pagina === "admin-home") painel();
  if (pagina === "admin-personalizados") personalizadosAdmin();
  if (pagina === "admin-marketing") marketing();
  if (pagina === "admin-fiscal") fiscal();
  if (pagina === "admin-produtos") produtos();
  if (pagina === "admin-produto") formularioProduto();
  if (pagina === "admin-config") configuracao();
  if (pagina === "admin-perguntas") perguntas();
  if (pagina === "admin-contato") contatoAdmin();
  if (pagina === "admin-depoimentos") depoimentos();
})();
