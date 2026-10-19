(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");

  function estado() { return Cria.estado; }
  function config() { return estado().configuracao; }
  function ico(nome) {
    return '<span class="ico">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
  }

  function totais(frete, forma, codigoCupom) {
    var cupom = CriaCarrinho.cupomPorCodigo(estado(), codigoCupom);
    var conta = CriaCalculos.fecharPedido(estado().carrinho, {
      forma: forma,
      descontoAVista: config().descontoAVista,
      freteGratisAPartir: config().freteGratisAPartir,
      frete: frete,
      cupomPercentual: cupom ? cupom.percentual : 0,
      parcelas: config().parcelas
    });
    conta.cupom = cupom;
    conta.cupomInvalido = Boolean(codigoCupom) && !cupom;
    return conta;
  }

  function carrinho() {
    document.title = "Carrinho · Cria Lab DM";
    var linhas = estado().carrinho;
    if (!linhas.length) {
      raiz.innerHTML = "<h1>Carrinho</h1>" + ui.vazio("O carrinho está vazio.");
      return;
    }
    raiz.innerHTML = "<h1>Carrinho</h1><div id=\"linhas\"></div><p class=\"acao-carrinho\"><a class=\"botao\" href=\"" + ui.url("paginas/checkout.html") + "\">" + ico("carrinho") + "Finalizar pedido</a></p>";
    pintarLinhas();
  }

  function pintarLinhas() {
    var caixa = document.getElementById("linhas");
    if (!caixa) return;
    caixa.innerHTML = estado().carrinho.map(function (item) {
      return '<article class="cartao item-carrinho"><div><strong>' + ui.esc(item.titulo) + "</strong><p>" + ui.esc(item.tipo === "personalizado" ? "Personalizado" : "Produto") + (item.detalhes ? " · " + ui.esc(item.detalhes) : "") + "</p><p class=\"preco\">" + dinheiro.real(item.precoUnitario) + "</p></div><label class=\"qtd-ficha\">Quantidade<input data-qtd=\"" + ui.esc(item.id) + "\" type=\"number\" min=\"1\" value=\"" + item.quantidade + "\"></label><button class=\"botao-linha\" type=\"button\" data-remover=\"" + ui.esc(item.id) + "\">Tirar</button></article>";
    }).join("");
    caixa.querySelectorAll("[data-qtd]").forEach(function (campo) {
      campo.addEventListener("change", function () {
        CriaCarrinho.definirQuantidade(campo.getAttribute("data-qtd"), Number(campo.value) || 1);
        pintarLinhas();
      });
    });
    caixa.querySelectorAll("[data-remover]").forEach(function (botao) {
      botao.addEventListener("click", function () {
        CriaCarrinho.remover(botao.getAttribute("data-remover"));
        if (!estado().carrinho.length) carrinho();
        else pintarLinhas();
      });
    });
  }

  function lerRascunho() {
    try { return JSON.parse(sessionStorage.getItem("crialab-checkout") || "{}"); } catch (erro) { return {}; }
  }
  function gravarRascunho(dados) {
    sessionStorage.setItem("crialab-checkout", JSON.stringify(dados));
  }

  function checkout() {
    document.title = "Fechamento · Cria Lab DM";
    if (!estado().carrinho.length) {
      raiz.innerHTML = "<h1>Fechamento</h1>" + ui.vazio("O carrinho está vazio.");
      return;
    }
    var rascunho = lerRascunho();
    var endereco = (estado().enderecos[0] && estado().enderecos[0].linha) || "";
    raiz.innerHTML = '<h1>Fechamento</h1><div class="checkout"><form id="fechamento" class="cartao formulario"><h2>Entrega e pagamento</h2><label>Endereço<input name="endereco" value="' + ui.esc(rascunho.endereco || endereco) + '" required></label><label>CEP<input name="cep" inputmode="numeric" value="' + ui.esc(rascunho.cep || "") + '" required></label><div id="opcoes-frete"></div><label>Cupom da loja<input name="cupom" value="' + ui.esc(rascunho.cupom || "") + '" placeholder="OFICINA10"></label><label>Pagamento<select name="forma"><option value="pix">PIX</option><option value="boleto">Boleto</option><option value="cartao">Cartão</option></select></label><p id="mensagem"></p><button class="botao" type="submit">' + ico("carrinho") + 'Confirmar pedido</button></form><aside class="cartao resumo" id="resumo"></aside></div><div class="barra-mobile" id="barra-mobile"></div><div id="depois"></div>';
    var form = document.getElementById("fechamento");
    if (rascunho.forma) form.forma.value = rascunho.forma;
    function dadosForm() {
      var dados = new FormData(form);
      return {
        endereco: String(dados.get("endereco") || ""),
        cep: String(dados.get("cep") || ""),
        cupom: String(dados.get("cupom") || ""),
        forma: String(dados.get("forma") || "pix"),
        freteId: form.querySelector("input[name=frete]:checked") ? form.querySelector("input[name=frete]:checked").value : ""
      };
    }
    function pintar() {
      var atual = dadosForm();
      gravarRascunho(atual);
      var digitos = atual.cep.replace(/\D/g, "");
      var temFisico = estado().carrinho.some(function (item) { return item.fisico; });
      var cotacao = temFisico ? CriaCalculos.cotarFrete(digitos, config().faixasCep) : { opcoes: [] };
      var freteEscolhido = 0;
      var caixaFrete = document.getElementById("opcoes-frete");
      if (!temFisico) caixaFrete.innerHTML = "<p>Sem peça física neste pedido.</p>";
      else if (digitos.length !== 8) caixaFrete.innerHTML = '<p class="erro">Informe o CEP com 8 dígitos.</p>';
      else if (cotacao.erro) caixaFrete.innerHTML = '<p class="erro">' + ui.esc(cotacao.erro) + "</p>";
      else {
        var previa = totais(cotacao.opcoes[0].preco, atual.forma, atual.cupom);
        caixaFrete.innerHTML = cotacao.opcoes.map(function (opcao) {
          var preco = previa.subtotalFisico >= config().freteGratisAPartir ? 0 : opcao.preco;
          var marcado = atual.freteId ? atual.freteId === opcao.id : opcao.id === "pac";
          if (marcado) freteEscolhido = preco;
          return '<label class="linha"><input type="radio" name="frete" value="' + opcao.id + '"' + (marcado ? " checked" : "") + "> " + opcao.nome + " · " + (preco === 0 ? "grátis" : dinheiro.real(preco)) + " · " + opcao.dias + " dias úteis</label>";
        }).join("");
        if (!atual.freteId) freteEscolhido = previa.subtotalFisico >= config().freteGratisAPartir ? 0 : cotacao.opcoes[0].preco;
      }
      var conta = totais(freteEscolhido, atual.forma, atual.cupom);
      var msg = document.getElementById("mensagem");
      if (conta.cupomInvalido) msg.innerHTML = '<span class="erro">Cupom não encontrado.</span>';
      else msg.innerHTML = "";
      var linhas = estado().carrinho.map(function (item) {
        return "<p>" + ui.esc(item.titulo) + " · " + item.quantidade + " × " + dinheiro.real(item.precoUnitario) + "</p>";
      }).join("");
      var resumo = linhas + "<p>Subtotal " + dinheiro.real(conta.subtotal + conta.descontoCupom) + "</p>" + (conta.descontoCupom ? "<p>Cupom − " + dinheiro.real(conta.descontoCupom) + "</p>" : "") + "<p>Desconto à vista − " + dinheiro.real(conta.descontoAVista) + "</p><p>Frete " + dinheiro.real(conta.freteCobrado) + "</p><p class=\"numero-grande\">Total " + dinheiro.real(conta.total) + "</p>" + (atual.forma === "cartao" ? "<p>" + config().parcelas + "x de " + dinheiro.real(conta.parcelas) + " sem o desconto à vista</p>" : "<p>PIX e boleto já com " + Math.round(config().descontoAVista * 100) + "% de desconto.</p>");
      document.getElementById("resumo").innerHTML = "<h2>Resumo</h2>" + resumo;
      document.getElementById("barra-mobile").innerHTML = "<strong>" + dinheiro.real(conta.total) + "</strong>";
    }
    form.addEventListener("input", pintar);
    form.addEventListener("change", pintar);
    form.addEventListener("submit", function (evento) {
      evento.preventDefault();
      var atual = dadosForm();
      var temFisico = estado().carrinho.some(function (item) { return item.fisico; });
      var digitos = atual.cep.replace(/\D/g, "");
      if (temFisico && digitos.length !== 8) {
        document.getElementById("mensagem").innerHTML = '<span class="erro">Informe o CEP com 8 dígitos.</span>';
        return;
      }
      var cotacao = temFisico ? CriaCalculos.cotarFrete(digitos, config().faixasCep) : { opcoes: [] };
      if (temFisico && cotacao.erro) {
        document.getElementById("mensagem").innerHTML = '<span class="erro">' + ui.esc(cotacao.erro) + "</span>";
        return;
      }
      var opcao = (cotacao.opcoes || []).find(function (item) { return item.id === (atual.freteId || "pac"); }) || { preco: 0, nome: "Sem frete", dias: 0 };
      var previa = totais(opcao.preco, atual.forma, atual.cupom);
      if (previa.cupomInvalido) return;
      var freteCobrado = previa.freteCobrado;
      var pedido = {
        id: CriaCarrinho.novoId("ped"),
        status: estado().carrinho.some(function (item) { return item.tipo === "personalizado"; }) ? "aguardando pagamento" : "aguardando pagamento",
        criadoEm: dinheiro.agoraSP(),
        itens: estado().carrinho.map(function (item) { return Object.assign({}, item); }),
        subtotal: previa.subtotal,
        descontoCupom: previa.descontoCupom,
        descontoAVista: previa.descontoAVista,
        freteCobrado: freteCobrado,
        total: previa.total,
        forma: atual.forma,
        endereco: atual.endereco,
        cep: digitos,
        freteNome: temFisico ? opcao.nome : "Sem frete",
        cupom: previa.cupom ? previa.cupom.codigo : "",
        repassarFrete: config().repassarFrete,
        nota: "pendente",
        os: null,
        configuracaoUsada: { freteGratisAPartir: config().freteGratisAPartir, descontoAVista: config().descontoAVista }
      };
      Cria.alterar(function (atualEstado) {
        atualEstado.pedidos.push(pedido);
        atualEstado.carrinho = [];
        if (atual.endereco && !atualEstado.enderecos.some(function (item) { return item.linha === atual.endereco; })) {
          atualEstado.enderecos.push({ id: CriaCarrinho.novoId("end"), rotulo: "Entrega", cep: digitos, linha: atual.endereco });
        }
      });
      CriaCasca.atualizar();
      sessionStorage.removeItem("crialab-checkout");
      raiz.innerHTML = '<section class="cartao"><h1>Pedido ' + ui.esc(pedido.id) + '</h1><p>Status: aguardando pagamento. Total ' + dinheiro.real(pedido.total) + '.</p><button class="botao" type="button" id="simular">Simular pagamento</button><p><a href="' + ui.url("paginas/conta.html") + '">Ir para a conta</a></p></section>';
      document.getElementById("simular").addEventListener("click", function () {
        Cria.alterar(function (atualEstado) {
          var salvo = atualEstado.pedidos.find(function (item) { return item.id === pedido.id; });
          if (!salvo || salvo.status !== "aguardando pagamento") return;
          salvo.status = "pago";
          salvo.itens.forEach(function (item) {
            if (!item.fisico || item.tipo === "personalizado") return;
            var produto = atualEstado.produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
            if (produto) produto.estoque = Math.max(0, produto.estoque - item.quantidade);
          });
        });
        raiz.querySelector("section").insertAdjacentHTML("beforeend", '<p class="ok">Pagamento simulado. O pedido está pago.</p>');
      });
    });
    pintar();
  }

  if (pagina === "carrinho") carrinho();
  if (pagina === "checkout") checkout();
})();
