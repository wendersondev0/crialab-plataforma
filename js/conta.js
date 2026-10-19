(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");

  function estado() { return Cria.estado; }

  function baixarArquivo(item) {
    var conteudo = "Arquivo simulado da Cria Lab DM\n" + item.titulo + "\n" + (item.arquivoFormato || "") + "\nLicença " + (item.arquivoLicenca || "");
    var blob = new Blob([conteudo], { type: "text/plain" });
    var link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = item.arquivoNome || "arquivo-simulado.txt";
    link.click();
    URL.revokeObjectURL(link.href);
  }

  function conta() {
    document.title = "Conta · Cria Lab DM";
    var vazio = estado().perfil === "vazia";
    var cliente = estado().cliente;
    var pedidos = vazio ? [] : estado().pedidos;
    var downloads = [];
    pedidos.forEach(function (pedido) {
      if (pedido.status === "aguardando pagamento" || pedido.status === "cancelado") return;
      pedido.itens.forEach(function (item) {
        if (item.arquivo) downloads.push(item);
      });
    });
    var orcamentos = vazio ? [] : estado().orcamentosCliente;
    var favoritos = vazio ? [] : estado().favoritos.map(function (id) {
      return estado().produtos.find(function (produto) { return produto.id === id; });
    }).filter(Boolean);
    var perguntas = vazio ? [] : estado().perguntas.filter(function (item) { return item.autor === cliente.nome; });
    raiz.innerHTML = '<div class="linha"><h1>Conta</h1><button class="botao-linha" type="button" id="trocar-perfil">' + (vazio ? "Voltar à conta exemplo" : "Ver conta vazia") + '</button></div><section class="cartao"><h2>Dados pessoais</h2><form id="dados" class="formulario"><label>Nome<input name="nome" value="' + ui.esc(vazio ? "" : cliente.nome) + '"></label><label>E-mail<input name="email" type="email" value="' + ui.esc(vazio ? "" : cliente.email) + '"></label><label>Telefone<input name="telefone" value="' + ui.esc(vazio ? "" : cliente.telefone) + '"></label><label>Documento<input name="documento" value="' + ui.esc(vazio ? "" : cliente.documento) + '"></label><button class="botao" type="submit">Salvar</button></form></section><section><h2>Pedidos</h2><div id="pedidos"></div></section><section class="cartao"><h2>Downloads</h2><div id="downloads"></div></section><section class="cartao"><h2>Orçamentos</h2><div id="orcamentos"></div></section><section class="cartao"><h2>Endereços</h2><div id="enderecos"></div><form id="novo-endereco" class="formulario"><label>Novo endereço<input name="linha" required></label><label>CEP<input name="cep" required></label><button class="botao-linha" type="submit">Guardar endereço</button></form></section><section><h2>Favoritos</h2><div id="favoritos" class="grade"></div></section><section class="cartao"><h2>Perguntas</h2><div id="perguntas"></div></section>';
    document.getElementById("pedidos").innerHTML = pedidos.length ? pedidos.map(function (pedido) {
      var devolver = pedido.status === "entregue" ? '<button class="botao-linha" type="button" data-devolver="' + ui.esc(pedido.id) + '">Pedir devolução</button>' : "";
      var recibo = pedido.status !== "aguardando pagamento" && pedido.status !== "cancelado" ? '<a class="botao-linha" href="' + ui.url("paginas/recibo.html?id=" + encodeURIComponent(pedido.id)) + '">Recibo</a>' : "";
      var pagar = pedido.status === "aguardando pagamento" ? '<button class="botao" type="button" data-pagar="' + ui.esc(pedido.id) + '">Simular pagamento</button>' : "";
      return '<article class="cartao"><strong>' + ui.esc(pedido.id) + "</strong><p>" + ui.esc(pedido.status) + " · " + dinheiro.real(pedido.total) + "</p><p>" + pedido.itens.map(function (item) { return ui.esc(item.titulo) + " × " + item.quantidade; }).join(", ") + "</p><div class=\"linha\">" + pagar + devolver + recibo + "</div></article>";
    }).join("") : ui.vazio("Nenhum pedido por aqui.");
    document.getElementById("downloads").innerHTML = downloads.length ? downloads.map(function (item, indice) {
      return '<p><button class="botao-linha" type="button" data-download="' + indice + '">Baixar ' + ui.esc(item.arquivoFormato || "arquivo") + "</button> <span class=\"discreto\">" + ui.esc(item.arquivoLicenca || "") + "</span></p>";
    }).join("") : "<p>Nenhum arquivo liberado.</p>";
    document.getElementById("orcamentos").innerHTML = orcamentos.length ? orcamentos.map(function (item) {
      var acoes = item.status === "respondido" ? '<button class="botao" type="button" data-aceitar="' + ui.esc(item.id) + '">Aceitar ' + dinheiro.real(item.valor) + '</button><button class="botao-linha" type="button" data-recusar="' + ui.esc(item.id) + '">Recusar</button>' : "";
      return "<article><strong>" + ui.esc(item.titulo) + "</strong><p>" + ui.esc(item.status) + "</p><p>" + ui.esc(item.detalhes) + "</p>" + (item.valor ? "<p>" + dinheiro.real(item.valor) + "</p>" : "") + '<div class="linha">' + acoes + "</div></article>";
    }).join("") : "<p>Nenhum orçamento.</p>";
    document.getElementById("enderecos").innerHTML = vazio || !estado().enderecos.length ? "<p>Nenhum endereço.</p>" : estado().enderecos.map(function (item) {
      return "<p><strong>" + ui.esc(item.rotulo) + "</strong> · " + ui.esc(item.linha) + " · " + ui.esc(item.cep) + "</p>";
    }).join("");
    document.getElementById("favoritos").innerHTML = favoritos.length ? favoritos.map(function (produto) {
      var foto = produto.fotos[0];
      return '<a class="produto" href="' + ui.url("paginas/produto.html?id=" + encodeURIComponent(produto.id)) + '"><img src="' + ui.url(foto.arquivo) + '" alt=""><div><h3>' + ui.esc(produto.titulo) + "</h3></div></a>";
    }).join("") : ui.vazio("Nenhum favorito.");
    document.getElementById("perguntas").innerHTML = perguntas.length ? perguntas.map(function (item) {
      return "<p><strong>" + ui.esc(item.texto) + "</strong><br>" + ui.esc(item.resposta || "Aguardando resposta") + "</p>";
    }).join("") : "<p>Nenhuma pergunta.</p>";
    document.getElementById("trocar-perfil").addEventListener("click", function () {
      Cria.alterar(function (atual) { atual.perfil = atual.perfil === "vazia" ? "exemplo" : "vazia"; });
      conta();
    });
    document.getElementById("dados").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.cliente.nome = String(dados.get("nome") || "");
        atual.cliente.email = String(dados.get("email") || "");
        atual.cliente.telefone = String(dados.get("telefone") || "");
        atual.cliente.documento = String(dados.get("documento") || "");
      });
    });
    raiz.addEventListener("click", function (evento) {
      var pagar = evento.target.getAttribute && evento.target.getAttribute("data-pagar");
      var devolver = evento.target.getAttribute && evento.target.getAttribute("data-devolver");
      var aceitar = evento.target.getAttribute && evento.target.getAttribute("data-aceitar");
      var recusar = evento.target.getAttribute && evento.target.getAttribute("data-recusar");
      var download = evento.target.getAttribute && evento.target.getAttribute("data-download");
      if (pagar) {
        Cria.alterar(function (atual) {
          var pedido = atual.pedidos.find(function (item) { return item.id === pagar; });
          if (!pedido) return;
          pedido.status = "pago";
          pedido.itens.forEach(function (item) {
            if (!item.fisico || item.tipo === "personalizado") return;
            var produto = atual.produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
            if (produto) produto.estoque = Math.max(0, produto.estoque - item.quantidade);
          });
        });
        conta();
      }
      if (devolver) {
        Cria.alterar(function (atual) {
          var pedido = atual.pedidos.find(function (item) { return item.id === devolver; });
          if (pedido) pedido.status = "devolvido";
        });
        conta();
      }
      if (aceitar) {
        Cria.alterar(function (atual) {
          var orcamento = atual.orcamentosCliente.find(function (item) { return item.id === aceitar; });
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
        conta();
      }
      if (recusar) {
        Cria.alterar(function (atual) {
          var orcamento = atual.orcamentosCliente.find(function (item) { return item.id === recusar; });
          if (orcamento) orcamento.status = "recusado";
        });
        conta();
      }
      if (download != null) baixarArquivo(downloads[Number(download)]);
    });
    var formEndereco = document.getElementById("novo-endereco");
    formEndereco.addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(formEndereco);
      Cria.alterar(function (atual) {
        atual.enderecos.push({ id: CriaCarrinho.novoId("end"), rotulo: "Extra", linha: String(dados.get("linha")), cep: String(dados.get("cep")).replace(/\D/g, "") });
      });
      conta();
    });
  }

  function contato() {
    document.title = "Contato · Cria Lab DM";
    raiz.innerHTML = '<h1>Contato</h1><form id="contato" class="cartao formulario"><label>Nome<input name="nome" required value="' + ui.esc(estado().cliente.nome) + '"></label><label>E-mail<input name="email" type="email" required value="' + ui.esc(estado().cliente.email) + '"></label><label>Tipo<select name="tipo"><option>dúvida</option><option>sugestão</option><option>elogio</option><option>reclamação</option><option>orçamento</option><option>outro</option></select></label><label>Mensagem<textarea name="texto" required></textarea></label><button class="botao" type="submit">Enviar</button><p id="confirma"></p></form>';
    document.getElementById("contato").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.mensagens.push({
          id: CriaCarrinho.novoId("msg"),
          tipo: String(dados.get("tipo")),
          nome: String(dados.get("nome")),
          email: String(dados.get("email")),
          texto: String(dados.get("texto")),
          status: "aberto",
          criadoEm: dinheiro.agoraSP()
        });
      });
      document.getElementById("confirma").innerHTML = '<span class="ok">Mensagem registrada. Vamos responder por aqui.</span>';
      evento.target.texto.value = "";
    });
  }

  if (pagina === "conta") conta();
  if (pagina === "contato") contato();
})();
