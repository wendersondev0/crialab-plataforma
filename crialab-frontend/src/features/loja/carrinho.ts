import { ref } from "vue";
import { defineStore } from "pinia";

export interface ItemCarrinho {
  produtoId: string;
  titulo: string;
  precoUnitario: number;
  quantidade: number;
  cor: string | null;
  tamanho: string | null;
  detalhes: string | null;
}

const CHAVE = "crialab-carrinho";

function ler(): ItemCarrinho[] {
  try {
    const bruto = localStorage.getItem(CHAVE);
    if (!bruto) return [];
    const lista = JSON.parse(bruto) as unknown;
    return Array.isArray(lista) ? (lista as ItemCarrinho[]) : [];
  } catch {
    return [];
  }
}

export const useCarrinho = defineStore("carrinho", () => {
  const itens = ref<ItemCarrinho[]>(ler());

  function salvar() {
    localStorage.setItem(CHAVE, JSON.stringify(itens.value));
  }

  function adicionar(item: ItemCarrinho) {
    const limpo: ItemCarrinho = {
      produtoId: item.produtoId,
      titulo: item.titulo,
      precoUnitario: item.precoUnitario,
      quantidade: item.quantidade,
      cor: item.cor,
      tamanho: item.tamanho,
      detalhes: item.detalhes,
    };
    const existente = itens.value.find(
      (x) => x.produtoId === limpo.produtoId && x.cor === limpo.cor && x.tamanho === limpo.tamanho && x.detalhes === limpo.detalhes,
    );
    if (existente) existente.quantidade += limpo.quantidade;
    else itens.value.push(limpo);
    salvar();
  }

  return { itens, adicionar };
});
