import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../features/loja/HomeView.vue";
import BuscaView from "../features/loja/BuscaView.vue";
import ProdutosView from "../features/loja/ProdutosView.vue";
import ProdutoView from "../features/loja/ProdutoView.vue";
import CarrinhoView from "../features/loja/CarrinhoView.vue";
import CheckoutView from "../features/loja/CheckoutView.vue";
import ReciboView from "../features/loja/ReciboView.vue";

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", name: "home", component: HomeView },
    { path: "/busca", name: "busca", component: BuscaView },
    { path: "/produtos", name: "produtos", component: ProdutosView },
    { path: "/produto/:id", name: "produto", component: ProdutoView },
    { path: "/carrinho", name: "carrinho", component: CarrinhoView },
    { path: "/checkout", name: "checkout", component: CheckoutView },
    { path: "/recibo/:id", name: "recibo", component: ReciboView },
    { path: "/pronta-entrega", redirect: { path: "/produtos", query: { ModalidadeFiltro: "pronta-entrega" } } },
    { path: "/sob-demanda", redirect: { path: "/produtos", query: { ModalidadeFiltro: "sob-demanda" } } },
    { path: "/personalizados", redirect: { path: "/produtos", query: { ModalidadeFiltro: "personalizado" } } },
  ],
});
