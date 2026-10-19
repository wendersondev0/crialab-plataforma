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
    return '<span class="ico">' + ui.icone(nome) + "</span>";
  }
  function personalizado(produto) {
    return produto.tipo === "personalizado";
  }
  function fichaUrl(produto) {
    return ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id));
  }
  function canais(produto) {
    var id = estado().identidade;
    return '<div class="canais"><a class="botao-linha" href="' + ui.esc(id.shopee) + '" target="_blank" rel="noopener">' + ico("shopee") + 'Shopee</a><a class="botao-linha" href="' + ui.esc(id.mercadoLivre) + '" target="_blank" rel="noopener">' + ico("mercadolivre") + 'Mercado Livre</a><a class="botao" href="' + fichaUrl(produto) + '#frete-ficha">' + ico("caminhao") + "Calcular frete</a></div>";
  }
  function card(produto) {
    var foto = produto.fotos[0];
    var sob = personalizado(produto);
    var tag = sob ? '<span class="tag tag-pers">' + ico("documento") + "Personalizado</span>" : '<span class="tag">' + ico("carrinho") + "Produto</span>";
    var comparativo = !sob && produto.precoComparativo ? "<s>" + dinheiro.real(produto.precoComparativo) + "</s> " : "";
    var preco = sob
      ? '<p class="preco">' + dinheiro.real(produto.preco) + '</p><p class="vista">Preço médio · o valor final sai depois do orçamento</p>'
      : '<p class="preco">' + comparativo + dinheiro.real(produto.preco) + '</p><p class="vista">À vista ' + dinheiro.real(precoVista(produto.preco)) + "</p>";
    return '<article class="produto"><a class="produto-foto" href="' + fichaUrl(produto) + '"><img src="' + ui.url(foto.arquivo) + '" alt="' + ui.esc(foto.legenda) + '"></a><div class="produto-corpo">' + tag + '<p class="discreto">' + ui.esc(ui.nomeCategoria(produto.categoria)) + '</p><h3><a href="' + fichaUrl(produto) + '">' + ui.esc(produto.titulo) + "</a></h3>" + preco + canais(produto) + "</div></article>";
  }
  function trilho(titulo, lista) {
    if (!lista.length) return "";
    return "<section><h2>" + ui.esc(titulo) + "</h2><div class=\"trilho\">" + lista.map(card).join("") + "</div></section>";
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
  function carrossel() {
    var gratis = dinheiro.real(config().freteGratisAPartir);
    var slides = [
      { titulo: "Peças prontas da oficina", texto: "Preço fixo, do jeito que a peça saiu da mesa.", fundo: "laranja" },
      { titulo: "Personalizados sob encomenda", texto: "O card mostra a média. O valor fecha no orçamento.", fundo: "teal" },
      { titulo: "Frete grátis a partir de " + gratis, texto: "PAC e SEDEX simulados pelo CEP, em cada produto.", fundo: "tinta" }
    ];
    return '<section class="carrossel" id="carrossel"><div class="carrossel-trilho">' + slides.map(function (slide) {
      return '<article class="slide fundo-' + slide.fundo + '"><div><p class="selo">Cria Lab DM</p><h2>' + ui.esc(slide.titulo) + "</h2><p>" + ui.esc(slide.texto) + '</p></div><img src="' + ui.url("marca/mascote.png") + '" alt=""></article>';
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
    var lista = produtosVisiveis().slice().sort(function (a, b) {
      return (a.tipo === "personalizado") - (b.tipo === "personalizado");
    });
    var categorias = estado().categorias.map(function (categoria) {
      return '<a class="chip" href="' + ui.url("paginas/busca.html?categoria=" + encodeURIComponent(categoria.id)) + '">' + ui.esc(categoria.nome) + "</a>";
    }).join("");
    raiz.innerHTML = carrossel() + '<section class="faixa-categorias"><div class="chips">' + categorias + '</div></section><section><div class="linha"><h2>Na loja</h2><a class="botao-linha" href="' + ui.url("paginas/produtos.html") + '">Produtos</a><a class="botao-linha" href="' + ui.url("paginas/personalizados.html") + '">Personalizados</a></div><div class="grade vitrine">' + lista.map(card).join("") + "</div></section>";
    ligarCarrossel();
  }
  function listaTipo(tipo) {
    var sob = tipo === "personalizado";
    document.title = (sob ? "Personalizados" : "Produtos") + " · Cria Lab DM";
    var lista = produtosVisiveis().filter(function (produto) {
      return sob ? personalizado(produto) : !personalizado(produto);
    });
    raiz.innerHTML = "<h1>" + (sob ? "Personalizados" : "Produtos") + "</h1><p class=\"discreto\">" + (sob ? "O preço do card é a média. O valor final sai depois do orçamento." : "Peças prontas, com preço fixo.") + "</p>" + (lista.length ? '<div class="grade vitrine">' + lista.map(card).join("") + "</div>" : ui.vazio("Nada publicado nesta lista."));
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
    raiz.innerHTML = "<h1>Busca</h1>" + ui.vazio("Olhando a mesa da oficina...") ;
    raiz.querySelector(".vazio").classList.add("carregando");
    window.setTimeout(function () {
      var lista = filtrar(consulta);
      var grade = lista.length ? '<div class="grade">' + lista.map(card).join("") + "</div>" : ui.vazio("Nenhuma peça com esses filtros.");
      raiz.innerHTML = "<h1>Busca</h1>" + htmlFiltros(consulta) + grade;
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
      return;
    }
    if (produto.situacao === "pausado") {
      raiz.innerHTML = ui.vazio("Esta peça está pausada.");
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
    var esgotado = !sobMedida && (produto.situacao === "esgotado" || produto.estoque <= 0);
    var compraPeca = sobMedida ? "" : '<button class="botao" type="button" id="comprar-peca"' + (esgotado ? " disabled" : "") + ">" + ico("carrinho") + (esgotado ? "Esgotado" : "Comprar") + "</button>";
    var whats = estado().identidade.whatsapp + "?text=" + encodeURIComponent("Olá, quero saber sobre " + produto.titulo);
    var tag = sobMedida ? '<span class="tag tag-pers">Personalizado</span>' : '<span class="tag">Produto</span>';
    var precoBloco = sobMedida
      ? '<p class="preco">' + dinheiro.real(produto.preco) + '</p><p class="vista">Preço médio. O valor final sai depois do orçamento.</p>'
      : '<p class="preco">' + dinheiro.real(produto.preco) + '</p><p class="vista">À vista ' + dinheiro.real(precoVista(produto.preco)) + " no PIX ou boleto</p><p class=\"discreto\">" + config().parcelas + "x de " + dinheiro.real(produto.preco / config().parcelas) + " no cartão, sem o desconto à vista</p>";
    var estoqueLinha = sobMedida ? "Sob encomenda" : (esgotado ? "Sem unidade pronta" : ui.esc(String(produto.estoque)) + " em estoque");
    var favoritoAtivo = estado().favoritos.indexOf(produto.id) >= 0;
    var lojas = '<div class="lojas-ficha"><a class="botao-linha" href="' + ui.esc(estado().identidade.shopee) + '" target="_blank" rel="noopener">' + ico("shopee") + 'Shopee</a><a class="botao-linha" href="' + ui.esc(estado().identidade.mercadoLivre) + '" target="_blank" rel="noopener">' + ico("mercadolivre") + "Mercado Livre</a></div>";
    var orcamento = (produto.personalizavel || sobMedida) ? '<button class="botao-teal" type="button" id="pedir-orcamento">' + ico("documento") + "Pedir orçamento</button>" : "";
    var acoes = '<div class="acoes-ficha">' + compraPeca + '<div class="acoes-dupla"><button class="botao-linha" type="button" id="favorito">' + ico("coracao") + '<span id="favorito-rotulo">' + (favoritoAtivo ? "Favorito" : "Favoritar") + '</span></button><a class="botao-linha" href="' + ui.esc(whats) + '" target="_blank" rel="noopener">' + ico("whatsapp") + "WhatsApp</a></div>" + orcamento + "</div>";
    raiz.innerHTML = '<div class="ficha"><div class="galeria"><img class="galeria-principal" id="foto-principal" src="' + ui.url(fotoInicial.arquivo) + '" alt="' + ui.esc(fotoInicial.legenda) + '"><div class="miniaturas">' + miniaturas + '</div><p class="discreto" id="legenda-foto">' + ui.esc(fotoInicial.legenda) + " · sessão " + ui.esc(dinheiro.dataBR(produto.sessao)) + '</p></div><div class="cartao ficha-info">' + tag + '<p class="discreto">' + ui.esc(ui.nomeCategoria(produto.categoria)) + "</p><h1>" + ui.esc(produto.titulo) + "</h1>" + precoBloco + '<p class="meta-peca"><span>' + estoqueLinha + "</span><span>" + ui.esc(produto.peso) + " g</span><span>" + ui.esc(produto.medidas) + "</span></p><p>" + ui.esc(produto.descricao) + "</p>" + lojas + '<section class="bloco-ficha"><h2>Filamento</h2><div class="amostras" id="amostras">' + amostras + '</div><p id="cor-nome">' + ui.esc(produto.cores[0].nome) + '</p><div class="linha" id="tamanhos">' + tamanhos + "</div></section>" + (etapas ? '<section class="bloco-ficha"><h2>Personalização</h2><div class="formulario">' + etapas + "</div></section>" : "") + '<label class="qtd-ficha">Quantidade<input id="qtd" type="number" min="1" value="1"></label>' + acoes + '<section class="frete-caixa"><h2 id="frete-titulo">Calcular frete</h2><form id="frete-ficha" class="frete-form"><input name="cep" type="text" inputmode="numeric" placeholder="00000-000" maxlength="9" aria-label="CEP"><button class="botao" type="submit">' + ico("caminhao") + 'Cotar</button></form><div id="frete-resultado"></div></section></div></div><section class="grade-3 painel-ficha"><article class="cartao"><h2>Perguntas</h2>' + perguntas + '<form id="nova-pergunta" class="formulario"><label>Sua pergunta<input name="texto" type="text" required placeholder="Escreva sua pergunta"></label><button class="botao" type="submit">' + ico("contato") + "Enviar</button></form></article><article class=\"cartao\"><h2>Avaliações</h2>" + avaliacoes + "</article></section>" + trilho("Relacionados", relacionados);
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
      if (produto.tipo !== "personalizado" && qtd > produto.estoque) {
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
      document.getElementById("favorito-rotulo").textContent = estado().favoritos.indexOf(produto.id) >= 0 ? "Favorito" : "Favoritar";
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
  }
  function sobre() {
    document.title = "Sobre · Cria Lab DM";
    var id = estado().identidade;
    var depoimentos = estado().depoimentos.filter(function (item) { return item.podeAparecer; }).map(function (item) {
      return "<blockquote><p>" + ui.esc(item.texto) + "</p><strong>" + ui.esc(item.autor) + "</strong></blockquote>";
    }).join("");
    var parceiros = estado().parceiros.filter(function (item) { return item.podeAparecer; }).map(function (item) {
      return "<span class=\"chip\">" + ui.esc(item.nome) + "</span>";
    }).join("");
    raiz.innerHTML = '<section class="hero"><div><img class="letreiro" src="' + ui.url("marca/logo-completa.jpeg") + '" alt=""><h1>' + ui.esc(id.nome) + '</h1><p>CNPJ ' + ui.esc(id.cnpj) + "</p><p>" + ui.esc(id.email) + "</p><p>" + ui.esc(id.telefone) + "</p>" + (id.endereco ? "<p>" + ui.esc(id.endereco) + "</p>" : "") + (id.horario ? "<p>" + ui.esc(id.horario) + "</p>" : "") + redes() + "</div><img class=\"mascote\" src=\"" + ui.url("marca/mascote.png") + '" alt=""></section>' + (parceiros ? '<section class="cartao"><h2>Parceiros</h2><div class="chips">' + parceiros + "</div></section>" : "") + (depoimentos ? '<section class="cartao"><h2>Depoimentos</h2>' + depoimentos + "</section>" : "") + '<section class="cartao"><h2>Template de postagem</h2><img src="' + ui.url("marca/template-de-postagem.png") + '" alt="Template de postagem da Cria Lab DM"></section>';
  }

  if (pagina === "home") home();
  if (pagina === "busca") busca();
  if (pagina === "vitrine-produtos") listaTipo("pronto");
  if (pagina === "vitrine-personalizados") listaTipo("personalizado");
  if (pagina === "produto") produtoPagina();
  if (pagina === "sobre") sobre();
})();
