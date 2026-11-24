<template>
  <div class="loja">
    <header class="topo">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Busca</p>
    </header>

    <main class="miolo">
      <p v-if="carregando" class="aviso">Carregando a busca...</p>
      <Message v-else-if="erro" severity="error" :closable="false">
        {{ erro }}
        <Button class="tentar" label="Tentar de novo" @click="carregar" />
      </Message>

      <template v-else-if="dados">
        <header class="cab">
          <div>
            <h1>Busca</h1>
            <p>{{ dados.ajuda }}</p>
          </div>
          <p class="contagem">{{ dados.resumo }}</p>
        </header>

        <form class="painel" autocomplete="off" @submit.prevent="aplicar">
          <p v-if="dados.mensagem" class="aviso-filtro" role="status">{{ dados.mensagem }}</p>
          <label class="rotulo texto">
            Texto
            <span class="caixa">
              <input
                v-model="termo"
                type="search"
                placeholder="Nome, material ou categoria"
                autocomplete="off"
                aria-autocomplete="list"
                :aria-expanded="sugestoes.length > 0"
                aria-controls="sugestoes"
                @input="agendarSugestoes"
                @keydown.esc="fecharSugestoes"
              />
              <div v-if="sugestoes.length" id="sugestoes" class="sugestoes">
                <RouterLink v-for="s in sugestoes" :key="s.id" :to="`/produto/${s.id}`" @click="fecharSugestoes">
                  <img v-if="s.foto" :src="caminhoMidia(s.foto)" alt="" />
                  <span>
                    <strong>{{ s.titulo }}</strong>
                    <em>{{ moeda(s.preco) }}<template v-if="s.categoria"> · {{ s.categoria }}</template></em>
                  </span>
                </RouterLink>
              </div>
            </span>
          </label>

          <div class="grade-form">
            <label class="rotulo">Categoria
              <select v-model="categoria">
                <option value="">Todas</option>
                <option v-for="c in dados.categorias" :key="c.id" :value="c.id">{{ c.nome }}</option>
              </select>
            </label>
            <label class="rotulo">Preço mínimo
              <input v-model="precoMin" type="text" inputmode="decimal" placeholder="0,00" />
            </label>
            <label class="rotulo">Preço máximo
              <input v-model="precoMax" type="text" inputmode="decimal" placeholder="0,00" />
            </label>
            <label class="rotulo">Nota mínima
              <select v-model="nota">
                <option value="">Qualquer</option>
                <option value="4">4 ou mais</option>
                <option value="5">5</option>
              </select>
            </label>
            <label class="rotulo">Ordenar
              <select v-model="ordem">
                <option value="relevancia">Relevância</option>
                <option value="preco">Menor preço</option>
                <option value="preco-desc">Maior preço</option>
                <option value="vendas">Mais vendidos</option>
                <option value="novidade">Novidades</option>
              </select>
            </label>
          </div>

          <div class="opcoes">
            <p>Opções</p>
            <label><input v-model="freteGratis" type="checkbox" /> Frete grátis</label>
            <label><input v-model="aVista" type="checkbox" /> À vista</label>
            <label><input v-model="personalizavel" type="checkbox" /> Personalizável</label>
          </div>
          <p class="dica">Frete grátis vale para peças a partir de {{ dados.freteGratisTexto }}.</p>
          <div class="acoes">
            <RouterLink v-if="dados.temFiltro" class="botao linha" to="/busca">Limpar</RouterLink>
            <Button type="submit" label="Aplicar filtros" />
          </div>
        </form>

        <div v-if="dados.ativos.length" class="ativos" aria-label="Filtros aplicados">
          <RouterLink
            v-for="ativo in dados.ativos"
            :key="ativo.rotulo"
            class="chip"
            :to="sem(ativo.remover)"
            :aria-label="`Remover filtro ${ativo.rotulo}`"
          >
            {{ ativo.rotulo }} <span aria-hidden="true">×</span>
          </RouterLink>
          <RouterLink class="chip limpar" to="/busca">Limpar tudo</RouterLink>
        </div>

        <div v-if="!dados.resultados.length" class="vazio">
          <img :src="mascote" alt="" width="120" height="120" />
          <p>{{ dados.temFiltro ? "Nenhuma peça com esses filtros." : "A vitrine ainda não tem peças publicadas." }}</p>
          <RouterLink v-if="dados.temFiltro" class="botao linha" to="/busca">Limpar filtros</RouterLink>
        </div>
        <div v-else class="grade">
          <RouterLink v-for="p in dados.resultados" :key="p.id" class="peca" :to="`/produto/${p.id}`">
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
import { onBeforeUnmount, onMounted, ref, watch } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { obterJson } from "../../shared/http";
import { caminhoMidia, moeda, textoPrazo } from "./home";

interface Categoria {
  id: string;
  nome: string;
}

interface FiltroAtivo {
  rotulo: string;
  remover: string[];
}

interface Sugestao {
  id: string;
  titulo: string;
  preco: number;
  categoria: string;
  foto: string | null;
}

interface BuscaItem {
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

interface BuscaDados {
  ajuda: string;
  mensagem: string | null;
  temFiltro: boolean;
  resumo: string;
  freteGratisAPartir: number;
  freteGratisTexto: string;
  filtros: {
    q: string | null;
    categoria: string | null;
    precoMin: string;
    precoMax: string;
    ordem: string;
    nota: string | null;
    freteGratis: boolean;
    aVista: boolean;
    personalizavel: boolean;
  };
  categorias: Categoria[];
  ativos: FiltroAtivo[];
  resultados: BuscaItem[];
}

const mascote = "/marca/mascote.png";
const route = useRoute();
const router = useRouter();
const dados = ref<BuscaDados | null>(null);
const carregando = ref(true);
const erro = ref("");
const termo = ref("");
const categoria = ref("");
const precoMin = ref("");
const precoMax = ref("");
const nota = ref("");
const ordem = ref("relevancia");
const freteGratis = ref(false);
const aVista = ref(false);
const personalizavel = ref(false);
const sugestoes = ref<Sugestao[]>([]);
let timer = 0;

function queryAtual(): Record<string, string> {
  const q: Record<string, string> = {};
  for (const chave of ["q", "categoria", "precoMin", "precoMax", "ordem", "nota", "freteGratis", "aVista", "personalizavel"]) {
    const valor = route.query[chave];
    if (typeof valor === "string" && valor.trim()) q[chave] = valor;
  }
  return q;
}

function sem(chaves: string[]) {
  const q = queryAtual();
  for (const chave of chaves) delete q[chave];
  return { path: "/busca", query: q };
}

function aplicar() {
  const q: Record<string, string> = {};
  const texto = termo.value.trim();
  if (texto) q.q = texto;
  if (categoria.value) q.categoria = categoria.value;
  if (precoMin.value.trim()) q.precoMin = precoMin.value.trim();
  if (precoMax.value.trim()) q.precoMax = precoMax.value.trim();
  if (nota.value) q.nota = nota.value;
  if (ordem.value && ordem.value !== "relevancia") q.ordem = ordem.value;
  if (freteGratis.value) q.freteGratis = "1";
  if (aVista.value) q.aVista = "1";
  if (personalizavel.value) q.personalizavel = "1";
  fecharSugestoes();
  router.push({ path: "/busca", query: q });
}

function preencher(busca: BuscaDados) {
  termo.value = busca.filtros.q ?? "";
  categoria.value = busca.filtros.categoria ?? "";
  precoMin.value = busca.filtros.precoMin;
  precoMax.value = busca.filtros.precoMax;
  nota.value = busca.filtros.nota ?? "";
  ordem.value = busca.filtros.ordem || "relevancia";
  freteGratis.value = busca.filtros.freteGratis;
  aVista.value = busca.filtros.aVista;
  personalizavel.value = busca.filtros.personalizavel;
}

async function carregar() {
  carregando.value = true;
  erro.value = "";
  fecharSugestoes();
  try {
    const params = new URLSearchParams(queryAtual());
    const busca = await obterJson<BuscaDados>(`/api/loja/busca?${params.toString()}`);
    dados.value = busca;
    preencher(busca);
  } catch {
    dados.value = null;
    erro.value = "Não foi possível carregar a busca.";
  } finally {
    carregando.value = false;
  }
}

function agendarSugestoes() {
  window.clearTimeout(timer);
  const valor = termo.value.trim();
  if (valor.length < 2) {
    fecharSugestoes();
    return;
  }
  timer = window.setTimeout(() => void buscarSugestoes(valor), 220);
}

async function buscarSugestoes(valor: string) {
  const params = new URLSearchParams();
  params.set("q", valor);
  if (categoria.value) params.set("categoria", categoria.value);
  if (precoMin.value.trim()) params.set("precoMin", precoMin.value.trim());
  if (precoMax.value.trim()) params.set("precoMax", precoMax.value.trim());
  if (nota.value) params.set("nota", nota.value);
  if (freteGratis.value) params.set("freteGratis", "1");
  if (aVista.value) params.set("aVista", "1");
  if (personalizavel.value) params.set("personalizavel", "1");
  try {
    const lista = await obterJson<Sugestao[]>(`/api/loja/busca/sugestoes?${params.toString()}`);
    sugestoes.value = termo.value.trim() === valor ? lista : [];
  } catch {
    fecharSugestoes();
  }
}

function fecharSugestoes() {
  sugestoes.value = [];
}

function cliqueFora(evento: MouseEvent) {
  const alvo = evento.target;
  if (alvo instanceof Node && !document.querySelector(".caixa")?.contains(alvo)) fecharSugestoes();
}

watch(() => route.query, () => void carregar());
onMounted(() => {
  document.addEventListener("click", cliqueFora);
  void carregar();
});
onBeforeUnmount(() => {
  document.removeEventListener("click", cliqueFora);
  window.clearTimeout(timer);
});
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

.cab {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: end;
  justify-content: space-between;
}

h1 {
  margin: 0 0 0.25rem;
}

.painel,
.vazio,
.peca,
.sugestoes a {
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.painel {
  display: grid;
  gap: 0.75rem;
  margin-top: 1rem;
  padding: 1rem;
}

.rotulo,
.opcoes label {
  display: grid;
  gap: 0.3rem;
  font-weight: 700;
}

input,
select {
  min-height: 44px;
  padding: 0.4rem 0.6rem;
  border: 2px solid #eadfce;
  border-radius: 12px;
  font: inherit;
}

.caixa {
  position: relative;
}

.sugestoes {
  position: absolute;
  z-index: 2;
  left: 0;
  right: 0;
  display: grid;
  gap: 0.35rem;
  margin-top: 0.35rem;
}

.sugestoes a {
  display: flex;
  gap: 0.6rem;
  align-items: center;
  padding: 0.45rem;
  color: inherit;
  text-decoration: none;
}

.sugestoes img {
  width: 48px;
  height: 48px;
  object-fit: cover;
  border-radius: 8px;
}

.sugestoes em {
  display: block;
  font-style: normal;
  font-weight: 500;
}

.grade-form,
.grade,
.opcoes,
.ativos,
.acoes {
  display: grid;
  gap: 0.75rem;
}

.opcoes label {
  grid-template-columns: auto 1fr;
  align-items: center;
}

.ativos {
  grid-auto-flow: column;
  grid-auto-columns: max-content;
  margin-top: 0.8rem;
  overflow-x: auto;
}

.chip {
  padding: 0.45rem 0.7rem;
  border-radius: 999px;
  background: #fff;
  border: 2px solid #e8772a;
  color: inherit;
  font-weight: 700;
  text-decoration: none;
  white-space: nowrap;
}

.vazio {
  margin-top: 1rem;
  padding: 1rem;
  text-align: center;
}

.grade {
  grid-template-columns: 1fr;
  margin-top: 1rem;
}

.peca {
  padding: 0.9rem;
  color: inherit;
  text-decoration: none;
}

.peca img,
.vazio img {
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
  .grade-form {
    grid-template-columns: repeat(2, 1fr);
  }

  .grade {
    grid-template-columns: repeat(2, 1fr);
  }

  .opcoes {
    grid-template-columns: repeat(3, max-content);
  }
}

@media (min-width: 1024px) {
  .grade {
    grid-template-columns: repeat(4, 1fr);
  }
}
</style>
