<template>
  <div class="loja">
    <header class="topo">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Ficha</p>
    </header>

    <main class="miolo">
      <p v-if="carregando" class="aviso">Carregando a peça...</p>
      <p v-else-if="naoEncontrado" class="vazio">Não encontrei essa peça.</p>
      <Message v-else-if="erro" severity="error" :closable="false">
        {{ erro }}
        <Button class="tentar" label="Tentar de novo" @click="carregar" />
      </Message>

      <template v-else-if="dados">
        <div class="ficha">
          <div class="galeria">
            <button class="principal" type="button" @click="zoom = true">
              <img :src="caminhoMidia(fotoAtual?.arquivo)" :alt="fotoAtual?.legenda || dados.titulo" />
            </button>
            <div v-if="dados.fotos.length" class="miniaturas">
              <button
                v-for="foto in dados.fotos"
                :key="foto.id"
                type="button"
                class="mini"
                :class="{ ativa: foto.id === fotoAtual?.id }"
                @click="fotoId = foto.id"
              >
                <img :src="caminhoMidia(foto.arquivo)" :alt="foto.legenda" />
              </button>
            </div>
            <p class="legenda">{{ (fotoAtual?.legenda || dados.titulo) }} · sessão {{ dados.sessao }}</p>
          </div>

          <div class="info">
            <p class="tag">{{ dados.modalidadeNome || dados.modalidadeId }}</p>
            <p class="tema">{{ dados.categoriaNome || dados.categoriaId }}</p>
            <h1>{{ dados.titulo }}</h1>
            <p v-if="dados.oferta && dados.precoComparativo > dados.preco" class="oferta">Oferta</p>
            <p class="preco">
              {{ moeda(dados.preco) }}
              <s v-if="dados.precoComparativo > dados.preco">{{ moeda(dados.precoComparativo) }}</s>
            </p>
            <p v-if="personalizado" class="vista">Preço médio. O valor final sai depois do orçamento.</p>
            <template v-else-if="dados.aVista">
              <p class="vista">À vista {{ moeda(precoVista) }} no PIX ou boleto</p>
              <p class="discreto">{{ dados.parcelas }} x de {{ moeda(parcela) }} no cartão, sem o desconto à vista</p>
            </template>
            <p>{{ textoPrazo(prazoItem) }}</p>
            <p class="meta">
              <span>{{ estoqueTexto }}</span>
              <span>{{ pesoTexto }}</span>
              <span>{{ dados.medidas }}</span>
            </p>
            <p>{{ dados.descricao }}</p>

            <section v-for="eixo in dados.variacoes" :key="eixo.id" class="bloco">
              <h2>{{ eixo.nome }}</h2>
              <div v-if="eixo.tipo === 'cor'" class="amostras">
                <button
                  v-for="opc in eixo.opcoes"
                  :key="opc.id"
                  type="button"
                  class="amostra"
                  :class="{ ativa: escolha[eixo.id] === opc.nome }"
                  :style="{ background: opc.hex }"
                  :title="opc.nome"
                  @click="escolha[eixo.id] = opc.nome"
                />
              </div>
              <div v-else class="opcoes">
                <button
                  v-for="opc in eixo.opcoes"
                  :key="opc.id"
                  type="button"
                  class="opcao"
                  :class="{ ativa: escolha[eixo.id] === opc.nome }"
                  @click="escolha[eixo.id] = opc.nome"
                >{{ opc.nome }}</button>
              </div>
              <p>{{ escolha[eixo.id] }}</p>
            </section>

            <section v-if="dados.cores.length" class="bloco">
              <h2>Cores</h2>
              <div class="amostras">
                <span v-for="cor in dados.cores" :key="cor.id" class="amostra fixa" :style="{ background: cor.hex }" :title="cor.nome" />
              </div>
              <p>{{ dados.cores.map(c => c.nome).join(", ") }}</p>
            </section>

            <section v-if="dados.tamanhos.length" class="bloco">
              <h2>Tamanhos</h2>
              <p>{{ dados.tamanhos.map(t => t.nome).join(", ") }}</p>
            </section>

            <section v-if="dados.personalizavel && dados.etapas.length" class="bloco">
              <h2>Personalização</h2>
              <label v-for="etapa in dados.etapas" :key="etapa.id" class="etapa">
                {{ etapa.nome }}<template v-if="etapa.acresce > 0"> · +{{ moeda(etapa.acresce) }}</template>
                <span v-if="etapa.tipo === 'arquivo'" class="arquivo">
                  <input type="file" accept="image/*" @change="nomeArquivo(etapa.id, $event)" />
                  <span>{{ arquivos[etapa.id] || "Nenhuma imagem" }}</span>
                </span>
                <input v-else-if="etapa.tipo === 'numero'" v-model="etapaValores[etapa.id]" type="number" min="1" />
                <input v-else v-model="etapaValores[etapa.id]" type="text" />
              </label>
            </section>

            <div v-if="!personalizado" class="compra">
              <div class="qtd">
                <span>Quantidade</span>
                <button type="button" aria-label="Diminuir quantidade" @click="definirQuantidade(qtd - 1)">−</button>
                <input v-model.number="qtd" type="number" min="1" :max="maxQtd" aria-label="Quantidade" @change="definirQuantidade(qtd)" />
                <button type="button" aria-label="Aumentar quantidade" @click="definirQuantidade(qtd + 1)">+</button>
              </div>
              <p v-if="erroCompra" class="erro-compra">{{ erroCompra }}</p>
              <button class="comprar" type="button" :disabled="esgotado || enviando" @click="adicionar('checkout')">{{ esgotado ? "Esgotado" : "Comprar agora" }}</button>
              <button class="opcao" type="button" :disabled="esgotado || enviando" @click="adicionar('toast')">Adicionar ao carrinho</button>
            </div>

            <div class="acoes">
              <button
                type="button"
                class="opcao"
                :class="{ ativo: favorito }"
                :aria-pressed="favorito ? 'true' : 'false'"
                :disabled="enviandoFavorito"
                @click="alternarFavorito"
              >{{ favorito ? "Nos favoritos" : "Salvar nos favoritos" }}</button>
            </div>
            <p v-if="erroFavorito" class="erro-compra">{{ erroFavorito }}</p>
            <button
              v-if="podeOrcar"
              type="button"
              class="orcamento"
              :disabled="enviandoOrcamento"
              @click="pedirOrcamento"
            >Pedir orçamento</button>
            <p v-if="erroOrcamento" class="erro-compra">{{ erroOrcamento }}</p>

            <p class="canais" v-if="whats || dados.shopee || dados.mercadoLivre">
              <a v-if="whats" :href="whats" target="_blank" rel="noopener">WhatsApp</a>
              <a v-if="dados.shopee" :href="dados.shopee" target="_blank" rel="noopener">Shopee</a>
              <a v-if="dados.mercadoLivre" :href="dados.mercadoLivre" target="_blank" rel="noopener">Mercado Livre</a>
            </p>

            <section class="frete">
              <h2>Calcular frete</h2>
              <form class="frete-form" @submit.prevent="cotarFrete">
                <input v-model="cep" type="text" inputmode="numeric" maxlength="9" placeholder="00000-000" aria-label="CEP" />
                <button class="comprar" type="submit" :disabled="cotando">Cotar</button>
              </form>
              <p v-if="erroFrete" class="erro-compra">{{ erroFrete }}</p>
              <p v-for="op in opcoesFrete" :key="op.id">
                <strong>{{ op.nome }}</strong> · {{ textoPrecoFrete(op) }} · {{ textoPrazoFrete(op.dias) }}
                <span v-if="op.descricao" class="frete-desc">{{ op.descricao }}</span>
              </p>
            </section>
          </div>
        </div>

        <div class="painel">
          <section class="cartao">
            <h2>Perguntas</h2>
            <p v-if="!dados.perguntas.length" class="discreto">Nenhuma pergunta ainda.</p>
            <article v-for="p in dados.perguntas" :key="p.id" class="pergunta">
              <strong>{{ p.autor }}</strong>
              <p>{{ p.texto }}</p>
              <p v-if="p.respondida" class="resposta"><strong>Cria Lab</strong> {{ p.resposta }}</p>
              <p v-else class="discreto">Aguardando resposta</p>
            </article>
            <form class="pergunta-form" @submit.prevent="enviarPergunta">
              <label class="etapa">Sua pergunta
                <input v-model="textoPergunta" type="text" required placeholder="Escreva sua pergunta" />
              </label>
              <button class="comprar" type="submit" :disabled="enviandoPergunta">Enviar</button>
              <p v-if="erroPergunta" class="erro-compra">{{ erroPergunta }}</p>
            </form>
          </section>
          <section class="cartao">
            <h2>Avaliações</h2>
            <p v-if="!dados.avaliacoes.length" class="discreto">Sem avaliações ainda.</p>
            <article v-for="a in dados.avaliacoes" :key="a.id">
              <p><strong>{{ a.autor }}</strong> {{ a.nota }}/5</p>
              <p>{{ a.texto }}</p>
            </article>
          </section>
        </div>

        <section v-if="dados.relacionados.length" class="relacionados">
          <h2>Relacionados</h2>
          <div class="grade">
            <RouterLink v-for="p in dados.relacionados" :key="p.id" class="peca" :to="`/produto/${p.id}`">
              <img :src="caminhoMidia(p.foto)" :alt="p.titulo" />
              <p class="tag">{{ p.modalidadeNome }}</p>
              <h3>{{ p.titulo }}</h3>
              <p class="preco">{{ moeda(p.preco) }}</p>
              <p v-if="p.modalidadeId === 'personalizado'" class="media">Preço médio · fecha no orçamento</p>
              <p>{{ textoPrazo({ ...p, maisVendido: false, categoriaNome: "" }) }}</p>
            </RouterLink>
          </div>
        </section>

        <button v-if="zoom" class="zoom" type="button" @click="zoom = false">
          <img :src="caminhoMidia(fotoAtual?.arquivo)" alt="" />
        </button>
        <p v-if="toast" class="toast" role="status">
          <strong>Adicionado ao carrinho</strong>
          <RouterLink to="/carrinho">Ver carrinho</RouterLink>
          <RouterLink to="/checkout">Finalizar</RouterLink>
        </p>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, reactive, ref, watch } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { caminhoMidia, moeda, textoPrazo, type DestaqueHome } from "./home";
import { useCarrinho, type ItemCarrinho } from "./carrinho";
import { lerToken, useFavoritos } from "./favoritos";

interface Foto {
  id: number;
  arquivo: string;
  legenda: string;
  ordem: number;
}

interface Opcao {
  id: string;
  nome: string;
  hex: string;
}

interface Eixo {
  id: string;
  nome: string;
  tipo: string;
  opcoes: Opcao[];
}

interface Relacionado {
  id: string;
  titulo: string;
  preco: number;
  modalidadeId: string;
  modalidadeNome: string;
  foto: string | null;
  prazoTipo: string;
  freteGratis: boolean;
  prazoDias: number;
  prazoInicio: string | null;
  prazoFim: string | null;
}

interface OpcaoFrete {
  id: string;
  nome: string;
  preco: number;
  dias: number;
  descricao: string | null;
  gratis: boolean;
}

interface Ficha {
  id: string;
  titulo: string;
  sessao: string;
  descricao: string;
  preco: number;
  precoComparativo: number;
  oferta: boolean;
  aVista: boolean;
  descontoAVista: number;
  parcelas: number;
  estoque: number;
  medidas: string;
  peso: number;
  situacao: string;
  personalizavel: boolean;
  modalidadeId: string;
  modalidadeNome: string;
  categoriaId: string;
  categoriaNome: string;
  prazoTipo: string;
  freteGratis: boolean;
  prazoDias: number;
  prazoInicio: string | null;
  prazoFim: string | null;
  whatsapp: string | null;
  shopee: string | null;
  mercadoLivre: string | null;
  fotos: Foto[];
  variacoes: Eixo[];
  cores: { id: number; nome: string; hex: string }[];
  tamanhos: { id: number; nome: string }[];
  etapas: { id: string; nome: string; tipo: string; acresce: number }[];
  avaliacoes: { id: string; autor: string; nota: number; texto: string }[];
  perguntas: { id: string; autor: string; texto: string; resposta: string; respondida: boolean }[];
  relacionados: Relacionado[];
}

const route = useRoute();
const router = useRouter();
const carrinho = useCarrinho();
const favoritos = useFavoritos();
const dados = ref<Ficha | null>(null);
const carregando = ref(true);
const erro = ref("");
const naoEncontrado = ref(false);
const fotoId = ref<number | null>(null);
const zoom = ref(false);
const escolha = reactive<Record<string, string>>({});
const arquivos = reactive<Record<string, string>>({});
const etapaValores = reactive<Record<string, string>>({});
const qtd = ref(1);
const enviando = ref(false);
const erroCompra = ref("");
const toast = ref(false);
const favorito = ref(false);
const enviandoFavorito = ref(false);
const erroFavorito = ref("");
const enviandoOrcamento = ref(false);
const erroOrcamento = ref("");
const textoPergunta = ref("");
const enviandoPergunta = ref(false);
const erroPergunta = ref("");
const cep = ref("");
const cotando = ref(false);
const erroFrete = ref("");
const opcoesFrete = ref<OpcaoFrete[]>([]);
let toastTimer = 0;

const personalizado = computed(() => dados.value?.modalidadeId.toLowerCase() === "personalizado");
const podeOrcar = computed(() => personalizado.value || dados.value?.personalizavel === true);
const sobDemanda = computed(() => dados.value?.modalidadeId.toLowerCase() === "sob-demanda");
const precoVista = computed(() => (dados.value ? dados.value.preco * (1 - dados.value.descontoAVista) : 0));
const parcela = computed(() => {
  if (!dados.value) return 0;
  const n = dados.value.parcelas > 0 ? dados.value.parcelas : 3;
  return dados.value.preco / n;
});
const estoqueTexto = computed(() => {
  if (!dados.value) return "";
  if (personalizado.value) return "Sob encomenda";
  if (sobDemanda.value) return "Impressão após o pedido";
  if (dados.value.situacao === "esgotado" || dados.value.estoque <= 0) return "Sem unidade pronta";
  return `${dados.value.estoque} em estoque`;
});
const pesoTexto = computed(() => (dados.value ? `${dados.value.peso.toLocaleString("pt-BR")} g` : ""));
const esgotado = computed(() => {
  if (!dados.value || personalizado.value || sobDemanda.value) return false;
  return dados.value.situacao === "esgotado" || dados.value.estoque <= 0;
});
const maxQtd = computed(() => (dados.value && dados.value.estoque > 0 ? dados.value.estoque : 99));
const fotoAtual = computed(() => dados.value?.fotos.find(f => f.id === fotoId.value) ?? dados.value?.fotos[0] ?? null);
const prazoItem = computed(() => {
  const item = dados.value;
  if (!item) return {} as DestaqueHome;
  return { ...item, foto: null, maisVendido: false };
});
const whats = computed(() => {
  if (!dados.value) return "";
  const base = dados.value.whatsapp || "https://wa.me/";
  return `${base}?text=${encodeURIComponent("Olá, quero saber sobre " + dados.value.titulo)}`;
});

function preparar(ficha: Ficha) {
  fotoId.value = ficha.fotos[0]?.id ?? null;
  zoom.value = false;
  for (const chave of Object.keys(escolha)) delete escolha[chave];
  for (const chave of Object.keys(arquivos)) delete arquivos[chave];
  for (const chave of Object.keys(etapaValores)) delete etapaValores[chave];
  qtd.value = 1;
  erroCompra.value = "";
  for (const eixo of ficha.variacoes) {
    escolha[eixo.id] = eixo.opcoes[0]?.nome ?? "";
  }
}

function definirQuantidade(valor: number) {
  const n = Math.max(1, Math.min(maxQtd.value, Number(valor) || 1));
  qtd.value = n;
}

function coletar() {
  const partes: string[] = [];
  let cor = "";
  let tamanho = "";
  if (!dados.value) return { cor: null as string | null, tamanho: null as string | null, detalhes: null as string | null, arquivoNome: null as string | null };
  for (const eixo of dados.value.variacoes) {
    const valor = (escolha[eixo.id] || "").trim();
    if (!valor) continue;
    partes.push(`${eixo.nome}: ${valor}`);
    if (eixo.tipo === "cor" && !cor) cor = valor;
    else if (!tamanho) tamanho = valor;
  }
  if (dados.value.personalizavel) {
    for (const etapa of dados.value.etapas) {
      if (etapa.tipo === "arquivo") {
        const nome = arquivos[etapa.id];
        if (nome && nome !== "Nenhuma imagem") partes.push(`${etapa.nome}: ${nome}`);
      } else if (etapaValores[etapa.id]) {
        partes.push(`${etapa.nome}: ${etapaValores[etapa.id]}`);
      }
    }
  }
  return {
    cor: cor || null,
    tamanho: tamanho || null,
    detalhes: partes.length ? partes.join(" · ") : null,
    arquivoNome: nomeArquivoEscolhido(),
  };
}

function nomeArquivoEscolhido() {
  if (!dados.value?.personalizavel) return null;
  for (const etapa of dados.value.etapas) {
    if (etapa.tipo !== "arquivo") continue;
    const nome = arquivos[etapa.id];
    if (nome && nome !== "Nenhuma imagem") return nome;
  }
  return null;
}

async function adicionar(destino: "checkout" | "toast") {
  if (!dados.value || esgotado.value || enviando.value) return;
  definirQuantidade(qtd.value);
  enviando.value = true;
  erroCompra.value = "";
  const escolhas = coletar();
  try {
    const resposta = await fetch("/api/loja/carrinho/itens", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        produtoId: dados.value.id,
        quantidade: qtd.value,
        cor: escolhas.cor,
        tamanho: escolhas.tamanho,
        detalhes: escolhas.detalhes,
        destino,
      }),
      signal: AbortSignal.timeout(8000),
    });
    if (!resposta.ok) throw new Error("falha");
    const item = (await resposta.json()) as ItemCarrinho & { destino: string };
    carrinho.adicionar(item);
    if (item.destino === "checkout") {
      toast.value = false;
      await router.push("/checkout");
      return;
    }
    toast.value = true;
    window.clearTimeout(toastTimer);
    toastTimer = window.setTimeout(() => {
      toast.value = false;
    }, 4200);
  } catch {
    erroCompra.value = "Não foi possível adicionar ao carrinho.";
  } finally {
    enviando.value = false;
  }
}

function textoPrecoFrete(op: OpcaoFrete) {
  if (op.gratis || op.preco === 0) return "grátis";
  return `R$ ${op.preco.toFixed(2).replace(".", ",")}`;
}

function textoPrazoFrete(dias: number) {
  return dias === 1 ? "1 dia útil" : `${dias} dias úteis`;
}

async function cotarFrete() {
  if (!dados.value || cotando.value) return;
  cotando.value = true;
  erroFrete.value = "";
  opcoesFrete.value = [];
  try {
    const resposta = await fetch(
      `/api/loja/produtos/${encodeURIComponent(dados.value.id)}/frete?cep=${encodeURIComponent(cep.value)}`,
      { signal: AbortSignal.timeout(8000) },
    );
    if (!resposta.ok) throw new Error("falha");
    const corpo = (await resposta.json()) as { erro: string | null; opcoes: OpcaoFrete[] };
    if (corpo.erro) {
      erroFrete.value = corpo.erro;
      return;
    }
    opcoesFrete.value = corpo.opcoes ?? [];
  } catch {
    erroFrete.value = "Não foi possível cotar agora.";
  } finally {
    cotando.value = false;
  }
}

async function enviarPergunta() {
  if (!dados.value || enviandoPergunta.value) return;
  enviandoPergunta.value = true;
  erroPergunta.value = "";
  const produtoId = dados.value.id;
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  const token = lerToken();
  if (token) headers.Authorization = `Bearer ${token}`;
  try {
    const resposta = await fetch(`/api/loja/produtos/${encodeURIComponent(produtoId)}/perguntas`, {
      method: "POST",
      headers,
      body: JSON.stringify({ texto: textoPergunta.value }),
      signal: AbortSignal.timeout(8000),
    });
    if (resposta.status === 400) {
      erroPergunta.value = "Escreva sua pergunta.";
      return;
    }
    if (!resposta.ok) throw new Error("falha");
    const criada = (await resposta.json()) as Ficha["perguntas"][number];
    dados.value.perguntas = [criada, ...dados.value.perguntas];
    textoPergunta.value = "";
  } catch {
    erroPergunta.value = "Não foi possível enviar a pergunta.";
  } finally {
    enviandoPergunta.value = false;
  }
}

async function pedirOrcamento() {
  if (!dados.value || enviandoOrcamento.value) return;
  const produtoId = dados.value.id;
  const token = lerToken();
  if (!token) {
    await router.push({ path: "/conta/entrar", query: { returnUrl: `/produto/${produtoId}` } });
    return;
  }
  enviandoOrcamento.value = true;
  erroOrcamento.value = "";
  const escolhas = coletar();
  try {
    const resposta = await fetch(`/api/loja/produtos/${encodeURIComponent(produtoId)}/orcamentos`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}`, "Content-Type": "application/json" },
      body: JSON.stringify({ detalhes: escolhas.detalhes, arquivoNome: escolhas.arquivoNome }),
      signal: AbortSignal.timeout(8000),
    });
    if (resposta.status === 401) {
      await router.push({ path: "/conta/entrar", query: { returnUrl: `/produto/${produtoId}` } });
      return;
    }
    if (!resposta.ok) throw new Error("falha");
    await router.push("/conta/orcamentos");
  } catch {
    erroOrcamento.value = "Não foi possível pedir o orçamento.";
  } finally {
    enviandoOrcamento.value = false;
  }
}

function nomeArquivo(id: string, evento: Event) {
  const campo = evento.target;
  if (!(campo instanceof HTMLInputElement)) return;
  arquivos[id] = campo.files?.[0]?.name || "Nenhuma imagem";
}

async function sincronizarFavorito(produtoId: string) {
  erroFavorito.value = "";
  const token = lerToken();
  if (!token) {
    favorito.value = favoritos.tem(produtoId);
    return;
  }
  try {
    const resposta = await fetch(`/api/loja/favoritos/${encodeURIComponent(produtoId)}`, {
      headers: { Authorization: `Bearer ${token}` },
      signal: AbortSignal.timeout(8000),
    });
    if (!resposta.ok) throw new Error("falha");
    const corpo = (await resposta.json()) as { favorito: boolean };
    favorito.value = corpo.favorito;
  } catch {
    erroFavorito.value = "Não foi possível carregar o favorito.";
  }
}

async function alternarFavorito() {
  if (!dados.value || enviandoFavorito.value) return;
  const produtoId = dados.value.id;
  const token = lerToken();
  if (!token) {
    favorito.value = favoritos.alternar(produtoId);
    erroFavorito.value = "";
    return;
  }
  enviandoFavorito.value = true;
  erroFavorito.value = "";
  try {
    const resposta = await fetch(`/api/loja/favoritos/${encodeURIComponent(produtoId)}`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      signal: AbortSignal.timeout(8000),
    });
    if (!resposta.ok) throw new Error("falha");
    const corpo = (await resposta.json()) as { favorito: boolean };
    favorito.value = corpo.favorito;
  } catch {
    erroFavorito.value = "Não foi possível salvar o favorito.";
  } finally {
    enviandoFavorito.value = false;
  }
}

async function carregar() {
  carregando.value = true;
  erro.value = "";
  naoEncontrado.value = false;
  dados.value = null;
  favorito.value = false;
  erroFavorito.value = "";
  erroOrcamento.value = "";
  erroPergunta.value = "";
  cep.value = "";
  erroFrete.value = "";
  opcoesFrete.value = [];
  const id = String(route.params.id ?? "");
  try {
    const resposta = await fetch(`/api/loja/produtos/${encodeURIComponent(id)}`, { signal: AbortSignal.timeout(8000) });
    if (resposta.status === 404) {
      naoEncontrado.value = true;
      return;
    }
    if (!resposta.ok) throw new Error("falha");
    const ficha = (await resposta.json()) as Ficha;
    dados.value = ficha;
    preparar(ficha);
    await sincronizarFavorito(ficha.id);
  } catch {
    erro.value = "Não foi possível carregar a peça.";
  } finally {
    carregando.value = false;
  }
}

watch(() => route.params.id, () => void carregar());
onBeforeUnmount(() => window.clearTimeout(toastTimer));
void carregar();
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

.ficha,
.painel,
.grade {
  display: grid;
  gap: 1rem;
}

.info,
.cartao,
.peca,
.vazio {
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.info,
.cartao,
.vazio {
  padding: 1rem;
}

.principal,
.mini,
.amostra,
.opcao,
.zoom {
  border: 2px solid #eadfce;
  background: #fff;
}

.principal {
  width: 100%;
  padding: 0;
  border-radius: 18px;
  cursor: zoom-in;
}

.principal img,
.peca img {
  width: 100%;
  max-height: 320px;
  object-fit: contain;
}

.miniaturas,
.amostras,
.opcoes,
.canais {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.mini {
  width: 64px;
  height: 64px;
  padding: 0;
  border-radius: 10px;
}

.mini img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.mini.ativa,
.opcao.ativa,
.opcao.ativo,
.amostra.ativa {
  border-color: #e8772a;
}

.acoes {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.orcamento {
  min-height: 44px;
  padding: 0.4rem 0.8rem;
  border: 0;
  border-radius: 999px;
  background: #148f93;
  color: #fff;
  font: inherit;
  font-weight: 700;
}

.orcamento:disabled {
  opacity: 0.55;
}

.pergunta-form {
  display: grid;
  gap: 0.75rem;
  margin-top: 0.75rem;
}

.pergunta-form input {
  width: 100%;
  box-sizing: border-box;
}

.pergunta-form .comprar {
  width: fit-content;
}

.frete,
.frete-form {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.frete {
  flex-direction: column;
  margin-top: 1rem;
}

.frete h2,
.frete p {
  margin: 0;
}

.frete-form input {
  flex: 1 1 8rem;
  min-width: 0;
  width: auto;
  box-sizing: border-box;
}

.frete-form .comprar {
  width: fit-content;
}

.frete-desc {
  display: block;
  color: #5c6b7a;
  font-weight: 400;
}

.amostra {
  width: 36px;
  height: 36px;
  border-radius: 999px;
}

.amostra.fixa {
  display: inline-block;
}

.opcao,
.comprar,
.qtd button {
  min-height: 44px;
  padding: 0.4rem 0.8rem;
  border-radius: 999px;
  font: inherit;
  font-weight: 700;
}

.comprar {
  border: 0;
  background: #e8772a;
  color: #fff;
}

.compra,
.qtd {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
}

.qtd input {
  width: 4.5rem;
}

.comprar:disabled,
.opcao:disabled {
  opacity: 0.55;
}

.erro-compra {
  width: 100%;
  margin: 0;
  color: #9b2c2c;
  font-weight: 700;
}

.toast {
  position: fixed;
  right: 1rem;
  bottom: 1rem;
  z-index: 4;
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
  max-width: min(420px, calc(100% - 2rem));
  padding: 0.8rem 1rem;
  border-radius: 16px;
  background: #1b2430;
  color: #fff;
}

.toast a {
  color: #fff;
  font-weight: 800;
}

.preco {
  font-size: 1.4rem;
  font-weight: 800;
}

.vista {
  color: #148f93;
  font-weight: 800;
}

.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
}

.etapa {
  display: grid;
  gap: 0.3rem;
  margin-top: 0.6rem;
  font-weight: 700;
}

input[type="text"],
input[type="number"] {
  min-height: 44px;
  padding: 0.4rem 0.6rem;
  border: 2px solid #eadfce;
  border-radius: 12px;
  font: inherit;
}

a.peca {
  color: inherit;
  text-decoration: none;
  padding: 0.9rem;
}

.tag {
  font-weight: 800;
}

.zoom {
  position: fixed;
  inset: 0;
  z-index: 5;
  display: grid;
  place-items: center;
  background: rgb(27 36 48 / 80%);
}

.zoom img {
  max-width: min(920px, 92vw);
  max-height: 88vh;
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
  .ficha,
  .painel {
    grid-template-columns: 1fr 1fr;
    align-items: start;
  }

  .grade {
    grid-template-columns: repeat(4, 1fr);
  }
}
</style>
