(function () {
  function revelar(escopo) {
    var raiz = escopo || document;
    var itens = Array.prototype.slice.call(raiz.querySelectorAll(".revela"));
    if (!itens.length) return;
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches || !("IntersectionObserver" in window)) {
      itens.forEach(function (el) { el.classList.add("visivel"); });
      return;
    }
    var obs = new IntersectionObserver(function (entradas) {
      entradas.forEach(function (entrada) {
        if (!entrada.isIntersecting) return;
        entrada.target.classList.add("visivel");
        obs.unobserve(entrada.target);
      });
    }, { threshold: 0.08, rootMargin: "0px 0px -24px 0px" });
    itens.forEach(function (el, i) {
      el.style.setProperty("--atraso", (Math.min(i % 8, 7) * 55) + "ms");
      obs.observe(el);
    });
    window.setTimeout(function () {
      itens.forEach(function (el) {
        if (!el.classList.contains("visivel")) el.classList.add("visivel");
      });
    }, 1200);
  }

  function ligarCarrossel() {
    var caixa = document.getElementById("carrossel");
    if (!caixa) return;
    var trilho = caixa.querySelector(".carrossel-trilho");
    var pontos = caixa.querySelectorAll("[data-slide]");
    if (!trilho || !pontos.length) return;
    var indice = 0;
    var total = pontos.length;
    var timer = 0;
    function ir(n) {
      indice = (n + total) % total;
      trilho.style.transform = "translateX(-" + indice * 100 + "%)";
      pontos.forEach(function (ponto, i) {
        ponto.classList.toggle("ativo", i === indice);
      });
    }
    function andar() {
      window.clearInterval(timer);
      if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
      timer = window.setInterval(function () {
        ir(indice + 1);
      }, 4000);
    }
    caixa.addEventListener("mouseenter", function () {
      window.clearInterval(timer);
    });
    caixa.addEventListener("mouseleave", andar);
    caixa.addEventListener("click", function (evento) {
      var ponto = evento.target.closest("[data-slide]");
      var passo = evento.target.closest("[data-passo]");
      if (ponto) ir(Number(ponto.getAttribute("data-slide")));
      if (passo) ir(indice + Number(passo.getAttribute("data-passo")));
    });
    andar();
  }

  function ligarMenus() {
    var topo = document.getElementById("topo");
    if (!topo) return;
    topo.querySelectorAll(".menu-suspenso").forEach(function (menu) {
      var botao = menu.querySelector(".atalho");
      if (!botao) return;
      botao.addEventListener("click", function (evento) {
        evento.preventDefault();
        var aberto = menu.classList.toggle("aberto");
        botao.setAttribute("aria-expanded", aberto ? "true" : "false");
        topo.querySelectorAll(".menu-suspenso.aberto").forEach(function (outro) {
          if (outro === menu) return;
          outro.classList.remove("aberto");
          var gatilho = outro.querySelector(".atalho");
          if (gatilho) gatilho.setAttribute("aria-expanded", "false");
        });
      });
    });

    var gaveta = document.getElementById("menu-gaveta");
    var abrir = document.getElementById("abrir-menu");
    var fechar = document.getElementById("fechar-menu");
    var fundo = gaveta ? gaveta.querySelector(".menu-gaveta-fundo") : null;
    function setMenu(aberto) {
      if (!gaveta || !abrir) return;
      gaveta.classList.toggle("aberta", aberto);
      document.body.classList.toggle("menu-aberto", aberto);
      abrir.setAttribute("aria-expanded", aberto ? "true" : "false");
    }
    if (abrir) abrir.addEventListener("click", function () { setMenu(true); });
    if (fechar) fechar.addEventListener("click", function () { setMenu(false); });
    if (fundo) fundo.addEventListener("click", function () { setMenu(false); });
    document.addEventListener("keydown", function (evento) {
      if (evento.key === "Escape") setMenu(false);
    });
    document.addEventListener("click", function (evento) {
      if (evento.target.closest(".menu-suspenso")) return;
      topo.querySelectorAll(".menu-suspenso.aberto").forEach(function (menu) {
        menu.classList.remove("aberto");
        var gatilho = menu.querySelector(".atalho");
        if (gatilho) gatilho.setAttribute("aria-expanded", "false");
      });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    revelar(document.getElementById("conteudo") || document);
    ligarCarrossel();
    ligarMenus();
  });
})();
