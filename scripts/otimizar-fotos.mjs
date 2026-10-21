import fs from "node:fs";
import path from "node:path";
import sharp from "sharp";

const raiz = path.resolve("fotos");
const maxLado = 1600;
const qualidade = 80;

async function listar(dir) {
  const saida = [];
  for (const entrada of fs.readdirSync(dir, { withFileTypes: true })) {
    const cheio = path.join(dir, entrada.name);
    if (entrada.isDirectory()) saida.push(...(await listar(cheio)));
    else if (/\.jpe?g$/i.test(entrada.name)) saida.push(cheio);
  }
  return saida;
}

const arquivos = await listar(raiz);
let alterados = 0;
let erros = 0;
for (const arquivo of arquivos) {
  try {
    const antes = fs.statSync(arquivo).size;
    const buffer = await sharp(arquivo)
      .rotate()
      .resize({ width: maxLado, height: maxLado, fit: "inside", withoutEnlargement: true })
      .jpeg({ quality: qualidade, mozjpeg: true })
      .toBuffer();
    if (buffer.length >= antes && antes <= 20 * 1024 * 1024) continue;
    const temp = arquivo + ".tmp.jpg";
    fs.writeFileSync(temp, buffer);
    fs.unlinkSync(arquivo);
    fs.renameSync(temp, arquivo);
    alterados += 1;
    console.log(
      path.relative(raiz, arquivo),
      (antes / 1024 / 1024).toFixed(1) + "MB ->",
      (buffer.length / 1024 / 1024).toFixed(1) + "MB"
    );
  } catch (erro) {
    erros += 1;
    console.error("falha", path.relative(raiz, arquivo), erro.message);
  }
}
console.log("pronto:", alterados, "de", arquivos.length, "erros:", erros);
