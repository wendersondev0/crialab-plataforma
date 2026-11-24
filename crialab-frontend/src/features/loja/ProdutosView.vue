<template>
  <div class="loja">
    <header class="topo">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Produtos</p>
    </header>

    <main class="miolo">
      <p v-if="carregando" class="aviso">Carregando a lista...</p>
      <Message v-else-if="erro" severity="error" :closable="false">
        {{ erro }}
        <Button class="tentar" label="Tentar de novo" @click="carregar" />
      </Message>

      <template v-else-if="dados">
        <h1 :title="dados.resumo || undefined">{{ dados.titulo }}</h1>
        <p class="intro">{{ dados.texto }}</p>
        <p v-if="!dados.produtos.length" class="vazio">Nada publicado nesta lista.</p>
        <div v-else class="grade">
          <RouterLink v-for="p in dados.produtos" :key="p.id" class="peca" :to="`/produto/${p.id}`">
            <img :src="caminhoMidia(p.foto)" :alt="p.titulo" />
            <p class="tag">{{ p.modalidadeNome }}</p>
            <p class="tema">{{ p.categoriaNome }}</p>
            <h3>{{ p.titulo }}</h3>
            <p class="preco">{{ moeda(p.preco) }}</p>
            <p v-if="p.modalidadeId === 'personalizado'" class="media">Preço médio · fecha no orçamento</p>
            <p class="prazo">{{ textoPrazo({ ...p, maisVendido: false }) }}</p>
          </RouterLink>
        </div>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { RouterLink, useRoute } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { obterJson } from "../../shared/http";
import { caminhoMidia, moeda, textoPrazo } from "./home";

interface ProdutoLista {
  id: string;
  titulo: string;
  preco: number;
  modalidadeId: string;
  modalidadeNome: string;
  categoriaNome: string;
  foto: string | null;
  prazoTipo: string;
  freteGratis: boolean;
  prazoDias: number;
  prazoInicio: string | null;
  prazoFim: string | null;
}

interface ListaDados {
  titulo: string;
  resumo: string | null;
  texto: string;
  freteGratisAPartir: number;
  produtos: ProdutoLista[];
}

const route = useRoute();
const dados = ref<ListaDados | null>(null);
const carregando = ref(true);
const erro = ref("");

async function carregar() {
  carregando.value = true;
  erro.value = "";
  try {
    const filtro = route.query.ModalidadeFiltro;
    const params = new URLSearchParams();
    if (typeof filtro === "string" && filtro.trim()) params.set("modalidadeFiltro", filtro);
    dados.value = await obterJson<ListaDados>(`/api/loja/produtos?${params.toString()}`);
  } catch {
    dados.value = null;
    erro.value = "Não foi possível carregar a lista.";
  } finally {
    carregando.value = false;
  }
}

watch(() => route.query.ModalidadeFiltro, () => void carregar());
onMounted(carregar);
</script>

<style scoped>
.loja {
  min-height: 100vh;
  color: #1b2430;
  background: #f7f4ee;
  font-family: "Segoe UI", sans-serif;
}

.topo {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem 1rem;
  align-items: center;
  justify-content: space-between;
  padding: 0.85rem 1rem;
  background: #e8772a;
  color: #fff;
}

.marca,
.atalho {
  margin: 0;
  color: #fff;
  font-weight: 800;
  text-decoration: none;
}

.miolo {
  width: min(1120px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 1.25rem 0 2.5rem;
}

h1 {
  margin: 0 0 0.35rem;
}

.intro {
  max-width: 42rem;
}

.vazio,
.peca {
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.vazio {
  padding: 1rem;
}

.grade {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
  margin-top: 1rem;
}

.peca {
  padding: 0.9rem;
  color: inherit;
  text-decoration: none;
}

.peca img {
  width: 100%;
  max-width: 160px;
  height: 140px;
  object-fit: contain;
}

.preco {
  font-size: 1.15rem;
  font-weight: 800;
}

.tentar {
  margin-left: 0.75rem;
}

@media (min-width: 700px) {
  .grade {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1024px) {
  .grade {
    grid-template-columns: repeat(4, 1fr);
  }
}
</style>
