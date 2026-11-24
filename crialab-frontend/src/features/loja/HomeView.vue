<template>
  <div class="loja">
    <header class="topo">
      <p class="marca">{{ home?.identidade.nome ?? "Cria Lab DM" }}</p>
      <RouterLink class="atalho" to="/busca">Busca</RouterLink>
      <nav v-if="home" class="redes" aria-label="Redes">
        <a v-if="home.identidade.whatsapp" :href="home.identidade.whatsapp" target="_blank" rel="noopener">WhatsApp</a>
        <a v-if="home.identidade.instagram" :href="home.identidade.instagram" target="_blank" rel="noopener">Instagram</a>
        <a v-if="home.identidade.tiktok" :href="home.identidade.tiktok" target="_blank" rel="noopener">TikTok</a>
        <a v-if="home.identidade.youtube" :href="home.identidade.youtube" target="_blank" rel="noopener">YouTube</a>
        <a v-if="home.identidade.shopee" :href="home.identidade.shopee" target="_blank" rel="noopener">Shopee</a>
      </nav>
    </header>

    <main class="miolo">
      <p v-if="carregando" class="aviso">Carregando a loja...</p>
      <Message v-else-if="erro" severity="error" :closable="false">
        {{ erro }}
        <Button class="tentar" label="Tentar de novo" @click="carregar" />
      </Message>

      <template v-else-if="home">
        <section class="carrossel" aria-roledescription="carrossel">
          <article class="slide" :class="slideAtual.fundo">
            <div>
              <p class="selo">{{ home.identidade.nome }}</p>
              <h2>{{ slideAtual.titulo }}</h2>
              <p>{{ slideAtual.texto }}</p>
            </div>
            <img :src="mascote" alt="" />
          </article>
          <div class="setas">
            <Button label="Anterior" severity="secondary" @click="passo(-1)" />
            <Button label="Próximo" @click="passo(1)" />
          </div>
        </section>

        <p class="intro">Explore a vitrine da Cria Lab: modalidades, temas e peças para toque e abrir a ficha.</p>
        <p class="intro ajuda">{{ home.intro }}</p>

        <section>
          <h2>Modalidades</h2>
          <div v-if="home.modalidades.length" class="chips">
            <RouterLink
              v-for="m in home.modalidades"
              :key="`chip-${m.id}`"
              class="chip"
              :to="{ path: '/produtos', query: { ModalidadeFiltro: m.id } }"
              :title="m.resumo"
            >{{ m.nome }}</RouterLink>
          </div>
          <div v-if="home.modalidades.length" class="grade-mod">
            <RouterLink
              v-for="m in home.modalidades"
              :key="m.id"
              class="cartao"
              :to="{ path: '/produtos', query: { ModalidadeFiltro: m.id } }"
              :title="m.resumo"
            >
              <h3>{{ m.nome }}</h3>
              <p>{{ m.explicacao || m.resumo }}</p>
            </RouterLink>
          </div>
          <p v-else class="vazio">Nenhuma modalidade cadastrada.</p>
        </section>

        <section>
          <h2>Temas</h2>
          <div v-if="home.categorias.length" class="chips">
            <span v-for="c in home.categorias" :key="c.id" class="chip">{{ c.nome }}</span>
          </div>
          <p v-else class="vazio">Nenhum tema cadastrado.</p>
        </section>

        <section>
          <h2>Na loja</h2>
          <p v-if="!home.destaques.length" class="vazio">Nenhuma peça publicada ainda.</p>
          <div v-else class="grade">
            <RouterLink v-for="p in home.destaques" :key="p.id" class="peca" :to="`/produto/${p.id}`">
              <img :src="caminhoMidia(p.foto)" :alt="p.titulo" />
              <p class="tag">{{ p.modalidadeNome }}</p>
              <p class="tema">{{ p.categoriaNome }}</p>
              <h3>{{ p.titulo }}</h3>
              <p class="preco">{{ moeda(p.preco) }}</p>
              <p v-if="p.modalidadeId === 'personalizado'" class="media">Preço médio · fecha no orçamento</p>
              <p class="prazo">{{ textoPrazo(p) }}</p>
            </RouterLink>
          </div>
        </section>

        <section>
          <h2>Depoimentos</h2>
          <p v-if="!home.depoimentos.length" class="vazio">Nenhum depoimento publicado.</p>
          <div v-else class="grade-mod">
            <blockquote v-for="d in home.depoimentos" :key="d.id" class="cartao">
              <p>{{ d.texto }}</p>
              <footer>{{ d.autor }}</footer>
            </blockquote>
          </div>
        </section>

        <section v-if="home.parceiros.length">
          <h2>Parceiros</h2>
          <div class="parceiros">
            <a
              v-for="p in home.parceiros"
              :key="p.id"
              :href="p.url || undefined"
              :target="p.url ? '_blank' : undefined"
              rel="noopener"
            >
              <img v-if="p.logo" :src="caminhoMidia(p.logo)" :alt="p.nome" />
              <span v-else>{{ p.nome }}</span>
            </a>
          </div>
        </section>
      </template>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { RouterLink } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { obterJson } from "../../shared/http";
import { caminhoMidia, moeda, textoPrazo, type HomeDados } from "./home";

const mascote = "/marca/mascote.png";
const home = ref<HomeDados | null>(null);
const carregando = ref(true);
const erro = ref("");
const indice = ref(0);

const slides = computed(() => {
  const frete = home.value ? moeda(home.value.freteGratisAPartir) : "";
  return [
    {
      fundo: "laranja",
      titulo: "Pronta entrega na oficina",
      texto: "Peças já impressas, prontas para envio ou retirada.",
    },
    {
      fundo: "teal",
      titulo: "Sob demanda e personalizados",
      texto: "Modelo pronto para imprimir, ou peça sob medida com orçamento.",
    },
    {
      fundo: "tinta",
      titulo: `Frete grátis a partir de ${frete}`,
      texto: "Na ficha você simula PAC e SEDEX pelo CEP.",
    },
  ];
});

const slideAtual = computed(() => slides.value[indice.value] ?? slides.value[0]);

function passo(delta: number) {
  const total = slides.value.length;
  indice.value = (indice.value + delta + total) % total;
}

async function carregar() {
  carregando.value = true;
  erro.value = "";
  try {
    home.value = await obterJson<HomeDados>("/api/loja/home");
  } catch {
    home.value = null;
    erro.value = "Não foi possível carregar a loja.";
  } finally {
    carregando.value = false;
  }
}

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

.marca {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 800;
}

.atalho {
  color: #fff;
  font-weight: 800;
}

.redes {
  display: flex;
  flex-wrap: wrap;
  gap: 0.6rem;
}

.redes a {
  color: #fff;
  font-weight: 700;
}

.miolo {
  width: min(1120px, calc(100% - 2rem));
  margin: 0 auto;
  padding: 1.25rem 0 2.5rem;
}

.aviso,
.vazio,
.intro {
  margin: 0 0 1rem;
}

.vazio {
  padding: 1rem;
  border: 2px dashed #d7cfc3;
  border-radius: 16px;
  background: #fff;
}

.carrossel {
  margin-bottom: 1rem;
}

.slide {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  align-items: center;
  justify-content: space-between;
  min-height: 180px;
  padding: 1.25rem;
  border-radius: 20px;
  color: #fff;
}

.slide img {
  width: 96px;
  height: 96px;
  object-fit: contain;
}

.slide h2,
.slide p,
.selo {
  margin: 0 0 0.35rem;
}

.laranja {
  background: #e8772a;
}

.teal {
  background: #148f93;
}

.tinta {
  background: #1b2430;
}

.setas {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.tentar {
  margin-left: 0.75rem;
}

section {
  margin-top: 1.5rem;
}

h2 {
  margin: 0 0 0.75rem;
}

.grade,
.grade-mod,
.parceiros,
.chips {
  display: grid;
  gap: 0.75rem;
}

.grade,
.grade-mod {
  grid-template-columns: 1fr;
}

.chips {
  grid-auto-flow: column;
  grid-auto-columns: max-content;
  overflow-x: auto;
}

.chip,
.cartao,
.peca,
.parceiros a {
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.chip {
  padding: 0.55rem 0.9rem;
  font-weight: 700;
}

a.chip,
a.cartao,
a.peca {
  color: inherit;
  text-decoration: none;
}

a.peca {
  display: block;
}

.cartao,
.peca {
  padding: 0.9rem;
}

.peca img,
.parceiros img {
  width: 100%;
  height: 160px;
  object-fit: cover;
  border-radius: 12px;
  background: #fff7ef;
}

.tag,
.tema,
.media,
.prazo,
.preco {
  margin: 0.2rem 0;
}

.preco {
  font-size: 1.15rem;
  font-weight: 800;
}

.parceiros {
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
}

.parceiros a {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 88px;
  padding: 0.5rem;
}

blockquote {
  margin: 0;
}

footer {
  margin-top: 0.5rem;
  font-weight: 700;
}

@media (min-width: 700px) {
  .grade {
    grid-template-columns: repeat(2, 1fr);
  }

  .grade-mod {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (min-width: 1024px) {
  .grade {
    grid-template-columns: repeat(4, 1fr);
  }
}
</style>
