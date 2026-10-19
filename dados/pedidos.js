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
  { id: "ava-1", produtoId: "peca-2026-05-24", pedidoId: "ped-entregue", autor: "Cliente exemplo", nota: 5, texto: "A peça chegou bem feita e na cor combinada.", fotoDoProduto: true }
];
window.CriaDados.perguntas = [
  { id: "per-1", produtoId: "peca-2026-05-24", autor: "Ana", texto: "Essa peça pode ser feita em PETG?", resposta: "", criadoEm: "2026-10-01T14:00" },
  { id: "per-2", produtoId: "peca-2026-10-15", autor: "Bruno", texto: "O arquivo inclui licença comercial?", resposta: "Esta licença é pessoal.", criadoEm: "2026-10-05T16:20" }
];
