# Cria Lab DM — estático

Loja estática (HTML, CSS e JavaScript). Dados iniciais em `dados/`; alterações da tela ficam no `localStorage`.

## Local

```bash
npm install
npm run build
python -m http.server 8080
```

Abra `http://localhost:8080`. Não abra o HTML direto do disco se quiser compartilhar carrinho e admin na mesma memória.

## Cloudflare — Workers + Git

| Campo | Valor |
| --- | --- |
| Root directory | `estatico` |
| Build command | `npm run build` |
| Deploy command | `npx wrangler deploy` |

`wrangler.toml` aponta os assets para esta pasta (`.`).

## Scripts

| Script | Uso |
| --- | --- |
| `npm run build` | Gera o CSS Tailwind |
| `npm run watch:css` | CSS em modo watch |
| `npm run deploy` | Build + Wrangler deploy |
| `npm run otimizar-fotos` | Otimiza imagens em `fotos/` |
