(function (root) {
  function real(valor, casas) {
    var n = Number(valor) || 0;
    var max = casas == null ? 2 : casas;
    var min = Math.min(2, max);
    return n.toLocaleString("pt-BR", { style: "currency", currency: "BRL", minimumFractionDigits: min, maximumFractionDigits: max });
  }

  function realKg(valor) {
    var n = Number(valor) || 0;
    var truncado = Math.trunc(n * 1e8 + 1e-6) / 1e8;
    return truncado.toLocaleString("pt-BR", { style: "currency", currency: "BRL", minimumFractionDigits: 2, maximumFractionDigits: 8 });
  }

  function numero(valor, casas) {
    return (Number(valor) || 0).toLocaleString("pt-BR", { minimumFractionDigits: casas || 0, maximumFractionDigits: casas || 0 });
  }

  function hojeISO() {
    return new Intl.DateTimeFormat("en-CA", { timeZone: "America/Sao_Paulo", year: "numeric", month: "2-digit", day: "2-digit" }).format(new Date());
  }

  function agoraSP() {
    var partes = new Intl.DateTimeFormat("en-CA", {
      timeZone: "America/Sao_Paulo",
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
      hourCycle: "h23"
    }).formatToParts(new Date());
    var mapa = {};
    partes.forEach(function (p) { if (p.type !== "literal") mapa[p.type] = p.value; });
    return mapa.year + "-" + mapa.month + "-" + mapa.day + "T" + mapa.hour + ":" + mapa.minute;
  }

  function dataBR(iso) {
    if (!iso) return "";
    var dia = String(iso).slice(0, 10);
    var p = dia.split("-");
    if (p.length !== 3) return dia;
    return p[2] + "/" + p[1] + "/" + p[0];
  }

  function competenciaBR(comp) {
    var nomes = ["janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"];
    var p = String(comp || "").split("-");
    var indice = Number(p[1]) - 1;
    if (!nomes[indice]) return comp || "";
    return nomes[indice] + " de " + p[0];
  }

  function diasNoMes(comp) {
    var p = String(comp).split("-");
    return new Date(Number(p[0]), Number(p[1]), 0).getDate();
  }

  root.CriaDinheiro = { real: real, realKg: realKg, numero: numero, hojeISO: hojeISO, agoraSP: agoraSP, dataBR: dataBR, competenciaBR: competenciaBR, diasNoMes: diasNoMes };
})(window);
