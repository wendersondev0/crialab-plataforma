(function (root) {
  function novoId(prefixo) {
    return prefixo + "-" + Date.now().toString(36) + Math.floor(Math.random() * 1000).toString(36);
  }

  function adicionar(item) {
    root.Cria.alterar(function (estado) {
      var assinatura = item.assinatura || "";
      var existente = estado.carrinho.find(function (linha) {
        return linha.produtoId === item.produtoId && linha.tipo === item.tipo && linha.assinatura === assinatura && item.tipo !== "personalizado";
      });
      if (existente) existente.quantidade += item.quantidade;
      else estado.carrinho.push(Object.assign({ id: novoId("cart") }, item));
    });
    if (root.CriaCasca) root.CriaCasca.atualizar();
  }

  function definirQuantidade(id, quantidade) {
    root.Cria.alterar(function (estado) {
      var linha = estado.carrinho.find(function (item) { return item.id === id; });
      if (!linha) return;
      linha.quantidade = Math.max(1, quantidade);
    });
    if (root.CriaCasca) root.CriaCasca.atualizar();
  }

  function remover(id) {
    root.Cria.alterar(function (estado) {
      estado.carrinho = estado.carrinho.filter(function (item) { return item.id !== id; });
    });
    if (root.CriaCasca) root.CriaCasca.atualizar();
  }

  function limpar() {
    root.Cria.alterar(function (estado) { estado.carrinho = []; });
    if (root.CriaCasca) root.CriaCasca.atualizar();
  }

  function cupomPorCodigo(estado, codigo) {
    var texto = String(codigo || "").trim().toUpperCase();
    if (!texto) return null;
    return (estado.configuracao.cupons || []).find(function (cupom) {
      return cupom.ativo && String(cupom.codigo).toUpperCase() === texto;
    }) || null;
  }

  root.CriaCarrinho = { novoId: novoId, adicionar: adicionar, definirQuantidade: definirQuantidade, remover: remover, limpar: limpar, cupomPorCodigo: cupomPorCodigo };
})(window);
