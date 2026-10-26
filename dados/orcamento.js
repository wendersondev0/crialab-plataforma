window.CriaDados = window.CriaDados || {};
window.CriaDados.parametrosCusto = [
  { id: "par-pla", nome: "PLA", unidade: "kg", preco: 85, travado: true },
  { id: "par-petg", nome: "PETG", unidade: "kg", preco: 65, travado: true },
  { id: "par-tpu", nome: "TPU", unidade: "kg", preco: 80, travado: true },
  { id: "par-saco-p", nome: "Saco kraft pequeno", unidade: "un", preco: 0.5 },
  { id: "par-saco-g", nome: "Saco kraft grande", unidade: "un", preco: 1.1286 },
  { id: "par-caixa-p", nome: "Caixa P", unidade: "un", preco: 0.69 },
  { id: "par-caixa-m", nome: "Caixa M", unidade: "un", preco: 0.69 },
  { id: "par-caixa-g", nome: "Caixa G", unidade: "un", preco: 1.33 },
  { id: "par-cartao", nome: "Cartão", unidade: "un", preco: 0.4 },
  { id: "par-ima-8", nome: "Ímã 8×2 mm", unidade: "un", preco: 0.35 },
  { id: "par-ima-10", nome: "Ímã 10×2 mm", unidade: "un", preco: 0.48 },
  { id: "par-argola", nome: "Argola", unidade: "un", preco: 0.3 },
  { id: "par-cola", nome: "Cola", unidade: "un", preco: 0.2 },
  { id: "par-nfc", nome: "Tag NFC", unidade: "un", preco: 0.72 },
  { id: "par-copo", nome: "Copo alumínio 500 ml", unidade: "un", preco: 3.89 },
  { id: "par-led", nome: "Lâmpada LED", unidade: "un", preco: 14.93 },
  { id: "par-verniz", nome: "Verniz", unidade: "un", preco: 15 },
  { id: "par-papel-foto", nome: "Papel fotográfico (cartão presente)", unidade: "un", preco: 0.41 },
  { id: "par-barbante", nome: "Barbante", unidade: "m", preco: 0.36 },
  { id: "par-fita", nome: "Fita de embalagem", unidade: "un", preco: 8 },
  { id: "par-bolha", nome: "Plástico bolha", unidade: "m", preco: 2.5 },
  { id: "par-saco-zip", nome: "Saco zip", unidade: "un", preco: 0.33 }
];
window.CriaDados.margens = [
  { id: "marketplace", nome: "Marketplace", percentual: 0.25, ativo: true },
  { id: "simples", nome: "Simples", percentual: 0.4, ativo: true },
  { id: "medio", nome: "Médio", percentual: 0.5, ativo: true },
  { id: "complexo", nome: "Complexo", percentual: 0.6, ativo: true }
];
window.CriaDados.descontosOrcamento = { recorrente: 0.05, volume: 0.05 };
window.CriaDados.referenciaMaquina = { energiaPorMinuto: 0.005, depreciacao: 0.02, manutencao: 0.01 };
window.CriaDados.niveisPintura = [
  { id: "simples", nome: "Simples", percentual: 0.2, tempo: "cerca de 30 min", cores: "até 2", acabamento: "cor chapada" },
  { id: "medio", nome: "Médio", percentual: 0.4, tempo: "1 h a 2 h", cores: "3 a 5", acabamento: "sombreamento leve, máscara, lixa e primer" },
  { id: "complexo", nome: "Complexo", percentual: 0.6, tempo: "mais de 2 h", cores: "mais de 5", acabamento: "degradê, textura, aerógrafo, montagem" }
];
window.CriaDados.faixasShopee = [
  { de: 0, ate: 79.99, percentual: 0.2, fixo: 4.5 },
  { de: 80, ate: 99.99, percentual: 0.14, fixo: 16 },
  { de: 100, ate: 199.99, percentual: 0.14, fixo: 20 },
  { de: 200, ate: 499.99, percentual: 0.14, fixo: 26 },
  { de: 500, ate: 999999, percentual: 0.14, fixo: 26 }
];
window.CriaDados.faixasMercadoLivre = [];
window.CriaDados.orcamentos = [
  {
    id: "orc-cachorro",
    nome: "Cachorro branco",
    material: "PLA",
    minutos: 367,
    pesoG: 41.66,
    lixoG: 0,
    servico: 0,
    itens: [{ nome: "Saco kraft pequeno", quantidade: 0.5, preco: 0.5 }],
    margemSite: 0.4,
    descontosMarcados: [],
    precoKg: 85,
    precoKgOrigem: "travado",
    energiaPorMinuto: 0.005,
    depreciacao: 0.02,
    manutencao: 0.01,
    taxaFalha: 0.05,
    enviado: true,
    maquinaId: "maq-bambu",
    freteCanal: 0,
    pintura: null,
    realizado: null
  },
  {
    id: "orc-faixa",
    nome: "Caso faixa Shopee",
    material: "PLA",
    minutos: 1310,
    pesoG: 0,
    lixoG: 0,
    servico: 0,
    itens: [],
    margemSite: 0.4,
    descontosMarcados: [],
    precoKg: 85,
    precoKgOrigem: "travado",
    energiaPorMinuto: 0.005,
    depreciacao: 0.02,
    manutencao: 0.01,
    taxaFalha: 0.05,
    enviado: true,
    maquinaId: "maq-bambu",
    freteCanal: 0,
    pintura: null,
    realizado: null
  }
];
