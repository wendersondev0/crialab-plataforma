(function () {
  var atual = document.currentScript.src;
  var base = new URL("../", atual);
  window.CriaBase = base.href;
  var lista = [
    "dados/loja.js",
    "dados/catalogo.js",
    "dados/pedidos.js",
    "dados/mensagens.js",
    "dados/financeiro.js",
    "dados/patrimonio.js",
    "dados/orcamento.js",
    "js/dinheiro.js",
    "js/calculos.js",
    "js/memoria.js",
    "js/carrinho.js",
    "js/casca.js"
  ];
  var html = "";
  for (var i = 0; i < lista.length; i++) html += '<script src="' + new URL(lista[i], base).href + '"><\/script>';
  document.write(html);
})();
