(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function guia() {
    if (estado().guiaVisto) return "";
    return '<div class="guia" id="guia"><div class="cartao"><img src="' + ui.url("marca/mascote.png") + '" alt="" style="width:120px"><h2>Primeiro acesso</h2><ol><li><a href="' + ui.url("paginas/admin/financeiro.html#lancar") + '">Lançar uma despesa</a></li><li><a href="' + ui.url("paginas/admin/mes.html?mes=2026-09") + '">Ver o mês</a></li><li><a href="' + ui.url("paginas/admin/orcamento.html?novo=1") + '">Fazer um orçamento</a></li></ol><button class="botao-linha" type="button" id="pular-guia">Pular</button></div></div>';
  }

  function painel() {
    document.title = "Administração · Cria Lab DM";
    var mostrarGuia = !estado().guiaVisto;
    var htmlGuia = mostrarGuia ? guia() : "";
    if (mostrarGuia) Cria.alterar(function (atual) { atual.guiaVisto = true; });
    var areas = [
      ["paginas/admin/produtos.html", "Gestão de produtos", "Peças prontas da vitrine."],
      ["paginas/admin/personalizados.html", "Gestão de personalizados", "Orçamentos, ordens e fabricação."],
      ["paginas/admin/marketing.html", "Marketing", "Cupom da loja."],
      ["paginas/admin/contato.html", "Atendimento ao cliente", "Mensagens e perguntas."],
      ["paginas/admin/patrimonio.html", "Patrimônio e estoque", "Máquinas, filamento e peças."],
      ["paginas/admin/financeiro.html", "Gestão financeira", "Despesas, receitas e o mês."],
      ["paginas/admin/fiscal.html", "Gestão fiscal e documental", "Recibos e notas pendentes."],
      ["paginas/admin/configuracao.html", "Administração do sistema", "Loja, frete e regras."],
      ["paginas/admin/impressao.html", "Gestão de impressão 3D", "Máquinas, mesa, qualidade e histórico."]
    ];
    var prontos = estado().produtos.filter(function (item) { return item.tipo !== "personalizado" && item.situacao === "publicado"; }).length;
    var sobMedida = estado().produtos.filter(function (item) { return item.tipo === "personalizado"; }).length;
    var abertos = (estado().mensagens || []).filter(function (item) { return item.status === "aberto"; }).length;
    var resumo = '<div class="grade-3"><article class="cartao"><h2>' + prontos + '</h2><p>Produtos publicados</p></article><article class="cartao"><h2>' + sobMedida + '</h2><p>Personalizados</p></article><article class="cartao"><h2>' + abertos + '</h2><p>Atendimentos abertos</p></article></div>';
    raiz.innerHTML = htmlGuia + '<h1>Painel gerencial</h1>' + resumo + '<div class="grade-3">' + areas.map(function (area) {
      return '<a class="cartao" href="' + ui.url(area[0]) + '"><h2>' + ui.esc(area[1]) + "</h2><p>" + ui.esc(area[2]) + "</p></a>";
    }).join("") + '</div><p><button class="botao-linha discreto" type="button" id="restaurar">Restaurar carga inicial</button></p>';
    var pular = document.getElementById("pular-guia");
    if (pular) pular.addEventListener("click", function () {
      Cria.alterar(function (atual) { atual.guiaVisto = true; });
      document.getElementById("guia").remove();
    });
    document.getElementById("restaurar").addEventListener("click", function () {
      if (!confirm("Isso apaga as alterações deste navegador e volta à carga inicial.")) return;
      Cria.restaurar();
      location.reload();
    });
  }

  function produtos() {
    document.title = "Gestão de produtos · Administração";
    var linhas = estado().produtos.filter(function (produto) { return produto.tipo !== "personalizado"; }).map(function (produto) {
      return "<tr><td>" + ui.esc(produto.titulo) + "</td><td>" + ui.esc(produto.situacao) + "</td><td>" + dinheiro.real(produto.preco) + "</td><td>" + produto.estoque + '</td><td><a class="botao-linha" href="' + ui.url("paginas/admin/produto.html?id=" + encodeURIComponent(produto.id)) + '">Editar</a></td></tr>';
    }).join("");
    raiz.innerHTML = '<h1>Gestão de produtos</h1><p>Peças prontas, com preço fixo.</p><p><a class="botao" href="' + ui.url("paginas/admin/produto.html") + '">Novo produto</a></p><div class="tabela-rolagem cartao"><table><thead><tr><th>Peça</th><th>Situação</th><th>Preço</th><th>Estoque</th><th></th></tr></thead><tbody>' + linhas + "</tbody></table></div>";
  }

  function formularioProduto() {
    var id = new URLSearchParams(location.search).get("id");
    var produto = estado().produtos.find(function (item) { return item.id === id; }) || {
      id: "", titulo: "", descricao: "", tipo: "pronto", comArquivo: false, categoria: "decoracao", preco: 0, precoComparativo: 0, precoArquivo: 0, estoque: 0, minimo: 1, medidas: "", peso: 0, material: "PLA", prazoDias: 5, situacao: "publicado", fotos: [], cores: [{ nome: "PLA laranja", hex: "#e8772a" }], tamanhos: ["Único"], personalizavel: false, etapas: [], sessao: "", oferta: false, maisVendido: false, aVista: true, vendas: 0, nota: 0, criadoEm: dinheiro.hojeISO(), arquivo: null
    };
    document.title = (produto.id ? "Editar" : "Novo") + " produto";
    var categorias = estado().categorias.map(function (categoria) {
      return '<option value="' + ui.esc(categoria.id) + '"' + (produto.categoria === categoria.id ? " selected" : "") + ">" + ui.esc(categoria.nome) + "</option>";
    }).join("");
    var fotos = (produto.fotos || []).map(function (foto, indice) {
      return '<label>Legenda da sessão ' + ui.esc(dinheiro.dataBR(foto.sessao || produto.sessao)) + '<input data-legenda="' + indice + '" value="' + ui.esc(foto.legenda) + '"></label><img src="' + ui.url(foto.arquivo) + '" alt="" style="width:120px;height:120px;object-fit:cover;border-radius:12px">';
    }).join("");
    raiz.innerHTML = '<h1>' + ui.esc(produto.titulo || "Novo produto") + '</h1><form id="produto" class="cartao formulario"><label>Título<input name="titulo" required value="' + ui.esc(produto.titulo) + '"></label><label>Descrição<textarea name="descricao">' + ui.esc(produto.descricao) + '</textarea></label><label>Tipo<select name="tipo"><option value="pronto">pronto</option><option value="personalizado">personalizado</option></select></label><label>Categoria<select name="categoria">' + categorias + '</select></label><div class="grade-3"><label>Preço<input name="preco" type="number" step="0.01" value="' + produto.preco + '"></label><label>Preço comparativo<input name="precoComparativo" type="number" step="0.01" value="' + (produto.precoComparativo || 0) + '"></label><label>Estoque<input name="estoque" type="number" value="' + produto.estoque + '"></label><label>Peso (g)<input name="peso" type="number" step="0.01" value="' + produto.peso + '"></label><label>Prazo (dias)<input name="prazoDias" type="number" value="' + produto.prazoDias + '"></label></div><label>Medidas<input name="medidas" value="' + ui.esc(produto.medidas) + '"></label><label>Material<input name="material" value="' + ui.esc(produto.material) + '"></label><h2>Fotos</h2>' + fotos + '<div class="linha"><button class="botao" type="submit">Salvar</button><button class="botao-teal" type="button" id="publicar">Publicar</button><button class="botao-linha" type="button" id="pausar">Pausar</button><button class="botao-linha" type="button" id="esgotar">Esgotar</button></div></form>';
    document.querySelector('[name=tipo]').value = produto.tipo === "personalizado" ? "personalizado" : "pronto";
    document.getElementById("produto").addEventListener("submit", function (evento) {
      evento.preventDefault();
      salvar("manter");
    });
    document.getElementById("publicar").addEventListener("click", function () { salvar("publicado"); });
    document.getElementById("pausar").addEventListener("click", function () { salvar("pausado"); });
    document.getElementById("esgotar").addEventListener("click", function () { salvar("esgotado"); });
    function salvar(situacao) {
      var dados = new FormData(document.getElementById("produto"));
      Cria.alterar(function (atual) {
        var alvo = atual.produtos.find(function (item) { return item.id === produto.id; });
        if (!alvo) {
          alvo = JSON.parse(JSON.stringify(produto));
          alvo.id = CriaCarrinho.novoId("peca");
          alvo.fotos = [];
          atual.produtos.push(alvo);
        }
        alvo.titulo = String(dados.get("titulo"));
        alvo.descricao = String(dados.get("descricao"));
        alvo.tipo = String(dados.get("tipo"));
        alvo.personalizavel = alvo.tipo === "personalizado";
        alvo.comArquivo = false;
        alvo.precoArquivo = 0;
        alvo.arquivo = null;
        alvo.categoria = String(dados.get("categoria"));
        alvo.preco = Number(dados.get("preco")) || 0;
        alvo.precoComparativo = Number(dados.get("precoComparativo")) || 0;
        alvo.estoque = Number(dados.get("estoque")) || 0;
        alvo.peso = Number(dados.get("peso")) || 0;
        alvo.prazoDias = Number(dados.get("prazoDias")) || 0;
        alvo.medidas = String(dados.get("medidas"));
        alvo.material = String(dados.get("material"));
        document.querySelectorAll("[data-legenda]").forEach(function (campo) {
          var foto = alvo.fotos[Number(campo.getAttribute("data-legenda"))];
          if (foto) foto.legenda = campo.value;
        });
        if (situacao !== "manter") alvo.situacao = situacao;
        if (situacao === "esgotado") alvo.estoque = 0;
      });
      location.href = ui.url("paginas/admin/produtos.html");
    }
  }

  function configuracao() {
    document.title = "Configuração · Administração";
    var conf = estado().configuracao;
    var id = estado().identidade;
    var faixas = conf.faixasCep.map(function (faixa, indice) {
      return '<div class="grade-3"><label>De<input data-faixa="de" data-i="' + indice + '" type="number" value="' + faixa.de + '"></label><label>Até<input data-faixa="ate" data-i="' + indice + '" type="number" value="' + faixa.ate + '"></label><label>PAC<input data-faixa="pac" data-i="' + indice + '" type="number" step="0.01" value="' + faixa.pac + '"></label><label>Dias PAC<input data-faixa="pacDias" data-i="' + indice + '" type="number" value="' + faixa.pacDias + '"></label><label>SEDEX<input data-faixa="sedex" data-i="' + indice + '" type="number" step="0.01" value="' + faixa.sedex + '"></label><label>Dias SEDEX<input data-faixa="sedexDias" data-i="' + indice + '" type="number" value="' + faixa.sedexDias + '"></label></div>';
    }).join("");
    var cupom = conf.cupons[0] || { codigo: "", percentual: 0 };
    raiz.innerHTML = '<h1>Configuração da loja</h1><form id="conf" class="cartao formulario"><label>Frete grátis a partir de<input name="freteGratisAPartir" type="number" step="0.01" value="' + conf.freteGratisAPartir + '"></label><label>Desconto à vista<input name="descontoAVista" type="number" step="0.01" value="' + conf.descontoAVista + '"></label><label>Parcelas no cartão<input name="parcelas" type="number" value="' + conf.parcelas + '"></label><label class="linha"><input type="checkbox" name="repassarFrete"' + (conf.repassarFrete ? " checked" : "") + "> Repassar o frete no líquido</label><label>Cupom<input name=\"cupom\" value=\"" + ui.esc(cupom.codigo) + "\"></label><label>Percentual do cupom<input name=\"cupomPercentual\" type=\"number\" step=\"0.01\" value=\"" + cupom.percentual + "\"></label><label>Endereço<input name=\"endereco\" value=\"" + ui.esc(id.endereco) + "\"></label><label>Horário<input name=\"horario\" value=\"" + ui.esc(id.horario) + "\"></label><h2>Faixas de CEP</h2>" + faixas + '<button class="botao" type="submit">Salvar</button></form>';
    document.getElementById("conf").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.configuracao.freteGratisAPartir = Number(dados.get("freteGratisAPartir")) || 0;
        atual.configuracao.descontoAVista = Number(dados.get("descontoAVista")) || 0;
        atual.configuracao.parcelas = Number(dados.get("parcelas")) || 1;
        atual.configuracao.repassarFrete = Boolean(dados.get("repassarFrete"));
        atual.configuracao.cupons = [{ codigo: String(dados.get("cupom") || "OFICINA10"), percentual: Number(dados.get("cupomPercentual")) || 0, ativo: true }];
        atual.identidade.endereco = String(dados.get("endereco") || "");
        atual.identidade.horario = String(dados.get("horario") || "");
        document.querySelectorAll("[data-faixa]").forEach(function (campo) {
          var faixa = atual.configuracao.faixasCep[Number(campo.getAttribute("data-i"))];
          faixa[campo.getAttribute("data-faixa")] = Number(campo.value);
        });
      });
      raiz.insertAdjacentHTML("afterbegin", '<p class="ok">Configuração salva. Pedidos já feitos continuam com o valor da hora.</p>');
    });
  }

  function perguntas() {
    document.title = "Perguntas · Administração";
    raiz.innerHTML = "<h1>Perguntas</h1>" + estado().perguntas.map(function (item) {
      var produto = estado().produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
      return '<form class="cartao formulario" data-id="' + ui.esc(item.id) + '"><strong>' + ui.esc(produto ? produto.titulo : "Peça") + "</strong><p>" + ui.esc(item.texto) + '</p><label>Resposta<textarea name="resposta">' + ui.esc(item.resposta || "") + '</textarea></label><button class="botao" type="submit">Publicar resposta</button></form>';
    }).join("");
    raiz.querySelectorAll("form").forEach(function (form) {
      form.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var id = form.getAttribute("data-id");
        var resposta = new FormData(form).get("resposta");
        Cria.alterar(function (atual) {
          var pergunta = atual.perguntas.find(function (item) { return item.id === id; });
          if (pergunta) pergunta.resposta = String(resposta || "");
        });
      });
    });
  }

  function contatoAdmin() {
    document.title = "Contato · Administração";
    raiz.innerHTML = "<h1>Mensagens</h1>" + estado().mensagens.map(function (item) {
      return '<article class="cartao"><p><strong>' + ui.esc(item.tipo) + "</strong> · " + ui.esc(item.status) + "</p><p>" + ui.esc(item.nome) + " · " + ui.esc(item.email) + "</p><p>" + ui.esc(item.texto) + '</p><div class="linha"><button class="botao-linha" type="button" data-status="em atendimento" data-id="' + ui.esc(item.id) + '">Em atendimento</button><button class="botao" type="button" data-status="resolvido" data-id="' + ui.esc(item.id) + '">Resolvido</button></div></article>';
    }).join("");
    raiz.addEventListener("click", function (evento) {
      var id = evento.target.getAttribute && evento.target.getAttribute("data-id");
      var status = evento.target.getAttribute && evento.target.getAttribute("data-status");
      if (!id || !status) return;
      Cria.alterar(function (atual) {
        var mensagem = atual.mensagens.find(function (item) { return item.id === id; });
        if (mensagem) mensagem.status = status;
      });
      contatoAdmin();
    });
  }

  function depoimentos() {
    document.title = "Depoimentos · Administração";
    function bloco(lista, chave, titulo) {
      return "<h2>" + titulo + "</h2>" + lista.map(function (item) {
        return '<label class="cartao linha"><input type="checkbox" data-chave="' + chave + '" data-id="' + ui.esc(item.id) + '"' + (item.podeAparecer ? " checked" : "") + "> <span><strong>" + ui.esc(item.autor || item.nome) + "</strong><br>" + ui.esc(item.texto || "Pode aparecer no site") + "</span></label>";
      }).join("");
    }
    raiz.innerHTML = "<h1>Publicação no site</h1>" + bloco(estado().depoimentos, "depoimentos", "Depoimentos") + bloco(estado().parceiros, "parceiros", "Parceiros");
    raiz.addEventListener("change", function (evento) {
      var id = evento.target.getAttribute("data-id");
      var chave = evento.target.getAttribute("data-chave");
      if (!id || !chave) return;
      Cria.alterar(function (atual) {
        var item = atual[chave].find(function (cadastro) { return cadastro.id === id; });
        if (item) item.podeAparecer = evento.target.checked;
      });
    });
  }

  function personalizadosAdmin() {
    document.title = "Gestão de personalizados · Administração";
    var linhas = estado().produtos.filter(function (produto) { return produto.tipo === "personalizado"; }).map(function (produto) {
      return "<tr><td>" + ui.esc(produto.titulo) + "</td><td>" + dinheiro.real(produto.preco) + '</td><td>média</td><td><a class="botao-linha" href="' + ui.url("paginas/admin/produto.html?id=" + encodeURIComponent(produto.id)) + '">Editar</a></td></tr>';
    }).join("");
    raiz.innerHTML = '<h1>Gestão de personalizados</h1><p>Ordens de serviço, orçamentos, acompanhamento e fabricação.</p><div class="linha"><a class="botao" href="' + ui.url("paginas/admin/orcamentos.html") + '">Orçamentos</a><a class="botao-linha" href="' + ui.url("paginas/admin/pedidos.html") + '">Pedidos</a><a class="botao-linha" href="' + ui.url("paginas/admin/os.html") + '">Ordens de serviço</a><a class="botao" href="' + ui.url("paginas/admin/produto.html") + '">Novo personalizado</a></div><div class="tabela-rolagem cartao"><table><thead><tr><th>Peça</th><th>Preço médio</th><th></th><th></th></tr></thead><tbody>' + linhas + "</tbody></table></div>";
  }

  function marketing() {
    document.title = "Marketing · Administração";
    var cupom = (estado().configuracao.cupons || [])[0] || { codigo: "", percentual: 0, ativo: true };
    raiz.innerHTML = '<h1>Marketing</h1><p>Cupom da própria loja. O desconto vale no fechamento do pedido.</p><form id="cupom" class="cartao formulario"><label>Código<input name="codigo" value="' + ui.esc(cupom.codigo) + '"></label><label>Percentual<input name="percentual" type="number" step="0.01" min="0" max="1" value="' + cupom.percentual + '"></label><label class="linha"><input type="checkbox" name="ativo"' + (cupom.ativo ? " checked" : "") + "> Cupom ativo</label><button class=\"botao\" type=\"submit\">Salvar cupom</button></form>";
    document.getElementById("cupom").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      Cria.alterar(function (atual) {
        atual.configuracao.cupons = [{ codigo: String(dados.get("codigo") || "").trim(), percentual: Number(dados.get("percentual")) || 0, ativo: Boolean(dados.get("ativo")) }];
      });
      marketing();
    });
  }

  function fiscal() {
    document.title = "Gestão fiscal · Administração";
    var linhas = (estado().pedidos || []).map(function (pedido) {
      var recibo = pedido.status !== "aguardando pagamento" && pedido.status !== "cancelado" ? '<a class="botao-linha" href="' + ui.url("paginas/recibo.html?id=" + encodeURIComponent(pedido.id)) + '">Recibo</a>' : "";
      return "<tr><td>" + ui.esc(pedido.id) + "</td><td>" + ui.esc(pedido.status) + "</td><td>" + dinheiro.real(pedido.total) + "</td><td>pendente de transmissão</td><td>" + recibo + "</td></tr>";
    }).join("");
    raiz.innerHTML = '<h1>Gestão fiscal e documental</h1><p>A nota permanece pendente de transmissão. Não há chave nem protocolo.</p><div class="tabela-rolagem cartao"><table><thead><tr><th>Pedido</th><th>Status</th><th>Total</th><th>Nota</th><th></th></tr></thead><tbody>' + (linhas || "<tr><td colspan=\"5\">Nenhum documento.</td></tr>") + "</tbody></table></div>";
  }

  if (pagina === "admin-home") painel();
  if (pagina === "admin-personalizados") personalizadosAdmin();
  if (pagina === "admin-marketing") marketing();
  if (pagina === "admin-fiscal") fiscal();
  if (pagina === "admin-produtos") produtos();
  if (pagina === "admin-produto") formularioProduto();
  if (pagina === "admin-config") configuracao();
  if (pagina === "admin-perguntas") perguntas();
  if (pagina === "admin-contato") contatoAdmin();
  if (pagina === "admin-depoimentos") depoimentos();
})();
