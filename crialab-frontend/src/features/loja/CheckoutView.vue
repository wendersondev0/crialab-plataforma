<template>
  <div class="loja">
    <header class="topo">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Checkout</p>
    </header>

    <main class="miolo">
      <h1>Checkout</h1>

      <div v-if="!token" class="vazio">
        <p>Entre na sua conta para finalizar o pedido.</p>
        <RouterLink class="botao" :to="{ path: '/conta/entrar', query: { returnUrl: '/checkout' } }">Entrar</RouterLink>
      </div>

      <div v-else-if="!carrinho.itens.length" class="vazio">
        <p>Adicione itens ao carrinho primeiro.</p>
        <RouterLink class="botao" to="/produtos">Ver produtos</RouterLink>
      </div>

      <template v-else>
        <p v-if="carregando || enviando" class="aviso">{{ enviando ? "Gravando o pedido..." : "Carregando o checkout..." }}</p>
        <Message v-if="erroRede" severity="error" :closable="false">
          {{ erroRede }}
          <Button class="tentar" label="Tentar de novo" @click="falhaConfirmacao ? confirmar() : cotar('cotar')" />
        </Message>

        <div class="grade">
          <section class="cartao">
            <h2>Seu pedido</h2>
            <article v-for="(item, indice) in linhas" :key="`${item.produtoId}-${indice}`" class="linha">
              <div>
                <strong class="titulo">{{ item.titulo }}</strong>
                <span v-if="item.detalhes" class="discreto">{{ item.detalhes }}</span>
                <span v-if="item.precoUnitario !== null" class="discreto">{{ item.quantidade }} × {{ moeda(item.precoUnitario) }}</span>
              </div>
              <strong v-if="item.totalLinha !== null">{{ moeda(item.totalLinha) }}</strong>
            </article>

            <dl v-if="dados">
              <div><dt>Subtotal</dt><dd>{{ moeda(dados.subtotal) }}</dd></div>
              <div><dt>Cupom</dt><dd>− {{ moeda(dados.descontoCupom) }}</dd></div>
              <div><dt>Desconto à vista</dt><dd>− {{ moeda(dados.descontoAVista) }}</dd></div>
              <div><dt>Frete{{ dados.freteNome ? ` (${dados.freteNome})` : "" }}</dt><dd>{{ moeda(dados.freteCobrado) }}</dd></div>
              <div class="total"><dt>Total</dt><dd>{{ moeda(dados.total) }}</dd></div>
            </dl>
            <p v-if="dados && dados.subtotal < dados.freteGratisAPartir" class="discreto">
              Frete grátis a partir de {{ moeda(dados.freteGratisAPartir) }}.
            </p>
          </section>

          <form class="cartao formulario" @submit.prevent>
            <ul v-if="erros.length" class="erros">
              <li v-for="erro in erros" :key="erro">{{ erro }}</li>
            </ul>
            <label>
              Endereço completo
              <textarea v-model="endereco" rows="3" placeholder="Rua, número, bairro, cidade"></textarea>
            </label>
            <label>
              CEP
              <input v-model="cep" type="text" maxlength="9" placeholder="00000-000" />
            </label>
            <label>
              Cupom
              <input v-model="cupom" type="text" placeholder="OFICINA10" />
            </label>
            <label>
              Forma de pagamento
              <select v-model="forma">
                <option value="pix">PIX (à vista)</option>
                <option value="boleto">Boleto (à vista)</option>
                <option value="cartao">Cartão</option>
              </select>
            </label>

            <fieldset v-if="dados && dados.opcoes.length">
              <legend>Frete</legend>
              <label v-for="opcao in dados.opcoes" :key="opcao.id" class="opcao">
                <span>
                  <input v-model="freteId" type="radio" name="frete" :value="opcao.id" />
                  {{ opcao.nome }} · {{ textoPrecoFrete(opcao.preco) }} · {{ textoPrazoFrete(opcao.dias) }}
                </span>
                <span v-if="opcao.descricao" class="discreto">{{ opcao.descricao }}</span>
              </label>
            </fieldset>
            <p v-else-if="dados?.erroFrete" class="erro">{{ dados.erroFrete }}</p>

            <div class="acoes">
              <button class="botao-linha" type="button" :disabled="carregando || enviando" @click="cotar('cotar')">Atualizar frete / cupom</button>
              <button class="botao" type="button" :disabled="carregando || enviando" @click="confirmar">Confirmar pedido</button>
            </div>
          </form>
        </div>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { RouterLink, useRouter } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { moeda } from "./home";
import { useCarrinho } from "./carrinho";
import { lerToken } from "./favoritos";

interface ItemCotacao {
  produtoId: string;
  titulo: string;
  precoUnitario: number;
  quantidade: number;
  detalhes: string | null;
  totalLinha: number;
}

interface OpcaoCotacao {
  id: string;
  nome: string;
  preco: number;
  dias: number;
  descricao: string | null;
}

interface Cotacao {
  subtotal: number;
  descontoCupom: number;
  descontoAVista: number;
  freteCobrado: number;
  freteNome: string;
  freteId: string;
  freteGratisAPartir: number;
  total: number;
  erroFrete: string | null;
  erros: string[];
  itens: ItemCotacao[];
  opcoes: OpcaoCotacao[];
  pedidoId?: string | null;
}

const carrinho = useCarrinho();
const router = useRouter();
const token = ref(lerToken());
const carregando = ref(false);
const enviando = ref(false);
const falhaConfirmacao = ref(false);
const erroRede = ref("");
const erros = ref<string[]>([]);
const dados = ref<Cotacao | null>(null);
const endereco = ref("");
const cep = ref("");
const cupom = ref("");
const forma = ref("pix");
const freteId = ref("pac");

const linhas = computed(() => {
  if (dados.value) return dados.value.itens.map((item) => ({ ...item, precoUnitario: item.precoUnitario as number | null, totalLinha: item.totalLinha as number | null }));
  return carrinho.itens.map((item) => ({
    produtoId: item.produtoId,
    titulo: item.titulo,
    precoUnitario: null as number | null,
    quantidade: item.quantidade,
    detalhes: item.detalhes,
    totalLinha: null as number | null,
  }));
});

function textoPrecoFrete(preco: number) {
  return preco === 0 ? "grátis" : moeda(preco);
}

function textoPrazoFrete(dias: number) {
  return dias === 1 ? "1 dia útil" : `${dias} dias úteis`;
}

async function cotar(acao: "cotar" | "finalizar") {
  erroRede.value = "";
  falhaConfirmacao.value = false;
  erros.value = [];
  token.value = lerToken();
  if (!token.value || !carrinho.itens.length) return;
  carregando.value = true;
  try {
    const resposta = await fetch("/api/loja/checkout/cotacao", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token.value}`,
      },
      body: JSON.stringify({
        itens: carrinho.itens,
        cep: cep.value,
        cupom: cupom.value,
        freteId: freteId.value,
        endereco: endereco.value,
        forma: forma.value,
        acao,
        total: 0,
      }),
    });
    if (resposta.status === 401) {
      token.value = null;
      return;
    }
    if (resposta.status === 404) {
      erroRede.value = "Não encontrei essa peça.";
      return;
    }
    if (!resposta.ok) throw new Error("cotacao");
    const corpo = (await resposta.json()) as Cotacao;
    dados.value = corpo;
    freteId.value = corpo.freteId || freteId.value;
    if (acao === "finalizar") erros.value = corpo.erros ?? [];
  } catch {
    erroRede.value = "Não foi possível carregar o checkout.";
  } finally {
    carregando.value = false;
  }
}

async function confirmar() {
  erroRede.value = "";
  falhaConfirmacao.value = false;
  erros.value = [];
  token.value = lerToken();
  if (!token.value || !carrinho.itens.length) return;
  enviando.value = true;
  try {
    const resposta = await fetch("/api/loja/checkout/pedidos", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token.value}`,
      },
      body: JSON.stringify({
        itens: carrinho.itens,
        cep: cep.value,
        cupom: cupom.value,
        freteId: freteId.value,
        endereco: endereco.value,
        forma: forma.value,
        acao: "finalizar",
        total: 0,
      }),
    });
    if (resposta.status === 401) {
      token.value = null;
      return;
    }
    if (resposta.status === 404) {
      erroRede.value = "Não encontrei essa peça.";
      return;
    }
    if (!resposta.ok) throw new Error("pedido");
    const corpo = (await resposta.json()) as Cotacao;
    dados.value = corpo;
    freteId.value = corpo.freteId || freteId.value;
    erros.value = corpo.erros ?? [];
    if (corpo.pedidoId && erros.value.length === 0) {
      carrinho.limpar();
      await router.push(`/recibo/${corpo.pedidoId}`);
    }
  } catch {
    falhaConfirmacao.value = true;
    erroRede.value = "Não foi possível confirmar o pedido.";
  } finally {
    enviando.value = false;
  }
}

if (token.value && carrinho.itens.length) void cotar("cotar");
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

h1,
h2 {
  margin-top: 0;
}

.grade {
  display: grid;
  gap: 1rem;
}

.vazio,
.cartao {
  padding: 1rem;
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.linha,
dl div,
.acoes {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
  justify-content: space-between;
}

.linha {
  padding: 0.75rem 0;
  border-bottom: 1px solid #eadfce;
}

.titulo,
.discreto,
.opcao {
  display: block;
}

.discreto {
  color: #5c6b7a;
  font-weight: 400;
}

dl {
  display: grid;
  gap: 0.5rem;
  margin: 1rem 0 0;
}

dd {
  margin: 0;
}

.total {
  font-size: 1.25rem;
  font-weight: 800;
}

.formulario,
.opcao {
  display: grid;
  gap: 0.35rem;
}

label {
  display: grid;
  gap: 0.35rem;
  font-weight: 700;
}

textarea,
input,
select {
  min-height: 44px;
  width: 100%;
  padding: 0.4rem 0.6rem;
  border: 2px solid #eadfce;
  border-radius: 12px;
  font: inherit;
  font-weight: 400;
}

textarea {
  min-height: 6rem;
  resize: vertical;
}

fieldset {
  display: grid;
  gap: 0.5rem;
  margin: 0;
  padding: 0;
  border: 0;
}

.opcao input {
  width: auto;
  min-height: auto;
}

.erros,
.erro {
  margin: 0;
  color: #9b2c2c;
  font-weight: 700;
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

.botao:disabled,
.botao-linha:disabled {
  opacity: 0.55;
}

.aviso {
  font-weight: 700;
}

.tentar {
  margin-left: 0.75rem;
}

@media (min-width: 900px) {
  .grade {
    grid-template-columns: 1fr 1fr;
    align-items: start;
  }
}
</style>
