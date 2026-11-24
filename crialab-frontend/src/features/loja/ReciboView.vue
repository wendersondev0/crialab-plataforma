<template>
  <div class="loja">
    <header class="topo no-print">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Recibo</p>
    </header>

    <main class="miolo">
      <div v-if="!token" class="vazio">
        <p>Entre na sua conta para ver o recibo.</p>
        <RouterLink class="botao" :to="{ path: '/conta/entrar', query: { returnUrl: `/recibo/${id}` } }">Entrar</RouterLink>
      </div>

      <template v-else>
        <p v-if="carregando" class="aviso">Carregando o recibo...</p>
        <Message v-if="erroRede" severity="error" :closable="false">
          {{ erroRede }}
          <Button class="tentar" label="Tentar de novo" @click="carregar" />
        </Message>
        <div v-else-if="naoEncontrado" class="vazio">
          <p>Recibo não encontrado.</p>
        </div>
        <article v-else-if="recibo" class="folha">
          <img :src="logo" alt="Cria Lab DM" />
          <h1>Recibo</h1>
          <p>{{ recibo.nomeLoja }} · CNPJ {{ recibo.cnpj }}</p>
          <p>{{ recibo.id }} · {{ dataHora(recibo.criadoEm) }}</p>
          <p class="discreto">{{ recibo.endereco }} · CEP {{ recibo.cep }} · {{ recibo.forma }} · frete {{ recibo.freteNome }}</p>
          <ul>
            <li v-for="(item, indice) in recibo.itens" :key="`${item.titulo}-${indice}`">
              <span>
                {{ item.titulo }}
                <span v-if="variacao(item)" class="discreto">({{ variacao(item) }})</span>
              </span>
              <span>{{ item.quantidade }}</span>
              <strong>{{ moeda(item.totalLinha) }}</strong>
            </li>
          </ul>
          <p>Desconto cupom {{ moeda(recibo.descontoCupom) }}</p>
          <p>Desconto à vista {{ moeda(recibo.descontoAVista) }}</p>
          <p>Frete {{ moeda(recibo.freteCobrado) }}</p>
          <p class="total">Total {{ moeda(recibo.total) }}</p>
          <p>Nota fiscal: pendente de transmissão</p>
          <div class="acoes no-print">
            <button class="botao" type="button" @click="imprimir">Imprimir</button>
            <RouterLink class="botao-linha" to="/conta/pedidos">Meus pedidos</RouterLink>
            <RouterLink class="botao-linha" to="/conta">Minha conta</RouterLink>
          </div>
        </article>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { RouterLink, useRoute } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { moeda } from "./home";
import { lerToken } from "./favoritos";

interface ItemRecibo {
  titulo: string;
  corEscolhida: string | null;
  tamanhoEscolhido: string | null;
  quantidade: number;
  precoUnitario: number;
  totalLinha: number;
}

interface Recibo {
  id: string;
  criadoEm: string;
  endereco: string;
  cep: string;
  forma: string;
  freteNome: string;
  descontoCupom: number;
  descontoAVista: number;
  freteCobrado: number;
  total: number;
  nomeLoja: string;
  cnpj: string;
  itens: ItemRecibo[];
}

const logo = "/marca/logo-preta.svg";
const route = useRoute();
const id = computed(() => String(route.params.id ?? ""));
const token = ref(lerToken());
const carregando = ref(false);
const erroRede = ref("");
const naoEncontrado = ref(false);
const recibo = ref<Recibo | null>(null);

function variacao(item: ItemRecibo) {
  return [item.corEscolhida, item.tamanhoEscolhido].filter((parte) => parte && parte.trim()).join(" ");
}

function dataHora(iso: string) {
  const data = new Date(iso);
  if (Number.isNaN(data.getTime())) return iso;
  return data
    .toLocaleString("pt-BR", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    })
    .replace(",", "");
}

function imprimir() {
  window.print();
}

async function carregar() {
  erroRede.value = "";
  naoEncontrado.value = false;
  recibo.value = null;
  token.value = lerToken();
  if (!token.value || !id.value) return;
  carregando.value = true;
  try {
    const resposta = await fetch(`/api/loja/pedidos/${encodeURIComponent(id.value)}`, {
      headers: { Authorization: `Bearer ${token.value}` },
    });
    if (resposta.status === 401) {
      token.value = null;
      return;
    }
    if (resposta.status === 404) {
      naoEncontrado.value = true;
      return;
    }
    if (!resposta.ok) throw new Error("recibo");
    recibo.value = (await resposta.json()) as Recibo;
  } catch {
    erroRede.value = "Não foi possível carregar o recibo.";
  } finally {
    carregando.value = false;
  }
}

watch(id, () => void carregar(), { immediate: true });
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
  width: min(720px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 1.25rem 0 2.5rem;
}

.vazio,
.folha {
  padding: 1rem;
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

img {
  height: 64px;
  max-width: 100%;
}

h1 {
  margin: 0.75rem 0;
}

.discreto {
  color: #5c6b7a;
}

ul {
  display: grid;
  gap: 0.75rem;
  margin: 1rem 0;
  padding: 0;
  list-style: none;
}

li,
.acoes {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
  justify-content: space-between;
}

li {
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #eadfce;
}

.total {
  font-size: 1.25rem;
  font-weight: 800;
}

.botao,
.botao-linha {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 44px;
  padding: 0.4rem 0.9rem;
  border-radius: 999px;
  font: inherit;
  font-weight: 800;
  text-decoration: none;
  cursor: pointer;
}

.botao {
  border: 0;
  background: #e8772a;
  color: #fff;
}

.botao-linha {
  border: 2px solid #eadfce;
  background: #fff;
  color: #1b2430;
}

.aviso {
  font-weight: 700;
}

.tentar {
  margin-left: 0.75rem;
}

@media print {
  .no-print {
    display: none;
  }

  .loja,
  .folha {
    background: #fff;
    border: 0;
  }
}
</style>
