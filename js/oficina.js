(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function patrimonio() {
    document.title = "Patrimônio · Administração";
    var compras = estado().lancamentos.filter(function (item) { return item.categoria === "investimento"; });
    raiz.innerHTML = '<h1>Patrimônio e estoque</h1><p>Impressoras, equipamentos, filamentos, ferramentas e outros insumos.</p><p><a class="botao" href="' + ui.url("paginas/admin/estoque.html") + '">Abrir estoque</a></p>' + estado().maquinas.map(function (maquina) {
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

  function gestaoImpressao() {
    document.title = "Gestão de impressão 3D · Administração";
    var maquinas = estado().maquinas.filter(function (item) { return item.controlaHora; });
    function daMaquina(id) {
      return estado().impressoes.filter(function (item) { return item.maquinaId === id; });
    }
    function produtoDe(item) {
      return estado().produtos.find(function (produto) { return produto.id === item.produtoId; });
    }
    function rotulo(maquina) {
      var lista = daMaquina(maquina.id);
      if (lista.some(function (item) { return item.status === "erro"; })) return "Com erro";
      if (lista.some(function (item) { return item.status === "pausada"; })) return "Pausada";
      if (lista.some(function (item) { return item.status === "imprimindo"; })) return "Imprimindo";
      return "Disponível";
    }
    function andamento(item) {
      var ini = new Date(item.inicio).getTime();
      var fim = new Date(item.fim).getTime();
      var agora = Date.now();
      if (!ini || !fim || fim <= ini) return { progresso: 0, restante: "sem previsão" };
      var progresso = agora <= ini ? 0 : agora >= fim ? 100 : Math.round(((agora - ini) / (fim - ini)) * 100);
      var minutos = Math.max(0, Math.round((fim - agora) / 60000));
      return { progresso: progresso, restante: minutos ? minutos + " min" : "no prazo" };
    }
    var monitor = maquinas.map(function (maquina) {
      var atual = daMaquina(maquina.id).find(function (item) { return item.status === "imprimindo" || item.status === "pausada" || item.status === "erro"; });
      var detalhe = "Sem trabalho atual.";
      if (atual) {
        var tempo = andamento(atual);
        detalhe = ui.esc(atual.titulo) + " · " + tempo.progresso + "% · " + tempo.restante + " · previsão " + ui.esc(String(atual.fim).replace("T", " ").slice(0, 16));
      }
      return '<article class="cartao"><h2>' + ui.esc(maquina.nome) + "</h2><p><strong>" + rotulo(maquina) + "</strong></p><p>" + detalhe + '</p><p class="discreto">Temperatura e alerta: sem leitura desta máquina.</p></article>';
    }).join("");
    var mesa = maquinas.map(function (maquina) {
      var trabalhos = daMaquina(maquina.id).filter(function (item) { return item.status === "imprimindo" || item.status === "na fila" || item.status === "pausada"; });
      var linhas = trabalhos.length ? trabalhos.map(function (item, indice) {
        var produto = produtoDe(item);
        return "<li>" + (item.status === "imprimindo" ? "Em andamento" : "Próximo") + " · " + ui.esc(item.titulo) + " · 1 peça · " + ui.esc(produto ? produto.material : "material não informado") + " · cor não informada · bico não informado · perfil não informado · prioridade " + (indice + 1) + "</li>";
      }).join("") : "<li>Mesa livre.</li>";
      var ocupadas = trabalhos.filter(function (item) { return item.status === "imprimindo"; }).length;
      return '<article class="cartao"><h2>' + ui.esc(maquina.nome) + "</h2><p>Ocupação: " + ocupadas + (ocupadas === 1 ? " trabalho em curso · " : " trabalhos em curso · ") + trabalhos.length + ' na mesa</p><ul>' + linhas + "</ul></article>";
    }).join("");
    var porMaquina = {};
    estado().impressoes.forEach(function (item) {
      if (item.status !== "concluida") return;
      if (!porMaquina[item.maquinaId]) porMaquina[item.maquinaId] = { ok: 0, falha: 0, motivos: [] };
      if (item.sucesso) porMaquina[item.maquinaId].ok += 1;
      else {
        porMaquina[item.maquinaId].falha += 1;
        if (item.motivo) porMaquina[item.maquinaId].motivos.push(item.motivo);
      }
    });
    var qualidade = maquinas.map(function (maquina) {
      var conta = porMaquina[maquina.id] || { ok: 0, falha: 0, motivos: [] };
      var total = conta.ok + conta.falha;
      var taxa = total ? Math.round((conta.ok / total) * 100) : 0;
      return '<article class="cartao"><h2>' + ui.esc(maquina.nome) + "</h2><p>" + conta.ok + (conta.ok === 1 ? " concluída com sucesso" : " concluídas com sucesso") + "</p><p>" + conta.falha + (conta.falha === 1 ? " com falha ou cancelada na conclusão" : " com falha ou canceladas na conclusão") + "</p><p>Taxa de sucesso " + taxa + "%</p><p>" + (conta.motivos.length ? "Motivos: " + ui.esc(conta.motivos.join(", ")) + ". Estas peças pedem reimpressão." : "Sem motivo de falha registrado.") + "</p></article>";
    }).join("");
    var horas = 0;
    var gramas = 0;
    estado().impressoes.forEach(function (item) {
      if (item.status !== "concluida") return;
      var ini = new Date(item.inicio).getTime();
      var fim = new Date(item.fim).getTime();
      if (fim > ini) horas += (fim - ini) / 3600000;
      gramas += Number(item.pesoReal) || Number(item.pesoPrevisto) || 0;
    });
    var emCurso = estado().impressoes.filter(function (item) { return item.status === "imprimindo"; }).length;
    var historico = maquinas.map(function (maquina) {
      var lista = daMaquina(maquina.id).filter(function (item) { return item.status === "concluida"; });
      var h = 0;
      var g = 0;
      lista.forEach(function (item) {
        var ini = new Date(item.inicio).getTime();
        var fim = new Date(item.fim).getTime();
        if (fim > ini) h += (fim - ini) / 3600000;
        g += Number(item.pesoReal) || 0;
      });
      return '<article class="cartao"><h2>' + ui.esc(maquina.nome) + "</h2><p>" + h.toFixed(1).replace(".", ",") + " h em operação nas conclusões</p><p>" + g + " g de filamento no peso real</p><p>Compra " + (maquina.compraValor ? dinheiro.real(maquina.compraValor) : "sem valor") + "</p><p>" + lista.length + " trabalhos concluídos</p></article>";
    }).join("");
    raiz.innerHTML = '<h1>Gestão de impressão 3D</h1><p>A mesa da oficina fica só aqui. A vitrine não mostra máquina nem fila.</p><p><a class="botao" href="' + ui.url("paginas/admin/impressoes.html") + '">Abrir a fila para concluir</a></p><h2>Monitoramento de impressoras</h2><p class="discreto">Disponível, imprimindo, pausada ou com erro.</p><div class="grade-3">' + monitor + '</div><h2>Gestão da mesa de trabalho</h2><div class="grade-3">' + mesa + '</div><h2>Qualidade e sucesso de impressão</h2><div class="grade-3">' + qualidade + '</div><h2>Histórico e desempenho</h2><article class="cartao"><p>' + horas.toFixed(1).replace(".", ",") + " h concluídas no período carregado</p><p>" + gramas + " g de filamento somados</p><p>" + emCurso + (emCurso === 1 ? " máquina em operação agora. " : " máquinas em operação agora. ") + "O restante das impressoras acompanhadas está parado.</p></article><div class=\"grade-3\">" + historico + '</div><p class="discreto">A leitura direta da impressora, por API, fica para uma etapa futura. Hoje a tela usa a fila gravada neste navegador.</p>';
  }

  if (pagina === "admin-patrimonio") patrimonio();
  if (pagina === "admin-impressao") gestaoImpressao();
  if (pagina === "admin-estoque") estoque();
  if (pagina === "admin-impressoes") impressoes();
})();
