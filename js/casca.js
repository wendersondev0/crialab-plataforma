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

  function link(href, texto, atual) {
    return '<a class="nav-link' + (atual ? " ativo" : "") + '" href="' + url(href) + '">' + texto + "</a>";
  }

  function redes(identidade) {
    return [
      ["WhatsApp", identidade.whatsapp],
      ["Instagram", identidade.instagram],
      ["TikTok", identidade.tiktok],
      ["YouTube", identidade.youtube],
      ["Shopee", identidade.shopee],
      ["Mercado Livre", identidade.mercadoLivre]
    ].map(function (item) {
      return '<a class="nav-link" href="' + esc(item[1]) + '" target="_blank" rel="noopener">' + item[0] + "</a>";
    }).join("");
  }

  function montar() {
    var estado = root.Cria.estado;
    var identidade = estado.identidade;
    var topo = document.getElementById("topo");
    var rodape = document.getElementById("rodape");
    if (!topo || !rodape) return;
    var admin = ehAdmin();
    var menu = admin
      ? [
          link("paginas/admin/index.html", "Painel"),
          link("paginas/admin/produtos.html", "Produtos"),
          link("paginas/admin/configuracao.html", "Configuração"),
          link("paginas/admin/financeiro.html", "Financeiro"),
          link("paginas/admin/patrimonio.html", "Patrimônio"),
          link("paginas/admin/estoque.html", "Estoque"),
          link("paginas/admin/impressoes.html", "Impressões"),
          link("paginas/admin/orcamentos.html", "Orçamentos"),
          link("paginas/admin/pedidos.html", "Pedidos"),
          link("paginas/admin/contato.html", "Contato"),
          link("index.html", "Ver loja")
        ].join("")
      : [
          link("index.html", "Início"),
          link("paginas/busca.html", "Busca"),
          link("paginas/carrinho.html", "Carrinho (" + quantidadeCarrinho() + ")"),
          link("paginas/conta.html", "Conta"),
          link("paginas/contato.html", "Contato")
        ].join("");
    topo.innerHTML = '<div class="barra"><a class="marca" href="' + url("index.html") + '"><img src="' + url("marca/logo-colorida.png") + '" alt="Cria Lab DM"></a><nav class="menu">' + menu + "</nav></div>";
    var endereco = identidade.endereco ? esc(identidade.endereco) : "Endereço a definir";
    var horario = identidade.horario ? esc(identidade.horario) : "Horário a definir";
    rodape.innerHTML = '<div class="rodape-grade"><div><img class="logo-rodape" src="' + url("marca/logo-colorida.png") + '" alt=""><strong>' + esc(identidade.nome) + '</strong><p>CNPJ ' + esc(identidade.cnpj) + "</p><p>" + endereco + "</p><p>" + horario + '</p></div><div><p><a href="mailto:' + esc(identidade.email) + '">' + esc(identidade.email) + '</a></p><p><a href="tel:+5527996530954">' + esc(identidade.telefone) + "</a></p><div class=\"menu\">" + redes(identidade) + '</div></div><div class="menu">' + link("paginas/sobre.html", "Sobre") + link("paginas/admin/index.html", "Administração") + "</div></div>";
  }

  function vazio(texto) {
    return '<div class="vazio"><img src="' + url("marca/mascote.png") + '" alt="Mascote da Cria Lab DM"><p>' + esc(texto) + "</p></div>";
  }

  function nomeCategoria(id) {
    var categoria = (root.Cria.estado.categorias || []).find(function (item) { return item.id === id; });
    return categoria ? categoria.nome : id;
  }

  root.CriaUi = { url: url, esc: esc, vazio: vazio, nomeCategoria: nomeCategoria };
  root.CriaCasca = { montar: montar, atualizar: montar, ehAdmin: ehAdmin };
  montar();
})(window);
