# Cria Lab DM

Loja estática da Cria Lab (HTML, CSS e JavaScript). Dados iniciais em `dados/`; alterações da tela ficam no `localStorage`.

## Local

```bash
npm install
npm run build
python -m http.server 8080
```

Abra `http://localhost:8080`. Não abra o HTML direto do disco se quiser compartilhar carrinho e admin na mesma memória.

## Cloudflare — o que colocar na tela

### Opção A · Workers + Git (a tela com Deploy command)

| Campo | Valor |
| --- | --- |
| Project name | `crialab-plataforma` |
| Build command | `npm run build` |
| Deploy command | `npx wrangler deploy` |
| Preview command | `npx wrangler versions upload` |
| Enable Preview builds | ligado |

O repositório já traz `wrangler.toml` (assets na raiz) e `.assetsignore` (ignora `node_modules`).

### Opção B · Cloudflare Pages

| Campo | Valor |
| --- | --- |
| Framework preset | None |
| Build command | `npm run build` |
| Build output directory | `/` |

## Scripts

| Script | Uso |
| --- | --- |
| `npm run build` | Gera `css/tailwind.css` |
| `npm run watch:css` | Rebuild do CSS ao editar |
| `npm run otimizar-fotos` | Redimensiona JPGs de `fotos/` para caber no limite de 25 MiB do Cloudflare |
| `npm run deploy` | Build + publish no Cloudflare (precisa estar logado no Wrangler) |

Fotos originais de câmera costumam passar de 25 MiB. Rode `npm run otimizar-fotos` antes do primeiro deploy se adicionar imagens novas grandes.

## Marca

Paleta: laranja `#e8772a`, teal `#148f93`, creme `#f6f1e7`, tinta `#1b2430`. Planejamento completo em `PLANEJAMENTO.md`.
