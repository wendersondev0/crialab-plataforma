window.CriaDados = window.CriaDados || {};
window.CriaDados.maquinas = [
  { id: "maq-bambu", nome: "Bambu Lab A1 com AMS Lite", watts: 150, depreciacaoPorMinuto: 0.02, manutencaoPorMinuto: 0.01, energiaPorMinuto: 0.005, controlaHora: true, compraCompetencia: "2026-03", compraValor: 4437.66 },
  { id: "maq-kobra", nome: "Anycubic Kobra S1", watts: 200, depreciacaoPorMinuto: 0.02, manutencaoPorMinuto: 0.01, energiaPorMinuto: 0.005, controlaHora: true, compraCompetencia: "2026-07", compraValor: 5359.93 },
  { id: "maq-secador", nome: "Secador de filamento", watts: 85, depreciacaoPorMinuto: 0.02, manutencaoPorMinuto: 0.01, energiaPorMinuto: 0.005, controlaHora: true, compraCompetencia: "", compraValor: 0 },
  { id: "maq-epson", nome: "Epson EcoTank L3250", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "2026-08", compraValor: 1071 },
  { id: "eq-secador-creality", nome: "Secador Creality", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "2026-03", compraValor: 396.36 },
  { id: "eq-macarico", nome: "Maçarico", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "", compraValor: 0 },
  { id: "eq-retifica", nome: "Retífica", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "", compraValor: 0 },
  { id: "eq-pinca", nome: "Pinça", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "", compraValor: 0 },
  { id: "eq-alicate", nome: "Alicate", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "", compraValor: 0 },
  { id: "eq-tomada", nome: "Tomada inteligente", watts: 0, depreciacaoPorMinuto: 0, manutencaoPorMinuto: 0, energiaPorMinuto: 0, controlaHora: false, compraCompetencia: "", compraValor: 0 }
];
window.CriaDados.estoqueInsumos = [
  { id: "ins-pla", nome: "PLA", unidade: "kg", saldo: 1.2, minimo: 2 },
  { id: "ins-petg", nome: "PETG", unidade: "kg", saldo: 4, minimo: 1 },
  { id: "ins-tpu", nome: "TPU", unidade: "kg", saldo: 0.8, minimo: 0.5 }
];
window.CriaDados.impressoes = [
  { id: "imp-1", maquinaId: "maq-bambu", produtoId: "peca-2026-05-24", titulo: "Peça 24/05", pesoPrevisto: 40, pesoReal: 0, sucesso: null, motivo: "", kwh: 0.4, inicio: "2026-10-19T09:00", fim: "2026-10-19T12:00", status: "imprimindo" },
  { id: "imp-2", maquinaId: "maq-kobra", produtoId: "peca-2026-10-15", titulo: "Peça 15/10", pesoPrevisto: 18, pesoReal: 0, sucesso: null, motivo: "", kwh: 0.3, inicio: "2026-10-19T09:00", fim: "2026-10-19T11:00", status: "imprimindo" },
  { id: "imp-3", maquinaId: "maq-bambu", produtoId: "peca-2026-09-26", titulo: "Peça 26/09", pesoPrevisto: 80, pesoReal: 0, sucesso: null, motivo: "", kwh: 0.6, inicio: "2026-10-19T10:00", fim: "2026-10-19T13:00", status: "na fila" },
  { id: "imp-4", maquinaId: "maq-bambu", produtoId: "peca-2026-05-24", titulo: "Peça 24/05", pesoPrevisto: 40, pesoReal: 42, sucesso: true, motivo: "", kwh: 0.35, inicio: "2026-10-10T08:00", fim: "2026-10-10T11:00", status: "concluida" },
  { id: "imp-5", maquinaId: "maq-kobra", produtoId: "peca-2026-10-15", titulo: "Peça 15/10", pesoPrevisto: 18, pesoReal: 20, sucesso: false, motivo: "Descolou da mesa", kwh: 0.12, inicio: "2026-10-11T14:00", fim: "2026-10-11T15:00", status: "concluida" }
];
window.CriaDados.contas = [
  { id: "conta-1", tipo: "pagar", descricao: "Conta de energia de outubro", valor: 80, vencimento: "2026-10-25", baixada: false, comprovante: "" },
  { id: "conta-2", tipo: "receber", descricao: "Molde combinado com a cliente", valor: 150, vencimento: "2026-10-22", baixada: false, comprovante: "" }
];
window.CriaDados.precoKwhAtual = 1.1193;
window.CriaDados.metas = {};
window.CriaDados.fechamentosEnergia = [];
