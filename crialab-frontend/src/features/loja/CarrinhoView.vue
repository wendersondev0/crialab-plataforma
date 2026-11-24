<template>
  <div class="loja">
    <header class="topo">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Carrinho</p>
    </header>

    <main class="miolo">
      <h1>Carrinho</h1>

      <div v-if="!carrinho.itens.length" class="vazio">
        <p>Seu carrinho está vazio.</p>
        <RouterLink class="botao" to="/produtos">Ver produtos</RouterLink>
      </div>

      <template v-else>
        <p v-if="carregando" class="aviso">Atualizando o carrinho...</p>
        <Message v-if="erro" severity="error" :closable="false">
          {{ erro }}
          <Button class="tentar" label="Tentar de novo" @click="atualizar" />
        </Message>

        <div class="grade">
          <article v-for="(item, indice) in linhas" :key="chave(item, indice)" class="cartao">
            <div>
              <strong class="titulo">{{ item.titulo }}</strong>
              <span v-if="item.detalhes" class="discreto">{{ item.detalhes }}</span>
              <span class="discreto">{{ item.quantidade }} × {{ moeda(item.precoUnitario) }}</span>
            </div>
            <div class="lado">
              <strong>{{ moeda(item.totalLinha) }}</strong>
              <button class="botao-linha" type="button" @click="remover(item)">Remover</button>
            </div>
          </article>
        </div>

        <div class="cartao fechamento">
          <p class="subtotal">Subtotal: {{ moeda(subtotal) }}</p>
          <div class="acoes">
            <RouterLink class="botao-linha" to="/produtos">Continuar comprando</RouterLink>
            <RouterLink class="botao" to="/checkout">Finalizar pedido</RouterLink>
          </div>
        </div>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { RouterLink } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { moeda } from "./home";
import { useCarrinho, type ItemCarrinho } from "./carrinho";

interface LinhaVista extends ItemCarrinho {
  totalLinha: number;
}

const carrinho = useCarrinho();
const erro = ref("");
const carregando = ref(false);
const subtotalServidor = ref<number | null>(null);

const linhas = computed<LinhaVista[]>(() =>
  carrinho.itens.map((item) => ({
    ...item,
    totalLinha: item.precoUnitario * item.quantidade,
  })),
);

const subtotal = computed(() => subtotalServidor.value ?? carrinho.subtotal());

function chave(item: LinhaVista, indice: number) {
  return `${item.produtoId}|${item.cor ?? ""}|${item.tamanho ?? ""}|${item.detalhes ?? ""}|${indice}`;
}

async function atualizar() {
  erro.value = "";
  subtotalServidor.value = null;
  if (!carrinho.itens.length) return;
  carregando.value = true;
  try {
    const resposta = await fetch("/api/loja/carrinho/resumo", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ itens: carrinho.itens, total: 0 }),
    });
    if (!resposta.ok) throw new Error("resumo");
    const corpo = (await resposta.json()) as { subtotal: number };
    subtotalServidor.value = corpo.subtotal;
  } catch {
    erro.value = "Não foi possível atualizar o carrinho.";
  } finally {
    carregando.value = false;
  }
}

function remover(item: LinhaVista) {
  carrinho.remover(item.produtoId, item.cor, item.tamanho);
  void atualizar();
}

void atualizar();
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
  margin-top: 0;
}

.grade {
  display: grid;
  gap: 0.75rem;
}

.vazio,
.cartao {
  padding: 1rem;
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.cartao,
.lado,
.acoes,
.fechamento {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
  justify-content: space-between;
}

.titulo,
.discreto {
  display: block;
}

.discreto {
  color: #5c6b7a;
}

.subtotal {
  margin: 0;
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
</style>
