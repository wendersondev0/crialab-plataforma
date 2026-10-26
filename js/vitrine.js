(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");

  function estado() { return Cria.estado; }
  function config() { return estado().configuracao; }
  function produtosVisiveis() {
    return estado().produtos.filter(function (produto) { return produto.situacao !== "pausado"; });
  }
  function achar(id) {
    return estado().produtos.find(function (produto) { return produto.id === id; });
  }
  function precoVista(valor) {
    return valor * (1 - config().descontoAVista);
  }
  function ico(nome) {
    return '<span class="ico inline-flex h-5 w-5 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full [&>img]:h-5 [&>img]:w-auto [&>img]:max-w-7 [&>img]:object-contain">' + ui.icone(nome) + "</span>";
  }
  function personalizado(produto) {
    return modalidadeDe(produto) === "personalizado" || produto.tipo === "personalizado";
  }
  function modalidadeDe(produto) {
    return produto.modalidade || (produto.tipo === "personalizado" ? "personalizado" : (Number(produto.estoque) > 0 ? "pronta-entrega" : "sob-demanda"));
  }
  function fichaUrl(produto) {
    return ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id));
  }
  function canaisFicha() {
    var id = estado().identidade;
    var linha = "botao-linha inline-flex w-full min-h-11 items-center justify-center gap-2.5 rounded-full border-2 border-tinta bg-white px-4 font-bold text-tinta no-underline transition hover:bg-teal/10";
    return '<div class="lojas-ficha mt-3 grid grid-cols-1 gap-2 sm:grid-cols-2"><a class="' + linha + '" href="' + ui.esc(id.shopee) + '" target="_blank" rel="noopener">' + ico("shopee") + 'Shopee</a><a class="' + linha + '" href="' + ui.esc(id.mercadoLivre) + '" target="_blank" rel="noopener">' + ico("mercadolivre") + "Mercado Livre</a></div>";
  }
  function addDias(base, dias) {
    var d = new Date(base.getFullYear(), base.getMonth(), base.getDate() + dias);
    return d;
  }
  function mesesCurto() {
    return ["jan.", "fev.", "mar.", "abr.", "mai.", "jun.", "jul.", "ago.", "set.", "out.", "nov.", "dez."];
  }
  function faixaDias(ini, fim) {
    var meses = mesesCurto();
    if (ini.getMonth() === fim.getMonth()) {
      return ini.getDate() + " - " + fim.getDate() + " de " + meses[fim.getMonth()];
    }
    return ini.getDate() + " de " + meses[ini.getMonth()] + " - " + fim.getDate() + " de " + meses[fim.getMonth()];
  }
  function htmlEntrega(produto) {
    var hoje = new Date();
    var prazo = Number(produto.prazoDias) || 5;
    var pac = 5;
    var faixa = (config().faixasCep || [])[2];
    if (faixa && faixa.pacDias) pac = Number(faixa.pacDias) || 5;
    var mod = modalidadeDe(produto);
    if (mod === "personalizado") {
      return 'Prazo após orçamento · cerca de <strong class="font-extrabold">' + prazo + " dias</strong>";
    }
    var ini;
    var fim;
    if (mod === "sob-demanda") {
      ini = addDias(hoje, prazo);
      fim = addDias(hoje, prazo + pac);
    } else {
      ini = addDias(hoje, Math.max(2, pac - 1));
      fim = addDias(hoje, pac + 3);
    }
    var gratis = Number(produto.preco) >= Number(config().freteGratisAPartir);
    var prefixo = gratis ? "Entrega GRÁTIS: " : "Entrega: ";
    return prefixo + '<strong class="font-extrabold">' + faixaDias(ini, fim) + "</strong>";
  }
  function dadosDesconto(produto) {
    if (personalizado(produto)) return null;
    var atual = Number(produto.preco) || 0;
    var anterior = Number(produto.precoComparativo) || 0;
    if (!(anterior > atual) && produto.oferta) anterior = Math.round(atual * 1.18 * 100) / 100;
    if (!(anterior > atual)) return null;
    var pct = Math.max(1, Math.round((1 - atual / anterior) * 100));
    return { atual: atual, anterior: anterior, pct: pct };
  }
  function tagOferta(produto) {
    var d = dadosDesconto(produto);
    if (!d) return "";
    return '<span class="tag-oferta" aria-label="' + d.pct + '% de desconto">-' + d.pct + "%</span>";
  }
  function botaoFavoritoCard(produto) {
    var ativo = estado().favoritos.indexOf(produto.id) >= 0;
    return '<button class="btn-favorito-card' + (ativo ? " ativo" : "") + '" type="button" data-favorito="' + ui.esc(produto.id) + '" aria-label="' + (ativo ? "Remover dos favoritos" : "Favoritar") + '" aria-pressed="' + (ativo ? "true" : "false") + '">' + ico("coracao") + "</button>";
  }
  function blocoPrecoCard(produto) {
    var sob = personalizado(produto);
    var atual = Number(produto.preco) || 0;
    if (sob) {
      return '<p class="preco-card m-0 text-[1.35rem] font-extrabold leading-none text-tinta">' + dinheiro.real(atual) + '</p><p class="m-0 mt-1 text-sm font-semibold text-tinta/65">Preço médio · fecha no orçamento</p>';
    }
    var d = dadosDesconto(produto);
    if (!d) {
      return '<p class="preco-card m-0 text-[1.45rem] font-extrabold leading-none text-tinta">' + dinheiro.real(atual) + "</p>";
    }
    return '<div class="preco-linha mt-1 flex flex-wrap items-end gap-2"><p class="preco-card m-0 text-[1.45rem] font-extrabold leading-none text-tinta">' + dinheiro.real(d.atual) + '</p><s class="preco-riscado text-sm font-medium text-tinta/45">' + dinheiro.real(d.anterior) + "</s></div>";
  }
  function card(produto) {
    var foto = produto.fotos[0];
    var mod = modalidadeDe(produto);
    var tagClasse = mod === "personalizado" ? "tag-pers bg-laranja" : (mod === "sob-demanda" ? "bg-tinta" : "bg-teal");
    var tag = '<span class="tag ' + tagClasse + ' inline-flex w-fit items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-extrabold text-white">' + ui.esc(ui.nomeModalidade(mod)) + "</span>";
    var nota = Number(produto.nota) > 0
      ? '<p class="m-0 flex items-center gap-1 text-sm text-tinta/75"><span class="font-bold">' + String(produto.nota).replace(".", ",") + '</span><span class="text-laranja" aria-hidden="true">★★★★★</span></p>'
      : "";
    var oferta = tagOferta(produto);
    return '<article class="produto revela relative flex flex-col overflow-hidden rounded-[18px] bg-white text-tinta shadow-cria"><a class="produto-link flex flex-1 flex-col no-underline text-inherit" href="' + fichaUrl(produto) + '"><span class="produto-foto relative block overflow-hidden"><img class="aspect-square w-full object-cover bg-[#efe8da]" src="' + ui.url(foto.arquivo) + '" alt="' + ui.esc(foto.legenda) + '">' + oferta + '</span><span class="produto-corpo flex flex-1 flex-col gap-1.5 p-4">' + tag + '<span class="discreto m-0 text-sm text-tinta/60">' + ui.esc(ui.nomeCategoria(produto.categoria)) + '</span><h3 class="m-0 text-[1.05rem] leading-snug font-bold text-tinta">' + ui.esc(produto.titulo) + "</h3>" + nota + blocoPrecoCard(produto) + '<p class="entrega-card m-0 mt-auto pt-2 text-sm text-tinta">' + htmlEntrega(produto) + "</p></span></a>" + botaoFavoritoCard(produto) + "</article>";
  }
  function ligarFavoritosCards(escopo) {
    var caixa = escopo || raiz;
    if (!caixa || caixa._favoritosLigados) return;
    caixa._favoritosLigados = true;
    caixa.addEventListener("click", function (evento) {
      var botao = evento.target.closest("[data-favorito]");
      if (!botao || !caixa.contains(botao)) return;
      evento.preventDefault();
      evento.stopPropagation();
      var id = botao.getAttribute("data-favorito");
      Cria.alterar(function (atual) {
        if (atual.favoritos.indexOf(id) >= 0) atual.favoritos = atual.favoritos.filter(function (item) { return item !== id; });
        else atual.favoritos.push(id);
      });
      var ativo = estado().favoritos.indexOf(id) >= 0;
      botao.classList.toggle("ativo", ativo);
      botao.setAttribute("aria-pressed", ativo ? "true" : "false");
      botao.setAttribute("aria-label", ativo ? "Remover dos favoritos" : "Favoritar");
      if (ativo) {
        botao.classList.remove("bate");
        void botao.offsetWidth;
        botao.classList.add("bate");
        window.setTimeout(function () { botao.classList.remove("bate"); }, 560);
      }
    });
  }
  function trilho(titulo, lista) {
    if (!lista.length) return "";
    return '<section class="mt-6"><h2 class="mb-3 text-2xl font-extrabold text-tinta">' + ui.esc(titulo) + '</h2><div class="trilho grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">' + lista.map(card).join("") + "</div></section>";
  }
  function redes() {
    return ui.redes();
  }
  function buscaCampo(valor) {
    return '<form class="formulario" id="form-busca" action="' + ui.url("paginas/busca.html") + '"><label>Buscar peça<input type="search" name="q" id="q" value="' + ui.esc(valor || "") + '" placeholder="Nome, material ou categoria"></label><div id="sugestoes"></div></form>';
  }
  function ligarSugestoes() {
    var campo = document.getElementById("q");
    var caixa = document.getElementById("sugestoes");
    if (!campo || !caixa) return;
    campo.addEventListener("input", function () {
      var texto = campo.value.trim().toLowerCase();
      if (texto.length < 2) { caixa.innerHTML = ""; return; }
      var lista = produtosVisiveis().filter(function (produto) {
        return (produto.titulo + " " + produto.descricao).toLowerCase().indexOf(texto) >= 0;
      }).slice(0, 6);
      caixa.innerHTML = lista.length ? '<div class="sugestoes">' + lista.map(function (produto) {
        return '<a href="' + ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id)) + '">' + ui.esc(produto.titulo) + "</a>";
      }).join("") + "</div>" : "";
    });
  }
  function introModalidade(id) {
    var info = ui.modalidadeInfo(id);
    if (!info) return "";
    return '<p class="intro-modalidade mt-1 mb-4 max-w-3xl text-base text-tinta/75" title="' + ui.esc(info.resumo) + '">' + ui.esc(info.explicacao) + "</p>";
  }
  function carrossel() {
    var gratis = dinheiro.real(config().freteGratisAPartir);
    var slides = [
      { titulo: "Pronta entrega na oficina", texto: "Peças já impressas, prontas para envio ou retirada.", fundo: "laranja" },
      { titulo: "Sob demanda e personalizados", texto: "Modelo pronto para imprimir, ou peça sob medida com orçamento.", fundo: "teal" },
      { titulo: "Frete grátis a partir de " + gratis, texto: "Na ficha você simula PAC e SEDEX pelo CEP.", fundo: "tinta" }
    ];
    return '<section class="carrossel revela" id="carrossel"><div class="carrossel-trilho">' + slides.map(function (slide) {
      return '<article class="slide fundo-' + slide.fundo + '"><div class="slide-texto"><p class="selo">Cria Lab DM</p><h2>' + ui.esc(slide.titulo) + "</h2><p>" + ui.esc(slide.texto) + '</p></div><img class="slide-mascote" src="' + ui.url("marca/mascote.png") + '" alt=""></article>';
    }).join("") + '</div><button class="carrossel-seta anterior" type="button" data-passo="-1" aria-label="Anterior">‹</button><button class="carrossel-seta proxima" type="button" data-passo="1" aria-label="Próximo">›</button><div class="carrossel-pontos">' + slides.map(function (_, indice) {
      return '<button type="button" data-slide="' + indice + '"' + (indice === 0 ? ' class="ativo"' : "") + ' aria-label="Slide ' + (indice + 1) + '"></button>';
    }).join("") + "</div></section>";
  }
  function ligarCarrossel() {
    var caixa = document.getElementById("carrossel");
    if (!caixa) return;
    var trilho = caixa.querySelector(".carrossel-trilho");
    var pontos = caixa.querySelectorAll("[data-slide]");
    var indice = 0;
    var total = pontos.length;
    var timer = 0;
    function ir(n) {
      indice = (n + total) % total;
      trilho.style.transform = "translateX(-" + (indice * 100) + "%)";
      pontos.forEach(function (ponto, i) { ponto.classList.toggle("ativo", i === indice); });
    }
    function andar() {
      window.clearInterval(timer);
      if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
      timer = window.setInterval(function () { ir(indice + 1); }, 4000);
    }
    caixa.addEventListener("mouseenter", function () { window.clearInterval(timer); });
    caixa.addEventListener("mouseleave", andar);
    caixa.addEventListener("click", function (evento) {
      var ponto = evento.target.closest("[data-slide]");
      var passo = evento.target.closest("[data-passo]");
      if (ponto) ir(Number(ponto.getAttribute("data-slide")));
      if (passo) ir(indice + Number(passo.getAttribute("data-passo")));
    });
    andar();
  }
  function home() {
    document.title = "Cria Lab DM";
    var ordem = { "pronta-entrega": 0, "sob-demanda": 1, personalizado: 2 };
    var lista = produtosVisiveis().slice().sort(function (a, b) {
      var ia = Object.prototype.hasOwnProperty.call(ordem, modalidadeDe(a)) ? ordem[modalidadeDe(a)] : 9;
      var ib = Object.prototype.hasOwnProperty.call(ordem, modalidadeDe(b)) ? ordem[modalidadeDe(b)] : 9;
      return ia - ib;
    });
    var ajuda = estado().menusAjuda || {};
    var modalidades = (estado().modalidades || []).map(function (item) {
      var href = item.id === "pronta-entrega" ? "paginas/pronta-entrega.html" : (item.id === "sob-demanda" ? "paginas/sob-demanda.html" : "paginas/personalizados.html");
      return '<a class="chip revela inline-flex min-h-11 items-center rounded-full border-2 border-linha bg-white px-3.5 font-bold text-tinta no-underline" href="' + ui.url(href) + '" title="' + ui.esc(item.resumo) + '">' + ui.esc(item.nome) + "</a>";
    }).join("");
    var temas = estado().categorias.map(function (categoria) {
      return '<a class="chip revela inline-flex min-h-11 items-center rounded-full border-2 border-linha bg-white px-3.5 font-bold text-tinta no-underline" href="' + ui.url("paginas/busca.html?categoria=" + encodeURIComponent(categoria.id)) + '" title="Filtrar a busca por ' + ui.esc(categoria.nome) + '">' + ui.esc(categoria.nome) + "</a>";
    }).join("");
    raiz.innerHTML = carrossel() + '<p class="intro-pagina intro-home mt-4 mb-2 max-w-3xl text-tinta/75" title="' + ui.esc(ajuda.inicio || "") + '">' + ui.esc(ajuda.inicio || "Explore a vitrine da Cria Lab.") + '</p><section class="faixa-categorias faixa-modalidades my-3"><p class="discreto m-0 mb-2 font-bold text-tinta/70">Modalidades</p><div class="chips flex flex-wrap gap-2">' + modalidades + '</div></section><section class="faixa-categorias my-3"><p class="discreto m-0 mb-2 font-bold text-tinta/70">Temas</p><div class="chips chips-temas flex flex-nowrap gap-2 overflow-x-auto pb-1">' + temas + '</div></section><section class="mt-2"><div class="titulo-secao linha"><h2>Na loja</h2></div><p class="discreto intro-grade mb-4 max-w-3xl">Cards sem botão: toque na peça para ver preço, frete, Shopee e Mercado Livre na ficha.</p><div class="grade vitrine grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">' + lista.map(card).join("") + "</div></section>";
    ligarCarrossel();
    ligarFavoritosCards();
    ui.revelar(raiz);
  }
  function listaModalidade(id) {
    var info = ui.modalidadeInfo(id) || { nome: id, resumo: "", explicacao: "" };
    document.title = info.nome + " · Cria Lab DM";
    var lista = produtosVisiveis().filter(function (produto) {
      return modalidadeDe(produto) === id;
    });
    raiz.innerHTML = '<h1 class="text-3xl font-extrabold text-tinta" title="' + ui.esc(info.resumo) + '">' + ui.esc(info.nome) + "</h1>" + introModalidade(id) + (lista.length ? '<div class="grade vitrine mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">' + lista.map(card).join("") + "</div>" : ui.vazio("Nada publicado nesta lista."));
    ligarFavoritosCards();
    ui.revelar(raiz);
  }
  function lerConsulta() {
    var q = new URLSearchParams(location.search);
    return {
      q: q.get("q") || "",
      categoria: q.get("categoria") || "",
      precoMin: q.get("precoMin") || "",
      precoMax: q.get("precoMax") || "",
      freteGratis: q.get("freteGratis") === "1",
      aVista: q.get("aVista") === "1",
      personalizavel: q.get("personalizavel") === "1",
      nota: q.get("nota") || "",
      ordem: q.get("ordem") || "relevancia"
    };
  }
  function filtrar(consulta) {
    var limite = config().freteGratisAPartir;
    var texto = consulta.q.trim().toLowerCase();
    var lista = produtosVisiveis().filter(function (produto) {
      if (texto && (produto.titulo + " " + produto.descricao + " " + produto.material).toLowerCase().indexOf(texto) < 0) return false;
      if (consulta.categoria && produto.categoria !== consulta.categoria) return false;
      if (consulta.precoMin && produto.preco < Number(consulta.precoMin)) return false;
      if (consulta.precoMax && produto.preco > Number(consulta.precoMax)) return false;
      if (consulta.freteGratis && produto.preco < limite) return false;
      if (consulta.aVista && !produto.aVista) return false;
      if (consulta.personalizavel && !produto.personalizavel) return false;
      if (consulta.nota && (produto.nota || 0) < Number(consulta.nota)) return false;
      return true;
    });
    var ordem = consulta.ordem;
    lista.sort(function (a, b) {
      if (ordem === "preco") return a.preco - b.preco;
      if (ordem === "vendas") return b.vendas - a.vendas;
      if (ordem === "novidade") return String(b.criadoEm).localeCompare(String(a.criadoEm));
      var pa = texto && a.titulo.toLowerCase().indexOf(texto) >= 0 ? 2 : 1;
      var pb = texto && b.titulo.toLowerCase().indexOf(texto) >= 0 ? 2 : 1;
      return pb - pa || b.vendas - a.vendas;
    });
    return lista;
  }
  function htmlFiltros(consulta) {
    function marcado(nome) { return consulta[nome] ? " checked" : ""; }
    var categorias = '<option value="">Todas</option>' + estado().categorias.map(function (categoria) {
      return '<option value="' + ui.esc(categoria.id) + '"' + (consulta.categoria === categoria.id ? " selected" : "") + ">" + ui.esc(categoria.nome) + "</option>";
    }).join("");
    return '<form id="filtros" class="cartao filtros"><label>Texto<input name="q" value="' + ui.esc(consulta.q) + '"></label><label>Categoria<select name="categoria">' + categorias + '</select></label><div class="grade-3"><label>Preço mínimo<input name="precoMin" type="number" min="0" step="0.01" value="' + ui.esc(consulta.precoMin) + '"></label><label>Preço máximo<input name="precoMax" type="number" min="0" step="0.01" value="' + ui.esc(consulta.precoMax) + '"></label><label>Ordenar<select name="ordem"><option value="relevancia">Relevância</option><option value="preco"' + (consulta.ordem === "preco" ? " selected" : "") + '>Preço</option><option value="vendas"' + (consulta.ordem === "vendas" ? " selected" : "") + '>Vendas</option><option value="novidade"' + (consulta.ordem === "novidade" ? " selected" : "") + '>Novidade</option></select></label></div><div class="linha"><label><input type="checkbox" name="freteGratis"' + marcado("freteGratis") + "> Frete grátis</label><label><input type=\"checkbox\" name=\"aVista\"" + marcado("aVista") + "> À vista</label><label><input type=\"checkbox\" name=\"personalizavel\"" + marcado("personalizavel") + "> Personalizável</label><label>Nota mínima<select name=\"nota\"><option value=\"\">Qualquer</option><option value=\"4\"" + (consulta.nota === "4" ? " selected" : "") + ">4 ou mais</option><option value=\"5\"" + (consulta.nota === "5" ? " selected" : "") + ">5</option></select></label></div><button class=\"botao\" type=\"submit\">Aplicar filtros</button></form>";
  }
  function busca() {
    document.title = "Busca · Cria Lab DM";
    var consulta = lerConsulta();
    var ajuda = (estado().menusAjuda || {}).busca || "Filtre peças por texto, tema e preço.";
    raiz.innerHTML = "<h1>Busca</h1><p class=\"intro-pagina max-w-3xl text-tinta/75\" title=\"" + ui.esc(ajuda) + "\">" + ui.esc(ajuda) + "</p>" + ui.vazio("Olhando a mesa da oficina...") ;
    raiz.querySelector(".vazio").classList.add("carregando");
    window.setTimeout(function () {
      var lista = filtrar(consulta);
      var grade = lista.length ? '<div class="grade">' + lista.map(card).join("") + "</div>" : ui.vazio("Nenhuma peça com esses filtros.");
      var ajuda = (estado().menusAjuda || {}).busca || "Filtre peças por texto, tema e preço.";
      raiz.innerHTML = "<h1>Busca</h1><p class=\"intro-pagina mb-3 max-w-3xl text-tinta/75\" title=\"" + ui.esc(ajuda) + "\">" + ui.esc(ajuda) + "</p>" + htmlFiltros(consulta) + grade;
      document.getElementById("filtros").addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        var params = new URLSearchParams();
        ["q", "categoria", "precoMin", "precoMax", "ordem", "nota"].forEach(function (nome) {
          var valor = String(dados.get(nome) || "");
          if (valor) params.set(nome, valor);
        });
        ["freteGratis", "aVista", "personalizavel"].forEach(function (nome) {
          if (dados.get(nome)) params.set(nome, "1");
        });
        location.href = ui.url("paginas/busca.html" + (params.toString() ? "?" + params.toString() : ""));
      });
      ligarFavoritosCards();
      ui.revelar(raiz);
    }, 350);
  }
  function registrarVisto(id) {
    Cria.alterar(function (atual) {
      atual.vistos = [id].concat(atual.vistos.filter(function (item) { return item !== id; })).slice(0, 8);
    });
  }
  function produtoPagina() {
    var id = new URLSearchParams(location.search).get("id");
    var produto = achar(id);
    if (!produto) {
      raiz.innerHTML = ui.vazio("Não encontrei essa peça.");
      ui.revelar(raiz);
      return;
    }
    if (produto.situacao === "pausado") {
      raiz.innerHTML = ui.vazio("Esta peça está pausada.");
      ui.revelar(raiz);
      return;
    }
    registrarVisto(produto.id);
    document.title = produto.titulo + " · Cria Lab DM";
    var fotoInicial = produto.fotos[0];
    var miniaturas = produto.fotos.map(function (foto, indice) {
      return '<button type="button" class="' + (indice === 0 ? "ativa" : "") + '" data-foto="' + ui.esc(foto.arquivo) + '"><img src="' + ui.url(foto.arquivo) + '" alt="' + ui.esc(foto.legenda) + '"></button>';
    }).join("");
    var amostras = produto.cores.map(function (cor, indice) {
      return '<button type="button" class="amostra' + (indice === 0 ? " ativa" : "") + '" style="background:' + ui.esc(cor.hex) + '" data-cor="' + ui.esc(cor.nome) + '" title="' + ui.esc(cor.nome) + '"></button>';
    }).join("");
    var tamanhos = produto.tamanhos.map(function (tamanho, indice) {
      return '<button type="button" class="botao-linha' + (indice === 0 ? " ativo" : "") + '" data-tamanho="' + ui.esc(tamanho) + '">' + ui.esc(tamanho) + "</button>";
    }).join("");
    var etapas = produto.personalizavel ? produto.etapas.map(function (etapa) {
      var campo = etapa.tipo === "arquivo" ? '<span class="arquivo-ref"><input type="file" accept="image/*" data-etapa="' + ui.esc(etapa.id) + '"><span class="botao-linha">' + ico("documento") + "Escolher imagem</span><span class=\"arquivo-nome\">Nenhuma imagem</span></span>" : '<input data-etapa="' + ui.esc(etapa.id) + '" ' + (etapa.tipo === "numero" ? 'type="number" min="1"' : 'type="text"') + ">";
      var extra = etapa.acresce ? " · +" + dinheiro.real(etapa.acresce) : "";
      return "<label>" + ui.esc(etapa.nome) + extra + campo + "</label>";
    }).join("") : "";
    var perguntas = estado().perguntas.filter(function (item) { return item.produtoId === produto.id; }).map(function (item) {
      var resposta = item.resposta ? '<p class="resposta"><strong>Cria Lab</strong><span>' + ui.esc(item.resposta) + "</span></p>" : '<p class="discreto">Aguardando resposta</p>';
      return '<article class="pergunta"><strong>' + ui.esc(item.autor) + "</strong><p>" + ui.esc(item.texto) + "</p>" + resposta + "</article>";
    }).join("") || "<p>Nenhuma pergunta ainda.</p>";
    var avaliacoes = estado().avaliacoes.filter(function (item) { return item.produtoId === produto.id && item.pedidoId; }).map(function (item) {
      var nota = Math.max(0, Math.min(5, Number(item.nota) || 0));
      var estrelas = "";
      for (var i = 1; i <= 5; i++) estrelas += '<span class="' + (i <= nota ? "cheia" : "") + '">★</span>';
      var foto = item.fotoDoProduto && produto.fotos[0] ? '<img src="' + ui.url(produto.fotos[0].arquivo) + '" alt="Foto da avaliação">' : "";
      return '<article class="avaliacao' + (foto ? "" : " sem-foto") + '">' + foto + '<div><div class="avaliacao-topo"><strong>' + ui.esc(item.autor) + '</strong><span class="estrelas" aria-label="' + nota + ' de 5">' + estrelas + '</span></div><p>' + ui.esc(item.texto) + "</p></div></article>";
    }).join("") || "<p>Sem avaliações de pedido entregue.</p>";
    var relacionados = produtosVisiveis().filter(function (item) { return item.categoria === produto.categoria && item.id !== produto.id; }).slice(0, 4);
    var sobMedida = personalizado(produto);
    var mod = modalidadeDe(produto);
    var esgotado = !sobMedida && mod === "pronta-entrega" && (produto.situacao === "esgotado" || produto.estoque <= 0);
    var compraPeca = sobMedida ? "" : '<button class="botao inline-flex w-full min-h-11 items-center justify-center gap-2 rounded-full border-2 border-laranja bg-laranja px-4 font-bold text-white hover:bg-laranja-escuro disabled:opacity-45" type="button" id="comprar-peca"' + (esgotado ? " disabled" : "") + ">" + ico("carrinho") + (esgotado ? "Esgotado" : "Comprar") + "</button>";
    var whats = estado().identidade.whatsapp + "?text=" + encodeURIComponent("Olá, quero saber sobre " + produto.titulo);
    var tagCor = mod === "personalizado" ? "tag-pers bg-laranja" : (mod === "sob-demanda" ? "bg-tinta" : "bg-teal");
    var tag = '<span class="tag ' + tagCor + ' inline-flex w-fit items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-extrabold text-white" title="' + ui.esc((ui.modalidadeInfo(mod) || {}).resumo || "") + '">' + ui.esc(ui.nomeModalidade(mod)) + "</span>";
    var desc = dadosDesconto(produto);
    var precoBloco = sobMedida
      ? '<p class="preco m-0 text-2xl font-extrabold">' + dinheiro.real(produto.preco) + '</p><p class="vista m-0 font-bold text-teal-escuro">Preço médio. O valor final sai depois do orçamento.</p><p class="entrega-card m-0 mt-1 text-sm text-tinta">' + htmlEntrega(produto) + "</p>"
      : (desc ? tagOferta(produto) : "") + '<div class="preco-linha flex flex-wrap items-end gap-2"><p class="preco m-0 text-2xl font-extrabold">' + dinheiro.real(produto.preco) + '</p>' + (desc ? '<s class="preco-riscado text-base font-medium text-tinta/45">' + dinheiro.real(desc.anterior) + "</s>" : "") + '</div><p class="vista m-0 font-bold text-teal-escuro">À vista ' + dinheiro.real(precoVista(produto.preco)) + " no PIX ou boleto</p><p class=\"discreto m-0 text-tinta/60\">" + config().parcelas + "x de " + dinheiro.real(produto.preco / config().parcelas) + " no cartão, sem o desconto à vista</p><p class=\"entrega-card m-0 mt-1 text-sm text-tinta\">" + htmlEntrega(produto) + "</p>";
    var estoqueLinha = sobMedida ? "Sob encomenda" : (mod === "sob-demanda" ? "Impressão após o pedido" : (esgotado ? "Sem unidade pronta" : ui.esc(String(produto.estoque)) + " em estoque"));
    var favoritoAtivo = estado().favoritos.indexOf(produto.id) >= 0;
    var lojas = canaisFicha();
    var orcamento = (produto.personalizavel || sobMedida) ? '<button class="botao-teal inline-flex w-full min-h-11 items-center justify-center gap-2 rounded-full border-2 border-teal bg-teal px-4 font-bold text-white" type="button" id="pedir-orcamento">' + ico("documento") + "Pedir orçamento</button>" : "";
    var acoes = '<div class="acoes-ficha mt-3 grid gap-2">' + compraPeca + '<div class="acoes-dupla grid grid-cols-1 gap-2 sm:grid-cols-2"><button class="btn-favorito-ficha' + (favoritoAtivo ? " ativo" : "") + '" type="button" id="favorito" aria-pressed="' + (favoritoAtivo ? "true" : "false") + '">' + ico("coracao") + '<span id="favorito-rotulo">' + (favoritoAtivo ? "Nos favoritos" : "Salvar nos favoritos") + '</span></button><a class="botao-linha inline-flex w-full min-h-11 items-center justify-center gap-2 rounded-full border-2 border-tinta bg-white px-4 font-bold no-underline" href="' + ui.esc(whats) + '" target="_blank" rel="noopener">' + ico("whatsapp") + "WhatsApp</a></div>" + orcamento + "</div>";
    raiz.innerHTML = '<div class="ficha grid grid-cols-1 items-start gap-5 lg:grid-cols-2"><div class="galeria revela"><img class="galeria-principal rounded-[18px]" id="foto-principal" src="' + ui.url(fotoInicial.arquivo) + '" alt="' + ui.esc(fotoInicial.legenda) + '"><div class="miniaturas">' + miniaturas + '</div><p class="discreto" id="legenda-foto">' + ui.esc(fotoInicial.legenda) + " · sessão " + ui.esc(dinheiro.dataBR(produto.sessao)) + '</p></div><div class="cartao ficha-info revela grid gap-3 rounded-[18px] bg-white p-4 shadow-cria">' + tag + '<p class="discreto m-0 text-tinta/60">' + ui.esc(ui.nomeCategoria(produto.categoria)) + "</p><h1 class=\"mt-0 text-2xl font-extrabold\">" + ui.esc(produto.titulo) + "</h1>" + precoBloco + '<p class="meta-peca m-0 flex flex-wrap gap-x-3 gap-y-1 text-sm text-tinta/70"><span>' + estoqueLinha + "</span><span>" + ui.esc(produto.peso) + " g</span><span>" + ui.esc(produto.medidas) + "</span></p><p class=\"m-0\">" + ui.esc(produto.descricao) + "</p>" + lojas + '<section class="bloco-ficha grid gap-2"><h2 class="mt-0 text-lg font-extrabold">Filamento</h2><div class="amostras" id="amostras">' + amostras + '</div><p id="cor-nome">' + ui.esc(produto.cores[0].nome) + '</p><div class="linha" id="tamanhos">' + tamanhos + "</div></section>" + (etapas ? '<section class="bloco-ficha"><h2>Personalização</h2><div class="formulario">' + etapas + "</div></section>" : "") + '<label class="qtd-ficha">Quantidade<input id="qtd" type="number" min="1" value="1"></label>' + acoes + '<section class="frete-caixa"><h2 id="frete-titulo">Calcular frete</h2><form id="frete-ficha" class="frete-form"><input name="cep" type="text" inputmode="numeric" placeholder="00000-000" maxlength="9" aria-label="CEP"><button class="botao" type="submit">' + ico("caminhao") + 'Cotar</button></form><div id="frete-resultado"></div></section></div></div><section class="painel-ficha mt-5 grid grid-cols-1 gap-4"><article class="cartao rounded-[18px] bg-white p-4 shadow-cria"><h2 class="mt-0 text-xl font-extrabold">Perguntas</h2>' + perguntas + '<form id="nova-pergunta" class="formulario mt-3 grid gap-3"><label class="grid gap-1.5 font-bold">Sua pergunta<input class="min-h-12 w-full rounded-full border-2 border-linha bg-white px-4.5 py-2.5" name="texto" type="text" required placeholder="Escreva sua pergunta"></label><button class="botao inline-flex min-h-11 w-fit items-center justify-center gap-2 rounded-full bg-laranja px-4 font-bold text-white" type="submit">' + ico("contato") + "Enviar</button></form></article><article class=\"cartao rounded-[18px] bg-white p-4 shadow-cria\"><h2 class=\"mt-0 text-xl font-extrabold\">Avaliações</h2>" + avaliacoes + "</article></section>" + trilho("Relacionados", relacionados);
    document.querySelectorAll(".arquivo-ref input").forEach(function (campoArquivo) {
      campoArquivo.addEventListener("change", function () {
        var nome = campoArquivo.files && campoArquivo.files[0] ? campoArquivo.files[0].name : "Nenhuma imagem";
        var alvo = campoArquivo.parentElement.querySelector(".arquivo-nome");
        if (alvo) alvo.textContent = nome;
      });
    });
    var cor = produto.cores[0].nome;
    var tamanho = produto.tamanhos[0];
    document.getElementById("amostras").addEventListener("click", function (evento) {
      var botao = evento.target.closest(".amostra");
      if (!botao) return;
      cor = botao.getAttribute("data-cor");
      document.querySelectorAll(".amostra").forEach(function (el) { el.classList.remove("ativa"); });
      botao.classList.add("ativa");
      document.getElementById("cor-nome").textContent = cor;
    });
    document.getElementById("tamanhos").addEventListener("click", function (evento) {
      var botao = evento.target.closest("[data-tamanho]");
      if (!botao) return;
      tamanho = botao.getAttribute("data-tamanho");
      document.querySelectorAll("[data-tamanho]").forEach(function (el) { el.classList.remove("ativo"); });
      botao.classList.add("ativo");
    });
    document.querySelector(".miniaturas").addEventListener("click", function (evento) {
      var botao = evento.target.closest("[data-foto]");
      if (!botao) return;
      var caminho = botao.getAttribute("data-foto");
      var img = document.getElementById("foto-principal");
      img.src = ui.url(caminho);
      document.querySelectorAll(".miniaturas button").forEach(function (el) { el.classList.remove("ativa"); });
      botao.classList.add("ativa");
      var foto = produto.fotos.find(function (item) { return item.arquivo === caminho; });
      document.getElementById("legenda-foto").textContent = (foto ? foto.legenda : produto.titulo) + " · sessão " + dinheiro.dataBR(produto.sessao);
    });
    document.getElementById("foto-principal").addEventListener("click", function () {
      var camada = document.createElement("div");
      camada.className = "zoom";
      camada.innerHTML = '<img src="' + document.getElementById("foto-principal").src + '" alt="">';
      camada.addEventListener("click", function () { camada.remove(); });
      document.body.appendChild(camada);
    });
    function detalhesPersonalizacao() {
      if (!produto.personalizavel) return { texto: "", extra: 0, arquivoNome: "" };
      var partes = [];
      var extra = 0;
      var arquivoNome = "";
      produto.etapas.forEach(function (etapa) {
        var campo = document.querySelector('[data-etapa="' + etapa.id + '"]');
        if (!campo) return;
        extra += Number(etapa.acresce) || 0;
        if (etapa.tipo === "arquivo") arquivoNome = campo.files && campo.files[0] ? campo.files[0].name : "";
        else if (campo.value) partes.push(etapa.nome + ": " + campo.value);
      });
      return { texto: partes.join(" · "), extra: extra, arquivoNome: arquivoNome };
    }
    var comprar = document.getElementById("comprar-peca");
    if (comprar) comprar.addEventListener("click", function () {
      var personalizacao = detalhesPersonalizacao();
      var qtd = Math.max(1, Number(document.getElementById("qtd").value) || 1);
      if (modalidadeDe(produto) === "pronta-entrega" && qtd > produto.estoque) {
        alert("A quantidade passa do estoque.");
        return;
      }
      CriaCarrinho.adicionar({
        produtoId: produto.id,
        titulo: produto.titulo,
        tipo: produto.tipo === "personalizado" ? "personalizado" : "pronto",
        fisico: true,
        arquivo: false,
        quantidade: qtd,
        precoUnitario: produto.preco + personalizacao.extra,
        detalhes: [cor, tamanho, personalizacao.texto].filter(Boolean).join(" · "),
        arquivoNome: personalizacao.arquivoNome,
        assinatura: produto.tipo === "personalizado" ? CriaCarrinho.novoId("pers") : cor + "|" + tamanho
      });
      location.href = ui.url("paginas/carrinho.html");
    });
    document.getElementById("favorito").addEventListener("click", function () {
      Cria.alterar(function (atual) {
        if (atual.favoritos.indexOf(produto.id) >= 0) atual.favoritos = atual.favoritos.filter(function (item) { return item !== produto.id; });
        else atual.favoritos.push(produto.id);
      });
      var ativo = estado().favoritos.indexOf(produto.id) >= 0;
      var botao = document.getElementById("favorito");
      botao.classList.toggle("ativo", ativo);
      botao.setAttribute("aria-pressed", ativo ? "true" : "false");
      document.getElementById("favorito-rotulo").textContent = ativo ? "Nos favoritos" : "Salvar nos favoritos";
      if (ativo) {
        botao.classList.remove("bate");
        void botao.offsetWidth;
        botao.classList.add("bate");
        window.setTimeout(function () { botao.classList.remove("bate"); }, 560);
      }
    });
    var pedir = document.getElementById("pedir-orcamento");
    if (pedir) pedir.addEventListener("click", function () {
      var personalizacao = detalhesPersonalizacao();
      Cria.alterar(function (atual) {
        atual.orcamentosCliente.push({
          id: CriaCarrinho.novoId("orc-cli"),
          produtoId: produto.id,
          titulo: produto.titulo,
          status: "aguardando",
          detalhes: personalizacao.texto || "Sem detalhes",
          arquivoNome: personalizacao.arquivoNome,
          valor: null,
          criadoEm: dinheiro.agoraSP()
        });
      });
      location.href = ui.url("paginas/conta.html");
    });
    document.getElementById("frete-ficha").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var cep = new FormData(evento.target).get("cep");
      var cotacao = CriaCalculos.cotarFrete(cep, config().faixasCep);
      var caixa = document.getElementById("frete-resultado");
      if (cotacao.erro) { caixa.innerHTML = '<p class="erro">' + ui.esc(cotacao.erro) + "</p>"; return; }
      var gratis = produto.preco >= config().freteGratisAPartir;
      caixa.innerHTML = cotacao.opcoes.map(function (opcao) {
        return "<p><strong>" + opcao.nome + "</strong> · " + (gratis ? "grátis" : dinheiro.real(opcao.preco)) + " · " + opcao.dias + " dias úteis</p>";
      }).join("");
    });
    document.getElementById("nova-pergunta").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var texto = new FormData(evento.target).get("texto");
      Cria.alterar(function (atual) {
        atual.perguntas.push({ id: CriaCarrinho.novoId("per"), produtoId: produto.id, autor: atual.cliente.nome || "Cliente", texto: String(texto), resposta: "", criadoEm: dinheiro.agoraSP() });
      });
      location.reload();
    });
    ui.revelar(raiz);
  }
  function sobre() {
    document.title = "Sobre · Cria Lab DM";
    var id = estado().identidade;
    var parceiros = estado().parceiros.filter(function (item) { return item.podeAparecer; }).map(function (item) {
      var marca = item.logo ? '<img src="' + ui.url(item.logo) + '" alt="' + ui.esc(item.nome) + '">' : "<span>" + ui.esc(item.nome) + "</span>";
      return '<figure class="parceiro revela m-0 flex min-h-[100px] items-center justify-center rounded-[18px] border-2 border-linha bg-white px-4 py-3">' + marca + "</figure>";
    }).join("");
    var local = (id.endereco ? "<p class=\"m-0\">" + ui.esc(id.endereco) + "</p>" : "") + (id.horario ? "<p class=\"m-0\">" + ui.esc(id.horario) + "</p>" : "");
    var ajuda = (estado().menusAjuda || {}).sobre || "";
    raiz.innerHTML = '<section class="sobre grid justify-items-center gap-4 py-2 text-center"><div class="passaro-palco relative grid min-h-[300px] w-[min(380px,100%)] items-end justify-items-center"><img class="passaro relative z-[1] w-[min(300px,78vw)]" src="' + ui.url("marca/mascote.png") + '" alt="Mascote da Cria Lab DM"><span class="passaro-sombra"></span></div><div class="sobre-texto revela grid w-[min(720px,100%)] justify-items-center gap-1.5"><img class="letreiro w-[min(280px,80%)] rounded-[18px] bg-white px-4 py-2.5 shadow-cria" src="' + ui.url("marca/logo-colorida.png") + '" alt="Cria Lab"><h1 class="mt-2" title="' + ui.esc(ajuda) + '">' + ui.esc(id.nome) + '</h1><p class="intro-pagina m-0 max-w-xl text-tinta/75">' + ui.esc(ajuda) + '</p><p class="m-0">CNPJ ' + ui.esc(id.cnpj) + '</p><p class="m-0"><a href="mailto:' + ui.esc(id.email) + '">' + ui.esc(id.email) + '</a></p><p class="m-0"><a href="tel:+5527996530954">' + ui.esc(id.telefone) + "</a></p>" + local + '<div class="mt-3.5">' + redes() + "</div></div></section>" + (parceiros ? '<section class="cartao revela mt-4 rounded-[18px] bg-white p-4 shadow-cria"><h2 class="mt-0">Parceiros</h2><div class="parceiros flex flex-wrap items-center justify-center gap-4">' + parceiros + "</div></section>" : "");
    ui.revelar(raiz);
  }

  if (pagina === "home") home();
  if (pagina === "busca") busca();
  if (pagina === "vitrine-produtos" || pagina === "vitrine-pronta-entrega") listaModalidade("pronta-entrega");
  if (pagina === "vitrine-sob-demanda") listaModalidade("sob-demanda");
  if (pagina === "vitrine-personalizados") listaModalidade("personalizado");
  if (pagina === "produto") produtoPagina();
  if (pagina === "sobre") sobre();
})();
