window.CriaDados = window.CriaDados || {};
window.CriaDados.identidade = {
  nome: "Cria Lab DM",
  site: "https://crialabdm.com.br",
  cnpj: "68.914.190/0001-96",
  email: "crialab.dm@gmail.com",
  telefone: "(27) 99653-0954",
  whatsapp: "https://wa.me/5527996530954",
  instagram: "https://www.instagram.com/crialab.dm",
  tiktok: "https://www.tiktok.com/@crialab.dm",
  youtube: "https://www.youtube.com/@crialabdm",
  shopee: "https://shopee.com.br/shop/1648584425",
  mercadoLivre: "https://www.mercadolivre.com.br/",
  endereco: "",
  horario: ""
};
window.CriaDados.categorias = [
  { id: "decoracao", nome: "Decoração" },
  { id: "chaveiros", nome: "Chaveiros" },
  { id: "utilidades", nome: "Utilidades" },
  { id: "moldes", nome: "Moldes" },
  { id: "pecas-sob-encomenda", nome: "Peças sob encomenda" },
  { id: "pintura", nome: "Pintura" }
];
window.CriaDados.configuracao = {
  freteGratisAPartir: 200,
  descontoAVista: 0.1,
  parcelas: 3,
  repassarFrete: true,
  taxaFalha: 0.05,
  freteCanal: 0,
  cupons: [{ codigo: "OFICINA10", percentual: 0.1, ativo: true }],
  faixasCep: [
    { de: 1000, ate: 19999, pac: 28.9, pacDias: 8, sedex: 46.5, sedexDias: 3 },
    { de: 20000, ate: 28999, pac: 24.5, pacDias: 6, sedex: 39.9, sedexDias: 2 },
    { de: 29000, ate: 29999, pac: 18.9, pacDias: 5, sedex: 32.4, sedexDias: 2 },
    { de: 30000, ate: 39999, pac: 22.4, pacDias: 7, sedex: 36.8, sedexDias: 3 },
    { de: 40000, ate: 69999, pac: 31.2, pacDias: 9, sedex: 49.7, sedexDias: 4 },
    { de: 70000, ate: 79999, pac: 34.8, pacDias: 8, sedex: 55.3, sedexDias: 3 },
    { de: 80000, ate: 89999, pac: 33.1, pacDias: 8, sedex: 52.6, sedexDias: 3 },
    { de: 90000, ate: 99999, pac: 36.4, pacDias: 9, sedex: 58.9, sedexDias: 4 }
  ],
  tiposInsumo: ["PLA", "PETG", "TPU", "NFC", "PAPEL_FOTOGRAFICO", "BARBANTE", "COPO", "VERNIZ", "LAMPADA_LED", "Argola", "IMA", "CAIXA P", "CAIXA M", "CAIXA G", "SACOLA KRAFT P", "SACOLA KRAFT G"]
};
window.CriaDados.depoimentos = [
  { id: "dep-1", autor: "Mileni", texto: "Os moldes chegaram no tamanho que a gente combinou.", podeAparecer: true },
  { id: "dep-2", autor: "Oficina vizinha", texto: "Texto guardado, ainda sem publicação.", podeAparecer: false }
];
window.CriaDados.parceiros = [
  { id: "par-1", nome: "Ateliê da rua", podeAparecer: true },
  { id: "par-2", nome: "Parceiro em conversa", podeAparecer: false }
];
