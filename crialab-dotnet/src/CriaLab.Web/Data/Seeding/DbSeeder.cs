using System.Globalization;
using System.Text.Json;
using CriaLab.Web.Constants;
using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Data.Seeding;

public static class DbSeeder
{
    private static DateTime Utc(string valor) =>
        DateTime.SpecifyKind(DateTime.Parse(valor, CultureInfo.InvariantCulture), DateTimeKind.Utc);

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);

        if (await context.LojaIdentidades.AnyAsync())
            return;

        var cliente = await userManager.FindByEmailAsync("cliente@crialab.local");
        SeedCatalogo(context);
        SeedConfiguracao(context);
        SeedMarketing(context);
        await SeedProdutosAsync(context, env);
        SeedOperacoes(context);
        SeedFinanceiro(context);
        SeedOrcamentoInterno(context);
        if (cliente != null)
            SeedAtendimento(context, cliente.Id);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { RoleNames.Usuario, RoleNames.Administrador })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var admin = await userManager.FindByEmailAsync("crialab.dm@gmail.com");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "crialab.dm@gmail.com",
                Email = "crialab.dm@gmail.com",
                EmailConfirmed = true,
                NomeCompleto = "Administrador Cria Lab",
                TelefoneContato = "(27) 99653-0954",
                CriadoEm = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin@CriaLab1!");
            await userManager.AddToRoleAsync(admin, RoleNames.Administrador);
            await userManager.AddToRoleAsync(admin, RoleNames.Usuario);
        }

        var cliente = await userManager.FindByEmailAsync("cliente@crialab.local");
        if (cliente == null)
        {
            cliente = new ApplicationUser
            {
                UserName = "cliente@crialab.local",
                Email = "cliente@crialab.local",
                EmailConfirmed = true,
                NomeCompleto = "Cliente exemplo",
                TelefoneContato = "(27) 99999-0000",
                CriadoEm = DateTime.UtcNow
            };
            await userManager.CreateAsync(cliente, "Cliente@123!");
            await userManager.AddToRoleAsync(cliente, RoleNames.Usuario);
        }
    }

    private static void SeedCatalogo(ApplicationDbContext context)
    {
        context.LojaIdentidades.Add(new LojaIdentidade
        {
            Id = "principal",
            Nome = "Cria Lab DM",
            Site = "https://crialabdm.com.br",
            Cnpj = "68.914.190/0001-96",
            Email = "crialab.dm@gmail.com",
            Telefone = "(27) 99653-0954",
            Whatsapp = "https://wa.me/5527996530954",
            Instagram = "https://www.instagram.com/crialab.dm",
            Tiktok = "https://www.tiktok.com/@crialab.dm",
            Youtube = "https://www.youtube.com/@crialabdm",
            Shopee = "https://shopee.com.br/shop/1648584425",
            MercadoLivre = "https://www.mercadolivre.com.br/"
        });

        context.Categorias.AddRange(
            new Categoria { Id = "decoracao", Nome = "Decoração" },
            new Categoria { Id = "chaveiros", Nome = "Chaveiros" },
            new Categoria { Id = "utilidades", Nome = "Utilidades" },
            new Categoria { Id = "moldes", Nome = "Moldes" },
            new Categoria { Id = "pecas-sob-encomenda", Nome = "Peças sob encomenda" },
            new Categoria { Id = "pintura", Nome = "Pintura" });

        context.Modalidades.AddRange(
            new Modalidade
            {
                Id = "pronta-entrega",
                Nome = "Pronta Entrega",
                Resumo = "Peças já impressas e disponíveis para envio ou retirada.",
                Explicacao = "Escolha, pague e receba sem esperar a impressão. O estoque é o que está na mesa da oficina agora."
            },
            new Modalidade
            {
                Id = "sob-demanda",
                Nome = "Sob Demanda",
                Resumo = "O modelo já está definido; a peça é impressa depois do pedido.",
                Explicacao = "O desenho é o mesmo da vitrine. Depois da compra, a oficina imprime a sua unidade no prazo indicado."
            },
            new Modalidade
            {
                Id = "personalizado",
                Nome = "Personalizados",
                Resumo = "Feitos sob encomenda com as alterações que você pedir.",
                Explicacao = "Você envia referência e detalhes. O preço do card é uma média; o valor final sai no orçamento antes de produzir."
            });

        context.MenusAjuda.AddRange(
            new MenuAjuda { Id = "inicio", Texto = "A home mostra o carrossel, os atalhos das três modalidades e a grade de peças. Clique no card para abrir a ficha completa." },
            new MenuAjuda { Id = "busca", Texto = "Filtre por texto, categoria temática, preço e ordenação. O link da busca pode ser compartilhado com os mesmos filtros." },
            new MenuAjuda { Id = "carrinho", Texto = "Revise quantidades e remova itens. O cupom e o frete entram no fechamento do pedido." },
            new MenuAjuda { Id = "checkout", Texto = "Informe CEP, escolha PAC ou SEDEX, use cupom se tiver e confirme o pagamento simulado." },
            new MenuAjuda { Id = "conta", Texto = "Pedidos, orçamentos, favoritos e dados do cliente." },
            new MenuAjuda { Id = "contato", Texto = "Envie dúvida, sugestão, elogio, reclamação ou orçamento. A mensagem aparece na lista do admin." },
            new MenuAjuda { Id = "sobre", Texto = "Somos uma oficina de impressão 3D: peças prontas, sob demanda e personalizadas do seu jeito." },
            new MenuAjuda { Id = "produto", Texto = "Na ficha você escolhe cor e tamanho, calcula frete, abre Shopee ou Mercado Livre e compra ou pede orçamento." });
    }

    private static void SeedConfiguracao(ApplicationDbContext context)
    {
        context.ConfiguracoesLoja.Add(new ConfiguracaoLoja
        {
            Id = "default",
            FreteGratisAPartir = 200,
            DescontoAVista = 0.1m,
            Parcelas = 3,
            RepassarFrete = true,
            TaxaFalha = 0.05m,
            FreteCanal = 0,
            PrecoKwhAtual = 1.1193m
        });

        context.Cupons.Add(new Cupom { Id = "cup-oficina10", Codigo = "OFICINA10", Percentual = 0.1m, Ativo = true });

        context.FaixasCep.AddRange(
            new FaixaCep { De = 1000, Ate = 19999, Pac = 28.9m, PacDias = 8, Sedex = 46.5m, SedexDias = 3 },
            new FaixaCep { De = 20000, Ate = 28999, Pac = 24.5m, PacDias = 6, Sedex = 39.9m, SedexDias = 2 },
            new FaixaCep { De = 29000, Ate = 29999, Pac = 18.9m, PacDias = 5, Sedex = 32.4m, SedexDias = 2 },
            new FaixaCep { De = 30000, Ate = 39999, Pac = 22.4m, PacDias = 7, Sedex = 36.8m, SedexDias = 3 },
            new FaixaCep { De = 40000, Ate = 69999, Pac = 31.2m, PacDias = 9, Sedex = 49.7m, SedexDias = 4 },
            new FaixaCep { De = 70000, Ate = 79999, Pac = 34.8m, PacDias = 8, Sedex = 55.3m, SedexDias = 3 },
            new FaixaCep { De = 80000, Ate = 89999, Pac = 33.1m, PacDias = 8, Sedex = 52.6m, SedexDias = 3 },
            new FaixaCep { De = 90000, Ate = 99999, Pac = 36.4m, PacDias = 9, Sedex = 58.9m, SedexDias = 4 });

        var tipos = new[]
        {
            "PLA", "PETG", "TPU", "NFC", "COPO", "VERNIZ", "LAMPADA_LED", "Argola", "IMA",
            "CAIXA P", "CAIXA M", "CAIXA G", "SACOLA KRAFT P", "SACOLA KRAFT G",
            "PAPEL_FOTOGRAFICO", "BARBANTE", "CARTAO", "FITA", "PLASTICO_BOLHA", "SACO_ZIP",
            "COLA", "SILICA", "SOQUETE_LED", "IMA_10"
        };
        for (var i = 0; i < tipos.Length; i++)
            context.TiposInsumoConfig.Add(new TipoInsumoConfig { Nome = tipos[i], Ordem = i });
    }

    private static void SeedMarketing(ApplicationDbContext context)
    {
        context.Depoimentos.AddRange(
            new Depoimento { Id = "dep-1", Autor = "Mileni", Texto = "Os moldes chegaram no tamanho que a gente combinou.", PodeAparecer = true },
            new Depoimento { Id = "dep-2", Autor = "Oficina vizinha", Texto = "Texto guardado, ainda sem publicação.", PodeAparecer = false });

        context.Parceiros.AddRange(
            new Parceiro { Id = "par-1", Nome = "Mercearia Vila Verde", Logo = "marca/parceiro-vila-verde.png", PodeAparecer = true },
            new Parceiro { Id = "par-2", Nome = "GoFit", Logo = "marca/parceiro-gofit.png", PodeAparecer = true });
    }

    private static async Task SeedProdutosAsync(ApplicationDbContext context, IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "Seeding", "seed-produtos.json");
        if (!File.Exists(path))
            return;

        var json = await File.ReadAllTextAsync(path);
        using var doc = JsonDocument.Parse(json);
        foreach (var element in doc.RootElement.EnumerateArray())
            context.Produtos.Add(MapProduto(element));
    }

    private static Produto MapProduto(JsonElement element)
    {
        var id = element.GetProperty("id").GetString() ?? Guid.NewGuid().ToString("N");
        var modalidade = element.TryGetProperty("modalidade", out var modEl) ? modEl.GetString() ?? "pronta-entrega" : "pronta-entrega";
        if (string.IsNullOrWhiteSpace(modalidade))
        {
            if (element.TryGetProperty("tipo", out var tipoEl) && tipoEl.GetString() == "personalizado")
                modalidade = "personalizado";
            else if (element.GetProperty("estoque").GetInt32() == 0)
                modalidade = "sob-demanda";
            else
                modalidade = "pronta-entrega";
        }

        DateOnly criadoEm = DateOnly.FromDateTime(DateTime.UtcNow);
        if (element.TryGetProperty("criadoEm", out var criadoEl))
            DateOnly.TryParse(criadoEl.GetString(), out criadoEm);

        var produto = new Produto
        {
            Id = id,
            Titulo = element.GetProperty("titulo").GetString() ?? "",
            Sessao = element.TryGetProperty("sessao", out var sessaoEl) ? sessaoEl.GetString() ?? "" : "",
            CategoriaId = element.GetProperty("categoria").GetString() ?? "decoracao",
            ModalidadeId = modalidade,
            Tipo = element.TryGetProperty("tipo", out var t) ? t.GetString() ?? "pronto" : "pronto",
            Descricao = element.TryGetProperty("descricao", out var d) ? d.GetString() ?? "" : "",
            Preco = element.GetProperty("preco").GetDecimal(),
            PrecoComparativo = element.TryGetProperty("precoComparativo", out var pc) ? pc.GetDecimal() : 0,
            PrecoArquivo = element.TryGetProperty("precoArquivo", out var pa) ? pa.GetDecimal() : 0,
            Estoque = element.TryGetProperty("estoque", out var e) ? e.GetInt32() : 0,
            Minimo = element.TryGetProperty("minimo", out var m) ? m.GetInt32() : 0,
            Medidas = element.TryGetProperty("medidas", out var med) ? med.GetString() ?? "" : "",
            Peso = element.TryGetProperty("peso", out var p) ? p.GetDecimal() : 0,
            Material = element.TryGetProperty("material", out var mat) ? mat.GetString() ?? "" : "",
            PrazoDias = element.TryGetProperty("prazoDias", out var pr) ? pr.GetInt32() : 5,
            ComArquivo = element.TryGetProperty("comArquivo", out var ca) && ca.GetBoolean(),
            Personalizavel = element.TryGetProperty("personalizavel", out var pe) && pe.GetBoolean(),
            Vendas = element.TryGetProperty("vendas", out var v) ? v.GetInt32() : 0,
            Nota = element.TryGetProperty("nota", out var n) ? n.GetDecimal() : 0,
            Oferta = element.TryGetProperty("oferta", out var o) && o.GetBoolean(),
            MaisVendido = element.TryGetProperty("maisVendido", out var mv) && mv.GetBoolean(),
            Situacao = element.TryGetProperty("situacao", out var s) ? s.GetString() ?? "publicado" : "publicado",
            AVista = !element.TryGetProperty("aVista", out var av) || av.GetBoolean(),
            CriadoEm = criadoEm
        };

        if (element.TryGetProperty("fotos", out var fotosEl) && fotosEl.ValueKind == JsonValueKind.Array)
        {
            var ordem = 0;
            foreach (var foto in fotosEl.EnumerateArray())
            {
                produto.Fotos.Add(new ProdutoFoto
                {
                    Arquivo = foto.TryGetProperty("arquivo", out var ar) ? ar.GetString() ?? "" : "",
                    Sessao = foto.TryGetProperty("sessao", out var se) ? se.GetString() ?? "" : "",
                    Legenda = foto.TryGetProperty("legenda", out var le) ? le.GetString() ?? "" : "",
                    Ordem = ordem++
                });
            }
        }

        if (element.TryGetProperty("etapas", out var etapasEl) && etapasEl.ValueKind == JsonValueKind.Array)
        {
            var ordemEtapa = 0;
            foreach (var etapa in etapasEl.EnumerateArray())
            {
                var etapaLocal = etapa.TryGetProperty("id", out var eid) ? eid.GetString() : null;
                if (string.IsNullOrWhiteSpace(etapaLocal))
                    etapaLocal = "etapa-" + ordemEtapa;
                produto.EtapasPersonalizacao.Add(new ProdutoEtapaPersonalizacao
                {
                    Id = id + "-" + etapaLocal,
                    Nome = etapa.TryGetProperty("nome", out var en) ? en.GetString() ?? "" : "",
                    Tipo = etapa.TryGetProperty("tipo", out var et) ? et.GetString() ?? "texto" : "texto",
                    Acresce = etapa.TryGetProperty("acresce", out var ac) ? ac.GetDecimal() : 0
                });
                ordemEtapa++;
            }
        }

        var cores = new List<(string Nome, string Hex)>();
        if (element.TryGetProperty("cores", out var coresEl) && coresEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var cor in coresEl.EnumerateArray())
            {
                var nome = cor.TryGetProperty("nome", out var cn) ? cn.GetString() ?? "" : "";
                var hex = cor.TryGetProperty("hex", out var ch) ? ch.GetString() ?? "#cccccc" : "#cccccc";
                if (!string.IsNullOrWhiteSpace(nome))
                {
                    produto.CoresLegado.Add(new ProdutoCorLegado { Nome = nome, Hex = hex });
                    cores.Add((nome, hex));
                }
            }
        }

        var tamanhos = new List<string>();
        if (element.TryGetProperty("tamanhos", out var tamEl) && tamEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var tam in tamEl.EnumerateArray())
            {
                var nome = tam.GetString() ?? "";
                if (!string.IsNullOrWhiteSpace(nome))
                {
                    produto.TamanhosLegado.Add(new ProdutoTamanhoLegado { Nome = nome });
                    tamanhos.Add(nome);
                }
            }
        }

        if (element.TryGetProperty("variacoes", out var varEl) && varEl.ValueKind == JsonValueKind.Array && varEl.GetArrayLength() > 0)
        {
            var ordemEixo = 0;
            foreach (var eixo in varEl.EnumerateArray())
            {
                var eixoLocal = eixo.TryGetProperty("id", out var veid) ? veid.GetString() : null;
                if (string.IsNullOrWhiteSpace(eixoLocal))
                    eixoLocal = "var-" + ordemEixo;
                var eixoId = id + "-" + eixoLocal;
                var eixoEntity = new VariacaoEixo
                {
                    Id = eixoId,
                    Nome = eixo.TryGetProperty("nome", out var vn) ? vn.GetString() ?? "Variação" : "Variação",
                    Tipo = eixo.TryGetProperty("tipo", out var vt) && vt.GetString() == "cor" ? "cor" : "texto",
                    Ordem = ordemEixo++
                };
                var ordemOpc = 0;
                if (eixo.TryGetProperty("opcoes", out var opcoesEl))
                {
                    foreach (var opc in opcoesEl.EnumerateArray())
                    {
                        var opcLocal = opc.TryGetProperty("id", out var oid) ? oid.GetString() : null;
                        if (string.IsNullOrWhiteSpace(opcLocal))
                            opcLocal = "opc-" + ordemOpc;
                        eixoEntity.Opcoes.Add(new VariacaoOpcao
                        {
                            Id = eixoId + "-" + opcLocal,
                            Nome = opc.TryGetProperty("nome", out var on) ? on.GetString() ?? "" : "",
                            Hex = opc.TryGetProperty("hex", out var oh) ? oh.GetString() ?? "#cccccc" : "#cccccc",
                            Ordem = ordemOpc++
                        });
                    }
                }
                produto.Variacoes.Add(eixoEntity);
            }
        }
        else
        {
            if (cores.Count > 0)
            {
                var eixoCor = new VariacaoEixo { Id = $"var-cor-{id}", Nome = "Cor", Tipo = "cor", Ordem = 0 };
                var i = 0;
                foreach (var (nome, hex) in cores)
                {
                    eixoCor.Opcoes.Add(new VariacaoOpcao { Id = $"cor-{id}-{i}", Nome = nome, Hex = hex, Ordem = i });
                    i++;
                }
                produto.Variacoes.Add(eixoCor);
            }

            var unico = tamanhos.Count == 1 && string.Equals(tamanhos[0], "único", StringComparison.OrdinalIgnoreCase);
            if (tamanhos.Count > 0 && !unico)
            {
                var eixoTam = new VariacaoEixo { Id = $"var-tam-{id}", Nome = "Tamanho", Tipo = "texto", Ordem = 1 };
                for (var i = 0; i < tamanhos.Count; i++)
                    eixoTam.Opcoes.Add(new VariacaoOpcao { Id = $"tam-{id}-{i}", Nome = tamanhos[i], Ordem = i });
                produto.Variacoes.Add(eixoTam);
            }
        }

        return produto;
    }

    private static void SeedOperacoes(ApplicationDbContext context)
    {
        context.Maquinas.AddRange(
            new Maquina
            {
                Id = "maq-bambu",
                Nome = "Bambu Lab A1 com AMS Lite",
                Categoria = "impressora",
                Imagem = "marca/patrimonio/pat-bambu-a1.jpg",
                Watts = 150,
                DepreciacaoPorMinuto = 0.02m,
                ManutencaoPorMinuto = 0.01m,
                EnergiaPorMinuto = 0.005m,
                ControlaHora = true,
                CompraCompetencia = "2026-03",
                AdquiridoEm = DateOnly.Parse("2026-03-14"),
                CompraValor = 4437.66m,
                Marca = "Bambu Lab",
                Modelo = "A1 + AMS Lite",
                Serial = "A1-AMS-001",
                Local = "Mesa principal",
                Status = "ativa",
                Notas = "Impressora principal com troca automática de cores."
            },
            new Maquina
            {
                Id = "maq-kobra",
                Nome = "Anycubic Kobra S1",
                Categoria = "impressora",
                Imagem = "marca/patrimonio/pat-kobra-s1.jpg",
                Watts = 200,
                DepreciacaoPorMinuto = 0.02m,
                ManutencaoPorMinuto = 0.01m,
                EnergiaPorMinuto = 0.005m,
                ControlaHora = true,
                CompraCompetencia = "2026-07",
                AdquiridoEm = DateOnly.Parse("2026-07-08"),
                CompraValor = 5359.93m,
                Marca = "Anycubic",
                Modelo = "Kobra S1",
                Serial = "KS1-002",
                Local = "Mesa secundária",
                Status = "ativa",
                Notas = "Câmara fechada para PETG e peças maiores."
            });

        var insPla = new EstoqueInsumo
        {
            Id = "ins-pla",
            Nome = "PLA",
            Unidade = "kg",
            Saldo = 20.3m,
            Minimo = 2,
            Categoria = "filamento",
            Imagem = "marca/patrimonio/pat-pla.jpg",
            AdquiridoEm = DateOnly.Parse("2026-03-01"),
            Fornecedor = "ELEGOO / F3D / Voolt3D",
            OrigemPlanilha = "Despesas e Receitas · filamentos mar–set/2026"
        };
        insPla.Cores.Add(new InsumoCor { Id = "pla-preto", Nome = "Preto", Hex = "#1b2430", Saldo = 1.6m, Bobinas = 2 });
        insPla.Cores.Add(new InsumoCor { Id = "pla-branco", Nome = "Branco", Hex = "#f4efe6", Saldo = 1.8m, Bobinas = 2 });
        context.EstoqueInsumos.Add(insPla);

        context.ImpressaoJobs.AddRange(
            new ImpressaoJob
            {
                Id = "imp-1",
                MaquinaId = "maq-bambu",
                ProdutoId = "sessao-2026-03-14",
                Titulo = "Peça da oficina — 14/03/2026",
                PesoPrevisto = 40,
                Kwh = 0.4m,
                Inicio = Utc("2026-10-19T09:00"),
                Fim = Utc("2026-10-19T12:00"),
                Status = "imprimindo"
            },
            new ImpressaoJob
            {
                Id = "imp-4",
                MaquinaId = "maq-bambu",
                ProdutoId = "sessao-2026-03-14",
                Titulo = "Peça da oficina — 14/03/2026",
                PesoPrevisto = 40,
                PesoReal = 42,
                Sucesso = true,
                Kwh = 0.35m,
                Inicio = Utc("2026-10-10T08:00"),
                Fim = Utc("2026-10-10T11:00"),
                Status = "concluida"
            });
    }

    private static void SeedFinanceiro(ApplicationDbContext context)
    {
        context.ContasFinanceiras.AddRange(
            new ContaFinanceira { Id = "conta-1", Tipo = "pagar", Descricao = "Conta de energia de outubro", Valor = 80, Vencimento = DateOnly.Parse("2026-10-25") },
            new ContaFinanceira { Id = "conta-2", Tipo = "receber", Descricao = "Molde combinado com a cliente", Valor = 150, Vencimento = DateOnly.Parse("2026-10-22") });

        context.LancamentosFinanceiros.AddRange(
            new LancamentoFinanceiro { Id = "lan-0001", Data = DateOnly.Parse("2026-09-02"), Competencia = "2026-09", Descricao = "Magno Carimbo de Biscoito com cortador", Categoria = "receita", Quantidade = 1, Valor = 15, Origem = "planilha" },
            new LancamentoFinanceiro { Id = "lan-0002", Data = DateOnly.Parse("2026-09-10"), Competencia = "2026-09", Descricao = "Mileni Parte dos Moldes", Categoria = "receita", Quantidade = 1, Valor = 150, Origem = "planilha" });

        context.ParametrosCusto.AddRange(
            new ParametroCusto { Id = "par-pla", Nome = "PLA", Unidade = "kg", Preco = 85, Travado = true },
            new ParametroCusto { Id = "par-petg", Nome = "PETG", Unidade = "kg", Preco = 65, Travado = true },
            new ParametroCusto { Id = "par-tpu", Nome = "TPU", Unidade = "kg", Preco = 80, Travado = true });

        context.MargensCanal.AddRange(
            new MargemCanal { Id = "marketplace", Nome = "Marketplace", Percentual = 0.25m, Ativo = true },
            new MargemCanal { Id = "simples", Nome = "Simples", Percentual = 0.4m, Ativo = true },
            new MargemCanal { Id = "medio", Nome = "Médio", Percentual = 0.5m, Ativo = true },
            new MargemCanal { Id = "complexo", Nome = "Complexo", Percentual = 0.6m, Ativo = true });

        context.DescontosOrcamento.Add(new DescontoOrcamento { Id = "default", Recorrente = 0.05m, Volume = 0.05m });

        context.ReferenciasMaquina.Add(new ReferenciaMaquina { Id = "default", EnergiaPorMinuto = 0.005m, Depreciacao = 0.02m, Manutencao = 0.01m });

        context.NiveisPintura.AddRange(
            new NivelPintura { Id = "simples", Nome = "Simples", Percentual = 0.2m, Tempo = "cerca de 30 min", Cores = "até 2", Acabamento = "cor chapada" },
            new NivelPintura { Id = "medio", Nome = "Médio", Percentual = 0.4m, Tempo = "1 h a 2 h", Cores = "3 a 5", Acabamento = "sombreamento leve, máscara, lixa e primer" },
            new NivelPintura { Id = "complexo", Nome = "Complexo", Percentual = 0.6m, Tempo = "mais de 2 h", Cores = "mais de 5", Acabamento = "degradê, textura, aerógrafo, montagem" });

        context.FaixasMarketplace.AddRange(
            new FaixaMarketplace { Canal = "Shopee", De = 0, Ate = 79.99m, Percentual = 0.2m, Fixo = 4.5m },
            new FaixaMarketplace { Canal = "Shopee", De = 80, Ate = 99.99m, Percentual = 0.14m, Fixo = 16 },
            new FaixaMarketplace { Canal = "Shopee", De = 100, Ate = 199.99m, Percentual = 0.14m, Fixo = 20 },
            new FaixaMarketplace { Canal = "Shopee", De = 200, Ate = 499.99m, Percentual = 0.14m, Fixo = 26 },
            new FaixaMarketplace { Canal = "Shopee", De = 500, Ate = 999999, Percentual = 0.14m, Fixo = 26 });
    }

    private static void SeedOrcamentoInterno(ApplicationDbContext context)
    {
        var orc = new OrcamentoInterno
        {
            Id = "orc-cachorro",
            Nome = "Cachorro branco",
            Material = "PLA",
            Minutos = 367,
            PesoG = 41.66m,
            MargemSite = 0.4m,
            PrecoKg = 85,
            PrecoKgOrigem = "travado",
            EnergiaPorMinuto = 0.005m,
            Depreciacao = 0.02m,
            Manutencao = 0.01m,
            TaxaFalha = 0.05m,
            Enviado = true,
            MaquinaId = "maq-bambu"
        };
        orc.Itens.Add(new OrcamentoItem { Nome = "Saco kraft pequeno", Quantidade = 0.5m, Preco = 0.5m });
        context.OrcamentosInternos.Add(orc);
    }

    private static void SeedAtendimento(ApplicationDbContext context, string adminUserId)
    {
        context.MensagensContato.Add(new MensagemContato
        {
            Id = "msg-1",
            Tipo = "elogio",
            Nome = "Cliente exemplo",
            Email = "cliente@crialab.local",
            Texto = "Gostei da peça que chegou.",
            Status = "aberto",
            CriadoEm = Utc("2026-10-08T10:00")
        });

        context.EnderecosCliente.Add(new EnderecoCliente
        {
            Id = "end-1",
            UsuarioId = adminUserId,
            Rotulo = "Casa",
            Cep = "29010000",
            Linha = "Rua das Impressoras, 10, Vitória/ES"
        });

        context.Pedidos.Add(new Pedido
        {
            Id = "ped-entregue",
            UsuarioId = adminUserId,
            Status = "entregue",
            CriadoEm = Utc("2026-09-20T15:00"),
            Subtotal = 89,
            DescontoAVista = 8.9m,
            Total = 80.1m,
            Forma = "pix",
            Endereco = "Rua das Impressoras, 10, Vitória/ES",
            Cep = "29010000",
            FreteNome = "Frete grátis",
            RepassarFrete = true,
            Nota = "pendente",
            Itens =
            {
                new PedidoItem
                {
                    Id = "item-ent-1",
                    ProdutoId = "sessao-2026-03-14",
                    Titulo = "Peça da oficina — 14/03/2026",
                    Tipo = "pronto",
                    Fisico = true,
                    Quantidade = 1,
                    PrecoUnitario = 89
                }
            },
            Os = new PedidoOs { Status = "concluida", InsumoBaixado = true }
        });

        context.OrcamentosCliente.AddRange(
            new OrcamentoCliente
            {
                Id = "orc-cli-1",
                UsuarioId = adminUserId,
                ProdutoId = "sessao-2026-04-14",
                Titulo = "Peça da oficina — 14/04/2026",
                Status = "respondido",
                Detalhes = "Medida 8 cm, cor teal, texto Cria",
                ArquivoNome = "referencia.png",
                Valor = 180,
                CriadoEm = Utc("2026-10-02T11:00")
            },
            new OrcamentoCliente
            {
                Id = "orc-cli-2",
                UsuarioId = adminUserId,
                ProdutoId = "sessao-2026-03-27",
                Titulo = "Peça da oficina — 27/03/2026",
                Status = "aguardando",
                Detalhes = "Texto gravado ainda sem preço",
                CriadoEm = Utc("2026-10-10T09:30")
            });

        context.Avaliacoes.Add(new Avaliacao
        {
            Id = "ava-1",
            ProdutoId = "sessao-2026-03-14",
            PedidoId = "ped-entregue",
            Autor = "Cliente exemplo",
            Nota = 5,
            Texto = "A peça chegou bem feita e na cor combinada.",
            FotoDoProduto = true
        });

        context.PerguntasProduto.Add(new PerguntaProduto
        {
            Id = "per-1",
            ProdutoId = "sessao-2026-03-14",
            Autor = "Ana",
            Texto = "Essa peça pode ser feita em PETG?",
            CriadoEm = Utc("2026-10-01T14:00")
        });
    }
}
