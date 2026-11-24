<template>
  <div class="loja">
    <header class="topo">
      <RouterLink class="marca" to="/">Cria Lab DM</RouterLink>
      <p class="atalho">Contato</p>
    </header>

    <main class="miolo">
      <h1>Contato</h1>
      <p class="intro">Manda uma mensagem para a oficina. Se for mais rápido, chama no WhatsApp.</p>

      <p v-if="carregando" class="aviso">Carregando o contato...</p>
      <Message v-if="erroRede && !contato" severity="error" :closable="false">
        {{ erroRede }}
        <Button class="tentar" label="Tentar de novo" @click="carregar()" />
      </Message>

      <div v-else-if="contato" class="grade">
        <aside class="cartao">
          <h2>Fale direto</h2>
          <a v-if="contato.whatsappHref" class="canal" :href="contato.whatsappHref" target="_blank" rel="noopener">
            <strong>WhatsApp</strong>
            <span>Resposta mais rápida</span>
          </a>
          <a v-if="contato.emailHref" class="canal" :href="contato.emailHref">
            <strong>E-mail</strong>
            <span>{{ contato.email }}</span>
          </a>
          <a v-if="contato.telefoneHref" class="canal" :href="contato.telefoneHref">
            <strong>Telefone</strong>
            <span>{{ contato.telefone }}</span>
          </a>
          <p v-if="contato.endereco || contato.horario" class="local">
            <span v-if="contato.endereco">{{ contato.endereco }}</span>
            <span v-if="contato.horario">{{ contato.horario }}</span>
          </p>
        </aside>

        <form class="cartao formulario" @submit.prevent="enviar">
          <h2>Escreva para a oficina</h2>
          <Message v-if="erroRede" severity="error" :closable="false">
            {{ erroRede }}
            <Button class="tentar" label="Tentar de novo" @click="enviar()" />
          </Message>
          <p v-if="enviado" class="ok" role="status">Mensagem enviada. A oficina responde no e-mail que você informou.</p>
          <p v-else-if="erros.length" class="aviso-form" role="alert">Confira os campos antes de enviar.</p>
          <label>
            Nome
            <input v-model="nome" type="text" maxlength="120" autocomplete="name" />
            <span v-if="erroNome" class="erro">{{ erroNome }}</span>
          </label>
          <label>
            E-mail
            <input v-model="email" type="email" maxlength="180" autocomplete="email" />
            <span v-if="erroEmail" class="erro">{{ erroEmail }}</span>
          </label>
          <label>
            Tipo
            <select v-model="tipo">
              <option value="duvida">Dúvida</option>
              <option value="sugestao">Sugestão</option>
              <option value="elogio">Elogio</option>
              <option value="reclamacao">Reclamação</option>
              <option value="orcamento">Orçamento</option>
              <option value="outro">Outro</option>
            </select>
            <span v-if="erroTipo" class="erro">{{ erroTipo }}</span>
          </label>
          <p v-if="tipo === 'orcamento'" class="dica">Conte o que quer imprimir, a quantidade e se já tem foto ou arquivo.</p>
          <label>
            Mensagem
            <textarea v-model="texto" rows="6" maxlength="4000"></textarea>
            <span v-if="erroTexto" class="erro">{{ erroTexto }}</span>
          </label>
          <button class="botao" type="submit" :disabled="enviando">{{ enviando ? "Enviando…" : "Enviar mensagem" }}</button>
        </form>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { RouterLink } from "vue-router";
import Button from "primevue/button";
import Message from "primevue/message";
import { lerToken } from "./favoritos";

interface Contato {
  email: string | null;
  emailHref: string | null;
  telefone: string | null;
  telefoneHref: string | null;
  whatsappHref: string | null;
  endereco: string | null;
  horario: string | null;
  nomeConta: string | null;
  emailConta: string | null;
}

const contato = ref<Contato | null>(null);
const carregando = ref(false);
const enviando = ref(false);
const erroRede = ref("");
const enviado = ref(false);
const erros = ref<string[]>([]);
const nome = ref("");
const email = ref("");
const tipo = ref("duvida");
const texto = ref("");

const erroNome = computed(() => erros.value.find((item) => item.startsWith("Diga como") || item.startsWith("Use um nome")));
const erroEmail = computed(() => erros.value.find((item) => item.startsWith("Informe um e-mail")));
const erroTipo = computed(() => erros.value.find((item) => item.startsWith("Escolha o tipo")));
const erroTexto = computed(() => erros.value.find((item) => item.startsWith("Escreva a mensagem") || item.startsWith("A mensagem passou")));

function cabecalhos() {
  const token = lerToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

async function carregar() {
  erroRede.value = "";
  carregando.value = true;
  try {
    const resposta = await fetch("/api/loja/contato", { headers: cabecalhos() });
    if (!resposta.ok) throw new Error("contato");
    const corpo = (await resposta.json()) as Contato;
    contato.value = corpo;
    if (!nome.value && corpo.nomeConta) nome.value = corpo.nomeConta;
    if (!email.value && corpo.emailConta) email.value = corpo.emailConta;
  } catch {
    erroRede.value = "Não foi possível carregar o contato.";
  } finally {
    carregando.value = false;
  }
}

async function enviar() {
  erroRede.value = "";
  enviado.value = false;
  erros.value = [];
  enviando.value = true;
  try {
    const resposta = await fetch("/api/loja/contato", {
      method: "POST",
      headers: { "Content-Type": "application/json", ...cabecalhos() },
      body: JSON.stringify({
        nome: nome.value,
        email: email.value,
        tipo: tipo.value,
        texto: texto.value,
      }),
    });
    if (resposta.status === 400) {
      const corpo = (await resposta.json()) as { erros?: string[] };
      erros.value = corpo.erros ?? [];
      return;
    }
    if (!resposta.ok) throw new Error("envio");
    enviado.value = true;
    texto.value = "";
    tipo.value = "duvida";
    nome.value = contato.value?.nomeConta ?? "";
    email.value = contato.value?.emailConta ?? "";
  } catch {
    erroRede.value = "Não foi possível enviar a mensagem.";
  } finally {
    enviando.value = false;
  }
}

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

h1,
h2 {
  margin-top: 0;
}

.intro {
  max-width: 42rem;
  color: #5c6b7a;
}

.grade {
  display: grid;
  gap: 1rem;
}

.cartao {
  padding: 1rem;
  background: #fff;
  border: 2px solid #eadfce;
  border-radius: 16px;
}

.canal,
.local span {
  display: block;
}

.canal {
  margin-bottom: 0.75rem;
  color: inherit;
  text-decoration: none;
}

.canal span,
.dica,
.local {
  color: #5c6b7a;
}

.formulario,
label {
  display: grid;
  gap: 0.35rem;
}

label {
  margin-bottom: 0.75rem;
  font-weight: 700;
}

input,
select,
textarea {
  min-height: 44px;
  width: 100%;
  padding: 0.4rem 0.6rem;
  border: 2px solid #eadfce;
  border-radius: 12px;
  font: inherit;
  font-weight: 400;
}

textarea {
  min-height: 8rem;
  resize: vertical;
}

.ok {
  margin: 0 0 1rem;
  color: #176b3a;
  font-weight: 700;
}

.aviso-form,
.erro {
  color: #9b2c2c;
  font-weight: 700;
}

.erro {
  font-size: 0.95rem;
}

.botao {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 44px;
  padding: 0.4rem 0.9rem;
  border: 0;
  border-radius: 999px;
  background: #e8772a;
  color: #fff;
  font: inherit;
  font-weight: 800;
  cursor: pointer;
}

.botao:disabled {
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
    grid-template-columns: 0.8fr 1.2fr;
    align-items: start;
  }
}
</style>
