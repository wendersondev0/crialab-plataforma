(function (root) {
  var CHAVE = "crialab-dm-v1";

  function clonar(valor) {
    return JSON.parse(JSON.stringify(valor));
  }

  function cargaInicial() {
    var dados = root.CriaDados;
    return {
      identidade: clonar(dados.identidade),
      categorias: clonar(dados.categorias),
      configuracao: clonar(dados.configuracao),
      depoimentos: clonar(dados.depoimentos),
      parceiros: clonar(dados.parceiros),
      produtos: clonar(dados.produtos),
      cliente: clonar(dados.cliente),
      enderecos: clonar(dados.enderecos),
      pedidos: clonar(dados.pedidos),
      orcamentosCliente: clonar(dados.orcamentosCliente),
      avaliacoes: clonar(dados.avaliacoes),
      perguntas: clonar(dados.perguntas),
      mensagens: clonar(dados.mensagens),
      lancamentos: clonar(dados.financeiro.lancamentos),
      abaTotal: clonar(dados.financeiro.abaTotal),
      maquinas: clonar(dados.maquinas),
      estoqueInsumos: clonar(dados.estoqueInsumos),
      impressoes: clonar(dados.impressoes),
      contas: clonar(dados.contas),
      precoKwhAtual: dados.precoKwhAtual,
      metas: clonar(dados.metas),
      fechamentosEnergia: clonar(dados.fechamentosEnergia),
      parametrosCusto: clonar(dados.parametrosCusto),
      margens: clonar(dados.margens),
      descontosOrcamento: clonar(dados.descontosOrcamento),
      referenciaMaquina: clonar(dados.referenciaMaquina),
      niveisPintura: clonar(dados.niveisPintura),
      faixasShopee: clonar(dados.faixasShopee),
      faixasMercadoLivre: clonar(dados.faixasMercadoLivre),
      orcamentos: clonar(dados.orcamentos),
      carrinho: [],
      favoritos: [],
      vistos: [],
      perfil: "exemplo",
      guiaVisto: false
    };
  }

  function ler() {
    try {
      var salvo = localStorage.getItem(CHAVE);
      if (salvo) {
        var estado = JSON.parse(salvo);
        var nomes = (estado.parceiros || []).map(function (item) { return item.nome; });
        if (nomes.indexOf("Ateliê da rua") >= 0 || nomes.indexOf("Parceiro em conversa") >= 0) {
          estado.parceiros = clonar(root.CriaDados.parceiros);
        }
        estado.produtos = estado.produtos || [];
        var faltantes = (root.CriaDados.produtos || []).filter(function (produto) {
          return !estado.produtos.some(function (item) { return item.id === produto.id; });
        });
        if (faltantes.length) estado.produtos = faltantes.map(clonar).concat(estado.produtos);
        estado.produtos.forEach(function (produto) {
          produto.comArquivo = false;
          produto.precoArquivo = 0;
          produto.arquivo = null;
        });
        estado.avaliacoes = estado.avaliacoes || [];
        var avaliacoesNovas = (root.CriaDados.avaliacoes || []).filter(function (item) {
          return !estado.avaliacoes.some(function (salva) { return salva.id === item.id; });
        });
        if (avaliacoesNovas.length) estado.avaliacoes = estado.avaliacoes.concat(avaliacoesNovas.map(clonar));
        estado.perguntas = estado.perguntas || [];
        (root.CriaDados.perguntas || []).forEach(function (item) {
          var salva = estado.perguntas.find(function (pergunta) { return pergunta.id === item.id; });
          if (!salva) estado.perguntas.push(clonar(item));
          else if (salva.texto === "O arquivo inclui licença comercial?") {
            salva.texto = item.texto;
            salva.resposta = item.resposta;
          }
        });
        return estado;
      }
    } catch (erro) {}
    return cargaInicial();
  }

  function gravar(estado) {
    localStorage.setItem(CHAVE, JSON.stringify(estado));
    root.Cria.estado = estado;
  }

  function alterar(fn) {
    var estado = clonar(root.Cria.estado);
    fn(estado);
    gravar(estado);
    return estado;
  }

  function restaurar() {
    localStorage.removeItem(CHAVE);
    gravar(cargaInicial());
  }

  root.Cria = root.Cria || {};
  root.Cria.ler = ler;
  root.Cria.gravar = gravar;
  root.Cria.alterar = alterar;
  root.Cria.restaurar = restaurar;
  root.Cria.estado = ler();
})(window);
