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
    var id = estado().identidade || {};
    var links = [];
    if (id.shopee) {
      links.push('<a class="canal-ficha-link" href="' + ui.esc(id.shopee) + '" target="_blank" rel="noopener">' + ico("shopee") + "<span>Shopee</span></a>");
    }
    if (id.mercadoLivre) {
      links.push('<a class="canal-ficha-link" href="' + ui.esc(id.mercadoLivre) + '" target="_blank" rel="noopener">' + ico("mercadolivre") + "<span>Mercado Livre</span></a>");
    }
    if (!links.length) return "";
    return '<aside class="canais-ficha" aria-label="Outros canais de venda"><p class="canais-ficha-rotulo">Disponível também em:</p><div class="canais-ficha-links">' + links.join("") + "</div></aside>";
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
  function semAcento(texto) {
    return String(texto || "").normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
  }
  function textoProduto(produto) {
    return semAcento([produto.titulo, produto.descricao, produto.material, ui.nomeCategoria(produto.categoria)].join(" "));
  }
  function ligarSugestoes() {
    var campo = document.getElementById("q");
    var caixa = document.getElementById("sugestoes");
    if (!campo || !caixa || campo.dataset.ligado) return;
    campo.dataset.ligado = "1";
    function fechar() {
      caixa.hidden = true;
      caixa.innerHTML = "";
      campo.setAttribute("aria-expanded", "false");
    }
    campo.addEventListener("input", function () {
      var texto = semAcento(campo.value.trim());
      if (texto.length < 2) { fechar(); return; }
      var lista = produtosVisiveis().filter(function (produto) {
        return textoProduto(produto).indexOf(texto) >= 0;
      }).slice(0, 6);
      if (!lista.length) { fechar(); return; }
      caixa.hidden = false;
      campo.setAttribute("aria-expanded", "true");
      caixa.innerHTML = lista.map(function (produto) {
        var foto = produto.fotos && produto.fotos[0] ? produto.fotos[0].arquivo : "marca/mascote.png";
        return '<a href="' + fichaUrl(produto) + '"><img src="' + ui.url(foto) + '" alt=""><span><strong>' + ui.esc(produto.titulo) + "</strong><em>" + ui.esc(dinheiro.real(produto.preco)) + " · " + ui.esc(ui.nomeCategoria(produto.categoria)) + "</em></span></a>";
      }).join("");
    });
    campo.addEventListener("keydown", function (evento) {
      if (evento.key === "Escape") fechar();
    });
    document.addEventListener("click", function (evento) {
      if (evento.target === campo || caixa.contains(evento.target)) return;
      fechar();
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
  function flagConsulta(valor) {
    return ["1", "true", "on", "sim"].indexOf(String(valor || "").trim().toLowerCase()) >= 0;
  }
  function numeroPreco(valor) {
    if (valor == null || String(valor).trim() === "") return null;
    var t = String(valor).trim().replace(/\s/g, "").replace(/R\$/ig, "");
    if (t.indexOf(",") >= 0 && t.indexOf(".") >= 0) t = t.replace(/\./g, "").replace(",", ".");
    else t = t.replace(",", ".");
    var n = Number(t);
    return isFinite(n) && n >= 0 ? n : null;
  }
  function campoPreco(valor) {
    if (valor == null || valor === "") return "";
    return Number(valor).toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }
  function lerConsulta() {
    var q = new URLSearchParams(location.search);
    var aviso = "";
    var catId = (q.get("categoria") || "").trim();
    var categorias = estado().categorias || [];
    var cat = categorias.find(function (item) { return String(item.id).toLowerCase() === catId.toLowerCase(); });
    if (catId && !cat) {
      aviso = "A categoria desse link não existe mais. Mostrando todas.";
      catId = "";
    } else if (cat) catId = cat.id;
    var min = numeroPreco(q.get("precoMin"));
    var max = numeroPreco(q.get("precoMax"));
    if ((q.get("precoMin") && min == null) || (q.get("precoMax") && max == null))
      aviso = "Use apenas números no preço. Exemplo: 25 ou 25,90.";
    if (min != null && max != null && min > max) {
      var troca = min;
      min = max;
      max = troca;
      aviso = "O preço mínimo estava acima do máximo. A faixa foi invertida.";
    }
    var ordem = (q.get("ordem") || "relevancia").trim().toLowerCase();
    if (["preco", "preco-desc", "vendas", "novidade"].indexOf(ordem) < 0) ordem = "relevancia";
    var nota = (q.get("nota") || "").trim().replace(",", ".");
    nota = nota === "4" || nota === "4.0" ? "4" : (nota === "5" || nota === "5.0" ? "5" : "");
    return {
      q: (q.get("q") || "").trim(),
      categoria: catId,
      precoMin: min == null ? "" : min.toFixed(2),
      precoMax: max == null ? "" : max.toFixed(2),
      freteGratis: flagConsulta(q.get("freteGratis")),
      aVista: flagConsulta(q.get("aVista")),
      personalizavel: flagConsulta(q.get("personalizavel")),
      nota: nota,
      ordem: ordem,
      aviso: aviso
    };
  }
  function temFiltro(consulta) {
    return !!(consulta.q || consulta.categoria || consulta.precoMin || consulta.precoMax || consulta.freteGratis || consulta.aVista || consulta.personalizavel || consulta.nota || consulta.ordem !== "relevancia");
  }
  function hrefBusca(consulta, sem) {
    var params = new URLSearchParams();
    function por(nome, valor) {
      if (!valor || sem === "*" || sem === nome) return;
      if (sem === "preco" && (nome === "precoMin" || nome === "precoMax")) return;
      if (nome === "ordem" && valor === "relevancia") return;
      params.set(nome, valor);
    }
    por("q", consulta.q);
    por("categoria", consulta.categoria);
    por("precoMin", consulta.precoMin);
    por("precoMax", consulta.precoMax);
    por("ordem", consulta.ordem);
    por("nota", consulta.nota);
    if (consulta.freteGratis) por("freteGratis", "1");
    if (consulta.aVista) por("aVista", "1");
    if (consulta.personalizavel) por("personalizavel", "1");
    var s = params.toString();
    return ui.url("paginas/busca.html" + (s ? "?" + s : ""));
  }
  function filtrar(consulta) {
    var limite = config().freteGratisAPartir;
    var texto = semAcento(consulta.q);
    var min = consulta.precoMin === "" ? null : Number(consulta.precoMin);
    var max = consulta.precoMax === "" ? null : Number(consulta.precoMax);
    var lista = produtosVisiveis().filter(function (produto) {
      if (texto && textoProduto(produto).indexOf(texto) < 0) return false;
      if (consulta.categoria && String(produto.categoria).toLowerCase() !== consulta.categoria.toLowerCase()) return false;
      if (min != null && produto.preco < min) return false;
      if (max != null && produto.preco > max) return false;
      if (consulta.freteGratis && produto.preco < limite) return false;
      if (consulta.aVista && !produto.aVista) return false;
      if (consulta.personalizavel && !produto.personalizavel) return false;
      if (consulta.nota && (produto.nota || 0) < Number(consulta.nota)) return false;
      return true;
    });
    var ordem = consulta.ordem;
    lista.sort(function (a, b) {
      var titulo = String(a.titulo).localeCompare(String(b.titulo), "pt");
      if (ordem === "preco") return a.preco - b.preco || titulo;
      if (ordem === "preco-desc") return b.preco - a.preco || titulo;
      if (ordem === "vendas") return b.vendas - a.vendas || titulo;
      if (ordem === "novidade") return String(b.criadoEm).localeCompare(String(a.criadoEm)) || titulo;
      var pa = texto && semAcento(a.titulo).indexOf(texto) >= 0 ? 2 : 1;
      var pb = texto && semAcento(b.titulo).indexOf(texto) >= 0 ? 2 : 1;
      return pb - pa || b.vendas - a.vendas || titulo;
    });
    return lista;
  }
  function htmlFiltros(consulta) {
    function marcado(nome) { return consulta[nome] ? " checked" : ""; }
    function sel(valor) { return consulta.ordem === valor ? " selected" : ""; }
    var categorias = '<option value="">Todas</option>' + (estado().categorias || []).map(function (categoria) {
      return '<option value="' + ui.esc(categoria.id) + '"' + (consulta.categoria === categoria.id ? " selected" : "") + ">" + ui.esc(categoria.nome) + "</option>";
    }).join("");
    var aviso = consulta.aviso ? '<p class="busca-aviso" role="status">' + ui.esc(consulta.aviso) + "</p>" : "";
    var limpar = temFiltro(consulta) ? '<a class="botao botao-linha" href="' + hrefBusca(consulta, "*") + '">Limpar</a>' : "";
    return [
      '<form id="filtros" class="busca-painel" autocomplete="off">',
      aviso,
      '<label class="busca-rotulo busca-texto">Texto<span class="busca-caixa">' + ico("busca") + '<input type="search" name="q" id="q" value="' + ui.esc(consulta.q) + '" placeholder="Nome, material ou categoria" autocomplete="off" enterkeyhint="search" aria-autocomplete="list" aria-controls="sugestoes" aria-expanded="false"><div id="sugestoes" class="busca-sugestoes" hidden></div></span></label>',
      '<div class="busca-grade">',
      '<label class="busca-rotulo">Categoria<select name="categoria">' + categorias + "</select></label>",
      '<label class="busca-rotulo">Preço mínimo<input name="precoMin" type="text" inputmode="decimal" value="' + ui.esc(campoPreco(consulta.precoMin)) + '" placeholder="0,00"></label>',
      '<label class="busca-rotulo">Preço máximo<input name="precoMax" type="text" inputmode="decimal" value="' + ui.esc(campoPreco(consulta.precoMax)) + '" placeholder="0,00"></label>',
      '<label class="busca-rotulo">Nota mínima<select name="nota"><option value="">Qualquer</option><option value="4"' + (consulta.nota === "4" ? " selected" : "") + '>4 ou mais</option><option value="5"' + (consulta.nota === "5" ? " selected" : "") + ">5</option></select></label>",
      '<label class="busca-rotulo">Ordenar<select name="ordem"><option value="relevancia"' + sel("relevancia") + '>Relevância</option><option value="preco"' + sel("preco") + '>Menor preço</option><option value="preco-desc"' + sel("preco-desc") + '>Maior preço</option><option value="vendas"' + sel("vendas") + '>Mais vendidos</option><option value="novidade"' + sel("novidade") + ">Novidades</option></select></label>",
      "</div>",
      '<div class="busca-opcoes" role="group" aria-labelledby="busca-opcoes-titulo"><p id="busca-opcoes-titulo" class="busca-kicker">Opções</p>',
      '<label class="busca-check"><input type="checkbox" name="freteGratis" value="1"' + marcado("freteGratis") + "><span>Frete grátis</span></label>",
      '<label class="busca-check"><input type="checkbox" name="aVista" value="1"' + marcado("aVista") + "><span>À vista</span></label>",
      '<label class="busca-check"><input type="checkbox" name="personalizavel" value="1"' + marcado("personalizavel") + "><span>Personalizável</span></label>",
      "</div>",
      '<p class="busca-dica">Frete grátis vale para peças a partir de ' + ui.esc(dinheiro.real(config().freteGratisAPartir)) + ".</p>",
      '<div class="busca-acoes">' + limpar + '<button class="botao" type="submit">Aplicar filtros</button></div>',
      "</form>"
    ].join("");
  }
  function htmlAtivos(consulta) {
    if (!temFiltro(consulta)) return "";
    var chips = [];
    function chip(rotulo, sem) {
      chips.push('<a class="busca-chip" href="' + hrefBusca(consulta, sem) + '" aria-label="Remover filtro ' + ui.esc(rotulo) + '">' + ui.esc(rotulo) + ' <span aria-hidden="true">×</span></a>');
    }
    if (consulta.q) chip("Texto: " + consulta.q, "q");
    if (consulta.categoria) chip(ui.nomeCategoria(consulta.categoria), "categoria");
    if (consulta.precoMin && consulta.precoMax) chip(dinheiro.real(consulta.precoMin) + " – " + dinheiro.real(consulta.precoMax), "preco");
    else if (consulta.precoMin) chip("A partir de " + dinheiro.real(consulta.precoMin), "precoMin");
    else if (consulta.precoMax) chip("Até " + dinheiro.real(consulta.precoMax), "precoMax");
    if (consulta.freteGratis) chip("Frete grátis", "freteGratis");
    if (consulta.aVista) chip("À vista", "aVista");
    if (consulta.personalizavel) chip("Personalizável", "personalizavel");
    if (consulta.nota === "4") chip("Nota 4 ou mais", "nota");
    if (consulta.nota === "5") chip("Nota 5", "nota");
    var ordens = { preco: "Menor preço", "preco-desc": "Maior preço", vendas: "Mais vendidos", novidade: "Novidades" };
    if (ordens[consulta.ordem]) chip(ordens[consulta.ordem], "ordem");
    chips.push('<a class="busca-chip busca-chip-limpar" href="' + hrefBusca(consulta, "*") + '">Limpar tudo</a>');
    return '<div class="busca-ativos" aria-label="Filtros aplicados">' + chips.join("") + "</div>";
  }
  function busca() {
    document.title = "Busca · Cria Lab DM";
    var consulta = lerConsulta();
    var ajuda = (estado().menusAjuda || {}).busca || "Filtre peças por texto, tema e preço.";
    raiz.innerHTML = '<header class="busca-cab"><div><h1 class="mt-0">Busca</h1><p class="intro-pagina m-0 max-w-3xl text-tinta/75">' + ui.esc(ajuda) + "</p></div></header>" + ui.vazio("Olhando a mesa da oficina...");
    raiz.querySelector(".vazio").classList.add("carregando");
    window.setTimeout(function () {
      var lista = filtrar(consulta);
      var n = lista.length;
      var resumo = (n === 1 ? "1 peça" : n + " peças");
      var grade = lista.length
        ? '<div class="grade">' + lista.map(card).join("") + "</div>"
        : '<div class="busca-vazio cartao"><img src="' + ui.url("marca/mascote.png") + '" alt="" width="120" height="120"><p>' + (temFiltro(consulta) ? "Nenhuma peça com esses filtros." : "A vitrine ainda não tem peças publicadas.") + "</p>" + (temFiltro(consulta) ? '<a class="botao botao-linha" href="' + hrefBusca(consulta, "*") + '">Limpar filtros</a>' : "") + "</div>";
      if (consulta.q) document.title = "Busca: " + consulta.q + " · Cria Lab DM";
      raiz.innerHTML = '<header class="busca-cab"><div><h1 class="mt-0">Busca</h1><p class="intro-pagina m-0 max-w-3xl text-tinta/75">' + ui.esc(ajuda) + '</p></div><p class="busca-contagem">' + resumo + "</p></header>" + htmlFiltros(consulta) + htmlAtivos(consulta) + grade;
      document.getElementById("filtros").addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        var params = new URLSearchParams();
        ["q", "categoria", "precoMin", "precoMax", "ordem", "nota"].forEach(function (nome) {
          var valor = String(dados.get(nome) || "").trim();
          if (nome === "precoMin" || nome === "precoMax") {
            valor = valor.replace(/\s/g, "").replace(/R\$/ig, "");
            if (valor.indexOf(",") >= 0 && valor.indexOf(".") >= 0) valor = valor.replace(/\./g, "").replace(",", ".");
            else valor = valor.replace(",", ".");
          }
          if (!valor || (nome === "ordem" && valor === "relevancia")) return;
          params.set(nome, valor);
        });
        ["freteGratis", "aVista", "personalizavel"].forEach(function (nome) {
          if (dados.get(nome)) params.set(nome, "1");
        });
        location.href = ui.url("paginas/busca.html" + (params.toString() ? "?" + params.toString() : ""));
      });
      ligarSugestoes();
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
    if (Cria.normalizarVariacoesProduto) Cria.normalizarVariacoesProduto(produto);
    var eixos = Array.isArray(produto.variacoes) ? produto.variacoes : [];
    var escolhas = {};
    eixos.forEach(function (eixo) {
      escolhas[eixo.id] = eixo.opcoes[0] ? eixo.opcoes[0].nome : "";
    });
    var htmlVariacoes = eixos.length ? '<section class="bloco-ficha grid gap-3" id="variacoes-ficha">' + eixos.map(function (eixo) {
      var opcoesHtml = eixo.tipo === "cor"
        ? '<div class="amostras">' + eixo.opcoes.map(function (opcao, indice) {
          return '<button type="button" class="amostra' + (indice === 0 ? " ativa" : "") + '" style="background:' + ui.esc(opcao.hex || "#cccccc") + '" data-eixo="' + ui.esc(eixo.id) + '" data-opcao="' + ui.esc(opcao.nome) + '" title="' + ui.esc(opcao.nome) + '"></button>';
        }).join("") + "</div>"
        : '<div class="linha opcoes-texto">' + eixo.opcoes.map(function (opcao, indice) {
          return '<button type="button" class="botao-linha' + (indice === 0 ? " ativo" : "") + '" data-eixo="' + ui.esc(eixo.id) + '" data-opcao="' + ui.esc(opcao.nome) + '">' + ui.esc(opcao.nome) + "</button>";
        }).join("") + "</div>";
      return '<div class="eixo-variacao grid gap-2" data-eixo-bloco="' + ui.esc(eixo.id) + '"><h2 class="mt-0 text-lg font-extrabold">' + ui.esc(eixo.nome) + "</h2>" + opcoesHtml + '<p class="opcao-escolhida m-0 text-tinta/80" data-rotulo="' + ui.esc(eixo.id) + '">' + ui.esc(escolhas[eixo.id] || "") + "</p></div>";
    }).join("") + "</section>" : "";
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
    var desabilitadoCompra = esgotado ? " disabled" : "";
    var compraPeca = sobMedida ? "" : '<div class="acoes-compra">' +
      '<button class="botao btn-comprar-agora" type="button" id="comprar-peca"' + desabilitadoCompra + ">" + ico("caixa") + (esgotado ? "Esgotado" : "Comprar agora") + "</button>" +
      '<button class="botao-linha btn-add-carrinho" type="button" id="add-carrinho"' + desabilitadoCompra + ">" + ico("carrinho") + "Adicionar ao carrinho</button>" +
      "</div>";
    var whats = estado().identidade.whatsapp + "?text=" + encodeURIComponent("Olá, quero saber sobre " + produto.titulo);
    var tagCor = mod === "personalizado" ? "tag-pers bg-laranja" : (mod === "sob-demanda" ? "bg-tinta" : "bg-teal");
    var tag = '<span class="tag ' + tagCor + ' inline-flex w-fit items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-extrabold text-white" title="' + ui.esc((ui.modalidadeInfo(mod) || {}).resumo || "") + '">' + ui.esc(ui.nomeModalidade(mod)) + "</span>";
    var desc = dadosDesconto(produto);
    var precoBloco = sobMedida
      ? '<div class="preco-bloco"><p class="preco m-0 text-2xl font-extrabold">' + dinheiro.real(produto.preco) + '</p><p class="vista m-0 font-bold text-teal-escuro">Preço médio. O valor final sai depois do orçamento.</p><p class="entrega-card m-0 text-sm text-tinta">' + htmlEntrega(produto) + "</p></div>"
      : '<div class="preco-bloco">' + (desc ? tagOferta(produto) : "") + '<div class="preco-linha flex flex-wrap items-end gap-2"><p class="preco m-0 text-2xl font-extrabold">' + dinheiro.real(produto.preco) + '</p>' + (desc ? '<s class="preco-riscado text-base font-medium text-tinta/45">' + dinheiro.real(desc.anterior) + "</s>" : "") + '</div><p class="vista m-0 font-bold text-teal-escuro">À vista ' + dinheiro.real(precoVista(produto.preco)) + " no PIX ou boleto</p><p class=\"discreto m-0 text-tinta/60\">" + config().parcelas + "x de " + dinheiro.real(produto.preco / config().parcelas) + " no cartão, sem o desconto à vista</p><p class=\"entrega-card m-0 text-sm text-tinta\">" + htmlEntrega(produto) + "</p></div>";
    var estoqueLinha = sobMedida ? "Sob encomenda" : (mod === "sob-demanda" ? "Impressão após o pedido" : (esgotado ? "Sem unidade pronta" : ui.esc(String(produto.estoque)) + " em estoque"));
    var favoritoAtivo = estado().favoritos.indexOf(produto.id) >= 0;
    var lojas = canaisFicha();
    var orcamento = (produto.personalizavel || sobMedida) ? '<button class="botao-teal inline-flex w-full min-h-11 items-center justify-center gap-2 rounded-full border-2 border-teal bg-teal px-4 font-bold text-white" type="button" id="pedir-orcamento">' + ico("documento") + "Pedir orçamento</button>" : "";
    var qtdBloco = sobMedida ? "" : '<div class="qtd-ficha-box"><span class="qtd-ficha-rotulo">Quantidade</span><div class="qtd-stepper"><button type="button" id="qtd-menos" aria-label="Diminuir quantidade">−</button><input id="qtd" type="number" min="1" value="1" aria-label="Quantidade"><button type="button" id="qtd-mais" aria-label="Aumentar quantidade">+</button></div></div>';
    var acoes = '<div class="acoes-ficha">' + qtdBloco + compraPeca + '<div class="acoes-dupla"><button class="btn-favorito-ficha' + (favoritoAtivo ? " ativo" : "") + '" type="button" id="favorito" aria-pressed="' + (favoritoAtivo ? "true" : "false") + '">' + ico("coracao") + '<span id="favorito-rotulo">' + (favoritoAtivo ? "Nos favoritos" : "Salvar nos favoritos") + '</span></button><a class="botao-linha btn-whats-ficha" href="' + ui.esc(whats) + '" target="_blank" rel="noopener">' + ico("whatsapp") + "WhatsApp</a></div>" + orcamento + lojas + "</div>";
    raiz.innerHTML = '<div class="ficha grid grid-cols-1 items-start gap-5 lg:grid-cols-2"><div class="galeria revela"><img class="galeria-principal rounded-[18px]" id="foto-principal" src="' + ui.url(fotoInicial.arquivo) + '" alt="' + ui.esc(fotoInicial.legenda) + '"><div class="miniaturas">' + miniaturas + '</div><p class="discreto" id="legenda-foto">' + ui.esc(fotoInicial.legenda) + " · sessão " + ui.esc(dinheiro.dataBR(produto.sessao)) + '</p></div><div class="cartao ficha-info revela">' + tag + '<p class="ficha-categoria discreto m-0">' + ui.esc(ui.nomeCategoria(produto.categoria)) + "</p><h1 class=\"ficha-titulo mt-0\">" + ui.esc(produto.titulo) + "</h1>" + precoBloco + '<p class="meta-peca m-0"><span>' + estoqueLinha + "</span><span>" + ui.esc(produto.peso) + " g</span><span>" + ui.esc(produto.medidas) + "</span></p><p class=\"ficha-desc m-0\">" + ui.esc(produto.descricao) + "</p>" + htmlVariacoes + (etapas ? '<section class="bloco-ficha"><h2>Personalização</h2><div class="formulario">' + etapas + "</div></section>" : "") + acoes + '<section class="frete-caixa"><h2 id="frete-titulo">Calcular frete</h2><form id="frete-ficha" class="frete-form"><input name="cep" type="text" inputmode="numeric" placeholder="00000-000" maxlength="9" aria-label="CEP"><button class="botao" type="submit">' + ico("caminhao") + 'Cotar</button></form><div id="frete-resultado"></div></section></div></div><div id="toast-carrinho" class="toast-carrinho" hidden role="status"></div><section class="painel-ficha mt-5 grid grid-cols-1 gap-4"><article class="cartao rounded-[18px] bg-white p-4 shadow-cria"><h2 class="mt-0 text-xl font-extrabold">Perguntas</h2>' + perguntas + '<form id="nova-pergunta" class="formulario mt-3 grid gap-3"><label class="grid gap-1.5 font-bold">Sua pergunta<input class="min-h-12 w-full rounded-full border-2 border-linha bg-white px-4.5 py-2.5" name="texto" type="text" required placeholder="Escreva sua pergunta"></label><button class="botao inline-flex min-h-11 w-fit items-center justify-center gap-2 rounded-full bg-laranja px-4 font-bold text-white" type="submit">' + ico("contato") + "Enviar</button></form></article><article class=\"cartao rounded-[18px] bg-white p-4 shadow-cria\"><h2 class=\"mt-0 text-xl font-extrabold\">Avaliações</h2>" + avaliacoes + "</article></section>" + trilho("Relacionados", relacionados);
    document.querySelectorAll(".arquivo-ref input").forEach(function (campoArquivo) {
      campoArquivo.addEventListener("change", function () {
        var nome = campoArquivo.files && campoArquivo.files[0] ? campoArquivo.files[0].name : "Nenhuma imagem";
        var alvo = campoArquivo.parentElement.querySelector(".arquivo-nome");
        if (alvo) alvo.textContent = nome;
      });
    });
    function textoEscolhas() {
      return eixos.map(function (eixo) {
        var nome = escolhas[eixo.id];
        return nome ? eixo.nome + ": " + nome : "";
      }).filter(Boolean).join(" · ");
    }
    function assinaturaEscolhas() {
      return eixos.map(function (eixo) { return escolhas[eixo.id] || ""; }).join("|");
    }
    var caixaVariacoes = document.getElementById("variacoes-ficha");
    if (caixaVariacoes) {
      caixaVariacoes.addEventListener("click", function (evento) {
        var botao = evento.target.closest("[data-opcao]");
        if (!botao) return;
        var eixoId = botao.getAttribute("data-eixo");
        var nome = botao.getAttribute("data-opcao");
        escolhas[eixoId] = nome;
        var bloco = caixaVariacoes.querySelector('[data-eixo-bloco="' + eixoId + '"]');
        if (!bloco) return;
        bloco.querySelectorAll("[data-opcao]").forEach(function (el) {
          el.classList.remove("ativa");
          el.classList.remove("ativo");
        });
        if (botao.classList.contains("amostra")) botao.classList.add("ativa");
        else botao.classList.add("ativo");
        var rotulo = bloco.querySelector("[data-rotulo]");
        if (rotulo) rotulo.textContent = nome;
      });
    }
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
    function lerQuantidade() {
      var campo = document.getElementById("qtd");
      return Math.max(1, Number(campo && campo.value) || 1);
    }
    function definirQuantidade(valor) {
      var campo = document.getElementById("qtd");
      if (!campo) return;
      var max = modalidadeDe(produto) === "pronta-entrega" && produto.estoque > 0 ? produto.estoque : 99;
      var n = Math.max(1, Math.min(max, Number(valor) || 1));
      campo.value = String(n);
    }
    var menos = document.getElementById("qtd-menos");
    var mais = document.getElementById("qtd-mais");
    if (menos) menos.addEventListener("click", function () { definirQuantidade(lerQuantidade() - 1); });
    if (mais) mais.addEventListener("click", function () { definirQuantidade(lerQuantidade() + 1); });
    var campoQtd = document.getElementById("qtd");
    if (campoQtd) campoQtd.addEventListener("change", function () { definirQuantidade(campoQtd.value); });
    function montarItemCarrinho() {
      var personalizacao = detalhesPersonalizacao();
      var qtd = lerQuantidade();
      if (modalidadeDe(produto) === "pronta-entrega" && qtd > produto.estoque) {
        alert("A quantidade passa do estoque.");
        return null;
      }
      return {
        produtoId: produto.id,
        titulo: produto.titulo,
        tipo: produto.tipo === "personalizado" ? "personalizado" : "pronto",
        fisico: true,
        arquivo: false,
        quantidade: qtd,
        precoUnitario: produto.preco + personalizacao.extra,
        detalhes: [textoEscolhas(), personalizacao.texto].filter(Boolean).join(" · "),
        arquivoNome: personalizacao.arquivoNome,
        assinatura: produto.tipo === "personalizado" ? CriaCarrinho.novoId("pers") : assinaturaEscolhas()
      };
    }
    function mostrarToastCarrinho() {
      var toast = document.getElementById("toast-carrinho");
      if (!toast) return;
      toast.hidden = false;
      toast.classList.add("visivel");
      toast.innerHTML = "<strong>Adicionado ao carrinho</strong><a href=\"" + ui.url("paginas/carrinho.html") + "\">Ver carrinho</a><a class=\"toast-checkout\" href=\"" + ui.url("paginas/checkout.html") + "\">Finalizar</a>";
      window.clearTimeout(mostrarToastCarrinho._timer);
      mostrarToastCarrinho._timer = window.setTimeout(function () {
        toast.classList.remove("visivel");
        toast.hidden = true;
      }, 4200);
    }
    var comprar = document.getElementById("comprar-peca");
    if (comprar) comprar.addEventListener("click", function () {
      var item = montarItemCarrinho();
      if (!item) return;
      CriaCarrinho.adicionar(item);
      location.href = ui.url("paginas/checkout.html");
    });
    var addCarrinho = document.getElementById("add-carrinho");
    if (addCarrinho) addCarrinho.addEventListener("click", function () {
      var item = montarItemCarrinho();
      if (!item) return;
      CriaCarrinho.adicionar(item);
      mostrarToastCarrinho();
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
          detalhes: [textoEscolhas(), personalizacao.texto].filter(Boolean).join(" · ") || "Sem detalhes",
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
      if (item.url) marca = '<a href="' + ui.esc(item.url) + '" target="_blank" rel="noopener" aria-label="' + ui.esc(item.nome) + '">' + marca + "</a>";
      return '<figure class="parceiro revela m-0 flex min-h-[100px] items-center justify-center rounded-[18px] border-2 border-linha bg-white px-4 py-3">' + marca + "</figure>";
    }).join("");
    var local = (id.endereco ? "<p class=\"m-0\">" + ui.esc(id.endereco) + "</p>" : "") + (id.horario ? "<p class=\"m-0\">" + ui.esc(id.horario) + "</p>" : "");
    var pilares = [
      { ico: "admin", titulo: "Impressão 3D de verdade", texto: "Projetamos e imprimimos na oficina: do protótipo à peça acabada, com material e acabamento combinados com você." },
      { ico: "caixa", titulo: "Pronta entrega", texto: "Chaveiros, decoração e utilidades já saídos da impressora, prontos para enviar ou retirar sem espera de produção." },
      { ico: "caminhao", titulo: "Sob demanda", texto: "O modelo já existe na vitrine; depois do pedido a gente imprime a sua unidade no prazo combinado." },
      { ico: "documento", titulo: "100% personalizado", texto: "Tem ideia, foto, logo ou medida? A gente transforma em peça. Orçamento sem compromisso antes de produzir." }
    ].map(function (item) {
      return '<article class="sobre-pilar revela"><span class="sobre-pilar-ico">' + ico(item.ico) + "</span><h3>" + ui.esc(item.titulo) + "</h3><p>" + ui.esc(item.texto) + "</p></article>";
    }).join("");
    var produtos = [
      ["Decoração", "Quadros, letras, enfeites e peças que dão cara ao ambiente."],
      ["Chaveiros e brindes", "Para presente, empresa, evento ou o dia a dia com a sua cara."],
      ["Utilidades", "Suportes, organizadores e soluções práticas impressas sob medida."],
      ["Moldes e peças técnicas", "Para quem precisa de forma, encaixe ou reposição que não acha pronta."],
      ["Pintura e acabamento", "Quando a peça pede cor, verniz ou aquele detalhe a mais."]
    ].map(function (par) {
      return "<li><strong>" + ui.esc(par[0]) + "</strong><span>" + ui.esc(par[1]) + "</span></li>";
    }).join("");
    var mensagens = [
      { titulo: "Não achou na loja?", texto: "Manda referência no WhatsApp. Se der para imprimir, a gente faz — e se precisar adaptar, a gente adapta." },
      { titulo: "Do rascunho à mão", texto: "Trabalhamos com ideia solta, arquivo 3D ou só uma foto. O importante é você sair com a peça que imaginou." },
      { titulo: "Para pessoa e para negócio", texto: "Presente, uso pessoal, brindes de marca ou peça sob encomenda para a sua empresa: a oficina atende os dois mundos." }
    ].map(function (item) {
      return '<article class="sobre-mensagem revela"><h3>' + ui.esc(item.titulo) + "</h3><p>" + ui.esc(item.texto) + "</p></article>";
    }).join("");
    var whats = (id.whatsapp || "") + "?text=" + encodeURIComponent("Olá! Quero saber mais sobre impressão 3D e peças personalizadas na Cria Lab.");
    raiz.innerHTML =
      '<section class="sobre">' +
        '<div class="passaro-palco"><img class="passaro" src="' + ui.url("marca/mascote.png") + '" alt="Mascote da Cria Lab DM"><span class="passaro-sombra"></span></div>' +
        '<div class="sobre-texto revela">' +
          '<img class="letreiro" src="' + ui.url("marca/logo-colorida.png") + '" alt="Cria Lab">' +
          "<h1>" + ui.esc(id.nome) + "</h1>" +
          '<p class="sobre-slogan">Oficina de impressão 3D que transforma ideia em peça — pronta, sob demanda ou do seu jeito.</p>' +
          '<p class="sobre-lead">A Cria Lab DM nasce da bancada: filamento, impressora e muita vontade de criar. Fazemos decoração, chaveiros, utilidades, moldes e o que mais couber na sua imaginação. Se dá para modelar e imprimir, a gente conversa.</p>' +
          '<div class="sobre-ctas">' +
            '<a class="botao" href="' + ui.url("paginas/produtos.html") + '">' + ico("caixa") + "Ver produtos</a>" +
            '<a class="botao-teal" href="' + ui.url("paginas/personalizados.html") + '">' + ico("admin") + "Peças personalizadas</a>" +
            '<a class="botao-linha" href="' + ui.esc(whats) + '" target="_blank" rel="noopener">' + ico("whatsapp") + "Falar no WhatsApp</a>" +
          "</div>" +
        "</div>" +
      "</section>" +
      '<section class="sobre-bloco revela">' +
        '<div class="sobre-bloco-cab"><h2>O que fazemos</h2><p>Impressão 3D com cara de oficina: do estoque da mesa ao projeto sob encomenda.</p></div>' +
        '<div class="sobre-pilares">' + pilares + "</div>" +
      "</section>" +
      '<section class="sobre-bloco revela">' +
        '<div class="sobre-bloco-cab"><h2>Produtos que saem da nossa impressora</h2><p>Uma vitrine ampla — e espaço para o que ainda não está nela.</p></div>' +
        '<ul class="sobre-produtos">' + produtos + "</ul>" +
      "</section>" +
      '<section class="sobre-bloco revela">' +
        '<div class="sobre-bloco-cab"><h2>Por que pedir na Cria Lab</h2><p>Mensagens diretas para quem quer criar, presentear ou resolver um problema com uma peça impressa.</p></div>' +
        '<div class="sobre-mensagens">' + mensagens + "</div>" +
      "</section>" +
      '<section class="sobre-bloco sobre-contato revela">' +
        '<div class="sobre-bloco-cab"><h2>Fale com a oficina</h2><p>Tire dúvida, peça orçamento ou conte o que você quer criar.</p></div>' +
        '<div class="sobre-contato-grade">' +
          "<p><strong>CNPJ</strong><span>" + ui.esc(id.cnpj) + "</span></p>" +
          '<p><strong>E-mail</strong><a href="mailto:' + ui.esc(id.email) + '">' + ui.esc(id.email) + "</a></p>" +
          '<p><strong>Telefone</strong><a href="tel:+5527996530954">' + ui.esc(id.telefone) + "</a></p>" +
          (local ? '<div class="sobre-local">' + local + "</div>" : "") +
        "</div>" +
        '<div class="sobre-redes">' + redes() + "</div>" +
      "</section>" +
      (parceiros ? '<section class="cartao revela sobre-parceiros"><h2 class="mt-0">Parceiros</h2><p class="sobre-parceiros-intro">Quem caminha com a gente.</p><div class="parceiros">' + parceiros + "</div></section>" : "");
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
