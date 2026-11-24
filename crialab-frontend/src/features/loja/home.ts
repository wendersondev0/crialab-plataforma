export interface IdentidadeHome {
  nome: string;
  site: string;
  cnpj: string;
  email: string;
  telefone: string;
  whatsapp: string;
  instagram: string;
  tiktok: string;
  youtube: string;
  shopee: string;
  mercadoLivre: string;
  endereco: string;
  horario: string;
}

export interface ModalidadeHome {
  id: string;
  nome: string;
  resumo: string;
  explicacao: string;
}

export interface CategoriaHome {
  id: string;
  nome: string;
}

export interface DestaqueHome {
  id: string;
  titulo: string;
  preco: number;
  modalidadeId: string;
  modalidadeNome: string;
  categoriaNome: string;
  foto: string | null;
  maisVendido: boolean;
  prazoTipo: string;
  freteGratis: boolean;
  prazoDias: number;
  prazoInicio: string | null;
  prazoFim: string | null;
}

export interface DepoimentoHome {
  id: string;
  autor: string;
  texto: string;
}

export interface ParceiroHome {
  id: string;
  nome: string;
  logo: string;
  url: string;
}

export interface HomeDados {
  identidade: IdentidadeHome;
  freteGratisAPartir: number;
  intro: string;
  modalidades: ModalidadeHome[];
  categorias: CategoriaHome[];
  destaques: DestaqueHome[];
  depoimentos: DepoimentoHome[];
  parceiros: ParceiroHome[];
}

export function moeda(valor: number): string {
  return valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
}

export function textoPrazo(item: DestaqueHome): string {
  if (item.prazoTipo === "apos-orcamento") {
    return `Prazo após orçamento · cerca de ${item.prazoDias} dias`;
  }
  if (!item.prazoInicio || !item.prazoFim) return "";
  const prefixo = item.freteGratis ? "Entrega GRÁTIS: " : "Entrega: ";
  return prefixo + faixa(item.prazoInicio, item.prazoFim);
}

function faixa(inicio: string, fim: string): string {
  const a = partes(inicio);
  const b = partes(fim);
  if (a.mes === b.mes && a.ano === b.ano) {
    return `${a.dia} a ${b.dia} de ${mes(inicio)}`;
  }
  return `${a.dia} de ${mes(inicio)} a ${b.dia} de ${mes(fim)}`;
}

function partes(iso: string): { ano: number; mes: number; dia: number } {
  const [ano, mes, dia] = iso.split("-").map(Number);
  return { ano, mes, dia };
}

function mes(iso: string): string {
  const p = partes(iso);
  return new Date(p.ano, p.mes - 1, p.dia).toLocaleDateString("pt-BR", { month: "short" });
}

export function caminhoMidia(arquivo: string | null | undefined): string {
  if (!arquivo) return "/marca/mascote.png";
  if (arquivo.startsWith("http") || arquivo.startsWith("/")) return arquivo;
  return `/${arquivo}`;
}
