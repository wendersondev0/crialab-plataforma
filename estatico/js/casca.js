(function (root) {
  function url(rel) {
    return new URL(rel, root.CriaBase).href;
  }

  function esc(texto) {
    return String(texto == null ? "" : texto)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function ehAdmin() {
    return location.pathname.replace(/\\/g, "/").indexOf("/paginas/admin/") >= 0;
  }

  function quantidadeCarrinho() {
    return (root.Cria.estado.carrinho || []).reduce(function (total, item) { return total + (Number(item.quantidade) || 0); }, 0);
  }

  function svg(miolo) {
    return '<svg viewBox="0 0 24 24" aria-hidden="true">' + miolo + "</svg>";
  }

  var traco = ' fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"';

  function logoCanal(arquivo) {
    return '<img class="logo-canal h-5 w-auto max-w-7 object-contain" src="' + url(arquivo) + '" alt="">';
  }

  function desenhoRede(chave, clara) {
    if (chave === "shopee") return logoCanal(clara ? "marca/shopee-branca.png" : "marca/shopee-preta.png");
    if (chave === "mercadolivre") return logoCanal(clara ? "marca/mercadolivre-branca.png" : "marca/mercadolivre-preta.png");
    return icones[chave];
  }

  var icones = {
    casa: svg('<path' + traco + ' d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6H10v6H5a1 1 0 0 1-1-1z"/>'),
    busca: svg('<circle' + traco + ' cx="11" cy="11" r="6"/><path' + traco + ' d="m16 16 4 4"/>'),
    carrinho: svg('<path' + traco + ' d="M4 6h2l1.4 9h10.2l1.6-6H8"/><circle' + traco + ' cx="10" cy="19" r="1.2"/><circle' + traco + ' cx="17" cy="19" r="1.2"/>'),
    conta: svg('<circle' + traco + ' cx="12" cy="8" r="3"/><path' + traco + ' d="M5 19c1.4-3 3.8-4.5 7-4.5S17.6 16 19 19"/>'),
    contato: svg('<rect' + traco + ' x="3" y="5" width="18" height="14" rx="2"/><path' + traco + ' d="m4 7 8 6 8-6"/>'),
    sobre: svg('<circle' + traco + ' cx="12" cy="12" r="8"/><path' + traco + ' d="M12 11v5M12 8h.01"/>'),
    admin: svg('<circle' + traco + ' cx="12" cy="12" r="3"/><path' + traco + ' d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M18.4 5.6 17 7M7 17l-1.4 1.4"/>'),
    whatsapp: svg('<path' + traco + ' d="M12 4a8 8 0 0 0-6.9 12L4 20l4.2-1.1A8 8 0 1 0 12 4z"/><path' + traco + ' d="M9 10.2c.2 1.6 1.6 3.2 3.2 3.8l1.1-.8c.2-.2.5-.2.7 0l1 .7"/>'),
    instagram: svg('<rect' + traco + ' x="4" y="4" width="16" height="16" rx="5"/><circle' + traco + ' cx="12" cy="12" r="3.5"/><circle fill="currentColor" cx="17" cy="7" r="0.8"/>'),
    tiktok: svg('<path' + traco + ' d="M14 5v9.2a3.2 3.2 0 1 1-2.2-3"/><path' + traco + ' d="M14 7.2c.8 1.4 2.1 2.3 4 2.5"/>'),
    youtube: svg('<rect' + traco + ' x="3" y="6" width="18" height="12" rx="3"/><path' + traco + ' d="m11 9.5 5 2.5-5 2.5z"/>'),
    shopee: logoCanal("marca/shopee-preta.png"),
    mercadolivre: logoCanal("marca/mercadolivre-preta.png"),
    email: svg('<rect' + traco + ' x="3" y="5" width="18" height="14" rx="2"/><path' + traco + ' d="m4 7 8 6 8-6"/>'),
    telefone: svg('<path' + traco + ' d="M8 4h3l1 4-2 1a12 12 0 0 0 5 5l1-2 4 1v3c0 .6-.4 1-1 1A15 15 0 0 1 5 6c0-.6.4-1 1-1z"/>'),
    sino: svg('<path' + traco + ' d="M6 16h12l-1.2-2.2V10a4.8 4.8 0 0 0-9.6 0v3.8z"/><path' + traco + ' d="M10 16a2 2 0 0 0 4 0"/>'),
    coracao: svg('<path' + traco + ' d="M12 19s-7-4.4-7-8.2A3.8 3.8 0 0 1 12 8a3.8 3.8 0 0 1 7 2.8C19 14.6 12 19 12 19z"/>'),
    caminhao: svg('<path' + traco + ' d="M3 7h11v8H3z"/><path' + traco + ' d="M14 10h4l3 3v2h-7z"/><circle' + traco + ' cx="7" cy="17.5" r="1.5"/><circle' + traco + ' cx="17" cy="17.5" r="1.5"/>'),
    documento: svg('<path' + traco + ' d="M7 3h7l5 5v13H7z"/><path' + traco + ' d="M14 3v5h5"/><path' + traco + ' d="M10 13h6M10 17h6"/>'),
    caixa: svg('<path' + traco + ' d="M4 8 12 4l8 4-8 4-8-4z"/><path' + traco + ' d="M4 8v8l8 4 8-4V8"/><path' + traco + ' d="M12 12v8"/>'),
    estoque: svg('<path' + traco + ' d="M4 7h16M4 12h16M4 17h16"/><path' + traco + ' d="M8 7v10M16 7v10"/>'),
    menu: svg('<path' + traco + ' d="M4 7h16M4 12h16M4 17h16"/>'),
    fechar: svg('<path' + traco + ' d="M6 6l12 12M18 6 6 18"/>'),
    sair: svg('<path' + traco + ' d="M10 5H6a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h4"/><path' + traco + ' d="M14 12H8"/><path' + traco + ' d="m16 8 4 4-4 4"/>'),
    pedidos: svg('<path' + traco + ' d="M7 4h10l1 4H6z"/><path' + traco + ' d="M6 8h12v11a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1z"/><path' + traco + ' d="M10 12h4"/>')
  };

  function icoSpan(nome) {
    return '<span class="ico inline-flex h-5 w-5 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full [&>img]:h-5 [&>img]:w-auto [&>img]:max-w-7 [&>img]:object-contain">' + (icones[nome] || "") + "</span>";
  }

  function ativo(href) {
    var caminho = location.pathname.replace(/\\/g, "/");
    if (href === "index.html") return (caminho === "/" || caminho.endsWith("/index.html")) && caminho.indexOf("/admin/") < 0;
    return caminho.endsWith("/" + href);
  }

  function link(href, texto, variante, dica) {
    var rotulo = texto;
    var qtd = "";
    var carrinho = /^Carrinho \((\d+)\)$/.exec(texto);
    if (carrinho) {
      rotulo = "Carrinho";
      if (Number(carrinho[1]) > 0) qtd = '<span class="selo-qtd ml-1 inline-flex h-[22px] min-w-[22px] items-center justify-center rounded-full bg-laranja px-1.5 text-[0.75rem] font-extrabold text-white">' + carrinho[1] + "</span>";
    }
    var chave = carrinho ? "carrinho" : texto;
    var mapa = { "Início": "casa", "Produtos": "caixa", "Pronta Entrega": "caixa", "Sob Demanda": "caminhao", "Personalizados": "admin", "carrinho": "carrinho", "Conta": "conta", "Contato": "contato", "Sobre": "sobre", "Sobre a Cria Lab": "sobre", "Administração": "admin", "Painel": "casa", "Painel Gerencial": "casa", "Gestão Produtos": "caixa", "Gestão Personalizados": "admin", "Marketing": "tiktok", "Atendimento": "contato", "Patrimônio": "estoque", "Patrimônio e Estoque": "estoque", "Financeiro": "carrinho", "Gestão Financeira": "carrinho", "Fiscal": "email", "Gestão Fiscal": "email", "Sistema": "admin", "Impressão": "admin", "Impressão 3D": "admin", "Ver loja": "casa" };
    var desenho = icones[mapa[chave]] ? '<span class="ico inline-flex h-5 w-5 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + icones[mapa[chave]] + "</span>" : "";
    var base = "nav-link inline-flex min-h-11 items-center justify-center gap-2 whitespace-nowrap rounded-full px-4 text-sm font-bold no-underline transition";
    var title = dica ? ' title="' + esc(dica) + '"' : "";
    var rotuloHtml = '<span class="nav-texto">' + esc(rotulo) + "</span>";
    if (texto === "Sobre a Cria Lab") {
      rotuloHtml = '<span class="nav-texto nav-texto-longo">Sobre a Cria Lab</span><span class="nav-texto nav-texto-curto">Sobre</span>';
    }
    if (carrinho) {
      rotuloHtml = '<span class="nav-texto nav-texto-carrinho">' + esc(rotulo) + "</span>";
    }
    if (texto === "Ver loja" || variante === "ver-loja") {
      return '<a class="' + base + ' bg-laranja text-white hover:bg-laranja-escuro" href="' + url(href) + '"' + title + ">" + desenho + rotuloHtml + qtd + "</a>";
    }
    var estadoClasse;
    if (variante === "loja") {
      estadoClasse = ativo(href) ? " ativo bg-white text-laranja" : " text-white hover:bg-white/20";
    } else if (variante === "admin") {
      estadoClasse = ativo(href) ? " ativo border border-tinta bg-tinta text-white" : " border border-linha bg-creme text-tinta hover:bg-teal/15";
    } else {
      estadoClasse = ativo(href) ? " ativo bg-tinta text-white" : " text-tinta hover:bg-teal/15";
    }
    return '<a class="' + base + estadoClasse + '" href="' + url(href) + '"' + title + ">" + desenho + rotuloHtml + qtd + "</a>";
  }

  function redes(identidade, claro) {
    return '<div class="rede-bloco flex flex-wrap gap-2.5' + (claro ? " claro" : "") + '">' + [
      ["whatsapp", "WhatsApp", identidade.whatsapp],
      ["instagram", "Instagram", identidade.instagram],
      ["tiktok", "TikTok", identidade.tiktok],
      ["youtube", "YouTube", identidade.youtube],
      ["shopee", "Shopee", identidade.shopee],
      ["mercadolivre", "Mercado Livre", identidade.mercadoLivre]
    ].map(function (item) {
      var fundo = claro
        ? "border-white/20 bg-white/10 text-white hover:bg-laranja hover:border-laranja"
        : "border-linha bg-white text-tinta shadow-cria hover:bg-laranja hover:border-laranja hover:text-white";
      return '<a class="icone-rede inline-flex w-[84px] min-h-[76px] flex-col items-center justify-center gap-1.5 rounded-[18px] border-2 px-1.5 py-2.5 no-underline transition hover:-translate-y-1 ' + fundo + '" data-rede="' + item[0] + '" href="' + esc(item[2]) + '" target="_blank" rel="noopener" aria-label="' + esc(item[1]) + '"><span class="ico inline-flex h-5 w-5 items-center justify-center [&>svg]:h-full [&>svg]:w-full [&>img]:h-5 [&>img]:w-auto [&>img]:max-w-7 [&>img]:object-contain">' + desenhoRede(item[0], claro) + '</span><span class="rotulo text-center text-[0.68rem] font-extrabold leading-tight">' + esc(item[1]) + "</span></a>";
    }).join("") + "</div>";
  }

  function iconesSociais(identidade) {
    return [
      ["whatsapp", "WhatsApp", identidade.whatsapp],
      ["instagram", "Instagram", identidade.instagram],
      ["tiktok", "TikTok", identidade.tiktok],
      ["youtube", "YouTube", identidade.youtube],
      ["shopee", "Shopee", identidade.shopee],
      ["mercadolivre", "Mercado Livre", identidade.mercadoLivre]
    ].map(function (item) {
      var extra = item[0] === "mercadolivre" ? " [&>img]:h-6 [&>img]:w-6" : " [&>svg]:h-[18px] [&>svg]:w-[18px] [&>img]:h-[18px] [&>img]:w-[18px]";
      return '<a class="icone-topo inline-flex h-8 w-8 min-h-8 items-center justify-center rounded-full text-white' + extra + '" href="' + esc(item[2]) + '" target="_blank" rel="noopener" aria-label="' + esc(item[1]) + '">' + desenhoRede(item[0], true) + "</a>";
    }).join("");
  }

  function listaNotificacoes(estado) {
    if (estado.perfil === "vazia") return [];
    var nome = estado.cliente && estado.cliente.nome;
    var lista = [];
    (estado.orcamentosCliente || []).forEach(function (item) {
      if (item.status === "respondido") lista.push("Orçamento respondido: " + item.titulo);
    });
    (estado.pedidos || []).forEach(function (item) {
      if (item.status === "aguardando pagamento") lista.push("Pedido aguardando pagamento");
      if (item.status === "pago") lista.push("Pedido pago");
    });
    (estado.perguntas || []).forEach(function (item) {
      if (item.resposta && item.autor === nome) lista.push("Sua pergunta foi respondida");
    });
    return lista.slice(0, 6);
  }

  function utilitariosLoja(estado) {
    var vazio = estado.perfil === "vazia";
    var avisos = listaNotificacoes(estado);
    var itemPopup = "item-popup-topo flex min-h-11 w-full items-center gap-2.5 rounded-lg px-3 text-left font-bold text-tinta no-underline hover:bg-teal/15";
    var itensAviso = avisos.length ? avisos.map(function (texto) {
      return '<a class="' + itemPopup + '" href="' + url("paginas/conta.html") + '">' + icoSpan("sino") + '<span>' + esc(texto) + "</span></a>";
    }).join("") : '<p class="popup-vazio m-2 text-sm text-tinta">Nenhuma notificação.</p>';
    var selo = avisos.length ? '<span class="selo-qtd ml-1 inline-flex h-[22px] min-w-[22px] items-center justify-center rounded-full bg-laranja px-1.5 text-[0.75rem] font-extrabold text-white">' + avisos.length + "</span>" : "";
    var sair = vazio ? "Entrar" : "Sair";
    var admin = !vazio && estado.cliente && estado.cliente.admin;
    var itemAdmin = admin ? '<a class="' + itemPopup + '" href="' + url("paginas/admin/index.html") + '">' + icoSpan("admin") + "<span>Administração</span></a>" : "";
    var atalho = "atalho inline-flex min-h-11 items-center gap-1.5 rounded-full px-2.5 text-[0.92rem] font-bold text-white no-underline hover:bg-white/20";
    var popup = "popup-topo absolute top-[calc(100%-6px)] right-0 z-30 hidden min-w-[240px] grid-cols-1 gap-0 rounded-xl bg-white p-2 text-tinta shadow-cria";
    var nomeConta = vazio ? "Conta" : estado.cliente.nome;
    var menuConta = [
      '<a class="' + itemPopup + '" href="' + url("paginas/conta.html") + '">' + icoSpan("conta") + "<span>Minha conta</span></a>",
      '<a class="' + itemPopup + '" href="' + url("paginas/conta.html#pedidos") + '">' + icoSpan("pedidos") + "<span>Meus pedidos</span></a>",
      itemAdmin,
      '<button class="' + itemPopup + ' cursor-pointer border-0 bg-transparent" type="button" id="sair-conta">' + icoSpan("sair") + "<span>" + sair + "</span></button>"
    ].join("");
    return {
      selo: selo,
      bloco: [
        '<div class="menu-suspenso relative"><button class="' + atalho + '" type="button" aria-label="Notificações" aria-expanded="false" aria-haspopup="true">' + icoSpan("sino") + '<span class="atalho-texto">Notificações</span>' + selo + '</button><div class="' + popup + '" role="menu">' + itensAviso + "</div></div>",
        '<a class="' + atalho + '" href="' + url("paginas/contato.html") + '" aria-label="Ajuda">' + icoSpan("contato") + '<span class="atalho-texto">Ajuda</span></a>',
        '<div class="menu-suspenso menu-conta relative"><button class="' + atalho + '" type="button" id="abrir-conta" aria-label="' + esc(nomeConta) + '" aria-expanded="false" aria-haspopup="true">' + icoSpan("conta") + '<span class="atalho-texto">' + esc(nomeConta) + '</span></button><div class="' + popup + '" role="menu">' + menuConta + "</div></div>"
      ].join("")
    };
  }

  function faixaLoja(estado) {
    return '<div class="faixa-fina bg-laranja-escuro"><div class="barra faixa-fina-barra mx-auto flex min-h-11 w-[min(1120px,calc(100%-32px))] items-center justify-between gap-3"><div class="siga flex items-center gap-2"><span class="siga-texto text-[0.92rem] font-bold text-white">Siga a Cria Lab</span>' + iconesSociais(estado.identidade) + '</div></div></div>';
  }

  function ligarMenus(topo) {
    topo.querySelectorAll(".menu-suspenso").forEach(function (menu) {
      var botao = menu.querySelector(".atalho");
      if (!botao) return;
      botao.addEventListener("click", function (evento) {
        evento.preventDefault();
        var aberto = menu.classList.toggle("aberto");
        botao.setAttribute("aria-expanded", aberto ? "true" : "false");
        topo.querySelectorAll(".menu-suspenso.aberto").forEach(function (outro) {
          if (outro === menu) return;
          outro.classList.remove("aberto");
          var gatilho = outro.querySelector(".atalho");
          if (gatilho) gatilho.setAttribute("aria-expanded", "false");
        });
      });
    });
    topo.querySelectorAll("#sair-conta").forEach(function (sair) {
      sair.addEventListener("click", function () {
        Cria.alterar(function (atual) { atual.perfil = atual.perfil === "vazia" ? "exemplo" : "vazia"; });
        location.href = url("paginas/conta.html");
      });
    });
    var gaveta = document.getElementById("menu-gaveta");
    var abrir = topo.querySelector("#abrir-menu");
    var fechar = gaveta ? gaveta.querySelector("#fechar-menu") : null;
    var fundo = gaveta ? gaveta.querySelector(".menu-gaveta-fundo") : null;
    function setMenu(aberto) {
      if (!gaveta || !abrir) return;
      gaveta.classList.toggle("aberta", aberto);
      document.body.classList.toggle("menu-aberto", aberto);
      abrir.setAttribute("aria-expanded", aberto ? "true" : "false");
    }
    if (abrir) abrir.addEventListener("click", function () { setMenu(true); });
    if (fechar) fechar.addEventListener("click", function () { setMenu(false); });
    if (fundo) fundo.addEventListener("click", function () { setMenu(false); });
    if (!root._criaEscMenu) {
      root._criaEscMenu = true;
      document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") setMenu(false);
      });
    }
  }

  function montar() {
    var estado = root.Cria.estado;
    var identidade = estado.identidade;
    var topo = document.getElementById("topo");
    var rodape = document.getElementById("rodape");
    if (!topo || !rodape) return;
    var admin = ehAdmin();
    topo.classList.toggle("topo-loja", !admin);
    topo.classList.toggle("topo-admin", admin);
    document.body.classList.add("text-tinta", "font-cria", "antialiased");
    if (admin) {
      var itensAdmin = [
        ["paginas/admin/index.html", "Painel"],
        ["paginas/admin/produtos.html", "Produtos"],
        ["paginas/admin/pedidos.html", "Pedidos"],
        ["paginas/admin/marketing.html", "Marketing"],
        ["paginas/admin/contato.html", "Atendimento"],
        ["paginas/admin/patrimonio.html", "Patrimônio"],
        ["paginas/admin/financeiro.html", "Financeiro"],
        ["paginas/admin/configuracao.html", "Sistema"],
        ["paginas/admin/impressao.html", "Impressão"]
      ];
      var menuAdmin = itensAdmin.map(function (item) {
        return link(item[0], item[1], "admin");
      }).join("");
      var menuGavetaAdmin = itensAdmin.map(function (item) {
        return link(item[0], item[1], "");
      }).join("") + link("index.html", "Ver loja", "ver-loja");
      topo.className = "topo-admin sticky top-0 z-20 border-b border-linha bg-white shadow-cria";
      topo.innerHTML = '<div class="admin-faixa"><div class="barra barra-admin mx-auto flex min-h-[68px] w-[min(1120px,calc(100%-32px))] items-center gap-3">' +
        '<button class="botao-hamburguer botao-hamburguer-admin" type="button" id="abrir-menu" aria-label="Abrir menu" aria-expanded="false" aria-controls="menu-gaveta"><span class="ico">' + icones.menu + "</span></button>" +
        '<a class="marca" href="' + url("paginas/admin/index.html") + '"><img class="h-11 rounded-xl bg-creme p-1" src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a>' +
        '<div class="admin-acoes ml-auto">' + link("index.html", "Ver loja", "ver-loja") + "</div></div>" +
        '<nav class="menu menu-admin mx-auto flex w-[min(1120px,calc(100%-32px))] flex-wrap gap-1.5 overflow-visible pb-3.5">' + menuAdmin + "</nav></div>";
      var gavetaAdminAntiga = document.getElementById("menu-gaveta");
      if (gavetaAdminAntiga) gavetaAdminAntiga.remove();
      var gavetaAdmin = document.createElement("div");
      gavetaAdmin.className = "menu-gaveta menu-gaveta-admin";
      gavetaAdmin.id = "menu-gaveta";
      gavetaAdmin.innerHTML = '<button class="menu-gaveta-fundo" type="button" aria-label="Fechar menu"></button><aside class="menu-gaveta-painel" role="dialog" aria-modal="true" aria-label="Menu da administração"><div class="menu-gaveta-topo"><strong>Administração</strong><button class="botao-fechar-menu" type="button" id="fechar-menu" aria-label="Fechar menu"><span class="ico">' + icones.fechar + '</span></button></div><nav class="menu-gaveta-lista">' + menuGavetaAdmin + "</nav></aside>";
      document.body.appendChild(gavetaAdmin);
      ligarMenus(topo);
    } else {
      var modalidades = estado.modalidades || [];
      function dicaModalidade(id) {
        var item = modalidades.find(function (m) { return m.id === id; });
        return item ? item.resumo : "";
      }
      var ajuda = estado.menusAjuda || {};
      var menuLoja = [
        link("index.html", "Início", "loja", ajuda.inicio),
        link("paginas/pronta-entrega.html", "Pronta Entrega", "loja", dicaModalidade("pronta-entrega")),
        link("paginas/sob-demanda.html", "Sob Demanda", "loja", dicaModalidade("sob-demanda")),
        link("paginas/personalizados.html", "Personalizados", "loja", dicaModalidade("personalizado")),
        link("paginas/sobre.html", "Sobre a Cria Lab", "loja", ajuda.sobre)
      ].join("");
      var util = utilitariosLoja(estado);
      var qtd = quantidadeCarrinho();
      var perfilVazio = estado.perfil === "vazia";
      var podeAdmin = !perfilVazio && estado.cliente && estado.cliente.admin;
      var menuGavetaItens = [
        link("index.html", "Início", "", ajuda.inicio),
        link("paginas/pronta-entrega.html", "Pronta Entrega", "", dicaModalidade("pronta-entrega")),
        link("paginas/sob-demanda.html", "Sob Demanda", "", dicaModalidade("sob-demanda")),
        link("paginas/personalizados.html", "Personalizados", "", dicaModalidade("personalizado")),
        link("paginas/sobre.html", "Sobre a Cria Lab", "", ajuda.sobre),
        link("paginas/contato.html", "Ajuda / Contato", "", ajuda.contato),
        link("paginas/conta.html", "Minha conta", "", ajuda.conta),
        link("paginas/carrinho.html", "Carrinho (" + qtd + ")", "", ajuda.carrinho)
      ];
      if (podeAdmin) {
        menuGavetaItens.push(link("paginas/admin/index.html", "Administração", ""));
      }
      var menuGaveta = menuGavetaItens.join("");
      topo.className = "topo-loja sticky top-0 z-20 bg-laranja";
      topo.innerHTML = faixaLoja(estado)
        + '<div class="loja-faixa"><div class="barra loja-barra mx-auto flex min-h-[76px] w-[min(1120px,calc(100%-32px))] items-center gap-3">'
        + '<button class="botao-hamburguer" type="button" id="abrir-menu" aria-label="Abrir menu" aria-expanded="false" aria-controls="menu-gaveta"><span class="ico">' + icones.menu + "</span></button>"
        + '<a class="marca" href="' + url("index.html") + '"><img class="h-11 rounded-xl bg-white p-1" src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a>'
        + '<form class="busca-topo flex min-w-0 flex-1 items-center gap-2 rounded-full bg-white py-1 pl-3 pr-1" action="' + url("paginas/busca.html") + '" method="get"><input class="min-h-10 min-w-0 flex-1 border-0 bg-transparent outline-none" name="q" type="search" placeholder="Buscar na Cria Lab" aria-label="Buscar na Cria Lab"><button class="botao botao-busca inline-flex min-h-10 items-center justify-center rounded-full bg-laranja px-4 font-bold text-white hover:bg-laranja-escuro" type="submit" aria-label="Buscar"><span class="botao-busca-texto">Buscar</span><span class="botao-busca-ico ico inline-flex h-5 w-5 [&>svg]:h-full [&>svg]:w-full" aria-hidden="true">' + icones.busca + "</span></button></form>"
        + '<div class="loja-acoes"><div class="acoes-util flex items-center gap-0">' + util.bloco + "</div>"
        + link("paginas/carrinho.html", "Carrinho (" + qtd + ")", "loja", ajuda.carrinho)
        + "</div></div>"
        + '<nav class="menu menu-loja mx-auto flex w-[min(1120px,calc(100%-32px))] flex-nowrap gap-1.5 overflow-x-auto overflow-y-visible pb-2.5">' + menuLoja + "</nav></div>";
      var gavetaAntiga = document.getElementById("menu-gaveta");
      if (gavetaAntiga) gavetaAntiga.remove();
      var gaveta = document.createElement("div");
      gaveta.className = "menu-gaveta";
      gaveta.id = "menu-gaveta";
      gaveta.innerHTML = '<button class="menu-gaveta-fundo" type="button" aria-label="Fechar menu"></button><aside class="menu-gaveta-painel" role="dialog" aria-modal="true" aria-label="Menu da loja"><div class="menu-gaveta-topo"><strong>Menu</strong><button class="botao-fechar-menu" type="button" id="fechar-menu" aria-label="Fechar menu"><span class="ico">' + icones.fechar + '</span></button></div><nav class="menu-gaveta-lista">' + menuGaveta + "</nav></aside>";
      document.body.appendChild(gaveta);
      ligarMenus(topo);
    }
    var local = (identidade.endereco ? "<p class=\"rodape-meta m-0\">" + esc(identidade.endereco) + "</p>" : "") + (identidade.horario ? "<p class=\"rodape-meta m-0\">" + esc(identidade.horario) + "</p>" : "");
    var parceirosVisiveis = (estado.parceiros || []).filter(function (item) { return item.podeAparecer; });
    var faixaParceiros = parceirosVisiveis.length
      ? '<section class="rodape-parceiros"><p class="rodape-parceiros-titulo">Parceiros</p><div class="parceiros parceiros-rodape">' + parceirosVisiveis.map(function (item) {
        var marca = item.logo
          ? '<img src="' + url(item.logo) + '" alt="' + esc(item.nome) + '">'
          : "<span>" + esc(item.nome) + "</span>";
        if (item.url) marca = '<a href="' + esc(item.url) + '" target="_blank" rel="noopener" aria-label="' + esc(item.nome) + '">' + marca + "</a>";
        return '<figure class="parceiro parceiro-rodape" title="' + esc(item.nome) + '">' + marca + "</figure>";
      }).join("") + "</div></section>"
      : "";
    var redesCompactas = [
      ["whatsapp", "WhatsApp", identidade.whatsapp],
      ["instagram", "Instagram", identidade.instagram],
      ["tiktok", "TikTok", identidade.tiktok],
      ["youtube", "YouTube", identidade.youtube],
      ["shopee", "Shopee", identidade.shopee],
      ["mercadolivre", "Mercado Livre", identidade.mercadoLivre]
    ].map(function (item) {
      return '<a class="rodape-rede" href="' + esc(item[2]) + '" target="_blank" rel="noopener" aria-label="' + esc(item[1]) + '">' + desenhoRede(item[0], true) + "</a>";
    }).join("");
    var linksLoja = [
      ["paginas/pronta-entrega.html", "Pronta Entrega", "caixa"],
      ["paginas/sob-demanda.html", "Sob Demanda", "caminhao"],
      ["paginas/personalizados.html", "Personalizados", "admin"],
      ["paginas/sobre.html", "Sobre a Cria Lab", "sobre"],
      ["paginas/contato.html", "Contato", "contato"]
    ].map(function (item) {
      return '<a class="rodape-link" href="' + url(item[0]) + '">' + icoSpan(item[2]) + "<span>" + esc(item[1]) + "</span></a>";
    }).join("");
    rodape.className = "rodape-loja";
    rodape.innerHTML = [
      '<div class="rodape-miolo">',
      faixaParceiros,
      '<div class="rodape-grade">',
      '<div class="rodape-marca"><img class="logo-rodape" src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab"><div><strong class="rodape-nome">' + esc(identidade.nome) + '</strong><p class="rodape-meta m-0">Impressão 3D com cara de oficina</p>' + local + "</div></div>",
      '<div class="rodape-bloco"><p class="rodape-titulo">Contato</p><a class="rodape-link" href="mailto:' + esc(identidade.email) + '">' + icoSpan("email") + "<span>" + esc(identidade.email) + '</span></a><a class="rodape-link" href="tel:+5527996530954">' + icoSpan("telefone") + "<span>" + esc(identidade.telefone) + '</span></a><div class="rodape-redes">' + redesCompactas + "</div></div>",
      '<div class="rodape-bloco"><p class="rodape-titulo">Loja</p><nav class="rodape-nav">' + linksLoja + "</nav></div>",
      "</div>",
      '<div class="rodape-base"><span>© ' + new Date().getFullYear() + " " + esc(identidade.nome) + "</span><span>CNPJ " + esc(identidade.cnpj) + "</span></div>",
      "</div>"
    ].join("");
  }

  function vazio(texto) {
    return '<div class="vazio revela mx-auto grid justify-items-center gap-3 rounded-[18px] bg-white p-8 text-center shadow-cria"><img class="mx-auto w-[min(240px,70%)]" src="' + url("marca/mascote.png") + '" alt="Mascote da Cria Lab DM"><p class="m-0 font-semibold">' + esc(texto) + "</p></div>";
  }

  function nomeCategoria(id) {
    var categoria = (root.Cria.estado.categorias || []).find(function (item) { return item.id === id; });
    return categoria ? categoria.nome : id;
  }

  function nomeModalidade(id) {
    var item = (root.Cria.estado.modalidades || []).find(function (m) { return m.id === id; });
    return item ? item.nome : id;
  }

  function modalidadeInfo(id) {
    return (root.Cria.estado.modalidades || []).find(function (m) { return m.id === id; }) || null;
  }

  function revelar(escopo) {
    var raiz = escopo || document;
    var itens = Array.prototype.slice.call(raiz.querySelectorAll(".revela"));
    if (!itens.length) return;
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches || !("IntersectionObserver" in window)) {
      itens.forEach(function (el) { el.classList.add("visivel"); });
      return;
    }
    var obs = new IntersectionObserver(function (entradas) {
      entradas.forEach(function (entrada) {
        if (!entrada.isIntersecting) return;
        entrada.target.classList.add("visivel");
        obs.unobserve(entrada.target);
      });
    }, { threshold: 0.12, rootMargin: "0px 0px -36px 0px" });
    itens.forEach(function (el, i) {
      el.style.setProperty("--atraso", (Math.min(i % 8, 7) * 55) + "ms");
      obs.observe(el);
    });
  }

  root.CriaUi = {
    url: url,
    esc: esc,
    vazio: vazio,
    nomeCategoria: nomeCategoria,
    nomeModalidade: nomeModalidade,
    modalidadeInfo: modalidadeInfo,
    revelar: revelar,
    icone: function (nome) { return icones[nome] || ""; },
    redes: function () { return redes(root.Cria.estado.identidade, false); }
  };
  root.CriaCasca = { montar: montar, atualizar: montar, ehAdmin: ehAdmin };
  montar();
})(window);
