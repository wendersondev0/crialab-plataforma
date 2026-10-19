window.CriaDados = window.CriaDados || {};
window.CriaDados.cliente = { nome: "Cliente exemplo", email: "cliente@example.com", telefone: "(27) 99999-0000", documento: "" };
window.CriaDados.enderecos = [{ id: "end-1", rotulo: "Casa", cep: "29010000", linha: "Rua das Impressoras, 10, Vitória/ES" }];
window.CriaDados.pedidos = [{
  id: "ped-entregue",
  status: "entregue",
  criadoEm: "2026-09-20T15:00",
  itens: [{ id: "item-ent-1", produtoId: "peca-2026-05-24", titulo: "Peça 24/05", tipo: "pronto", fisico: true, arquivo: false, quantidade: 1, precoUnitario: 89 }],
  subtotal: 89,
  descontoCupom: 0,
  descontoAVista: 8.9,
  freteCobrado: 0,
  total: 80.1,
  forma: "pix",
  endereco: "Rua das Impressoras, 10, Vitória/ES",
  cep: "29010000",
  freteNome: "Frete grátis",
  cupom: "",
  repassarFrete: true,
  nota: "pendente",
  os: null
}];
window.CriaDados.orcamentosCliente = [
  { id: "orc-cli-1", produtoId: "peca-2026-05-13", titulo: "Peça 13/05", status: "respondido", detalhes: "Medida 8 cm, cor teal, texto Cria", arquivoNome: "referencia.png", valor: 180, criadoEm: "2026-10-02T11:00" },
  { id: "orc-cli-2", produtoId: "peca-2026-07-08", titulo: "Peça 08/07", status: "aguardando", detalhes: "Texto gravado ainda sem preço", arquivoNome: "", valor: null, criadoEm: "2026-10-10T09:30" }
];
window.CriaDados.avaliacoes = [
  { id: "ava-1", produtoId: "peca-2026-05-24", pedidoId: "ped-entregue", autor: "Cliente exemplo", nota: 5, texto: "A peça chegou bem feita e na cor combinada.", fotoDoProduto: true },
  { id: "ava-2", produtoId: "sessao-2026-03-14", pedidoId: "ped-entregue", autor: "Marina", nota: 5, texto: "Ficou igual à foto e a cor laranja veio bem viva.", fotoDoProduto: true },
  { id: "ava-3", produtoId: "sessao-2026-03-14", pedidoId: "ped-entregue", autor: "Lucas", nota: 4, texto: "Chegou bem embalado. O prazo passou um pouco do combinado.", fotoDoProduto: false },
  { id: "ava-4", produtoId: "sessao-2026-03-14", pedidoId: "ped-entregue", autor: "Helena", nota: 5, texto: "Presente certeiro. O acabamento das asas veio caprichado.", fotoDoProduto: true },
  { id: "ava-5", produtoId: "pers-chaveiro-cachorro", pedidoId: "ped-entregue", autor: "Camila", nota: 5, texto: "O chaveiro saiu parecido com a foto que eu mandei.", fotoDoProduto: true },
  { id: "ava-6", produtoId: "pers-chaveiro-cachorro", pedidoId: "ped-entregue", autor: "Rafael", nota: 4, texto: "Tamanho bom para o molho de chaves. A argola veio resistente.", fotoDoProduto: false }
];
window.CriaDados.perguntas = [
  { id: "per-1", produtoId: "peca-2026-05-24", autor: "Ana", texto: "Essa peça pode ser feita em PETG?", resposta: "", criadoEm: "2026-10-01T14:00" },
  { id: "per-2", produtoId: "peca-2026-10-15", autor: "Bruno", texto: "Dá para lavar esse chaveiro?", resposta: "Sim, com água e sabão neutro. Evite água quente.", criadoEm: "2026-10-05T16:20" },
  { id: "per-3", produtoId: "sessao-2026-03-14", autor: "Marina", texto: "A cor laranja da foto é PLA?", resposta: "Sim, é PLA laranja. Também dá para pedir em outra cor da cartela.", criadoEm: "2026-10-08T10:15" },
  { id: "per-4", produtoId: "sessao-2026-03-14", autor: "Lucas", texto: "Vocês enviam para Vitória?", resposta: "", criadoEm: "2026-10-09T18:40" },
  { id: "per-5", produtoId: "sessao-2026-03-14", autor: "Helena", texto: "Qual o tamanho aproximado?", resposta: "Cerca de 12 cm de altura, como na foto da sessão.", criadoEm: "2026-10-11T09:05" },
  { id: "per-6", produtoId: "pers-chaveiro-cachorro", autor: "Camila", texto: "Posso mandar a foto do cachorro de lado?", resposta: "Pode. Quanto mais nítida a foto, mais fiel fica o chaveiro.", criadoEm: "2026-10-12T13:20" },
  { id: "per-7", produtoId: "pers-chaveiro-cachorro", autor: "Rafael", texto: "A argola já vem inclusa no preço médio?", resposta: "", criadoEm: "2026-10-14T11:00" },
  { id: "per-8", produtoId: "sessao-2026-04-11", autor: "Paula", texto: "Essa peça de utilidade aguenta uso no dia a dia?", resposta: "Sim. É PLA e serve para uso leve em casa.", criadoEm: "2026-10-15T16:10" }
];
