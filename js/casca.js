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
    return '<img class="logo-canal" src="' + url(arquivo) + '" alt="">';
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
    estoque: svg('<path' + traco + ' d="M4 7h16M4 12h16M4 17h16"/><path' + traco + ' d="M8 7v10M16 7v10"/>')
  };

  function ativo(href) {
    var caminho = location.pathname.replace(/\\/g, "/");
    if (href === "index.html") return (caminho === "/" || caminho.endsWith("/index.html")) && caminho.indexOf("/admin/") < 0;
    return caminho.endsWith("/" + href);
  }

  function link(href, texto) {
    var rotulo = texto;
    var qtd = "";
    var carrinho = /^Carrinho \((\d+)\)$/.exec(texto);
    if (carrinho) {
      rotulo = "Carrinho";
      if (Number(carrinho[1]) > 0) qtd = '<span class="selo-qtd">' + carrinho[1] + "</span>";
    }
    var chave = carrinho ? "carrinho" : texto;
    var mapa = { "Início": "casa", "Produtos": "caixa", "Personalizados": "admin", "carrinho": "carrinho", "Conta": "conta", "Contato": "contato", "Sobre": "sobre", "Administração": "admin", "Painel": "casa", "Painel Gerencial": "casa", "Gestão Produtos": "caixa", "Gestão Personalizados": "admin", "Marketing": "tiktok", "Atendimento": "contato", "Patrimônio": "estoque", "Patrimônio e Estoque": "estoque", "Financeiro": "carrinho", "Gestão Financeira": "carrinho", "Fiscal": "email", "Gestão Fiscal": "email", "Sistema": "admin", "Impressão": "admin", "Impressão 3D": "admin", "Ver loja": "casa" };
    var desenho = icones[mapa[chave]] ? '<span class="ico">' + icones[mapa[chave]] + "</span>" : "";
    return '<a class="nav-link' + (ativo(href) ? " ativo" : "") + '" href="' + url(href) + '">' + desenho + esc(rotulo) + qtd + "</a>";
  }

  function redes(identidade, claro) {
    return '<div class="rede-bloco' + (claro ? " claro" : "") + '">' + [
      ["whatsapp", "WhatsApp", identidade.whatsapp],
      ["instagram", "Instagram", identidade.instagram],
      ["tiktok", "TikTok", identidade.tiktok],
      ["youtube", "YouTube", identidade.youtube],
      ["shopee", "Shopee", identidade.shopee],
      ["mercadolivre", "Mercado Livre", identidade.mercadoLivre]
    ].map(function (item) {
      return '<a class="icone-rede" data-rede="' + item[0] + '" href="' + esc(item[2]) + '" target="_blank" rel="noopener" aria-label="' + esc(item[1]) + '"><span class="ico">' + desenhoRede(item[0], claro) + '</span><span class="rotulo">' + esc(item[1]) + "</span></a>";
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
      return '<a class="icone-topo" href="' + esc(item[2]) + '" target="_blank" rel="noopener" aria-label="' + esc(item[1]) + '">' + desenhoRede(item[0], true) + "</a>";
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

  function faixaLoja(estado) {
    var identidade = estado.identidade;
    var vazio = estado.perfil === "vazia";
    var avisos = listaNotificacoes(estado);
    var itensAviso = avisos.length ? avisos.map(function (texto) {
      return '<a href="' + url("paginas/conta.html") + '">' + esc(texto) + "</a>";
    }).join("") : '<p class="popup-vazio">Nenhuma notificação.</p>';
    var selo = avisos.length ? '<span class="selo-qtd">' + avisos.length + "</span>" : "";
    var sair = vazio ? "Entrar" : "Sair";
    var admin = !vazio && estado.cliente && estado.cliente.admin;
    var itemAdmin = admin ? '<a href="' + url("paginas/admin/index.html") + '">Administração</a>' : "";
    return '<div class="faixa-fina"><div class="barra faixa-fina-barra"><div class="siga"><span>Siga a Cria Lab</span>' + iconesSociais(identidade) + '</div><div class="faixa-direita"><div class="menu-suspenso"><button class="atalho" type="button" aria-expanded="false" aria-haspopup="true"><span class="ico">' + icones.sino + "</span>Notificações" + selo + '</button><div class="popup-topo" role="menu">' + itensAviso + '</div></div><a class="atalho" href="' + url("paginas/contato.html") + '">Ajuda</a><div class="menu-suspenso menu-conta"><button class="atalho" type="button" id="abrir-conta" aria-expanded="false" aria-haspopup="true"><span class="ico">' + icones.conta + "</span>" + esc(vazio ? "Conta" : estado.cliente.nome) + '</button><div class="popup-topo" role="menu"><a href="' + url("paginas/conta.html") + '">Minha conta</a><a href="' + url("paginas/conta.html#pedidos") + '">Meus pedidos</a>' + itemAdmin + '<button type="button" id="sair-conta">' + sair + "</button></div></div></div></div></div>";
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
    var sair = document.getElementById("sair-conta");
    if (sair) sair.addEventListener("click", function () {
      Cria.alterar(function (atual) { atual.perfil = atual.perfil === "vazia" ? "exemplo" : "vazia"; });
      location.href = url("paginas/conta.html");
    });
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
    if (admin) {
      var menuAdmin = [
        link("paginas/admin/index.html", "Painel"),
        link("paginas/admin/produtos.html", "Produtos"),
        link("paginas/admin/personalizados.html", "Personalizados"),
        link("paginas/admin/marketing.html", "Marketing"),
        link("paginas/admin/contato.html", "Atendimento"),
        link("paginas/admin/patrimonio.html", "Patrimônio"),
        link("paginas/admin/financeiro.html", "Financeiro"),
        link("paginas/admin/fiscal.html", "Fiscal"),
        link("paginas/admin/configuracao.html", "Sistema"),
        link("paginas/admin/impressao.html", "Impressão")
      ].join("");
      topo.innerHTML = '<div class="barra barra-admin"><a class="marca" href="' + url("paginas/admin/index.html") + '"><img src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a><div class="admin-acoes">' + link("index.html", "Ver loja") + '</div></div><nav class="menu menu-admin">' + menuAdmin + "</nav>";
    } else {
      var menuLoja = [
        link("index.html", "Início"),
        link("paginas/produtos.html", "Produtos"),
        link("paginas/personalizados.html", "Personalizados")
      ].join("");
      topo.innerHTML = faixaLoja(estado) + '<div class="loja-faixa"><div class="barra loja-barra"><a class="marca" href="' + url("index.html") + '"><img src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a><form class="busca-topo" action="' + url("paginas/busca.html") + '" method="get"><input name="q" type="search" placeholder="Buscar na Cria Lab" aria-label="Buscar na Cria Lab"><button class="botao" type="submit">Buscar</button></form><div class="loja-acoes">' + link("paginas/carrinho.html", "Carrinho (" + quantidadeCarrinho() + ")") + "</div></div><nav class=\"menu menu-loja\">" + menuLoja + "</nav></div>";
      ligarMenus(topo);
    }
    var local = (identidade.endereco ? "<p>" + esc(identidade.endereco) + "</p>" : "") + (identidade.horario ? "<p>" + esc(identidade.horario) + "</p>" : "");
    rodape.innerHTML = '<div class="rodape-grade"><div><img class="logo-rodape" src="' + url("marca/logo-colorida.png") + '" alt=""><strong>' + esc(identidade.nome) + '</strong><p>CNPJ ' + esc(identidade.cnpj) + "</p>" + local + '</div><div class="contatos"><a class="contato-linha" href="mailto:' + esc(identidade.email) + '"><span class="ico">' + icones.email + "</span>" + esc(identidade.email) + '</a><a class="contato-linha" href="tel:+5527996530954"><span class="ico">' + icones.telefone + "</span>" + esc(identidade.telefone) + "</a>" + redes(identidade, true) + '</div><div class="menu">' + link("paginas/sobre.html", "Sobre") + link("paginas/admin/index.html", "Administração") + "</div></div>";
  }

  function vazio(texto) {
    return '<div class="vazio"><img src="' + url("marca/mascote.png") + '" alt="Mascote da Cria Lab DM"><p>' + esc(texto) + "</p></div>";
  }

  function nomeCategoria(id) {
    var categoria = (root.Cria.estado.categorias || []).find(function (item) { return item.id === id; });
    return categoria ? categoria.nome : id;
  }

  root.CriaUi = { url: url, esc: esc, vazio: vazio, nomeCategoria: nomeCategoria, icone: function (nome) { return icones[nome] || ""; }, redes: function () { return redes(root.Cria.estado.identidade, false); } };
  root.CriaCasca = { montar: montar, atualizar: montar, ehAdmin: ehAdmin };
  montar();
})(window);
