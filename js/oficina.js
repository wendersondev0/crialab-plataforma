(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function patrimonio() {
    document.title = "Patrimônio · Administração";
    var compras = estado().lancamentos.filter(function (item) { return item.categoria === "investimento"; });
    raiz.innerHTML = "<h1>Patrimônio</h1>" + estado().maquinas.map(function (maquina) {
      return '<article class="cartao"><h2>' + ui.esc(maquina.nome) + "</h2><p>" + (maquina.compraValor ? dinheiro.real(maquina.compraValor) + " · " + ui.esc(maquina.compraCompetencia) : "Sem compra marcada") + "</p><p>Watts " + maquina.watts + "</p><p>Depreciação " + dinheiro.real(maquina.depreciacaoPorMinuto, 3) + "/min · manutenção " + dinheiro.real(maquina.manutencaoPorMinuto, 3) + "/min</p><p>Energia " + dinheiro.real(maquina.energiaPorMinuto, 4) + "/min" + (maquina.controlaHora ? "" : " · sem hora de máquina") + "</p></article>";
    }).join("") + "<h2>Compras marcadas na planilha</h2>" + compras.map(function (item) {
      return "<p>" + ui.esc(dinheiro.competenciaBR(item.competencia)) + " · " + ui.esc(item.descricao) + " · " + dinheiro.real(item.valor) + "</p>";
    }).join("");
  }

  function estoque() {
    document.title = "Estoque · Administração";
    var insumos = estado().estoqueInsumos.map(function (item) {
      var alerta = item.saldo <= item.minimo;
      return '<article class="cartao"><h2>' + ui.esc(item.nome) + "</h2><p>Saldo " + item.saldo + " " + ui.esc(item.unidade) + " · mínimo " + item.minimo + "</p>" + (alerta ? '<p class="aviso">Abaixo do mínimo.</p><a class="botao" href="' + ui.url("paginas/admin/mes.html?mes=" + dinheiro.hojeISO().slice(0, 7) + "&compra=" + encodeURIComponent(item.nome)) + '">Lançar compra</a>' : "") + "</article>";
    }).join("");
    var produtos = estado().produtos.filter(function (item) { return item.tipo !== "personalizado"; }).map(function (item) {
      var alerta = item.estoque <= item.minimo;
      return "<tr><td>" + ui.esc(item.titulo) + "</td><td>" + item.estoque + "</td><td>" + item.minimo + "</td><td>" + (alerta ? "alerta" : "") + "</td></tr>";
    }).join("");
    raiz.innerHTML = "<h1>Estoque</h1><div class=\"grade-3\">" + insumos + '</div><div class="tabela-rolagem cartao"><table><thead><tr><th>Peça pronta</th><th>Saldo</th><th>Mínimo</th><th></th></tr></thead><tbody>' + produtos + "</tbody></table></div>";
  }

  function choque(impressoes, atual) {
    return impressoes.some(function (item) {
      if (item.id === atual.id || item.maquinaId !== atual.maquinaId || item.status === "cancelada") return false;
      return item.inicio < atual.fim && atual.inicio < item.fim;
    });
  }

  function impressoes() {
    document.title = "Impressões · Administração";
    var concluidas = estado().impressoes.filter(function (item) { return item.status === "concluida"; });
    var falhas = concluidas.filter(function (item) { return item.sucesso === false; }).length;
    var taxa = concluidas.length ? falhas / concluidas.length : 0;
    raiz.innerHTML = "<h1>Impressões</h1><p>Taxa de falha realizada " + (taxa * 100).toLocaleString("pt-BR", { maximumFractionDigits: 1 }) + '%</p><button class="botao-linha" type="button" id="copiar-falha">Copiar taxa para a configuração</button><div id="lista"></div><form id="nova" class="cartao formulario"><h2>Nova impressão</h2><label>Máquina<select name="maquinaId">' + estado().maquinas.filter(function (item) { return item.controlaHora; }).map(function (item) { return '<option value="' + ui.esc(item.id) + '">' + ui.esc(item.nome) + "</option>"; }).join("") + '</select></label><label>Peça<select name="produtoId"><option value="">Sem peça de estoque</option>' + estado().produtos.map(function (item) { return '<option value="' + ui.esc(item.id) + '">' + ui.esc(item.titulo) + "</option>"; }).join("") + '</select></label><label>Peso previsto (g)<input name="pesoPrevisto" type="number" step="0.01"></label><label>Início<input name="inicio" type="datetime-local" required></label><label>Fim<input name="fim" type="datetime-local" required></label><button class="botao" type="submit">Colocar na fila</button></form><form id="manutencao" class="cartao formulario"><h2>Manutenção</h2><label>Tipo<select name="tipo"><option>Bico</option><option>PTFE</option><option>Placa</option><option>Limpeza</option></select></label><label>Valor<input name="valor" type="number" step="0.01" value="0"></label><button class="botao-teal" type="submit">Registrar</button></form>';
    function pintar() {
      document.getElementById("lista").innerHTML = estado().impressoes.map(function (item) {
        var maquina = estado().maquinas.find(function (maq) { return maq.id === item.maquinaId; });
        var aviso = choque(estado().impressoes, item) ? '<p class="aviso">Choque de horário nesta máquina.</p>' : "";
        var concluir = item.status !== "concluida" ? '<button class="botao" type="button" data-concluir="' + ui.esc(item.id) + '">Concluir</button>' : "";
        return '<article class="cartao"><strong>' + ui.esc(item.titulo) + "</strong><p>" + ui.esc(maquina ? maquina.nome : "") + " · " + ui.esc(item.status) + "</p><p>" + ui.esc(item.inicio) + " → " + ui.esc(item.fim) + "</p>" + aviso + concluir + "</article>";
      }).join("");
    }
    pintar();
    document.getElementById("copiar-falha").addEventListener("click", function () {
      if (!confirm("Copiar a taxa de falha realizada para a configuração? Orçamentos já fechados não mudam.")) return;
      Cria.alterar(function (atual) { atual.configuracao.taxaFalha = taxa; });
    });
    document.getElementById("nova").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      var produto = estado().produtos.find(function (item) { return item.id === dados.get("produtoId"); });
      Cria.alterar(function (atual) {
        atual.impressoes.push({ id: CriaCarrinho.novoId("imp"), maquinaId: String(dados.get("maquinaId")), produtoId: String(dados.get("produtoId") || ""), titulo: produto ? produto.titulo : "Peça avulsa", pesoPrevisto: Number(dados.get("pesoPrevisto")) || 0, pesoReal: 0, sucesso: null, motivo: "", kwh: 0, inicio: String(dados.get("inicio")), fim: String(dados.get("fim")), status: "na fila" });
      });
      impressoes();
    });
    document.getElementById("manutencao").addEventListener("submit", function (evento) {
      evento.preventDefault();
      var dados = new FormData(evento.target);
      if (!confirm("Lançar esta manutenção como despesa do mês?")) return;
      Cria.alterar(function (atual) {
        atual.lancamentos.push({ id: CriaCarrinho.novoId("lan"), data: dinheiro.hojeISO(), competencia: dinheiro.hojeISO().slice(0, 7), descricao: "Manutenção " + dados.get("tipo"), categoria: "gastoFixo", tipoInsumo: "", quantidade: 1, valor: Number(dados.get("valor")) || 0, link: "", kwh: null, tarifa: null, estornado: false, comprovante: "", origem: "manutencao" });
      });
      alert("Despesa lançada.");
    });
    if (!raiz.dataset.oficinaCliques) {
      raiz.dataset.oficinaCliques = "1";
    raiz.addEventListener("click", function (evento) {
      var id = evento.target.getAttribute && evento.target.getAttribute("data-concluir");
      if (!id) return;
      var sucesso = confirm("A impressão deu certo? OK para sucesso, Cancelar para falha.");
      var peso = Number(prompt("Peso real em gramas", "40")) || 0;
      var motivo = sucesso ? "" : (prompt("Motivo da falha", "Falha de adesão") || "Falha");
      Cria.alterar(function (atual) {
        var item = atual.impressoes.find(function (imp) { return imp.id === id; });
        if (!item) return;
        item.status = "concluida";
        item.sucesso = sucesso;
        item.pesoReal = peso;
        item.motivo = motivo;
        var insumo = atual.estoqueInsumos.find(function (cadastro) { return cadastro.nome === "PLA"; });
        if (insumo) insumo.saldo = Math.max(0, insumo.saldo - peso / 1000);
        if (sucesso && item.produtoId) {
          var produto = atual.produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
          if (produto) produto.estoque = Math.max(0, produto.estoque - 1);
        }
      });
      impressoes();
    });
    }
  }

  if (pagina === "admin-patrimonio") patrimonio();
  if (pagina === "admin-estoque") estoque();
  if (pagina === "admin-impressoes") impressoes();
})();
