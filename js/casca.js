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
    shopee: svg('<path' + traco + ' d="M6 8h12l-1 11H7z"/><path' + traco + ' d="M9 8V7a3 3 0 0 1 6 0v1"/>'),
    mercadolivre: svg('<path' + traco + ' d="M4 10 12 4l8 6"/><path' + traco + ' d="M6 10v8h12v-8"/><path' + traco + ' d="M10 18v-4h4v4"/>'),
    email: svg('<rect' + traco + ' x="3" y="5" width="18" height="14" rx="2"/><path' + traco + ' d="m4 7 8 6 8-6"/>'),
    telefone: svg('<path' + traco + ' d="M8 4h3l1 4-2 1a12 12 0 0 0 5 5l1-2 4 1v3c0 .6-.4 1-1 1A15 15 0 0 1 5 6c0-.6.4-1 1-1z"/>')
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
    var mapa = { "Início": "casa", "Produtos": "shopee", "Personalizados": "admin", "carrinho": "carrinho", "Conta": "conta", "Contato": "contato", "Sobre": "sobre", "Administração": "admin", "Painel Gerencial": "casa", "Gestão Produtos": "shopee", "Gestão Personalizados": "admin", "Marketing": "tiktok", "Atendimento": "contato", "Patrimônio e Estoque": "mercadolivre", "Gestão Financeira": "carrinho", "Gestão Fiscal": "email", "Impressão 3D": "admin", "Ver loja": "casa" };
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
      return '<a class="icone-rede" data-rede="' + item[0] + '" href="' + esc(item[2]) + '" target="_blank" rel="noopener" aria-label="' + esc(item[1]) + '"><span class="ico">' + icones[item[0]] + '</span><span class="rotulo">' + esc(item[1]) + "</span></a>";
    }).join("") + "</div>";
  }

  function montar() {
    var estado = root.Cria.estado;
    var identidade = estado.identidade;
    var topo = document.getElementById("topo");
    var rodape = document.getElementById("rodape");
    if (!topo || !rodape) return;
    var admin = ehAdmin();
    topo.classList.toggle("topo-loja", !admin);
    if (admin) {
      var menuAdmin = [
        link("paginas/admin/index.html", "Painel Gerencial"),
        link("paginas/admin/produtos.html", "Gestão Produtos"),
        link("paginas/admin/personalizados.html", "Gestão Personalizados"),
        link("paginas/admin/marketing.html", "Marketing"),
        link("paginas/admin/contato.html", "Atendimento"),
        link("paginas/admin/patrimonio.html", "Patrimônio e Estoque"),
        link("paginas/admin/financeiro.html", "Gestão Financeira"),
        link("paginas/admin/fiscal.html", "Gestão Fiscal"),
        link("paginas/admin/configuracao.html", "Administração"),
        link("paginas/admin/impressao.html", "Impressão 3D"),
        link("index.html", "Ver loja")
      ].join("");
      topo.innerHTML = '<div class="barra"><a class="marca" href="' + url("paginas/admin/index.html") + '"><img src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a><nav class="menu">' + menuAdmin + "</nav></div>";
    } else {
      var menuLoja = [
        link("index.html", "Início"),
        link("paginas/produtos.html", "Produtos"),
        link("paginas/personalizados.html", "Personalizados")
      ].join("");
      topo.innerHTML = '<div class="loja-faixa"><div class="barra loja-barra"><a class="marca" href="' + url("index.html") + '"><img src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a><form class="busca-topo" action="' + url("paginas/busca.html") + '" method="get"><input name="q" type="search" placeholder="Buscar na Cria Lab" aria-label="Buscar na Cria Lab"><button class="botao" type="submit">Buscar</button></form><div class="loja-acoes">' + link("paginas/carrinho.html", "Carrinho (" + quantidadeCarrinho() + ")") + link("paginas/conta.html", "Conta") + "</div></div><nav class=\"menu menu-loja\">" + menuLoja + "</nav></div>";
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

  root.CriaUi = { url: url, esc: esc, vazio: vazio, nomeCategoria: nomeCategoria, redes: function () { return redes(root.Cria.estado.identidade, false); } };
  root.CriaCasca = { montar: montar, atualizar: montar, ehAdmin: ehAdmin };
  montar();
})(window);
