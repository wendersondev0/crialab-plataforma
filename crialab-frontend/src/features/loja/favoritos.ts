import { ref } from "vue";
import { defineStore } from "pinia";

const CHAVE = "crialab-favoritos";
const CHAVE_TOKEN = "crialab-access-token";

function ler(): string[] {
  try {
    const bruto = localStorage.getItem(CHAVE);
    if (!bruto) return [];
    const lista = JSON.parse(bruto) as unknown;
    return Array.isArray(lista) ? lista.filter((x): x is string => typeof x === "string") : [];
  } catch {
    return [];
  }
}

function salvar(lista: string[]) {
  localStorage.setItem(CHAVE, JSON.stringify(lista));
}

export function lerToken(): string | null {
  const token = localStorage.getItem(CHAVE_TOKEN);
  return token && token.trim() ? token : null;
}

export const useFavoritos = defineStore("favoritos", () => {
  const ids = ref<string[]>(ler());

  function tem(produtoId: string) {
    return ids.value.some((x) => x.toLowerCase() === produtoId.toLowerCase());
  }

  function alternar(produtoId: string) {
    const lista = ler();
    const indice = lista.findIndex((x) => x.toLowerCase() === produtoId.toLowerCase());
    const agora = indice < 0;
    if (agora) lista.push(produtoId);
    else lista.splice(indice, 1);
    ids.value = lista;
    salvar(lista);
    return agora;
  }

  return { ids, tem, alternar };
});
