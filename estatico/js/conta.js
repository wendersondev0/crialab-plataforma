(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");

  function estado() { return Cria.estado; }

  function ico(nome) {
    return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
  }

  function conta() {
    document.title = "Minha conta · Cria Lab DM";
    var params = new URLSearchParams(location.search);
    var abasOk = { pedidos: 1, orcamentos: 1, favoritos: 1, enderecos: 1, dados: 1, perguntas: 1 };
    var vista = {
      aba: abasOk[params.get("aba")] ? params.get("aba") : "pedidos",
      ok: ""
    };

    function sincronizarUrl() {
      var url = new URL(location.href);
      url.searchParams.set("aba", vista.aba);
      history.replaceState(null, "", url.pathname + url.search);
    }

    function flash(msg) {
      vista.ok = msg;
      pintar();
      window.setTimeout(function () {
        if (vista.ok === msg) { vista.ok = ""; pintar(); }
      }, 2200);
    }

    function seloStatus(status) {
      var mapa = {
        "aguardando pagamento": "cta-selo alerta",
        pago: "cta-selo ok",
        "em produção": "cta-selo",
        enviado: "cta-selo",
        entregue: "cta-selo ok",
        cancelado: "cta-selo mute",
        devolvido: "cta-selo mute",
        aguardando: "cta-selo alerta",
        respondido: "cta-selo ok",
        aceito: "cta-selo ok",
        recusado: "cta-selo mute"
      };
      return mapa[status] || "cta-selo";
    }

    function pintar() {
      sincronizarUrl();
      var vazio = estado().perfil === "vazia";
      var cliente = estado().cliente || {};
      var pedidos = vazio ? [] : (estado().pedidos || []).slice().sort(function (a, b) {
        return String(b.criadoEm || "").localeCompare(String(a.criadoEm || ""));
      });
      var orcamentos = vazio ? [] : (estado().orcamentosCliente || []).slice();
      var favoritos = vazio ? [] : (estado().favoritos || []).map(function (id) {
        return estado().produtos.find(function (produto) { return produto.id === id; });
      }).filter(Boolean);
      var enderecos = vazio ? [] : (estado().enderecos || []);
      var perguntas = vazio ? [] : (estado().perguntas || []).filter(function (item) {
        return item.autor === cliente.nome;
      });
      var aguardando = pedidos.filter(function (item) { return item.status === "aguardando pagamento"; }).length;
      var orcPend = orcamentos.filter(function (item) { return item.status === "respondido" || item.status === "aguardando"; }).length;
      var nomeExib = vazio ? "Conta vazia" : (cliente.nome || "Cliente");

      var kpis = '<div class="cta-kpis">' +
        '<button class="cta-kpi' + (vista.aba === "pedidos" ? " ativo" : "") + '" type="button" data-aba="pedidos"><strong>' + pedidos.length + '</strong><span>Pedidos</span></button>' +
        '<button class="cta-kpi' + (vista.aba === "orcamentos" ? " ativo" : "") + '" type="button" data-aba="orcamentos"><strong>' + orcamentos.length + '</strong><span>Orçamentos</span></button>' +
        '<button class="cta-kpi' + (vista.aba === "favoritos" ? " ativo" : "") + '" type="button" data-aba="favoritos"><strong>' + favoritos.length + '</strong><span>Favoritos</span></button>' +
        '<button class="cta-kpi' + (vista.aba === "enderecos" ? " ativo" : "") + '" type="button" data-aba="enderecos"><strong>' + enderecos.length + '</strong><span>Endereços</span></button>' +
        '<button class="cta-kpi' + (vista.aba === "perguntas" ? " ativo" : "") + '" type="button" data-aba="perguntas"><strong>' + perguntas.length + '</strong><span>Perguntas</span></button>' +
        "</div>";

      var abas = '<div class="cta-abas">' +
        [["pedidos", "Pedidos"], ["orcamentos", "Orçamentos"], ["favoritos", "Favoritos"], ["enderecos", "Endereços"], ["perguntas", "Perguntas"], ["dados", "Meus dados"]].map(function (par) {
          return '<button class="cta-aba' + (vista.aba === par[0] ? " ativo" : "") + '" type="button" data-aba="' + par[0] + '">' + par[1] + "</button>";
        }).join("") + "</div>";

      var alerta = "";
      if (!vazio && aguardando) alerta += '<button class="cta-alerta" type="button" data-aba="pedidos">' + aguardando + " pedido" + (aguardando === 1 ? "" : "s") + " aguardando pagamento</button>";
      if (!vazio && orcPend) alerta += '<button class="cta-alerta" type="button" data-aba="orcamentos">' + orcPend + " orçamento" + (orcPend === 1 ? "" : "s") + " para olhar</button>";
      if (alerta) alerta = '<div class="cta-alertas">' + alerta + "</div>";

      var okHtml = vista.ok ? '<p class="cta-ok">' + ui.esc(vista.ok) + "</p>" : "";
      var corpo = "";

      if (vista.aba === "pedidos") {
        corpo = '<div class="cta-lista">' + (pedidos.length ? pedidos.map(function (pedido) {
          var itens = (pedido.itens || []).map(function (item) {
            return ui.esc(item.titulo) + " × " + item.quantidade;
          }).join(", ");
          var acoes = '<div class="cta-acoes">';
          if (pedido.status === "aguardando pagamento") {
            acoes += '<button class="botao" type="button" data-pagar="' + ui.esc(pedido.id) + '">' + ico("carrinho") + "<span>Simular pagamento</span></button>";
            acoes += '<button class="botao-linha" type="button" data-cancelar="' + ui.esc(pedido.id) + '"><span>Cancelar</span></button>';
          }
          if (pedido.status === "entregue") {
            acoes += '<button class="botao-linha" type="button" data-devolver="' + ui.esc(pedido.id) + '"><span>Pedir devolução</span></button>';
          }
          if (pedido.status !== "aguardando pagamento" && pedido.status !== "cancelado") {
            acoes += '<a class="botao-linha" href="' + ui.url("paginas/recibo.html?id=" + encodeURIComponent(pedido.id)) + '">' + ico("documento") + "<span>Recibo</span></a>";
          }
          acoes += "</div>";
          return '<article class="cta-card">' +
            '<div class="cta-card-topo">' +
              '<div><strong class="cta-id">' + ui.esc(pedido.id) + "</strong>" +
              '<p class="cta-meta">' + ui.esc(dinheiro.dataBR(String(pedido.criadoEm || "").slice(0, 10))) +
              (pedido.forma ? " · " + ui.esc(pedido.forma) : "") + "</p></div>" +
              '<span class="' + seloStatus(pedido.status) + '">' + ui.esc(pedido.status) + "</span>" +
            "</div>" +
            '<p class="cta-itens">' + (itens || "Sem itens") + "</p>" +
            '<div class="cta-total"><span>Total</span><strong>' + dinheiro.real(pedido.total) + "</strong></div>" +
            acoes +
            "</article>";
        }).join("") : ui.vazio("Nenhum pedido por aqui.")) + "</div>";
      } else if (vista.aba === "orcamentos") {
        corpo = '<div class="cta-lista">' + (orcamentos.length ? orcamentos.map(function (item) {
          var acoes = '<div class="cta-acoes">';
          if (item.status === "respondido") {
            acoes += '<button class="botao" type="button" data-aceitar="' + ui.esc(item.id) + '">' + ico("carrinho") + "<span>Aceitar " + dinheiro.real(item.valor) + "</span></button>";
            acoes += '<button class="botao-linha" type="button" data-recusar="' + ui.esc(item.id) + '"><span>Recusar</span></button>';
          }
          acoes += "</div>";
          return '<article class="cta-card">' +
            '<div class="cta-card-topo">' +
              '<div><strong class="cta-id">' + ui.esc(item.titulo) + "</strong>" +
              '<p class="cta-meta">' + ui.esc(dinheiro.dataBR(String(item.criadoEm || "").slice(0, 10))) + "</p></div>" +
              '<span class="' + seloStatus(item.status) + '">' + ui.esc(item.status) + "</span>" +
            "</div>" +
            '<p class="cta-itens">' + ui.esc(item.detalhes || "") + "</p>" +
            (item.valor != null ? '<div class="cta-total"><span>Valor proposto</span><strong>' + dinheiro.real(item.valor) + "</strong></div>" : '<p class="cta-meta">Aguardando o valor da oficina.</p>') +
            acoes +
            "</article>";
        }).join("") : ui.vazio("Nenhum orçamento.")) + "</div>";
      } else if (vista.aba === "favoritos") {
        corpo = favoritos.length
          ? '<div class="cta-fav-grade">' + favoritos.map(function (produto) {
            var foto = (produto.fotos && produto.fotos[0]) || { arquivo: "marca/mascote.png", legenda: "" };
            return '<a class="cta-fav" href="' + ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id)) + '">' +
              '<img src="' + ui.url(foto.arquivo) + '" alt="' + ui.esc(foto.legenda || produto.titulo) + '">' +
              '<div><strong>' + ui.esc(produto.titulo) + "</strong>" +
              '<span>' + dinheiro.real(produto.preco) + "</span></div></a>";
          }).join("") + "</div>"
          : ui.vazio("Nenhum favorito.");
      } else if (vista.aba === "enderecos") {
        corpo = '<div class="cta-endereco-layout">' +
          '<div class="cta-lista">' + (enderecos.length ? enderecos.map(function (item) {
            return '<article class="cta-card cta-end">' +
              '<div class="cta-card-topo"><strong class="cta-id">' + ui.esc(item.rotulo || "Endereço") + "</strong>" +
              '<button class="botao-linha" type="button" data-apagar-end="' + ui.esc(item.id) + '">Remover</button></div>' +
              '<p class="cta-itens m-0">' + ui.esc(item.linha) + "</p>" +
              '<p class="cta-meta m-0">CEP ' + ui.esc(item.cep) + "</p></article>";
          }).join("") : ui.vazio("Nenhum endereço guardado.")) + "</div>" +
          '<form id="novo-endereco" class="cta-card cta-form">' +
          '<h2 class="cta-titulo">Novo endereço</h2>' +
          '<label class="cta-campo"><span>Apelido</span><input name="rotulo" placeholder="Casa, Trabalho…" value="Extra"></label>' +
          '<label class="cta-campo"><span>Endereço completo</span><input name="linha" required placeholder="Rua, número, bairro, cidade/UF"></label>' +
          '<label class="cta-campo"><span>CEP</span><input name="cep" required inputmode="numeric" placeholder="29000-000"></label>' +
          '<button class="botao" type="submit">' + ico("caminhao") + "<span>Guardar endereço</span></button>" +
          "</form></div>";
      } else if (vista.aba === "perguntas") {
        corpo = '<div class="cta-lista">' + (perguntas.length ? perguntas.map(function (item) {
          var produto = estado().produtos.find(function (p) { return p.id === item.produtoId; });
          return '<article class="cta-card">' +
            '<div class="cta-card-topo">' +
              '<strong class="cta-id">' + ui.esc(produto ? produto.titulo : "Produto") + "</strong>" +
              '<span class="' + (item.resposta ? "cta-selo ok" : "cta-selo alerta") + '">' + (item.resposta ? "Respondida" : "Aguardando") + "</span>" +
            "</div>" +
            '<p class="cta-itens">' + ui.esc(item.texto) + "</p>" +
            '<p class="cta-meta">' + ui.esc(item.resposta || "A oficina ainda não respondeu.") + "</p>" +
            (produto ? '<a class="botao-linha" href="' + ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id)) + '">Ver produto</a>' : "") +
            "</article>";
        }).join("") : ui.vazio("Você ainda não fez perguntas nos anúncios.")) + "</div>";
      } else {
        corpo = '<form id="dados" class="cta-card cta-form cta-dados">' +
          '<h2 class="cta-titulo">Dados pessoais</h2>' +
          '<p class="cta-meta">Sem senha nesta etapa da loja estática. Os dados ficam neste navegador.</p>' +
          '<div class="cta-grade">' +
          '<label class="cta-campo"><span>Nome</span><input name="nome" value="' + ui.esc(vazio ? "" : (cliente.nome || "")) + '"></label>' +
          '<label class="cta-campo"><span>E-mail</span><input name="email" type="email" value="' + ui.esc(vazio ? "" : (cliente.email || "")) + '"></label>' +
          '<label class="cta-campo"><span>Telefone</span><input name="telefone" value="' + ui.esc(vazio ? "" : (cliente.telefone || "")) + '"></label>' +
          '<label class="cta-campo"><span>Documento</span><input name="documento" value="' + ui.esc(vazio ? "" : (cliente.documento || "")) + '"></label>' +
          "</div>" +
          '<button class="botao" type="submit">' + ico("conta") + "<span>Salvar dados</span></button>" +
          "</form>";
      }

      raiz.innerHTML =
        '<div class="cta-cab">' +
          '<div class="cta-cab-txt">' +
            '<p class="cta-eyebrow">Minha conta</p>' +
            '<h1 class="cta-nome">' + ui.esc(nomeExib) + "</h1>" +
            '<p class="cta-intro">' + (vazio
              ? "Demonstração sem pedidos. Volte à conta exemplo para ver o fluxo completo."
              : "Pedidos, orçamentos, favoritos e endereços em um só lugar.") + "</p>" +
          "</div>" +
          '<button class="botao-linha cta-trocar" type="button" id="trocar-perfil">' +
            ico("conta") + "<span>" + (vazio ? "Voltar à conta exemplo" : "Ver conta vazia") + "</span></button>" +
        "</div>" +
        kpis + abas + alerta + okHtml + '<div class="cta-corpo">' + corpo + "</div>";

      raiz.querySelectorAll("[data-aba]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-aba") || "pedidos";
          vista.ok = "";
          pintar();
        });
      });

      document.getElementById("trocar-perfil").addEventListener("click", function () {
        Cria.alterar(function (atual) { atual.perfil = atual.perfil === "vazia" ? "exemplo" : "vazia"; });
        vista.aba = "pedidos";
        flash(estado().perfil === "vazia" ? "Conta vazia ativa." : "Conta exemplo ativa.");
      });

      var formDados = document.getElementById("dados");
      if (formDados) formDados.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        Cria.alterar(function (atual) {
          atual.cliente.nome = String(dados.get("nome") || "");
          atual.cliente.email = String(dados.get("email") || "");
          atual.cliente.telefone = String(dados.get("telefone") || "");
          atual.cliente.documento = String(dados.get("documento") || "");
          atual.perfil = "exemplo";
        });
        flash("Dados salvos neste navegador.");
      });

      var formEndereco = document.getElementById("novo-endereco");
      if (formEndereco) formEndereco.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(formEndereco);
        Cria.alterar(function (atual) {
          atual.enderecos.push({
            id: CriaCarrinho.novoId("end"),
            rotulo: String(dados.get("rotulo") || "Extra").trim() || "Extra",
            linha: String(dados.get("linha")),
            cep: String(dados.get("cep")).replace(/\D/g, "")
          });
          atual.perfil = "exemplo";
        });
        flash("Endereço guardado.");
      });

      raiz.querySelectorAll("[data-apagar-end]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-apagar-end");
          Cria.alterar(function (atual) {
            atual.enderecos = (atual.enderecos || []).filter(function (item) { return item.id !== id; });
          });
          flash("Endereço removido.");
        });
      });

      raiz.querySelectorAll("[data-pagar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-pagar");
          Cria.alterar(function (atual) {
            var pedido = atual.pedidos.find(function (item) { return item.id === id; });
            if (!pedido) return;
            pedido.status = "pago";
            pedido.itens.forEach(function (item) {
              if (!item.fisico || item.tipo === "personalizado") return;
              var produto = atual.produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
              if (produto) produto.estoque = Math.max(0, produto.estoque - item.quantidade);
            });
          });
          flash("Pagamento simulado. Pedido marcado como pago.");
        });
      });
      raiz.querySelectorAll("[data-cancelar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-cancelar");
          Cria.alterar(function (atual) {
            var pedido = atual.pedidos.find(function (item) { return item.id === id; });
            if (pedido && pedido.status === "aguardando pagamento") pedido.status = "cancelado";
          });
          flash("Pedido cancelado.");
        });
      });
      raiz.querySelectorAll("[data-devolver]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-devolver");
          Cria.alterar(function (atual) {
            var pedido = atual.pedidos.find(function (item) { return item.id === id; });
            if (pedido) pedido.status = "devolvido";
          });
          flash("Devolução pedida.");
        });
      });
      raiz.querySelectorAll("[data-aceitar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-aceitar");
          Cria.alterar(function (atual) {
            var orcamento = atual.orcamentosCliente.find(function (item) { return item.id === id; });
            if (!orcamento || orcamento.status !== "respondido") return;
            orcamento.status = "aceito";
            atual.pedidos.push({
              id: CriaCarrinho.novoId("ped"),
              status: "pago",
              criadoEm: dinheiro.agoraSP(),
              itens: [{ id: CriaCarrinho.novoId("item"), produtoId: orcamento.produtoId, titulo: orcamento.titulo, tipo: "personalizado", fisico: true, arquivo: false, quantidade: 1, precoUnitario: orcamento.valor, detalhes: orcamento.detalhes }],
              subtotal: orcamento.valor,
              descontoCupom: 0,
              descontoAVista: 0,
              freteCobrado: 0,
              total: orcamento.valor,
              forma: "pix",
              endereco: (atual.enderecos[0] && atual.enderecos[0].linha) || "",
              cep: (atual.enderecos[0] && atual.enderecos[0].cep) || "",
              freteNome: "A combinar",
              cupom: "",
              repassarFrete: atual.configuracao.repassarFrete,
              nota: "pendente",
              os: { status: "aberta", insumoBaixado: false }
            });
          });
          vista.aba = "pedidos";
          flash("Orçamento aceito. Pedido criado.");
        });
      });
      raiz.querySelectorAll("[data-recusar]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-recusar");
          Cria.alterar(function (atual) {
            var orcamento = atual.orcamentosCliente.find(function (item) { return item.id === id; });
            if (orcamento) orcamento.status = "recusado";
          });
          flash("Orçamento recusado.");
        });
      });

      ui.revelar(raiz);
    }

    pintar();
  }

  function telHref(telefone) {
    var digitos = String(telefone || "").replace(/\D/g, "");
    if (digitos.length < 8) return "";
    if (digitos.indexOf("55") !== 0 && digitos.length <= 11) digitos = "55" + digitos;
    return "tel:+" + digitos;
  }
  function contato() {
    document.title = "Contato · Cria Lab DM";
    var id = estado().identidade || {};
    var cliente = estado().cliente || {};
    var opcoes = [
      ["duvida", "Dúvida"],
      ["sugestao", "Sugestão"],
      ["elogio", "Elogio"],
      ["reclamacao", "Reclamação"],
      ["orcamento", "Orçamento"],
      ["outro", "Outro"]
    ];
    var canais = [];
    if (id.whatsapp) canais.push('<a class="contato-canal contato-canal-whats" href="' + ui.esc(id.whatsapp) + '" target="_blank" rel="noopener"><span class="ico">' + (ui.icone ? ui.icone("whatsapp") : "") + '</span><span><strong>WhatsApp</strong><small>Resposta mais rápida</small></span></a>');
    if (id.email) canais.push('<a class="contato-canal" href="mailto:' + ui.esc(id.email) + '"><span class="ico">' + (ui.icone ? ui.icone("email") : "") + '</span><span><strong>E-mail</strong><small>' + ui.esc(id.email) + "</small></span></a>");
    var fone = telHref(id.telefone);
    if (fone) canais.push('<a class="contato-canal" href="' + fone + '"><span class="ico">' + (ui.icone ? ui.icone("telefone") : "") + '</span><span><strong>Telefone</strong><small>' + ui.esc(id.telefone) + "</small></span></a>");
    var local = "";
    if (id.endereco || id.horario) {
      local = '<p class="contato-local">' + (id.endereco ? "<span>" + ui.esc(id.endereco) + "</span>" : "") + (id.horario ? "<span>" + ui.esc(id.horario) + "</span>" : "") + "</p>";
    }
    var selects = opcoes.map(function (item) {
      return '<option value="' + item[0] + '">' + ui.esc(item[1]) + "</option>";
    }).join("");
    raiz.innerHTML = [
      '<section class="contato-pagina">',
      '<header class="contato-cab"><h1 class="mt-0">Contato</h1><p class="intro-pagina m-0 max-w-3xl text-tinta/75">Manda uma mensagem para a oficina. Se for mais rápido, chama no WhatsApp.</p></header>',
      '<div class="contato-layout">',
      '<aside class="contato-canais cartao"><h2>Fale direto</h2>' + canais.join("") + local + "</aside>",
      '<form id="contato" class="contato-form cartao">',
      "<h2>Escreva para a oficina</h2>",
      '<p class="contato-ok" id="contato-ok" hidden>Mensagem enviada. A oficina responde no e-mail que você informou.</p>',
      '<p class="contato-aviso" id="contato-aviso" hidden role="alert"></p>',
      '<label>Nome<input name="nome" required minlength="2" maxlength="120" autocomplete="name" value="' + ui.esc(cliente.nome || "") + '"></label>',
      '<label>E-mail<input name="email" type="email" required maxlength="180" autocomplete="email" value="' + ui.esc(cliente.email || "") + '"></label>',
      '<label>Tipo<select name="tipo" id="tipo-contato" required>' + selects + "</select></label>",
      '<p class="contato-dica" id="dica-orcamento" hidden>Conte o que quer imprimir, a quantidade e se já tem foto ou arquivo.</p>',
      '<label>Mensagem<textarea name="texto" required minlength="8" maxlength="4000" rows="6"></textarea></label>',
      '<button class="botao" type="submit">Enviar mensagem</button>',
      "</form></div></section>"
    ].join("");
    var tipo = document.getElementById("tipo-contato");
    var dica = document.getElementById("dica-orcamento");
    if (tipo && dica) {
      tipo.addEventListener("change", function () {
        dica.hidden = tipo.value !== "orcamento";
      });
    }
    document.getElementById("contato").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      var nome = String(dados.get("nome") || "").trim();
      var email = String(dados.get("email") || "").trim();
      var texto = String(dados.get("texto") || "").trim();
      var valorTipo = String(dados.get("tipo") || "");
      var rotulo = (opcoes.find(function (item) { return item[0] === valorTipo; }) || ["", ""])[1];
      var aviso = document.getElementById("contato-aviso");
      var ok = document.getElementById("contato-ok");
      var falhas = [];
      if (nome.length < 2) falhas.push("Diga como podemos te chamar.");
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) falhas.push("Informe um e-mail válido.");
      if (!rotulo) falhas.push("Escolha o tipo da mensagem.");
      if (texto.length < 8) falhas.push("Escreva a mensagem com um pouco mais de detalhe.");
      if (falhas.length) {
        ok.hidden = true;
        aviso.hidden = false;
        aviso.textContent = falhas[0];
        return;
      }
      Cria.alterar(function (atual) {
        atual.mensagens.push({
          id: CriaCarrinho.novoId("msg"),
          tipo: rotulo,
          nome: nome,
          email: email,
          texto: texto,
          status: "aberto",
          criadoEm: dinheiro.agoraSP()
        });
      });
      aviso.hidden = true;
      ok.hidden = false;
      evento.target.texto.value = "";
    });
  }

  if (pagina === "conta") conta();
  if (pagina === "contato") contato();
})();
