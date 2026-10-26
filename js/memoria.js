(function (root) {
  var CHAVE = "crialab-dm-v1";

  function clonar(valor) {
    return JSON.parse(JSON.stringify(valor));
  }

  function soTamanhoUnico(tamanhos) {
    return Array.isArray(tamanhos) && tamanhos.length === 1 && String(tamanhos[0]).toLowerCase() === "único";
  }

  function sincronizarLegadoVariacoes(produto) {
    var eixos = Array.isArray(produto.variacoes) ? produto.variacoes : [];
    var eixoCor = eixos.find(function (eixo) { return eixo.tipo === "cor"; });
    if (eixoCor) {
      produto.cores = eixoCor.opcoes.map(function (opcao) {
        return { nome: opcao.nome, hex: opcao.hex || "#cccccc" };
      });
    } else if (!Array.isArray(produto.cores)) {
      produto.cores = [];
    }
    var eixoTam = eixos.find(function (eixo) {
      return eixo.tipo === "texto" && /^tamanho$/i.test(String(eixo.nome || ""));
    });
    if (eixoTam) {
      produto.tamanhos = eixoTam.opcoes.map(function (opcao) { return opcao.nome; });
    } else if (!Array.isArray(produto.tamanhos) || !produto.tamanhos.length) {
      produto.tamanhos = ["Único"];
    }
  }

  function normalizarVariacoesProduto(produto) {
    if (!produto) return produto;
    if (Array.isArray(produto.variacoes)) {
      produto.variacoes = produto.variacoes.map(function (eixo, indiceEixo) {
        return {
          id: eixo.id || ("var-" + indiceEixo),
          nome: String(eixo.nome || "Variação").trim() || "Variação",
          tipo: eixo.tipo === "cor" ? "cor" : "texto",
          opcoes: (Array.isArray(eixo.opcoes) ? eixo.opcoes : []).map(function (opcao, indiceOpcao) {
            return {
              id: opcao.id || ("opc-" + indiceEixo + "-" + indiceOpcao),
              nome: String(opcao.nome || "").trim() || ("Opção " + (indiceOpcao + 1)),
              hex: opcao.hex || "#cccccc"
            };
          }).filter(function (opcao) { return opcao.nome; })
        };
      }).filter(function (eixo) { return eixo.opcoes.length; });
      sincronizarLegadoVariacoes(produto);
      return produto;
    }
    var eixos = [];
    if (Array.isArray(produto.cores) && produto.cores.length) {
      eixos.push({
        id: "var-cor",
        nome: "Cor",
        tipo: "cor",
        opcoes: produto.cores.map(function (cor, indice) {
          return {
            id: "cor-" + indice,
            nome: String(cor.nome || "Cor"),
            hex: cor.hex || "#cccccc"
          };
        })
      });
    }
    if (Array.isArray(produto.tamanhos) && produto.tamanhos.length && !soTamanhoUnico(produto.tamanhos)) {
      eixos.push({
        id: "var-tam",
        nome: "Tamanho",
        tipo: "texto",
        opcoes: produto.tamanhos.map(function (tamanho, indice) {
          return { id: "tam-" + indice, nome: String(tamanho) };
        })
      });
    }
    produto.variacoes = eixos;
    sincronizarLegadoVariacoes(produto);
    return produto;
  }

  function cargaInicial() {
    var dados = root.CriaDados;
    var produtos = clonar(dados.produtos);
    produtos.forEach(normalizarVariacoesProduto);
    return {
      identidade: clonar(dados.identidade),
      categorias: clonar(dados.categorias),
      modalidades: clonar(dados.modalidades),
      menusAjuda: clonar(dados.menusAjuda),
      configuracao: clonar(dados.configuracao),
      depoimentos: clonar(dados.depoimentos),
      parceiros: clonar(dados.parceiros),
      produtos: produtos,
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
        (root.CriaDados.parceiros || []).forEach(function (item) {
          var salvo = (estado.parceiros || []).find(function (parceiro) { return parceiro.id === item.id; });
          if (salvo && item.logo) salvo.logo = item.logo;
        });
        estado.cliente = estado.cliente || {};
        if (root.CriaDados.cliente && root.CriaDados.cliente.admin) estado.cliente.admin = true;
        estado.produtos = estado.produtos || [];
        var faltantes = (root.CriaDados.produtos || []).filter(function (produto) {
          return !estado.produtos.some(function (item) { return item.id === produto.id; });
        });
        if (faltantes.length) estado.produtos = faltantes.map(clonar).concat(estado.produtos);
        estado.modalidades = clonar(root.CriaDados.modalidades);
        estado.menusAjuda = clonar(root.CriaDados.menusAjuda);
        estado.produtos.forEach(function (produto) {
          produto.comArquivo = false;
          produto.precoArquivo = 0;
          produto.arquivo = null;
          var origem = (root.CriaDados.produtos || []).find(function (item) { return item.id === produto.id; });
          if (origem && origem.modalidade) produto.modalidade = origem.modalidade;
          if (origem && Number(origem.precoComparativo) > 0) produto.precoComparativo = origem.precoComparativo;
          if (!produto.modalidade) {
            if (produto.tipo === "personalizado") produto.modalidade = "personalizado";
            else if (produto.categoria === "pecas-sob-encomenda" || Number(produto.estoque) === 0) produto.modalidade = "sob-demanda";
            else produto.modalidade = "pronta-entrega";
          }
          normalizarVariacoesProduto(produto);
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
        estado.maquinas = estado.maquinas || [];
        (root.CriaDados.maquinas || []).forEach(function (item) {
          var salva = estado.maquinas.find(function (maq) { return maq.id === item.id; });
          if (!salva) estado.maquinas.push(clonar(item));
          else {
            ["categoria", "imagem", "adquiridoEm", "marca", "modelo", "serial", "local", "status", "notas"].forEach(function (campo) {
              if (item[campo] != null && item[campo] !== "") salva[campo] = item[campo];
            });
          }
        });
        estado.estoqueInsumos = estado.estoqueInsumos || [];
        (root.CriaDados.estoqueInsumos || []).forEach(function (item) {
          var salva = estado.estoqueInsumos.find(function (insumo) { return insumo.id === item.id; });
          if (!salva) estado.estoqueInsumos.push(clonar(item));
          else {
            ["categoria", "imagem", "adquiridoEm", "fornecedor", "origemPlanilha", "cores", "variantes", "minimo", "unidade", "nome"].forEach(function (campo) {
              if (item[campo] != null) salva[campo] = clonar(item[campo]);
            });
            if (Array.isArray(salva.cores) && salva.cores.length) {
              salva.saldo = salva.cores.reduce(function (total, cor) { return total + (Number(cor.saldo) || 0); }, 0);
            } else if (Array.isArray(salva.variantes) && salva.variantes.length) {
              salva.saldo = salva.variantes.reduce(function (total, itemVar) { return total + (Number(itemVar.saldo) || 0); }, 0);
            } else if (item.saldo != null) {
              salva.saldo = item.saldo;
            }
          }
        });
        if (root.CriaDados.configuracao && Array.isArray(root.CriaDados.configuracao.tiposInsumo)) {
          estado.configuracao = estado.configuracao || {};
          estado.configuracao.tiposInsumo = clonar(root.CriaDados.configuracao.tiposInsumo);
        }
        if (Array.isArray(root.CriaDados.parametrosCusto)) {
          estado.parametrosCusto = clonar(root.CriaDados.parametrosCusto);
        }
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
  root.Cria.normalizarVariacoesProduto = normalizarVariacoesProduto;
  root.Cria.sincronizarLegadoVariacoes = sincronizarLegadoVariacoes;
  root.Cria.estado = ler();
})(window);
