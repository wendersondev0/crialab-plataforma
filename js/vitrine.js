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
  function card(produto) {
    var foto = produto.fotos[0];
    var comparativo = produto.precoComparativo ? "<s>" + dinheiro.real(produto.precoComparativo) + "</s>" : "";
    return '<a class="produto" href="' + ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id)) + '"><img src="' + ui.url(foto.arquivo) + '" alt="' + ui.esc(foto.legenda) + '"><div><p class="discreto">' + ui.esc(ui.nomeCategoria(produto.categoria)) + "</p><h3>" + ui.esc(produto.titulo) + "</h3><p class=\"preco\">" + comparativo + dinheiro.real(produto.preco) + '</p><p class="vista">À vista ' + dinheiro.real(precoVista(produto.preco)) + "</p></div></a>";
  }
  function trilho(titulo, lista) {
    if (!lista.length) return "";
    return "<section><h2>" + ui.esc(titulo) + "</h2><div class=\"trilho\">" + lista.map(card).join("") + "</div></section>";
  }
  function mesa() {
    var trabalhos = estado().impressoes.filter(function (item) { return item.status === "imprimindo"; });
    if (!trabalhos.length) return ui.vazio("A mesa está livre agora.");
    return trabalhos.map(function (item) {
      var maquina = estado().maquinas.find(function (maq) { return maq.id === item.maquinaId; });
      return '<article class="mesa cartao"><img src="' + ui.url("marca/mascote.png") + '" alt=""><div><strong>' + ui.esc(item.titulo) + "</strong><p>" + ui.esc(maquina ? maquina.nome : "Máquina") + "</p><p class=\"discreto\">" + ui.esc(dinheiro.dataBR(item.inicio)) + " · " + ui.esc(String(item.inicio).slice(11, 16)) + " às " + ui.esc(String(item.fim).slice(11, 16)) + "</p></div></article>";
    }).join("");
  }
  function redes() {
    var id = estado().identidade;
    return '<div class="rede-bloco">' + [
      ["WhatsApp", id.whatsapp], ["Instagram", id.instagram], ["TikTok", id.tiktok], ["YouTube", id.youtube], ["Shopee", id.shopee], ["Mercado Livre", id.mercadoLivre]
    ].map(function (item) {
      return '<a class="botao-linha" href="' + ui.esc(item[1]) + '" target="_blank" rel="noopener">' + item[0] + "</a>";
    }).join("") + "</div>";
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
  function home() {
    document.title = "Cria Lab DM";
    var categorias = estado().categorias.map(function (categoria) {
      return '<a class="chip" href="' + ui.url("paginas/busca.html?categoria=" + encodeURIComponent(categoria.id)) + '">' + ui.esc(categoria.nome) + "</a>";
    }).join("");
    var depoimentos = estado().depoimentos.filter(function (item) { return item.podeAparecer; }).map(function (item) {
      return "<blockquote><p>" + ui.esc(item.texto) + "</p><strong>" + ui.esc(item.autor) + "</strong></blockquote>";
    }).join("");
    var parceiros = estado().parceiros.filter(function (item) { return item.podeAparecer; }).map(function (item) {
      return "<span class=\"chip\">" + ui.esc(item.nome) + "</span>";
    }).join("");
    var vistos = estado().vistos.map(achar).filter(Boolean);
    raiz.innerHTML = '<section class="hero"><div><p class="selo"><img src="' + ui.url("marca/mascote.png") + '" alt="">Oficina da Cria Lab</p><img class="letreiro" src="' + ui.url("marca/logo-completa.jpeg") + '" alt="Cria Lab DM"><h1>Peças impressas na nossa mesa</h1><p>Decoração, chaveiros, utilidades, moldes e encomendas. Quando a peça tem arquivo, você escolhe comprar a peça ou o arquivo.</p>' + buscaCampo("") + '</div><img class="mascote" src="' + ui.url("marca/mascote.png") + '" alt="Pássaro laranja de óculos com mochila de impressora"></section><section><h2>Na mesa agora</h2>' + mesa() + '</section><section><h2>Categorias</h2><div class="chips">' + categorias + "</div></section>" + trilho("Ofertas", produtosVisiveis().filter(function (produto) { return produto.oferta; })) + trilho("Mais vendidos", produtosVisiveis().filter(function (produto) { return produto.maisVendido; })) + trilho("Personalizados", produtosVisiveis().filter(function (produto) { return produto.personalizavel; })) + trilho("Também vendem o arquivo", produtosVisiveis().filter(function (produto) { return produto.comArquivo; })) + trilho("Vistos recentemente", vistos) + '<section class="cartao"><h2>Redes da oficina</h2>' + redes() + (depoimentos ? "<h3>Quem já levou</h3>" + depoimentos : "") + (parceiros ? "<h3>Parceiros</h3><div class=\"chips\">" + parceiros + "</div>" : "") + "</section>";
    ligarSugestoes();
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
      arquivo: q.get("arquivo") === "1",
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
      if (consulta.arquivo && !produto.comArquivo) return false;
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
    return '<form id="filtros" class="cartao filtros"><label>Texto<input name="q" value="' + ui.esc(consulta.q) + '"></label><label>Categoria<select name="categoria">' + categorias + '</select></label><div class="grade-3"><label>Preço mínimo<input name="precoMin" type="number" min="0" step="0.01" value="' + ui.esc(consulta.precoMin) + '"></label><label>Preço máximo<input name="precoMax" type="number" min="0" step="0.01" value="' + ui.esc(consulta.precoMax) + '"></label><label>Ordenar<select name="ordem"><option value="relevancia">Relevância</option><option value="preco"' + (consulta.ordem === "preco" ? " selected" : "") + '>Preço</option><option value="vendas"' + (consulta.ordem === "vendas" ? " selected" : "") + '>Vendas</option><option value="novidade"' + (consulta.ordem === "novidade" ? " selected" : "") + '>Novidade</option></select></label></div><div class="linha"><label><input type="checkbox" name="freteGratis"' + marcado("freteGratis") + "> Frete grátis</label><label><input type=\"checkbox\" name=\"aVista\"" + marcado("aVista") + "> À vista</label><label><input type=\"checkbox\" name=\"personalizavel\"" + marcado("personalizavel") + "> Personalizável</label><label><input type=\"checkbox\" name=\"arquivo\"" + marcado("arquivo") + "> Com arquivo</label><label>Nota mínima<select name=\"nota\"><option value=\"\">Qualquer</option><option value=\"4\"" + (consulta.nota === "4" ? " selected" : "") + ">4 ou mais</option><option value=\"5\"" + (consulta.nota === "5" ? " selected" : "") + ">5</option></select></label></div><button class=\"botao\" type=\"submit\">Aplicar filtros</button></form>";
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
        ["freteGratis", "aVista", "personalizavel", "arquivo"].forEach(function (nome) {
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
      var campo = etapa.tipo === "arquivo" ? '<input type="file" accept="image/*" data-etapa="' + ui.esc(etapa.id) + '">' : '<input data-etapa="' + ui.esc(etapa.id) + '" ' + (etapa.tipo === "numero" ? 'type="number" min="1"' : 'type="text"') + ">";
      var extra = etapa.acresce ? " · +" + dinheiro.real(etapa.acresce) : "";
      return "<label>" + ui.esc(etapa.nome) + extra + campo + "</label>";
    }).join("") : "";
    var arquivoBloco = produto.comArquivo ? '<button class="botao-teal" type="button" id="comprar-arquivo">Comprar arquivo</button><p class="discreto">' + ui.esc(produto.arquivo.formato) + " · licença " + ui.esc(produto.arquivo.licenca) + " · " + dinheiro.real(produto.precoArquivo) + " · sem frete</p>" : "";
    var perguntas = estado().perguntas.filter(function (item) { return item.produtoId === produto.id; }).map(function (item) {
      return "<article><strong>" + ui.esc(item.autor) + "</strong><p>" + ui.esc(item.texto) + "</p>" + (item.resposta ? "<p><em>" + ui.esc(item.resposta) + "</em></p>" : "<p class=\"discreto\">Aguardando resposta</p>") + "</article>";
    }).join("") || "<p>Nenhuma pergunta ainda.</p>";
    var avaliacoes = estado().avaliacoes.filter(function (item) { return item.produtoId === produto.id && item.pedidoId; }).map(function (item) {
      var foto = item.fotoDoProduto && produto.fotos[0] ? '<img src="' + ui.url(produto.fotos[0].arquivo) + '" alt="Foto da avaliação" style="width:120px;height:120px;object-fit:cover;border-radius:12px">' : "";
      return "<article>" + foto + "<strong>" + ui.esc(item.autor) + " · " + item.nota + "/5</strong><p>" + ui.esc(item.texto) + "</p></article>";
    }).join("") || "<p>Sem avaliações de pedido entregue.</p>";
    var relacionados = produtosVisiveis().filter(function (item) { return item.categoria === produto.categoria && item.id !== produto.id; }).slice(0, 4);
    var esgotado = produto.situacao === "esgotado" || produto.estoque <= 0;
    var compraPeca = produto.tipo === "personalizado" ? "" : '<button class="botao" type="button" id="comprar-peca"' + (esgotado ? " disabled" : "") + ">" + (esgotado ? "Esgotado" : "Comprar peça") + "</button>";
    var whats = estado().identidade.whatsapp + "?text=" + encodeURIComponent("Olá, quero saber sobre " + produto.titulo);
    raiz.innerHTML = '<div class="ficha"><div class="galeria"><img class="galeria-principal" id="foto-principal" src="' + ui.url(fotoInicial.arquivo) + '" alt="' + ui.esc(fotoInicial.legenda) + '"><div class="miniaturas">' + miniaturas + '</div><p class="discreto" id="legenda-foto">' + ui.esc(fotoInicial.legenda) + " · sessão " + ui.esc(dinheiro.dataBR(produto.sessao)) + '</p></div><div class="cartao"><p class="discreto">' + ui.esc(ui.nomeCategoria(produto.categoria)) + "</p><h1>" + ui.esc(produto.titulo) + "</h1><p class=\"preco\">" + dinheiro.real(produto.preco) + "</p><p class=\"vista\">À vista " + dinheiro.real(precoVista(produto.preco)) + " no PIX ou boleto</p><p>" + config().parcelas + "x de " + dinheiro.real(produto.preco / config().parcelas) + " no cartão, sem o desconto à vista</p><p>" + (esgotado ? "Sem unidade pronta" : ui.esc(String(produto.estoque)) + " em estoque") + " · " + ui.esc(produto.peso) + " g · " + ui.esc(produto.medidas) + "</p><p>" + ui.esc(produto.descricao) + "</p><h2>Filamento</h2><div class=\"amostras\" id=\"amostras\">" + amostras + '</div><p id="cor-nome">' + ui.esc(produto.cores[0].nome) + '</p><div class="linha" id="tamanhos">' + tamanhos + "</div>" + (etapas ? "<h2>Personalização</h2>" + etapas : "") + '<label>Quantidade<input id="qtd" type="number" min="1" value="1"></label><div class="linha">' + compraPeca + arquivoBloco + '<button class="botao-linha" type="button" id="favorito">Favoritar</button></div>' + (produto.personalizavel ? '<button class="botao-teal" type="button" id="pedir-orcamento">Pedir orçamento</button>' : "") + '<a class="botao-linha" href="' + ui.esc(whats) + '" target="_blank" rel="noopener">Chamar no WhatsApp</a><h2>Frete simulado</h2><form id="frete-ficha" class="linha"><input name="cep" inputmode="numeric" placeholder="CEP" maxlength="9"><button class="botao-linha" type="submit">Cotar</button></form><div id="frete-resultado"></div></div></div><section class="grade-3"><article class="cartao"><h2>Perguntas</h2>' + perguntas + '<form id="nova-pergunta" class="formulario"><label>Sua pergunta<input name="texto" required></label><button class="botao-linha" type="submit">Enviar</button></form></article><article class="cartao"><h2>Avaliações</h2>' + avaliacoes + "</article></section>" + trilho("Relacionados", relacionados);
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
    var comprarArquivo = document.getElementById("comprar-arquivo");
    if (comprarArquivo) comprarArquivo.addEventListener("click", function () {
      CriaCarrinho.adicionar({
        produtoId: produto.id,
        titulo: produto.titulo,
        tipo: "arquivo",
        fisico: false,
        arquivo: true,
        quantidade: 1,
        precoUnitario: produto.precoArquivo,
        arquivoFormato: produto.arquivo.formato,
        arquivoLicenca: produto.arquivo.licenca,
        arquivoNome: produto.arquivo.nome,
        detalhes: produto.arquivo.formato + " · licença " + produto.arquivo.licenca,
        assinatura: "arquivo"
      });
      location.href = ui.url("paginas/carrinho.html");
    });
    document.getElementById("favorito").addEventListener("click", function () {
      Cria.alterar(function (atual) {
        if (atual.favoritos.indexOf(produto.id) >= 0) atual.favoritos = atual.favoritos.filter(function (item) { return item !== produto.id; });
        else atual.favoritos.push(produto.id);
      });
      document.getElementById("favorito").textContent = estado().favoritos.indexOf(produto.id) >= 0 ? "Favorito" : "Favoritar";
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
    raiz.innerHTML = '<section class="hero"><div><img class="letreiro" src="' + ui.url("marca/logo-completa.jpeg") + '" alt=""><h1>' + ui.esc(id.nome) + '</h1><p>CNPJ ' + ui.esc(id.cnpj) + "</p><p>" + ui.esc(id.email) + "</p><p>" + ui.esc(id.telefone) + "</p><p>" + ui.esc(id.endereco || "Endereço a definir") + "</p><p>" + ui.esc(id.horario || "Horário a definir") + "</p>" + redes() + "</div><img class=\"mascote\" src=\"" + ui.url("marca/mascote.png") + '" alt=""></section>' + (depoimentos ? '<section class="cartao"><h2>Depoimentos</h2>' + depoimentos + "</section>" : "") + '<section class="cartao"><h2>Template de postagem</h2><img src="' + ui.url("marca/template-de-postagem.png") + '" alt="Template de postagem da Cria Lab DM"></section>';
  }

  if (pagina === "home") home();
  if (pagina === "busca") busca();
  if (pagina === "produto") produtoPagina();
  if (pagina === "sobre") sobre();
})();
