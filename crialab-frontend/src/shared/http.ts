export async function obterJson<T>(url: string): Promise<T> {
  const resposta = await fetch(url, { signal: AbortSignal.timeout(8000) });
  if (!resposta.ok) {
    throw new Error("Não foi possível carregar a loja.");
  }
  return (await resposta.json()) as T;
}
