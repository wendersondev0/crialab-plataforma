(function () {
  var ui = CriaUi;
  var dinheiro = CriaDinheiro;
  var pagina = document.body.getAttribute("data-pagina");
  var raiz = document.getElementById("conteudo");
  function estado() { return Cria.estado; }

  function patrimonio() {
    document.title = "Patrimônio · Administração";
    var params = new URLSearchParams(location.search);
    var vista = {
      aba: params.get("aba") || "tudo",
      q: params.get("q") || "",
      aberto: params.get("id") || ""
    };
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function categoriaDe(item) {
      return item.categoria || (item.controlaHora ? "impressora" : "equipamento");
    }
    function nomeCat(cat) {
      var mapa = {
        impressora: "Impressora 3D",
        "impressora-tinta": "Impressora tinta",
        secador: "Secador",
        ferramenta: "Ferramenta",
        acessorio: "Acessório",
        filamento: "Filamento",
        embalagem: "Embalagens",
        acabamento: "Acabamento",
        eletronico: "Elétrico / LED",
        ferragem: "Ferragem",
        equipamento: "Equipamento"
      };
      return mapa[cat] || cat;
    }
    function ehFilamento(item) {
      return (item.categoria || "filamento") === "filamento";
    }
    function ehEmbalagem(item) {
      return (item.categoria || "") === "embalagem";
    }
    function dataAq(iso) {
      if (!iso) return "Sem data";
      return dinheiro.dataBR(String(iso).slice(0, 10));
    }
    function saldoInsumo(item) {
      if (Array.isArray(item.cores) && item.cores.length) {
        return item.cores.reduce(function (total, cor) { return total + (Number(cor.saldo) || 0); }, 0);
      }
      if (Array.isArray(item.variantes) && item.variantes.length) {
        return item.variantes.reduce(function (total, linha) { return total + (Number(linha.saldo) || 0); }, 0);
      }
      return Number(item.saldo) || 0;
    }
    function bobinasInsumo(item) {
      if (!Array.isArray(item.cores)) return 0;
      return item.cores.reduce(function (total, cor) { return total + (Number(cor.bobinas) || 0); }, 0);
    }
    function passoInsumo(item) {
      return ehFilamento(item) ? 0.1 : 1;
    }
    function barraSaldo(saldo, minimo, unidade) {
      var max = Math.max(minimo * 2, saldo, 0.1);
      var pct = Math.min(100, Math.round((saldo / max) * 100));
      var classe = saldo <= minimo ? "baixa" : (saldo <= minimo * 1.5 ? "media" : "ok");
      return '<div class="pat-barra"><span class="pat-barra-fill ' + classe + '" style="width:' + pct + '%"></span></div><p class="pat-barra-txt">' + String(saldo).replace(".", ",") + " " + ui.esc(unidade) + " · mín. " + String(minimo).replace(".", ",") + "</p>";
    }
    function cardMaquina(item) {
      var cat = categoriaDe(item);
      var img = item.imagem ? '<img class="pat-thumb" src="' + ui.url(item.imagem) + '" alt="">' : '<span class="pat-thumb vazia">' + ico("estoque") + "</span>";
      var aberto = vista.aberto === item.id;
      return '<article class="pat-card' + (aberto ? " aberto" : "") + '" data-id="' + ui.esc(item.id) + '" data-tipo="maquina">' +
        '<button class="pat-card-btn" type="button" data-toggle="' + ui.esc(item.id) + '">' +
          '<span class="pat-foto">' + img + '<span class="pat-cat">' + ui.esc(nomeCat(cat)) + "</span></span>" +
          '<span class="pat-info"><strong class="pat-nome">' + ui.esc(item.nome) + "</strong>" +
            '<span class="pat-meta">Aquisição ' + ui.esc(dataAq(item.adquiridoEm || item.compraCompetencia)) + "</span>" +
            '<span class="pat-meta">' + (item.compraValor ? dinheiro.real(item.compraValor) : "Sem valor lançado") + (item.local ? " · " + ui.esc(item.local) : "") + "</span>" +
            (item.controlaHora ? '<span class="pat-chip">' + item.watts + " W</span>" : '<span class="pat-chip">Sem hora-máquina</span>') +
          "</span></button>" +
        '<div class="pat-detalhe"' + (aberto ? "" : " hidden") + ">" +
          '<div class="pat-grade-det">' +
            '<p><span>Marca / modelo</span><strong>' + ui.esc((item.marca || "—") + (item.modelo ? " · " + item.modelo : "")) + "</strong></p>" +
            '<p><span>Serial</span><strong>' + ui.esc(item.serial || "—") + "</strong></p>" +
            '<p><span>Status</span><strong>' + ui.esc(item.status || "ativa") + "</strong></p>" +
            '<p><span>Local</span><strong>' + ui.esc(item.local || "—") + "</strong></p>" +
            (item.controlaHora ? '<p><span>Depreciação</span><strong>' + dinheiro.real(item.depreciacaoPorMinuto, 3) + "/min</strong></p><p><span>Manutenção</span><strong>" + dinheiro.real(item.manutencaoPorMinuto, 3) + "/min</strong></p><p><span>Energia</span><strong>" + dinheiro.real(item.energiaPorMinuto, 4) + "/min</strong></p>" : "") +
          "</div>" +
          (item.notas ? '<p class="pat-notas">' + ui.esc(item.notas) + "</p>" : "") +
        "</div></article>";
    }
    function cardInsumo(item) {
      var saldo = saldoInsumo(item);
      var alerta = saldo <= Number(item.minimo);
      var img = item.imagem ? '<img class="pat-thumb" src="' + ui.url(item.imagem) + '" alt="">' : '<span class="pat-thumb vazia">' + ico("caixa") + "</span>";
      var aberto = vista.aberto === item.id;
      var passo = passoInsumo(item);
      var fil = ehFilamento(item);
      var chip = fil
        ? bobinasInsumo(item) + " bobinas · " + (item.cores || []).length + " cores"
        : nomeCat(item.categoria) + " · " + String(saldo).replace(".", ",") + " " + (item.unidade || "un");
      var linhasDetalhe = "";
      if (fil && item.cores && item.cores.length) {
        linhasDetalhe = item.cores.map(function (cor) {
          return '<li class="pat-cor" data-insumo="' + ui.esc(item.id) + '" data-cor="' + ui.esc(cor.id) + '">' +
            '<span class="pat-cor-swatch" style="background:' + ui.esc(cor.hex || "#ccc") + '"></span>' +
            '<span class="pat-cor-info"><strong>' + ui.esc(cor.nome) + "</strong><small>" + String(cor.saldo).replace(".", ",") + " " + ui.esc(item.unidade) + " · " + (Number(cor.bobinas) || 0) + " bobina" + ((Number(cor.bobinas) || 0) === 1 ? "" : "s") + "</small></span>" +
            '<span class="pat-cor-acoes"><button type="button" data-delta="-' + passo + '" aria-label="Retirar">−</button><button type="button" data-delta="' + passo + '" aria-label="Adicionar">+</button></span></li>';
        }).join("");
      } else if (item.variantes && item.variantes.length) {
        linhasDetalhe = item.variantes.map(function (linha) {
          return '<li class="pat-cor" data-insumo="' + ui.esc(item.id) + '" data-var="' + ui.esc(linha.id) + '">' +
            '<span class="pat-cor-swatch" style="background:#e8772a"></span>' +
            '<span class="pat-cor-info"><strong>' + ui.esc(linha.nome) + "</strong><small>" + String(linha.saldo).replace(".", ",") + " " + ui.esc(linha.unidade || item.unidade) + "</small></span>" +
            '<span class="pat-cor-acoes"><button type="button" data-delta="-' + passo + '" aria-label="Retirar">−</button><button type="button" data-delta="' + passo + '" aria-label="Adicionar">+</button></span></li>';
        }).join("");
      } else {
        linhasDetalhe = '<li class="pat-cor" data-insumo="' + ui.esc(item.id) + '" data-saldo-direto="1">' +
          '<span class="pat-cor-swatch" style="background:#148f93"></span>' +
          '<span class="pat-cor-info"><strong>Saldo atual</strong><small>' + String(saldo).replace(".", ",") + " " + ui.esc(item.unidade || "un") + "</small></span>" +
          '<span class="pat-cor-acoes"><button type="button" data-delta="-' + passo + '" aria-label="Retirar">−</button><button type="button" data-delta="' + passo + '" aria-label="Adicionar">+</button></span></li>';
      }
      return '<article class="pat-card' + (aberto ? " aberto" : "") + (alerta ? " alerta" : "") + '" data-id="' + ui.esc(item.id) + '" data-tipo="insumo">' +
        '<button class="pat-card-btn" type="button" data-toggle="' + ui.esc(item.id) + '">' +
          '<span class="pat-foto">' + img + '<span class="pat-cat">' + ui.esc(nomeCat(item.categoria || "filamento")) + "</span>" + (alerta ? '<span class="pat-alerta">Baixo</span>' : "") + "</span>" +
          '<span class="pat-info"><strong class="pat-nome">' + ui.esc(item.nome) + "</strong>" +
            '<span class="pat-meta">Aquisição ' + ui.esc(dataAq(item.adquiridoEm)) + (item.fornecedor ? " · " + ui.esc(item.fornecedor) : "") + "</span>" +
            barraSaldo(saldo, Number(item.minimo) || 0, item.unidade || "un") +
            '<span class="pat-chip">' + ui.esc(chip) + "</span>" +
          "</span></button>" +
        '<div class="pat-detalhe"' + (aberto ? "" : " hidden") + ">" +
          (item.origemPlanilha ? '<p class="pat-notas">Planilha: ' + ui.esc(item.origemPlanilha) + "</p>" : "") +
          '<p class="pat-det-titulo">' + (fil ? "Quantidade por cor" : "Detalhe do estoque") + "</p><ul class=\"pat-cores\">" + linhasDetalhe + "</ul>" +
          (alerta ? '<a class="botao at-btn" href="' + ui.url("paginas/admin/financeiro.html?aba=caderno&mes=" + dinheiro.hojeISO().slice(0, 7) + "&categoria=filamento&compra=" + encodeURIComponent(item.nome)) + '">' + ico("carrinho") + "<span>Lançar compra</span></a>" : "") +
        "</div></article>";
    }
    function pintar() {
      var texto = vista.q.trim().toLowerCase();
      var maquinas = estado().maquinas || [];
      var insumos = estado().estoqueInsumos || [];
      var compras = (estado().lancamentos || []).filter(function (item) { return item.categoria === "investimento"; });
      var impressoras = maquinas.filter(function (item) { return categoriaDe(item) === "impressora" || categoriaDe(item) === "impressora-tinta"; });
      var equipamentos = maquinas.filter(function (item) { return categoriaDe(item) !== "impressora" && categoriaDe(item) !== "impressora-tinta"; });
      var filamentos = insumos.filter(ehFilamento);
      var embalagens = insumos.filter(ehEmbalagem);
      var outrosIns = insumos.filter(function (item) { return !ehFilamento(item) && !ehEmbalagem(item); });
      var alertas = insumos.filter(function (item) { return saldoInsumo(item) <= Number(item.minimo); }).length;
      function passaTexto(blob) {
        return !texto || String(blob).toLowerCase().indexOf(texto) >= 0;
      }
      var listaMaq = maquinas.filter(function (item) {
        if (vista.aba === "impressoras" && categoriaDe(item) !== "impressora" && categoriaDe(item) !== "impressora-tinta") return false;
        if (vista.aba === "equipamentos" && (categoriaDe(item) === "impressora" || categoriaDe(item) === "impressora-tinta")) return false;
        if (vista.aba === "filamentos" || vista.aba === "insumos" || vista.aba === "embalagens" || vista.aba === "compras") return false;
        return passaTexto([item.nome, item.marca, item.modelo, item.local, nomeCat(categoriaDe(item))].join(" "));
      });
      var listaIns = insumos.filter(function (item) {
        if (vista.aba === "impressoras" || vista.aba === "equipamentos" || vista.aba === "compras") return false;
        if (vista.aba === "filamentos" && !ehFilamento(item)) return false;
        if (vista.aba === "embalagens" && !ehEmbalagem(item)) return false;
        if (vista.aba === "insumos" && (ehFilamento(item) || ehEmbalagem(item))) return false;
        if (vista.aba !== "tudo" && vista.aba !== "filamentos" && vista.aba !== "insumos" && vista.aba !== "embalagens") return false;
        return passaTexto([item.nome, item.fornecedor, item.origemPlanilha, nomeCat(item.categoria), (item.cores || []).map(function (c) { return c.nome; }).join(" "), (item.variantes || []).map(function (v) { return v.nome; }).join(" ")].join(" "));
      });
      var kpis = '<div class="admin-prod-kpis pat-kpis">' +
        '<button class="admin-prod-kpi' + (vista.aba === "tudo" ? " ativo" : "") + '" type="button" data-aba="tudo"><strong>' + (maquinas.length + insumos.length) + "</strong><span>Tudo</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "impressoras" ? " ativo" : "") + '" type="button" data-aba="impressoras"><strong>' + impressoras.length + "</strong><span>Impressoras</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "filamentos" ? " ativo" : "") + '" type="button" data-aba="filamentos"><strong>' + filamentos.length + "</strong><span>Filamentos</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "embalagens" ? " ativo" : "") + '" type="button" data-aba="embalagens"><strong>' + embalagens.length + "</strong><span>Embalagens</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "insumos" ? " ativo" : "") + '" type="button" data-aba="insumos"><strong>' + outrosIns.length + "</strong><span>Insumos</span></button>" +
        '<button class="admin-prod-kpi' + (vista.aba === "equipamentos" ? " ativo" : "") + '" type="button" data-aba="equipamentos"><strong>' + equipamentos.length + "</strong><span>Ferramentas</span></button>" +
        "</div>";
      var busca = '<form class="at-busca" id="pat-busca"><label class="campo-busca"><span class="visually-hidden">Buscar</span>' + ico("busca") + '<input name="q" type="search" placeholder="Buscar caixa, fita, bolha, papel foto, filamento..." value="' + ui.esc(vista.q) + '"></label>' +
        (alertas ? '<span class="pat-kpi-alerta">' + alertas + " item" + (alertas === 1 ? "" : "s") + " abaixo do mínimo</span>" : "") + "</form>";
      var secoes = "";
      if (vista.aba === "compras") {
        secoes = '<div class="pat-compras">' + (compras.length ? compras.map(function (item) {
          return '<article class="pat-compra"><strong>' + ui.esc(item.descricao) + "</strong><span>" + ui.esc(dinheiro.competenciaBR(item.competencia)) + "</span><span>" + dinheiro.real(item.valor) + "</span></article>";
        }).join("") : ui.vazio("Nenhuma compra de investimento lançada.")) + "</div>";
      } else {
        if (listaMaq.length) {
          secoes += '<section class="pat-sec"><h2 class="pat-sec-titulo">' + (vista.aba === "equipamentos" ? "Equipamentos e ferramentas" : (vista.aba === "impressoras" ? "Impressoras" : "Máquinas e equipamentos")) + '</h2><div class="pat-grade">' + listaMaq.map(cardMaquina).join("") + "</div></section>";
        }
        if (listaIns.length) {
          var tituloIns = "Filamentos e insumos";
          if (vista.aba === "filamentos") tituloIns = "Filamentos (cores e kg em estoque)";
          else if (vista.aba === "embalagens") tituloIns = "Embalagens (caixas, fitas, bolha, papéis e cartões)";
          else if (vista.aba === "insumos") tituloIns = "Insumos (LED, copos, argolas, verniz...)";
          secoes += '<section class="pat-sec"><h2 class="pat-sec-titulo">' + tituloIns + '</h2><div class="pat-grade">' + listaIns.map(cardInsumo).join("") + "</div></section>";
        }
        if (!listaMaq.length && !listaIns.length) secoes = ui.vazio("Nada encontrado com esses filtros.");
      }
      var fluxo = '<div class="admin-prod-fluxo">' +
        '<button class="botao-linha admin-fluxo' + (vista.aba === "compras" ? " ativo-fluxo" : "") + '" type="button" data-aba="compras">' + ico("documento") + "<span>Compras da planilha</span></button>" +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/impressao.html?aba=mesa") + '">' + ico("admin") + "<span>Mesa de impressão</span></a>" +
        '<a class="botao-linha admin-fluxo" href="' + ui.url("paginas/admin/impressoes.html") + '">' + ico("estoque") + "<span>Fila de impressões</span></a>" +
        "</div>";
      raiz.innerHTML = '<div class="admin-prod-cab"><div><h1 class="m-0">Patrimônio e estoque</h1><p class="admin-prod-intro m-0 mt-1">Estoque alinhado às planilhas de gastos: PLA/PETG/TPU por cor, LED, copos, argolas, caixas, NFC e ferramentas. Toque no card para ajustar saldos.</p></div></div>' +
        kpis + fluxo + busca + '<div class="admin-prod-painel cartao at-painel">' + secoes + "</div>";
      ligar();
      ui.revelar(raiz);
    }
    function ligar() {
      raiz.querySelectorAll("[data-aba]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-aba") || "tudo";
          vista.aberto = "";
          pintar();
        });
      });
      var form = document.getElementById("pat-busca");
      if (form) {
        form.addEventListener("submit", function (evento) { evento.preventDefault(); });
        form.querySelector('[name="q"]').addEventListener("input", function (evento) {
          vista.q = evento.target.value;
          window.clearTimeout(form._t);
          form._t = window.setTimeout(pintar, 180);
        });
      }
      raiz.querySelectorAll("[data-toggle]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-toggle");
          vista.aberto = vista.aberto === id ? "" : id;
          pintar();
        });
      });
      raiz.querySelectorAll(".pat-cor [data-delta]").forEach(function (botao) {
        botao.addEventListener("click", function (evento) {
          evento.stopPropagation();
          var linha = botao.closest(".pat-cor");
          var insumoId = linha.getAttribute("data-insumo");
          var corId = linha.getAttribute("data-cor");
          var varId = linha.getAttribute("data-var");
          var direto = linha.getAttribute("data-saldo-direto");
          var delta = Number(botao.getAttribute("data-delta")) || 0;
          Cria.alterar(function (atual) {
            var insumo = atual.estoqueInsumos.find(function (item) { return item.id === insumoId; });
            if (!insumo) return;
            function arred(valor) {
              return Math.max(0, Math.round((valor + delta) * 10) / 10);
            }
            if (corId && insumo.cores) {
              var cor = insumo.cores.find(function (item) { return item.id === corId; });
              if (!cor) return;
              cor.saldo = arred(Number(cor.saldo) || 0);
              insumo.saldo = insumo.cores.reduce(function (total, item) { return total + (Number(item.saldo) || 0); }, 0);
              return;
            }
            if (varId && insumo.variantes) {
              var variante = insumo.variantes.find(function (item) { return item.id === varId; });
              if (!variante) return;
              variante.saldo = arred(Number(variante.saldo) || 0);
              insumo.saldo = insumo.variantes.reduce(function (total, item) { return total + (Number(item.saldo) || 0); }, 0);
              return;
            }
            if (direto) insumo.saldo = arred(Number(insumo.saldo) || 0);
          });
          vista.aberto = insumoId;
          pintar();
        });
      });
    }
    pintar();
  }

  function estoque() {
    location.replace(ui.url("paginas/admin/patrimonio.html?aba=filamentos"));
  }

  function choque(impressoes, atual) {
    return impressoes.some(function (item) {
      if (item.id === atual.id || item.maquinaId !== atual.maquinaId || item.status === "cancelada") return false;
      return item.inicio < atual.fim && atual.inicio < item.fim;
    });
  }

  function impressoes() {
    location.replace(ui.url("paginas/admin/impressao.html?aba=fila"));
  }

  function gestaoImpressao() {
    document.title = "Impressão · Administração";
    var params = new URLSearchParams(location.search);
    var abasOk = { mesa: 1, fila: 1, qualidade: 1, historico: 1, manutencao: 1 };
    var vista = {
      aba: abasOk[params.get("aba")] ? params.get("aba") : "mesa",
      maquina: params.get("maquina") || "",
      status: params.get("status") || "",
      q: params.get("q") || "",
      concluirId: params.get("concluir") || "",
      ok: ""
    };
    function ico(nome) {
      return '<span class="ico inline-flex h-4 w-4 shrink-0 items-center justify-center [&>svg]:h-full [&>svg]:w-full">' + (ui.icone ? ui.icone(nome) : "") + "</span>";
    }
    function maquinasHora() {
      return (estado().maquinas || []).filter(function (item) { return item.controlaHora; });
    }
    function maquinaDe(id) {
      return (estado().maquinas || []).find(function (maq) { return maq.id === id; });
    }
    function produtoDe(item) {
      return (estado().produtos || []).find(function (produto) { return produto.id === item.produtoId; });
    }
    function daMaquina(id) {
      return (estado().impressoes || []).filter(function (item) { return item.maquinaId === id; });
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
    function rotuloMaquina(maquina) {
      var lista = daMaquina(maquina.id);
      if (lista.some(function (item) { return item.status === "erro"; })) return { txt: "Com erro", cls: "imp-selo alerta" };
      if (lista.some(function (item) { return item.status === "pausada"; })) return { txt: "Pausada", cls: "imp-selo mute" };
      if (lista.some(function (item) { return item.status === "imprimindo"; })) return { txt: "Imprimindo", cls: "imp-selo ok" };
      if (lista.some(function (item) { return item.status === "na fila"; })) return { txt: "Na fila", cls: "imp-selo" };
      return { txt: "Disponível", cls: "imp-selo ok" };
    }
    function seloStatus(status, sucesso) {
      if (status === "concluida" && sucesso === false) return "imp-selo alerta";
      if (status === "concluida") return "imp-selo ok";
      if (status === "imprimindo") return "imp-selo ok";
      if (status === "pausada" || status === "cancelada") return "imp-selo mute";
      if (status === "erro") return "imp-selo alerta";
      return "imp-selo";
    }
    function fmtQuando(iso) {
      var t = String(iso || "").replace("T", " ");
      return t.length >= 16 ? t.slice(0, 16) : t;
    }
    function taxaFalha() {
      var concluidas = (estado().impressoes || []).filter(function (item) { return item.status === "concluida"; });
      var falhas = concluidas.filter(function (item) { return item.sucesso === false; }).length;
      return { total: concluidas.length, falhas: falhas, taxa: concluidas.length ? falhas / concluidas.length : 0 };
    }
    function sincronizarUrl() {
      var url = new URL(location.href);
      url.searchParams.set("aba", vista.aba);
      ["maquina", "status", "q"].forEach(function (chave) {
        if (vista[chave]) url.searchParams.set(chave, vista[chave]);
        else url.searchParams.delete(chave);
      });
      if (vista.concluirId) url.searchParams.set("concluir", vista.concluirId);
      else url.searchParams.delete("concluir");
      history.replaceState(null, "", url.pathname + url.search);
    }
    function flash(msg) {
      vista.ok = msg;
      pintar();
      window.setTimeout(function () {
        if (vista.ok === msg) { vista.ok = ""; pintar(); }
      }, 2200);
    }
    function concluirTrabalho(id, sucesso, peso, motivo, kwh) {
      Cria.alterar(function (atual) {
        var item = atual.impressoes.find(function (imp) { return imp.id === id; });
        if (!item) return;
        item.status = "concluida";
        item.sucesso = sucesso;
        item.pesoReal = Number(peso) || 0;
        item.motivo = sucesso ? "" : (motivo || "Falha");
        item.kwh = Number(kwh) || item.kwh || 0;
        var insumo = atual.estoqueInsumos.find(function (cadastro) { return cadastro.nome === "PLA"; });
        if (insumo) insumo.saldo = Math.max(0, insumo.saldo - item.pesoReal / 1000);
        if (sucesso && item.produtoId) {
          var produto = atual.produtos.find(function (cadastro) { return cadastro.id === item.produtoId; });
          if (produto) produto.estoque = Math.max(0, (Number(produto.estoque) || 0) - 1);
        }
      });
      vista.concluirId = "";
      flash(sucesso ? "Impressão concluída com sucesso." : "Falha registrada. Filamento baixado.");
    }
    function cardTrabalho(item, opcoes) {
      var maquina = maquinaDe(item.maquinaId);
      var produto = produtoDe(item);
      var tempo = andamento(item);
      var conflito = choque(estado().impressoes, item);
      var acoes = '<div class="imp-acoes">';
      if (item.status === "na fila") {
        acoes += '<button class="botao" type="button" data-imp="' + ui.esc(item.id) + '" data-para="imprimindo">' + ico("admin") + "<span>Iniciar</span></button>";
      }
      if (item.status === "imprimindo") {
        acoes += '<button class="botao-linha" type="button" data-imp="' + ui.esc(item.id) + '" data-para="pausada"><span>Pausar</span></button>';
      }
      if (item.status === "pausada" || item.status === "erro") {
        acoes += '<button class="botao" type="button" data-imp="' + ui.esc(item.id) + '" data-para="imprimindo"><span>Retomar</span></button>';
      }
      if (item.status !== "concluida" && item.status !== "cancelada") {
        acoes += '<button class="botao-teal" type="button" data-abrir-concluir="' + ui.esc(item.id) + '">' + ico("documento") + "<span>Concluir</span></button>";
        acoes += '<button class="botao-linha" type="button" data-imp="' + ui.esc(item.id) + '" data-para="cancelada"><span>Cancelar</span></button>';
      }
      acoes += "</div>";
      var formConcluir = "";
      if (vista.concluirId === item.id && item.status !== "concluida") {
        formConcluir = '<form class="imp-concluir" data-form-concluir="' + ui.esc(item.id) + '">' +
          '<p class="imp-meta m-0">Informe o resultado. Sucesso baixa estoque da peça; falha baixa só o filamento.</p>' +
          '<div class="imp-concluir-grade">' +
          '<label class="fin-campo"><span>Resultado</span><select name="sucesso"><option value="1">Sucesso</option><option value="0">Falha</option></select></label>' +
          '<label class="fin-campo"><span>Peso real (g)</span><input name="peso" type="number" step="0.01" value="' + (item.pesoPrevisto || 40) + '" required></label>' +
          '<label class="fin-campo"><span>kWh</span><input name="kwh" type="number" step="0.01" value="' + (item.kwh || 0) + '"></label>' +
          '<label class="fin-campo imp-span2"><span>Motivo (se falha)</span><input name="motivo" placeholder="Ex.: Descolou da mesa"></label>' +
          "</div>" +
          '<div class="imp-acoes"><button class="botao" type="submit">Salvar conclusão</button><button class="botao-linha" type="button" data-cancelar-concluir="1">Fechar</button></div>' +
          "</form>";
      }
      return '<article class="imp-card' + (conflito ? " choque" : "") + (item.status === "imprimindo" ? " ativo" : "") + '">' +
        '<div class="imp-card-topo">' +
          '<div><strong class="imp-titulo">' + ui.esc(item.titulo) + "</strong>" +
          '<p class="imp-meta">' + ui.esc(maquina ? maquina.nome : "Máquina") +
          (produto && produto.material ? " · " + ui.esc(produto.material) : "") +
          " · " + ui.esc(fmtQuando(item.inicio)) + " → " + ui.esc(fmtQuando(item.fim)) + "</p></div>" +
          '<span class="' + seloStatus(item.status, item.sucesso) + '">' + ui.esc(item.status === "concluida" ? (item.sucesso ? "sucesso" : "falha") : item.status) + "</span>" +
        "</div>" +
        (item.status === "imprimindo" || item.status === "pausada" ? '<div class="imp-barra"><span style="width:' + tempo.progresso + '%"></span></div><p class="imp-meta">' + tempo.progresso + "% · " + ui.esc(tempo.restante) + "</p>" : "") +
        (conflito ? '<p class="imp-aviso">Choque de horário nesta máquina.</p>' : "") +
        (item.motivo ? '<p class="imp-meta">Motivo: ' + ui.esc(item.motivo) + "</p>" : "") +
        (item.status === "concluida" ? '<p class="imp-meta">Peso real ' + String(item.pesoReal).replace(".", ",") + " g · " + (item.kwh || 0) + " kWh</p>" : "") +
        (opcoes && opcoes.semAcoes ? "" : acoes) +
        formConcluir +
        "</article>";
    }
    function pintar() {
      sincronizarUrl();
      var maquinas = maquinasHora();
      var impressoesLista = estado().impressoes || [];
      var emCurso = impressoesLista.filter(function (item) { return item.status === "imprimindo"; }).length;
      var naFila = impressoesLista.filter(function (item) { return item.status === "na fila"; }).length;
      var falhaInfo = taxaFalha();
      var choques = impressoesLista.filter(function (item) { return item.status !== "concluida" && item.status !== "cancelada" && choque(impressoesLista, item); }).length;
      var kpis = '<div class="imp-kpis">' +
        '<button class="imp-kpi' + (vista.aba === "mesa" ? " ativo" : "") + '" type="button" data-aba="mesa"><strong>' + emCurso + '</strong><span>Imprimindo</span></button>' +
        '<button class="imp-kpi' + (vista.aba === "fila" ? " ativo" : "") + '" type="button" data-aba="fila" data-status="na fila"><strong>' + naFila + '</strong><span>Na fila</span></button>' +
        '<button class="imp-kpi' + (vista.aba === "fila" && !vista.status ? " ativo" : "") + '" type="button" data-aba="fila" data-status=""><strong>' + impressoesLista.length + '</strong><span>Trabalhos</span></button>' +
        '<button class="imp-kpi' + (vista.aba === "qualidade" ? " ativo" : "") + '" type="button" data-aba="qualidade"><strong>' + (falhaInfo.taxa * 100).toLocaleString("pt-BR", { maximumFractionDigits: 1 }) + '%</strong><span>Taxa de falha</span></button>' +
        '<button class="imp-kpi' + (choques ? " alerta" : "") + '" type="button" data-aba="fila"><strong>' + choques + '</strong><span>Choques</span></button>' +
        "</div>";
      var abas = '<div class="fin-abas">' +
        [["mesa", "Mesa"], ["fila", "Fila"], ["qualidade", "Qualidade"], ["historico", "Histórico"], ["manutencao", "Manutenção"]].map(function (par) {
          return '<button class="fin-aba' + (vista.aba === par[0] ? " ativo" : "") + '" type="button" data-aba="' + par[0] + '">' + par[1] + "</button>";
        }).join("") + "</div>";
      var okHtml = vista.ok ? '<p class="sys-ok">' + ui.esc(vista.ok) + "</p>" : "";
      var corpo = "";

      if (vista.aba === "mesa") {
        corpo = '<div class="imp-mesa-grade">' + maquinas.map(function (maquina) {
          var estadoMaq = rotuloMaquina(maquina);
          var atuais = daMaquina(maquina.id).filter(function (item) {
            return item.status === "imprimindo" || item.status === "pausada" || item.status === "erro" || item.status === "na fila";
          }).sort(function (a, b) {
            var ordem = { imprimindo: 0, pausada: 1, erro: 2, "na fila": 3 };
            return (ordem[a.status] || 9) - (ordem[b.status] || 9);
          });
          var atual = atuais[0];
          var img = maquina.imagem ? '<img class="imp-maq-foto" src="' + ui.url(maquina.imagem) + '" alt="">' : "";
          return '<section class="imp-maq-card">' +
            '<div class="imp-maq-topo">' + img +
            '<div><h2 class="imp-titulo m-0">' + ui.esc(maquina.nome) + '</h2><p class="imp-meta m-0">' + ui.esc(maquina.local || "") + " · " + (maquina.watts || 0) + " W</p></div>" +
            '<span class="' + estadoMaq.cls + '">' + estadoMaq.txt + "</span></div>" +
            (atuais.length ? atuais.map(function (item) { return cardTrabalho(item); }).join("") : '<p class="imp-livre">Mesa livre nesta máquina.</p>') +
            '<button class="botao-linha imp-link-fila" type="button" data-aba="fila" data-maquina="' + ui.esc(maquina.id) + '">Ver fila desta máquina</button>' +
            "</section>";
        }).join("") + "</div>";
      } else if (vista.aba === "fila") {
        var lista = impressoesLista.filter(function (item) {
          if (vista.maquina && item.maquinaId !== vista.maquina) return false;
          if (vista.status && item.status !== vista.status) return false;
          if (vista.q) {
            var txt = (item.titulo + " " + item.status + " " + (maquinaDe(item.maquinaId) || {}).nome).toLowerCase();
            if (txt.indexOf(vista.q.toLowerCase()) < 0) return false;
          }
          return true;
        }).slice().sort(function (a, b) { return String(b.inicio).localeCompare(String(a.inicio)); });
        corpo = '<div class="imp-fila-layout">' +
          '<div><form id="imp-filtros" class="imp-filtros">' +
          '<label class="fin-campo fis-busca"><span>Buscar</span><input name="q" type="search" value="' + ui.esc(vista.q) + '" placeholder="Peça ou máquina"></label>' +
          '<label class="fin-campo"><span>Máquina</span><select name="maquina"><option value="">Todas</option>' +
          maquinas.map(function (maq) {
            return '<option value="' + ui.esc(maq.id) + '"' + (vista.maquina === maq.id ? " selected" : "") + ">" + ui.esc(maq.nome) + "</option>";
          }).join("") + "</select></label>" +
          '<label class="fin-campo"><span>Status</span><select name="status"><option value="">Todos</option>' +
          ["na fila", "imprimindo", "pausada", "erro", "concluida", "cancelada"].map(function (st) {
            return '<option value="' + st + '"' + (vista.status === st ? " selected" : "") + ">" + st + "</option>";
          }).join("") + "</select></label></form>" +
          '<div class="imp-lista">' + (lista.length ? lista.map(function (item) { return cardTrabalho(item); }).join("") : '<div class="fis-vazio cartao"><p>Nenhum trabalho com esses filtros.</p></div>') + "</div></div>" +
          '<form id="imp-nova" class="imp-card imp-form-nova">' +
          '<h2 class="imp-titulo">Nova impressão</h2>' +
          '<label class="fin-campo"><span>Máquina</span><select name="maquinaId" required>' +
          maquinas.map(function (maq) { return '<option value="' + ui.esc(maq.id) + '">' + ui.esc(maq.nome) + "</option>"; }).join("") +
          "</select></label>" +
          '<label class="fin-campo"><span>Peça do estoque</span><select name="produtoId"><option value="">Peça avulsa</option>' +
          (estado().produtos || []).map(function (produto) {
            return '<option value="' + ui.esc(produto.id) + '">' + ui.esc(produto.titulo) + "</option>";
          }).join("") + "</select></label>" +
          '<label class="fin-campo"><span>Peso previsto (g)</span><input name="pesoPrevisto" type="number" step="0.01" value="40"></label>' +
          '<label class="fin-campo"><span>Início</span><input name="inicio" type="datetime-local" required></label>' +
          '<label class="fin-campo"><span>Fim</span><input name="fim" type="datetime-local" required></label>' +
          '<button class="botao" type="submit">' + ico("caixa") + "<span>Colocar na fila</span></button>" +
          "</form></div>";
      } else if (vista.aba === "qualidade") {
        var info = falhaInfo;
        var porMaquina = {};
        impressoesLista.forEach(function (item) {
          if (item.status !== "concluida") return;
          if (!porMaquina[item.maquinaId]) porMaquina[item.maquinaId] = { ok: 0, falha: 0, motivos: [] };
          if (item.sucesso) porMaquina[item.maquinaId].ok += 1;
          else {
            porMaquina[item.maquinaId].falha += 1;
            if (item.motivo) porMaquina[item.maquinaId].motivos.push(item.motivo);
          }
        });
        corpo = '<div class="imp-card imp-qualidade-topo">' +
          '<div><h2 class="imp-titulo m-0">Taxa de falha realizada</h2><p class="imp-meta">' + info.falhas + " falha" + (info.falhas === 1 ? "" : "s") + " em " + info.total + " conclusões · " + (info.taxa * 100).toLocaleString("pt-BR", { maximumFractionDigits: 1 }) + "%</p></div>" +
          '<button class="botao" type="button" id="imp-copiar-falha">' + ico("documento") + "<span>Copiar para o Sistema</span></button>" +
          "</div>" +
          '<div class="imp-mesa-grade">' + maquinas.map(function (maquina) {
            var conta = porMaquina[maquina.id] || { ok: 0, falha: 0, motivos: [] };
            var total = conta.ok + conta.falha;
            var okPct = total ? Math.round((conta.ok / total) * 100) : 0;
            return '<article class="imp-card"><div class="imp-card-topo"><h2 class="imp-titulo m-0">' + ui.esc(maquina.nome) + '</h2><span class="imp-selo ok">' + okPct + "% sucesso</span></div>" +
              '<div class="imp-barra"><span style="width:' + okPct + '%"></span></div>' +
              '<p class="imp-meta">' + conta.ok + " ok · " + conta.falha + " falha" + (conta.falha === 1 ? "" : "s") + "</p>" +
              '<p class="imp-meta">' + (conta.motivos.length ? "Motivos: " + ui.esc(conta.motivos.join(", ")) : "Sem motivo de falha.") + "</p></article>";
          }).join("") + "</div>";
      } else if (vista.aba === "historico") {
        var horas = 0;
        var gramas = 0;
        impressoesLista.forEach(function (item) {
          if (item.status !== "concluida") return;
          var ini = new Date(item.inicio).getTime();
          var fim = new Date(item.fim).getTime();
          if (fim > ini) horas += (fim - ini) / 3600000;
          gramas += Number(item.pesoReal) || Number(item.pesoPrevisto) || 0;
        });
        var concluidas = impressoesLista.filter(function (item) { return item.status === "concluida"; }).slice().sort(function (a, b) {
          return String(b.fim).localeCompare(String(a.fim));
        });
        corpo = '<div class="imp-kpis imp-hist-kpis">' +
          '<article class="imp-kpi valor"><strong>' + horas.toFixed(1).replace(".", ",") + " h</strong><span>Concluídas</span></article>" +
          '<article class="imp-kpi valor"><strong>' + String(Math.round(gramas)).replace(/\B(?=(\d{3})+(?!\d))/g, ".") + " g</strong><span>Filamento</span></article>" +
          '<article class="imp-kpi valor"><strong>' + emCurso + "</strong><span>Em operação agora</span></article>" +
          '<article class="imp-kpi valor"><strong>' + concluidas.length + "</strong><span>Trabalhos fechados</span></article>" +
          "</div>" +
          '<div class="imp-mesa-grade">' + maquinas.map(function (maquina) {
            var lista = daMaquina(maquina.id).filter(function (item) { return item.status === "concluida"; });
            var h = 0;
            var g = 0;
            lista.forEach(function (item) {
              var ini = new Date(item.inicio).getTime();
              var fim = new Date(item.fim).getTime();
              if (fim > ini) h += (fim - ini) / 3600000;
              g += Number(item.pesoReal) || 0;
            });
            return '<article class="imp-card"><h2 class="imp-titulo">' + ui.esc(maquina.nome) + "</h2>" +
              '<p class="imp-meta">' + h.toFixed(1).replace(".", ",") + " h · " + g + " g · " + lista.length + " conclusões</p>" +
              '<p class="imp-meta">Compra ' + (maquina.compraValor ? dinheiro.real(maquina.compraValor) : "sem valor") + "</p></article>";
          }).join("") + "</div>" +
          '<div class="imp-lista">' + (concluidas.length ? concluidas.map(function (item) { return cardTrabalho(item, { semAcoes: true }); }).join("") : '<p class="imp-meta">Ainda não há conclusões.</p>') + "</div>";
      } else {
        corpo = '<form id="imp-manutencao" class="imp-card imp-form-nova">' +
          '<h2 class="imp-titulo">Registrar manutenção</h2>' +
          '<p class="imp-meta">Se confirmar, vira despesa do mês no Financeiro (gasto fixo).</p>' +
          '<label class="fin-campo"><span>Tipo</span><select name="tipo"><option>Bico</option><option>PTFE</option><option>Placa</option><option>Limpeza</option></select></label>' +
          '<label class="fin-campo"><span>Máquina</span><select name="maquinaId"><option value="">Geral</option>' +
          maquinas.map(function (maq) { return '<option value="' + ui.esc(maq.id) + '">' + ui.esc(maq.nome) + "</option>"; }).join("") +
          "</select></label>" +
          '<label class="fin-campo"><span>Valor (R$)</span><input name="valor" type="number" step="0.01" value="0" required></label>' +
          '<button class="botao-teal" type="submit">' + ico("estoque") + "<span>Lançar despesa</span></button>" +
          "</form>";
      }

      raiz.innerHTML =
        '<div class="admin-prod-cab"><div><h1 class="m-0">Impressão</h1><p class="admin-prod-intro m-0 mt-1">Mesa da oficina, fila por máquina, qualidade e manutenção — fora da vitrine.</p></div>' +
        '<a class="botao-linha" href="' + ui.url("paginas/admin/patrimonio.html") + '">' + ico("estoque") + "<span>Patrimônio</span></a></div>" +
        kpis + abas + okHtml + corpo;

      raiz.querySelectorAll("[data-aba]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.aba = botao.getAttribute("data-aba") || "mesa";
          if (botao.hasAttribute("data-status")) vista.status = botao.getAttribute("data-status") || "";
          if (botao.hasAttribute("data-maquina")) vista.maquina = botao.getAttribute("data-maquina") || "";
          vista.concluirId = "";
          vista.ok = "";
          pintar();
        });
      });

      var filtros = document.getElementById("imp-filtros");
      if (filtros) {
        filtros.addEventListener("change", function () {
          var dados = new FormData(filtros);
          vista.maquina = String(dados.get("maquina") || "");
          vista.status = String(dados.get("status") || "");
          pintar();
        });
        filtros.querySelector('[name="q"]').addEventListener("input", function (evento) {
          vista.q = evento.target.value;
          window.clearTimeout(raiz._impT);
          raiz._impT = window.setTimeout(pintar, 180);
        });
      }

      var nova = document.getElementById("imp-nova");
      if (nova) nova.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        var produto = estado().produtos.find(function (item) { return item.id === dados.get("produtoId"); });
        var inicio = String(dados.get("inicio"));
        var fim = String(dados.get("fim"));
        if (fim <= inicio) {
          flash("O fim precisa ser depois do início.");
          return;
        }
        Cria.alterar(function (atual) {
          atual.impressoes.push({
            id: CriaCarrinho.novoId("imp"),
            maquinaId: String(dados.get("maquinaId")),
            produtoId: String(dados.get("produtoId") || ""),
            titulo: produto ? produto.titulo : "Peça avulsa",
            pesoPrevisto: Number(dados.get("pesoPrevisto")) || 0,
            pesoReal: 0,
            sucesso: null,
            motivo: "",
            kwh: 0,
            inicio: inicio,
            fim: fim,
            status: "na fila"
          });
        });
        vista.status = "na fila";
        flash("Trabalho colocado na fila.");
      });

      raiz.querySelectorAll("[data-imp][data-para]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          var id = botao.getAttribute("data-imp");
          var para = botao.getAttribute("data-para");
          Cria.alterar(function (atual) {
            var item = atual.impressoes.find(function (imp) { return imp.id === id; });
            if (item) item.status = para;
          });
          flash(para === "cancelada" ? "Trabalho cancelado." : "Status atualizado.");
        });
      });
      raiz.querySelectorAll("[data-abrir-concluir]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.concluirId = botao.getAttribute("data-abrir-concluir") || "";
          pintar();
        });
      });
      raiz.querySelectorAll("[data-cancelar-concluir]").forEach(function (botao) {
        botao.addEventListener("click", function () {
          vista.concluirId = "";
          pintar();
        });
      });
      raiz.querySelectorAll("[data-form-concluir]").forEach(function (form) {
        form.addEventListener("submit", function (evento) {
          evento.preventDefault();
          var id = form.getAttribute("data-form-concluir");
          var dados = new FormData(form);
          concluirTrabalho(id, dados.get("sucesso") === "1", dados.get("peso"), String(dados.get("motivo") || ""), dados.get("kwh"));
        });
      });

      var copiar = document.getElementById("imp-copiar-falha");
      if (copiar) copiar.addEventListener("click", function () {
        if (!confirm("Copiar a taxa de falha realizada para o Sistema? Orçamentos já fechados não mudam.")) return;
        var taxa = taxaFalha().taxa;
        Cria.alterar(function (atual) { atual.configuracao.taxaFalha = taxa; });
        flash("Taxa de falha copiada para o Sistema.");
      });

      var manut = document.getElementById("imp-manutencao");
      if (manut) manut.addEventListener("submit", function (evento) {
        evento.preventDefault();
        var dados = new FormData(evento.target);
        if (!confirm("Lançar esta manutenção como despesa do mês?")) return;
        var maq = maquinaDe(String(dados.get("maquinaId") || ""));
        Cria.alterar(function (atual) {
          atual.lancamentos.push({
            id: CriaCarrinho.novoId("lan"),
            data: dinheiro.hojeISO(),
            competencia: dinheiro.hojeISO().slice(0, 7),
            descricao: "Manutenção " + dados.get("tipo") + (maq ? " · " + maq.nome : ""),
            categoria: "gastoFixo",
            tipoInsumo: "",
            quantidade: 1,
            valor: Number(dados.get("valor")) || 0,
            link: "",
            kwh: null,
            tarifa: null,
            estornado: false,
            comprovante: "",
            origem: "manutencao"
          });
        });
        flash("Despesa de manutenção lançada no Financeiro.");
      });

      ui.revelar(raiz);
    }
    pintar();
  }

  if (pagina === "admin-patrimonio") patrimonio();
  if (pagina === "admin-impressao") gestaoImpressao();
  if (pagina === "admin-estoque") estoque();
  if (pagina === "admin-impressoes") impressoes();
})();
