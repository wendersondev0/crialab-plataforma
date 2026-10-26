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
window.CriaDados.modalidades = [
  {
    id: "pronta-entrega",
    nome: "Pronta Entrega",
    resumo: "Peças já impressas e disponíveis para envio ou retirada.",
    explicacao: "Escolha, pague e receba sem esperar a impressão. O estoque é o que está na mesa da oficina agora."
  },
  {
    id: "sob-demanda",
    nome: "Sob Demanda",
    resumo: "O modelo já está definido; a peça é impressa depois do pedido.",
    explicacao: "O desenho é o mesmo da vitrine. Depois da compra, a oficina imprime a sua unidade no prazo indicado."
  },
  {
    id: "personalizado",
    nome: "Personalizados",
    resumo: "Feitos sob encomenda com as alterações que você pedir.",
    explicacao: "Você envia referência e detalhes. O preço do card é uma média; o valor final sai no orçamento antes de produzir."
  }
];
window.CriaDados.menusAjuda = {
  inicio: "A home mostra o carrossel, os atalhos das três modalidades e a grade de peças. Clique no card para abrir a ficha completa.",
  busca: "Filtre por texto, categoria temática, preço e ordenação. O link da busca pode ser compartilhado com os mesmos filtros.",
  carrinho: "Revise quantidades e remova itens. O cupom e o frete entram no fechamento do pedido.",
  checkout: "Informe CEP, escolha PAC ou SEDEX, use cupom se tiver e confirme o pagamento simulado.",
  conta: "Pedidos, orçamentos, favoritos e dados do cliente de exemplo. Sem senha nesta etapa da loja estática.",
  contato: "Envie dúvida, sugestão, elogio, reclamação ou orçamento. A mensagem aparece na lista do admin.",
  sobre: "Somos uma oficina de impressão 3D: peças prontas, sob demanda e personalizadas do seu jeito.",
  produto: "Na ficha você escolhe cor e tamanho, calcula frete, abre Shopee ou Mercado Livre e compra ou pede orçamento."
};
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
  tiposInsumo: [
    "PLA", "PETG", "TPU",
    "NFC", "COPO", "VERNIZ", "LAMPADA_LED", "Argola", "IMA",
    "CAIXA P", "CAIXA M", "CAIXA G", "SACOLA KRAFT P", "SACOLA KRAFT G",
    "PAPEL_FOTOGRAFICO", "BARBANTE", "CARTAO", "FITA", "PLASTICO_BOLHA", "SACO_ZIP",
    "COLA", "SILICA", "SOQUETE_LED", "IMA_10"
  ]
};
window.CriaDados.depoimentos = [
  { id: "dep-1", autor: "Mileni", texto: "Os moldes chegaram no tamanho que a gente combinou.", podeAparecer: true },
  { id: "dep-2", autor: "Oficina vizinha", texto: "Texto guardado, ainda sem publicação.", podeAparecer: false }
];
window.CriaDados.parceiros = [
  { id: "par-1", nome: "Mercearia Vila Verde", logo: "marca/parceiro-vila-verde.png", podeAparecer: true },
  { id: "par-2", nome: "GoFit", logo: "marca/parceiro-gofit.png", podeAparecer: true }
];
