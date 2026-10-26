(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function pedidos() {
    document.title = "Pedidos · Administração";
    var orcamentos = estado().orcamentosCliente.filter(function (item) { return item.status === "aguardando" || item.status === "respondido"; });
    raiz.innerHTML = "<h1>Pedidos</h1>" + estado().pedidos.map(function (pedido) {
      var liquido = pedido.total - (estado().configuracao.repassarFrete ? pedido.freteCobrado : 0);
      return '<article class="cartao"><strong>' + ui.esc(pedido.id) + "</strong><p>" + ui.esc(pedido.status) + " · " + dinheiro.real(pedido.total) + "</p><p>Líquido da venda " + dinheiro.real(liquido) + '</p><p>Nota fiscal: pendente de transmissão</p><div class="linha"><a class="botao-linha" href="' + ui.url("paginas/recibo.html?id=" + encodeURIComponent(pedido.id)) + '">Recibo</a>' + (pedido.itens.some(function (item) { return item.tipo === "personalizado"; }) ? '<a class="botao-linha" href="' + ui.url("paginas/admin/os.html?id=" + encodeURIComponent(pedido.id)) + '">Ordem de serviço</a>' : "") + "</div></article>";
    }).join("") + "<h2>Personalizados aguardando preço</h2>" + (orcamentos.length ? orcamentos.map(function (item) {
      return '<form class="cartao formulario" data-id="' + ui.esc(item.id) + '"><strong>' + ui.esc(item.titulo) + "</strong><p>" + ui.esc(item.detalhes) + '</p><label>Valor<input name="valor" type="number" step="0.01" value="' + (item.valor || "") + '" required></label><button class="botao" type="submit">Devolver o valor</button></form>';
    }).join("") : "<p>Nenhum personalizado pendente.</p>");
    raiz.querySelectorAll("form").forEach(function (form) {
      form.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var id = form.getAttribute("data-id");
        var valor = Number(new FormData(form).get("valor"));
        Cria.alterar(function (atual) {
          var orcamento = atual.orcamentosCliente.find(function (item) { return item.id === id; });
          if (orcamento) { orcamento.valor = valor; orcamento.status = "respondido"; }
        });
        pedidos();
      });
    });
  }

  function os() {
    var id = new URLSearchParams(location.search).get("id");
    var pedido = estado().pedidos.find(function (item) { return item.id === id; });
    if (!pedido) { raiz.innerHTML = ui.vazio("Pedido não encontrado."); return; }
    document.title = "Ordem de serviço · Administração";
    raiz.innerHTML = '<h1>Ordem de serviço</h1><article class="cartao"><p>' + ui.esc(pedido.id) + "</p><p>" + pedido.itens.map(function (item) { return ui.esc(item.titulo) + " · " + dinheiro.real(item.precoUnitario); }).join("<br>") + '</p><p>Status do pedido: ' + ui.esc(pedido.status) + '</p><button class="botao" type="button" id="concluir">Concluir ordem</button> <a class="botao-linha" href="' + ui.url("paginas/recibo.html?id=" + encodeURIComponent(pedido.id)) + '">Recibo</a></article>';
    document.getElementById("concluir").addEventListener("click", function () {
      Cria.alterar(function (atual) {
        var salvo = atual.pedidos.find(function (item) { return item.id === pedido.id; });
        if (!salvo) return;
        salvo.status = "entregue";
        salvo.os = { status: "concluida", insumoBaixado: true };
        var insumo = atual.estoqueInsumos[0];
        if (insumo) insumo.saldo = Math.max(0, insumo.saldo - 0.05);
      });
      os();
    });
  }

  function recibo() {
    var id = new URLSearchParams(location.search).get("id");
    var pedido = estado().pedidos.find(function (item) { return item.id === id; });
    if (!pedido) { raiz.innerHTML = ui.vazio("Recibo não encontrado."); return; }
    var identidade = estado().identidade;
    document.title = "Recibo " + pedido.id;
    raiz.innerHTML = '<article class="cartao"><img src="' + ui.url("marca/logo-preta.svg") + '" alt="Cria Lab DM" style="height:64px"><h1>Recibo</h1><p>' + ui.esc(identidade.nome) + " · CNPJ " + ui.esc(identidade.cnpj) + "</p><p>" + ui.esc(pedido.id) + " · " + ui.esc(dinheiro.dataBR(pedido.criadoEm)) + "</p><table><tbody>" + pedido.itens.map(function (item) {
      return "<tr><td>" + ui.esc(item.titulo) + "</td><td>" + item.quantidade + "</td><td>" + dinheiro.real(item.precoUnitario * item.quantidade) + "</td></tr>";
    }).join("") + "</tbody></table><p>Desconto à vista " + dinheiro.real(pedido.descontoAVista || 0) + "</p><p>Frete " + dinheiro.real(pedido.freteCobrado || 0) + "</p><p class=\"numero-grande\">Total " + dinheiro.real(pedido.total) + '</p><p>Nota fiscal: pendente de transmissão</p><button class="botao no-print" type="button" onclick="print()">Imprimir</button></article>';
  }

  if (pagina === "admin-pedidos") pedidos();
  if (pagina === "admin-os") os();
  if (pagina === "recibo") recibo();
})();
