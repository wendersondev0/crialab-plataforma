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
        await SeedCoreAsync(scope.ServiceProvider, forcar: false);
    }

    public static async Task RestaurarDemoAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment())
            throw new InvalidOperationException("Restauração da carga inicial só é permitida em Development.");

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await SeedCoreAsync(scope.ServiceProvider, forcar: true);
    }

    private static async Task SeedCoreAsync(IServiceProvider services, bool forcar)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var env = services.GetRequiredService<IWebHostEnvironment>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);

        if (!forcar && await context.LojaIdentidades.AnyAsync())
        {
            await SeedUsersAsync(userManager);
            await EnsureInsumosPlanilhaAsync(context);
            return;
        }

        var clientes = await SeedUsersAsync(userManager);
        SeedCatalogo(context);
        SeedConfiguracao(context);
        SeedMarketing(context);
        await SeedProdutosAsync(context, env);
        SeedOperacoes(context);
        SeedFinanceiro(context, env);
        SeedOrcamentoInterno(context, env);
        SeedAtendimento(context, clientes);
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

    private static async Task<List<ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        const string senhaAdmin = "@123Mudar";
        const string senhaTeste = "Teste@123!";
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
            await userManager.CreateAsync(admin, senhaAdmin);
            await userManager.AddToRoleAsync(admin, RoleNames.Administrador);
            await userManager.AddToRoleAsync(admin, RoleNames.Usuario);
        }
        else
        {
            await userManager.RemovePasswordAsync(admin);
            await userManager.AddPasswordAsync(admin, senhaAdmin);
            if (!await userManager.IsInRoleAsync(admin, RoleNames.Administrador))
                await userManager.AddToRoleAsync(admin, RoleNames.Administrador);
            if (!await userManager.IsInRoleAsync(admin, RoleNames.Usuario))
                await userManager.AddToRoleAsync(admin, RoleNames.Usuario);
        }

        var clientesDef = new (string Email, string Nome, string Tel)[]
        {
            ("cliente@crialab.local", "Cliente exemplo", "(27) 99999-0000"),
            ("ana.silva@teste.crialab.local", "Ana Silva", "(27) 98811-1001"),
            ("bruno.costa@teste.crialab.local", "Bruno Costa", "(27) 98822-2002"),
            ("camila.souza@teste.crialab.local", "Camila Souza", "(27) 98833-3003"),
            ("diego.ferreira@teste.crialab.local", "Diego Ferreira", "(27) 98844-4004"),
            ("elena.ramos@teste.crialab.local", "Elena Ramos", "(27) 98855-5005")
        };

        var clientes = new List<ApplicationUser>();
        foreach (var (email, nome, tel) in clientesDef)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    NomeCompleto = nome,
                    TelefoneContato = tel,
                    CriadoEm = DateTime.UtcNow
                };
                var senha = email == "cliente@crialab.local" ? "Cliente@123!" : senhaTeste;
                await userManager.CreateAsync(user, senha);
                await userManager.AddToRoleAsync(user, RoleNames.Usuario);
            }
            else if (!await userManager.IsInRoleAsync(user, RoleNames.Usuario))
            {
                await userManager.AddToRoleAsync(user, RoleNames.Usuario);
            }
            clientes.Add(user);
        }

        return clientes;
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

        context.EstoqueInsumos.AddRange(CriarInsumosPlanilha());

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

    private static async Task EnsureInsumosPlanilhaAsync(ApplicationDbContext context)
    {
        var existentes = await context.EstoqueInsumos.AsNoTracking().Select(x => x.Id).ToListAsync();
        var set = existentes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var faltando = CriarInsumosPlanilha().Where(x => !set.Contains(x.Id)).ToList();
        if (faltando.Count == 0) return;
        context.EstoqueInsumos.AddRange(faltando);
        await context.SaveChangesAsync();
    }

    private static List<EstoqueInsumo> CriarInsumosPlanilha()
    {
        var lista = new List<EstoqueInsumo>();

        EstoqueInsumo Ins(
            string id, string nome, string unidade, decimal saldo, decimal minimo, string categoria,
            string imagem, string adquirido, string fornecedor, string origem,
            Action<EstoqueInsumo>? extra = null)
        {
            var item = new EstoqueInsumo
            {
                Id = id,
                Nome = nome,
                Unidade = unidade,
                Saldo = saldo,
                Minimo = minimo,
                Categoria = categoria,
                Imagem = imagem,
                AdquiridoEm = DateOnly.Parse(adquirido),
                Fornecedor = fornecedor,
                OrigemPlanilha = origem
            };
            extra?.Invoke(item);
            if (item.Cores.Count > 0)
                item.Saldo = item.Cores.Sum(c => c.Saldo);
            if (item.Variantes.Count > 0)
                item.Saldo = item.Variantes.Sum(v => v.Saldo);
            lista.Add(item);
            return item;
        }

        Ins("ins-pla", "PLA", "kg", 20.3m, 2, "filamento", "marca/patrimonio/pat-pla.jpg", "2026-03-01", "ELEGOO / F3D / Voolt3D", "Despesas e Receitas · filamentos mar–set/2026", i =>
        {
            i.Cores.Add(new InsumoCor { Id = "pla-preto", Nome = "Preto", Hex = "#1b2430", Saldo = 1.6m, Bobinas = 2 });
            i.Cores.Add(new InsumoCor { Id = "pla-branco", Nome = "Branco", Hex = "#f4efe6", Saldo = 1.8m, Bobinas = 2 });
            i.Cores.Add(new InsumoCor { Id = "pla-bronze", Nome = "Bronze", Hex = "#b87333", Saldo = 0.8m, Bobinas = 1 });
            i.Cores.Add(new InsumoCor { Id = "pla-verde", Nome = "Verde", Hex = "#2f9e44", Saldo = 1.2m, Bobinas = 2 });
            i.Cores.Add(new InsumoCor { Id = "pla-azul", Nome = "Azul", Hex = "#2563eb", Saldo = 1.1m, Bobinas = 1 });
            i.Cores.Add(new InsumoCor { Id = "pla-amarelo", Nome = "Amarelo", Hex = "#eab308", Saldo = 0.9m, Bobinas = 1 });
            i.Cores.Add(new InsumoCor { Id = "pla-cinza", Nome = "Cinza / Marble", Hex = "#94a3b8", Saldo = 0.7m, Bobinas = 1 });
        });

        Ins("ins-petg", "PETG", "kg", 14.5m, 2, "filamento", "marca/patrimonio/pat-petg.jpg", "2026-07-05", "Masterprint / 3d Print Pulse", "Despesas e Receitas · PETG jul–set/2026", i =>
        {
            i.Cores.Add(new InsumoCor { Id = "petg-branco", Nome = "Branco", Hex = "#f4efe6", Saldo = 2.5m, Bobinas = 3 });
            i.Cores.Add(new InsumoCor { Id = "petg-preto", Nome = "Preto", Hex = "#1b2430", Saldo = 3.2m, Bobinas = 4 });
            i.Cores.Add(new InsumoCor { Id = "petg-verde", Nome = "Verde", Hex = "#2f9e44", Saldo = 1.1m, Bobinas = 1 });
            i.Cores.Add(new InsumoCor { Id = "petg-gold", Nome = "Gold / Dourado", Hex = "#d4a017", Saldo = 1.8m, Bobinas = 2 });
            i.Cores.Add(new InsumoCor { Id = "petg-azul", Nome = "Azul", Hex = "#2563eb", Saldo = 1.0m, Bobinas = 1 });
            i.Cores.Add(new InsumoCor { Id = "petg-prata", Nome = "Prata", Hex = "#94a3b8", Saldo = 0.8m, Bobinas = 1 });
            i.Cores.Add(new InsumoCor { Id = "petg-vermelho", Nome = "Vermelho", Hex = "#dc2626", Saldo = 0.9m, Bobinas = 1 });
        });

        Ins("ins-tpu", "TPU 95A", "kg", 0.7m, 0.5m, "filamento", "marca/patrimonio/pat-tpu.jpg", "2026-06-10", "ELEGOO", "Despesas e Receitas · junho/2026", i =>
        {
            i.Cores.Add(new InsumoCor { Id = "tpu-preto", Nome = "Preto flex", Hex = "#111827", Saldo = 0.7m, Bobinas = 1 });
        });

        Ins("ins-nfc", "Tags NFC", "un", 42, 10, "eletronico", "marca/patrimonio/pat-nfc.jpg", "2026-09-01", "Mercado Livre", "kit 50 etiquetas · set/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "nfc-padrao", Nome = "Tag redonda", Saldo = 42, Unidade = "un" });
        });

        Ins("ins-lampada-led", "Lâmpada LED", "un", 5, 2, "eletronico", "marca/patrimonio/pat-led.jpg", "2026-09-17", "Shopee", "kit 5 peças + LED smart · set/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "led-kit", Nome = "LED para luminária", Saldo = 4, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "led-smart", Nome = "LED smart", Saldo = 1, Unidade = "un" });
        });

        Ins("ins-soquete", "Soquete flexível para lâmpada", "un", 1, 1, "eletronico", "marca/patrimonio/pat-led.jpg", "2026-06-15", "Shopee", "junho/2026");

        Ins("ins-copo", "Copo de alumínio 500 ml", "un", 8, 4, "acabamento", "marca/patrimonio/pat-copo.jpg", "2026-09-13", "Shopee", "10 un · set/2026");

        Ins("ins-argola", "Argola / elo de chaveiro", "un", 220, 40, "ferragem", "marca/patrimonio/pat-argola.jpg", "2026-03-15", "Mercado Livre / Shopee", "100 argolas mar/2026 + kit 150 jun/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "arg-simples", Nome = "Argola com corrente", Saldo = 80, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "arg-kit", Nome = "Kit prata + elo + lagosta", Saldo = 140, Unidade = "un" });
        });

        Ins("ins-ima-8", "Ímã neodímio 8×2 mm", "un", 180, 30, "ferragem", "marca/patrimonio/pat-ima.jpg", "2026-03-15", "Mercado Livre", "200 un mar/2026 + 50 un ago/2026");
        Ins("ins-ima-10", "Ímã neodímio 10×2 mm", "un", 40, 20, "ferragem", "marca/patrimonio/pat-ima.jpg", "2026-07-24", "Mercado Livre", "100 un jul/2026");

        Ins("ins-verniz", "Verniz / spray", "un", 6, 2, "acabamento", "marca/patrimonio/pat-pla.jpg", "2026-04-20", "Shopee / ML", "spray brilhante, fosco e Acrilex · abr–set/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "ver-bril", Nome = "Spray brilhante", Saldo = 3, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "ver-fosco", Nome = "Spray / potes fosco", Saldo = 3, Unidade = "un" });
        });

        Ins("ins-cola", "Cola", "un", 10, 3, "acabamento", "marca/patrimonio/pat-caixa.jpg", "2026-03-10", "Shopee / ML", "instantânea, universal e bastão · mar–jul/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "cola-inst", Nome = "Cola instantânea", Saldo = 4, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "cola-univ", Nome = "Kit universal + ativador", Saldo = 1, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "cola-bastao", Nome = "Bastão", Saldo = 5, Unidade = "un" });
        });

        Ins("ins-caixa-p", "Caixa P", "un", 38, 10, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-08-10", "Mercado Livre", "50 caixas 16×11×6 · ago/2026");
        Ins("ins-caixa-m", "Caixa M", "un", 22, 8, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-08-25", "Shopee", "25 caixas 19×12×12 · ago/2026");
        Ins("ins-caixa-g", "Caixa G", "un", 20, 8, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-08-25", "Shopee", "25 caixas 20×20×10 · ago/2026");
        Ins("ins-sacola-p", "Sacola kraft P", "un", 40, 15, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-07-18", "Shopee", "50 sacolas kraft P · jul/2026");
        Ins("ins-sacola-g", "Sacola kraft G", "un", 42, 15, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-08-12", "Shopee", "50 unidades GG1 34×31×18 · ago/2026");
        Ins("ins-saco-zip", "Saco zip holográfico", "un", 80, 20, "embalagem", "marca/patrimonio/pat-bolha.jpg", "2026-09-09", "Shopee", "100 un 9×16 · set/2026");
        Ins("ins-plastico-bolha", "Plástico bolha", "m", 12, 3, "embalagem", "marca/patrimonio/pat-bolha.jpg", "2026-08-26", "Shopee", "30cm × 10m · ago/2026");
        Ins("ins-fita", "Fita adesiva de embalagem", "un", 4, 2, "embalagem", "marca/patrimonio/pat-fita.jpg", "2026-07-01", "Oficina", "Fechamento de caixas e sacolas", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "fita-marrom", Nome = "Fita marrom", Saldo = 2, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "fita-transp", Nome = "Fita transparente", Saldo = 2, Unidade = "un" });
        });
        Ins("ins-barbante", "Barbante", "m", 42, 10, "embalagem", "marca/patrimonio/pat-argola.jpg", "2026-09-13", "Shopee", "50 m bicolor vermelho/branco · set/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "bar-bicolor", Nome = "Vermelho/branco", Saldo = 42, Unidade = "m" });
        });
        Ins("ins-papel-foto", "Papel fotográfico (cartão presente)", "un", 85, 20, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-09-03", "Shopee", "100 folhas 180g Masterprint · set/2026", i =>
        {
            i.Variantes.Add(new InsumoVariante { Id = "foto-a4", Nome = "Folha A4 glossy 180g", Saldo = 70, Unidade = "un" });
            i.Variantes.Add(new InsumoVariante { Id = "foto-cartao", Nome = "Cartões de presente cortados", Saldo = 15, Unidade = "un" });
        });
        Ins("ins-cartao", "Cartão de presente / tag", "un", 80, 20, "embalagem", "marca/patrimonio/pat-caixa.jpg", "2026-05-01", "Oficina", "Parâmetro de orçamento (Base Orçamento)");
        Ins("ins-silica", "Sílica gel", "un", 2, 1, "acabamento", "marca/patrimonio/pat-secador.jpg", "2026-09-09", "Shopee", "sílica laranja · mar e set/2026");

        return lista;
    }

    private static void SeedFinanceiro(ApplicationDbContext context, IWebHostEnvironment env)
    {
        context.ContasFinanceiras.AddRange(
            new ContaFinanceira { Id = "conta-1", Tipo = "pagar", Descricao = "Energia / mensalidade máquinas (out/2026)", Valor = 90, Vencimento = DateOnly.Parse("2026-10-28") },
            new ContaFinanceira { Id = "conta-2", Tipo = "receber", Descricao = "Mileni — saldo moldes", Valor = 150, Vencimento = DateOnly.Parse("2026-10-22") },
            new ContaFinanceira { Id = "conta-3", Tipo = "receber", Descricao = "Edu — caminhão (parcial)", Valor = 120, Vencimento = DateOnly.Parse("2026-10-30"), Baixada = false },
            new ContaFinanceira { Id = "conta-4", Tipo = "pagar", Descricao = "Filamento PETG pedido aberto", Valor = 198.91m, Vencimento = DateOnly.Parse("2026-10-18"), Baixada = true });

        var lancamentos = PlanilhaSeedLoader.CarregarLancamentos(env);
        if (lancamentos.Count > 0)
            context.LancamentosFinanceiros.AddRange(lancamentos);
        else
        {
            context.LancamentosFinanceiros.AddRange(
                new LancamentoFinanceiro { Id = "lan-0001", Data = DateOnly.Parse("2026-09-02"), Competencia = "2026-09", Descricao = "Magno Carimbo de Biscoito com cortador", Categoria = "receita", Quantidade = 1, Valor = 15, Origem = "planilha" },
                new LancamentoFinanceiro { Id = "lan-0002", Data = DateOnly.Parse("2026-09-10"), Competencia = "2026-09", Descricao = "Mileni Parte dos Moldes", Categoria = "receita", Quantidade = 1, Valor = 150, Origem = "planilha" });
        }

        var energia = PlanilhaSeedLoader.CarregarEnergia(env);
        if (energia.Count > 0)
            context.FechamentosEnergia.AddRange(energia);

        context.ParametrosCusto.AddRange(
            new ParametroCusto { Id = "par-pla", Nome = "PLA", Unidade = "kg", Preco = 85, Travado = true },
            new ParametroCusto { Id = "par-petg", Nome = "PETG", Unidade = "kg", Preco = 65, Travado = true },
            new ParametroCusto { Id = "par-tpu", Nome = "TPU", Unidade = "kg", Preco = 80, Travado = true },
            new ParametroCusto { Id = "par-kraft-p", Nome = "Saco kraft pequeno", Unidade = "un", Preco = 0.5m, Travado = true },
            new ParametroCusto { Id = "par-kraft-g", Nome = "Saco kraft grande", Unidade = "un", Preco = 1.1286m, Travado = true },
            new ParametroCusto { Id = "par-ia", Nome = "Uso de IA", Unidade = "un", Preco = 2m, Travado = true });

        context.MargensCanal.AddRange(
            new MargemCanal { Id = "marketplace", Nome = "Revenda / Shopee", Percentual = 0.25m, Ativo = true },
            new MargemCanal { Id = "simples", Nome = "Simples", Percentual = 0.4m, Ativo = true },
            new MargemCanal { Id = "medio", Nome = "Médio", Percentual = 0.45m, Ativo = true },
            new MargemCanal { Id = "complexo", Nome = "Complexo", Percentual = 0.5m, Ativo = true },
            new MargemCanal { Id = "exclusivo", Nome = "Modelagem exclusiva", Percentual = 0.55m, Ativo = true });

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

        context.MetasFinanceiras.Add(new MetaFinanceira
        {
            Chave = "receita-2026-09",
            ValorJson = """{"meta":800,"notas":"Meta baseada no ritmo de set/2026 (R$ 621,50 realizados)"}"""
        });
    }

    private static void SeedOrcamentoInterno(ApplicationDbContext context, IWebHostEnvironment env)
    {
        var lista = PlanilhaSeedLoader.CarregarOrcamentos(env);
        if (lista.Count > 0)
        {
            context.OrcamentosInternos.AddRange(lista);
            return;
        }

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

    private static void SeedAtendimento(ApplicationDbContext context, List<ApplicationUser> clientes)
    {
        if (clientes.Count == 0) return;
        var c0 = clientes[0];
        var c1 = clientes.ElementAtOrDefault(1) ?? c0;
        var c2 = clientes.ElementAtOrDefault(2) ?? c0;
        var c3 = clientes.ElementAtOrDefault(3) ?? c0;
        var c4 = clientes.ElementAtOrDefault(4) ?? c0;
        var c5 = clientes.ElementAtOrDefault(5) ?? c0;

        context.MensagensContato.AddRange(
            new MensagemContato { Id = "msg-1", Tipo = "elogio", Nome = c0.NomeCompleto, Email = c0.Email!, Texto = "Gostei da peça que chegou — acabamento ótimo.", Status = "aberto", CriadoEm = Utc("2026-10-08T10:00") },
            new MensagemContato { Id = "msg-2", Tipo = "duvida", Nome = c1.NomeCompleto, Email = c1.Email!, Texto = "Vocês fazem molde de silicone a partir de modelo 3D?", Status = "em atendimento", CriadoEm = Utc("2026-10-12T14:20") },
            new MensagemContato { Id = "msg-3", Tipo = "orcamento", Nome = c2.NomeCompleto, Email = c2.Email!, Texto = "Quero orçar um suporte de fone personalizado com nome.", Status = "aberto", CriadoEm = Utc("2026-10-15T09:10") },
            new MensagemContato { Id = "msg-4", Tipo = "reclamacao", Nome = c3.NomeCompleto, Email = c3.Email!, Texto = "O frete demorou mais que o estimado no PAC.", Status = "resolvido", CriadoEm = Utc("2026-09-28T16:40") },
            new MensagemContato { Id = "msg-5", Tipo = "elogio", Nome = c4.NomeCompleto, Email = c4.Email!, Texto = "Os chaveiros da academia ficaram perfeitos.", Status = "resolvido", CriadoEm = Utc("2026-09-24T11:00") },
            new MensagemContato { Id = "msg-6", Tipo = "duvida", Nome = c5.NomeCompleto, Email = c5.Email!, Texto = "Dá para pintar a peça em degradê azul/branco?", Status = "aberto", CriadoEm = Utc("2026-10-18T18:05") });

        context.EnderecosCliente.AddRange(
            new EnderecoCliente { Id = "end-1", UsuarioId = c0.Id, Rotulo = "Casa", Cep = "29010000", Linha = "Rua das Impressoras, 10, Vitória/ES" },
            new EnderecoCliente { Id = "end-2", UsuarioId = c1.Id, Rotulo = "Apartamento", Cep = "29055000", Linha = "Av. Leitão da Silva, 200, Vitória/ES" },
            new EnderecoCliente { Id = "end-3", UsuarioId = c2.Id, Rotulo = "Trabalho", Cep = "29101000", Linha = "Rua Sete de Setembro, 55, Vila Velha/ES" },
            new EnderecoCliente { Id = "end-4", UsuarioId = c3.Id, Rotulo = "Casa", Cep = "29160000", Linha = "Rua das Palmeiras, 88, Serra/ES" },
            new EnderecoCliente { Id = "end-5", UsuarioId = c4.Id, Rotulo = "Casa", Cep = "29025000", Linha = "Rua Aleixo Netto, 312, Vitória/ES" });

        context.Pedidos.AddRange(
            new Pedido
            {
                Id = "ped-entregue",
                UsuarioId = c0.Id,
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
                Nota = "revisada",
                Itens = { new PedidoItem { Id = "item-ent-1", ProdutoId = "sessao-2026-03-14", Titulo = "Peça da oficina — 14/03/2026", Tipo = "pronto", Fisico = true, Quantidade = 1, PrecoUnitario = 89 } },
                Os = new PedidoOs { Status = "concluida", InsumoBaixado = true }
            },
            new Pedido
            {
                Id = "ped-ana-1",
                UsuarioId = c1.Id,
                Status = "pago",
                CriadoEm = Utc("2026-10-05T12:30"),
                Subtotal = 64,
                FreteCobrado = 29.9m,
                Total = 93.9m,
                Forma = "cartao",
                Endereco = "Av. Leitão da Silva, 200, Vitória/ES",
                Cep = "29055000",
                FreteNome = "PAC",
                Nota = "pendente",
                Itens = { new PedidoItem { Id = "item-ana-1", ProdutoId = "sessao-2026-03-27", Titulo = "Peça da oficina — 27/03/2026", Tipo = "pronto", Fisico = true, Quantidade = 1, PrecoUnitario = 64 } },
                Os = new PedidoOs { Status = "em producao", InsumoBaixado = false }
            },
            new Pedido
            {
                Id = "ped-bruno-1",
                UsuarioId = c2.Id,
                Status = "aguardando pagamento",
                CriadoEm = Utc("2026-10-16T09:00"),
                Subtotal = 120,
                DescontoCupom = 12,
                Total = 108,
                Forma = "boleto",
                Endereco = "Rua Sete de Setembro, 55, Vila Velha/ES",
                Cep = "29101000",
                FreteNome = "Retirada",
                Cupom = "OFICINA10",
                Nota = "nao_aplicavel",
                Itens = { new PedidoItem { Id = "item-bru-1", ProdutoId = "sessao-2026-04-14", Titulo = "Peça da oficina — 14/04/2026", Tipo = "personalizado", Fisico = true, Quantidade = 1, PrecoUnitario = 120 } }
            },
            new Pedido
            {
                Id = "ped-camila-1",
                UsuarioId = c3.Id,
                Status = "enviado",
                CriadoEm = Utc("2026-09-30T17:20"),
                Subtotal = 52.5m,
                FreteCobrado = 0,
                Total = 52.5m,
                Forma = "pix",
                Endereco = "Rua das Palmeiras, 88, Serra/ES",
                Cep = "29160000",
                FreteNome = "Frete grátis",
                Nota = "pendente",
                Itens = { new PedidoItem { Id = "item-cam-1", ProdutoId = "sessao-2026-03-14", Titulo = "Chaveiros Academia GoFit", Tipo = "pronto", Fisico = true, Quantidade = 21, PrecoUnitario = 2.5m } },
                Os = new PedidoOs { Status = "concluida", InsumoBaixado = true }
            },
            new Pedido
            {
                Id = "ped-diego-1",
                UsuarioId = c4.Id,
                Status = "entregue",
                CriadoEm = Utc("2026-08-18T11:00"),
                Subtotal = 40,
                Total = 40,
                Forma = "pix",
                Endereco = "Rua Aleixo Netto, 312, Vitória/ES",
                Cep = "29025000",
                FreteNome = "Retirada",
                Nota = "revisada",
                Itens = { new PedidoItem { Id = "item-die-1", ProdutoId = "sessao-2026-03-14", Titulo = "Pikachu", Tipo = "pronto", Fisico = true, Quantidade = 1, PrecoUnitario = 40 } },
                Os = new PedidoOs { Status = "concluida", InsumoBaixado = true }
            });

        context.OrcamentosCliente.AddRange(
            new OrcamentoCliente { Id = "orc-cli-1", UsuarioId = c0.Id, ProdutoId = "sessao-2026-04-14", Titulo = "Peça personalizada — texto Cria", Status = "respondido", Detalhes = "Medida 8 cm, cor teal", ArquivoNome = "referencia.png", Valor = 180, CriadoEm = Utc("2026-10-02T11:00") },
            new OrcamentoCliente { Id = "orc-cli-2", UsuarioId = c0.Id, ProdutoId = "sessao-2026-03-27", Titulo = "Texto gravado", Status = "aguardando", Detalhes = "Ainda sem preço", CriadoEm = Utc("2026-10-10T09:30") },
            new OrcamentoCliente { Id = "orc-cli-3", UsuarioId = c1.Id, ProdutoId = "sessao-2026-04-14", Titulo = "Molde mamão", Status = "respondido", Detalhes = "PETG, 2 unidades", Valor = 95, CriadoEm = Utc("2026-09-18T10:00") },
            new OrcamentoCliente { Id = "orc-cli-4", UsuarioId = c2.Id, ProdutoId = "sessao-2026-03-14", Titulo = "Suporte fone com nome", Status = "aguardando", Detalhes = "Nome: Bruno", CriadoEm = Utc("2026-10-15T09:15") },
            new OrcamentoCliente { Id = "orc-cli-5", UsuarioId = c5.Id, ProdutoId = "sessao-2026-04-14", Titulo = "Animais Flexi", Status = "respondido", Detalhes = "Kit 3 peças coloridas", Valor = 75, CriadoEm = Utc("2026-10-08T16:00") });

        context.Avaliacoes.AddRange(
            new Avaliacao { Id = "ava-1", ProdutoId = "sessao-2026-03-14", PedidoId = "ped-entregue", Autor = c0.NomeCompleto, Nota = 5, Texto = "A peça chegou bem feita e na cor combinada.", FotoDoProduto = true },
            new Avaliacao { Id = "ava-2", ProdutoId = "sessao-2026-03-14", PedidoId = "ped-diego-1", Autor = c4.NomeCompleto, Nota = 5, Texto = "Pikachu ficou lindo, minha filha amou.", FotoDoProduto = true },
            new Avaliacao { Id = "ava-3", ProdutoId = "sessao-2026-03-27", PedidoId = "ped-ana-1", Autor = c1.NomeCompleto, Nota = 4, Texto = "Boa qualidade; só demorou um pouco na produção.", FotoDoProduto = false },
            new Avaliacao { Id = "ava-4", ProdutoId = "sessao-2026-03-14", PedidoId = "ped-camila-1", Autor = c3.NomeCompleto, Nota = 5, Texto = "Chaveiros iguais ao pedido, ótimo para a academia.", FotoDoProduto = true },
            new Avaliacao { Id = "ava-5", ProdutoId = "sessao-2026-04-14", Autor = "Visitante loja", Nota = 4, Texto = "Acabamento profissional nas peças personalizadas.", FotoDoProduto = false },
            new Avaliacao { Id = "ava-6", ProdutoId = "sessao-2026-04-14", Autor = c5.NomeCompleto, Nota = 5, Texto = "Flexi perfeito para presente.", FotoDoProduto = true });

        context.PerguntasProduto.AddRange(
            new PerguntaProduto { Id = "per-1", ProdutoId = "sessao-2026-03-14", Autor = c1.NomeCompleto, Texto = "Essa peça pode ser feita em PETG?", Resposta = "Sim, em PETG fica mais resistente. Posso orçar.", CriadoEm = Utc("2026-10-01T14:00") },
            new PerguntaProduto { Id = "per-2", ProdutoId = "sessao-2026-03-14", Autor = c2.NomeCompleto, Texto = "Tem nas cores preto e branco?", Resposta = "Temos preto, branco e outras sob consulta.", CriadoEm = Utc("2026-10-03T10:20") },
            new PerguntaProduto { Id = "per-3", ProdutoId = "sessao-2026-03-27", Autor = c3.NomeCompleto, Texto = "Serve como molde para chocolate?", Resposta = "", CriadoEm = Utc("2026-10-11T08:45") },
            new PerguntaProduto { Id = "per-4", ProdutoId = "sessao-2026-04-14", Autor = c4.NomeCompleto, Texto = "Dá para gravar um nome na base?", Resposta = "Sim, personalizamos com texto na base.", CriadoEm = Utc("2026-09-25T19:00") },
            new PerguntaProduto { Id = "per-5", ProdutoId = "sessao-2026-04-14", Autor = c5.NomeCompleto, Texto = "Qual o prazo médio para personalizados?", Resposta = "Em geral 5 a 10 dias úteis após aprovação do orçamento.", CriadoEm = Utc("2026-10-07T13:30") },
            new PerguntaProduto { Id = "per-6", ProdutoId = "sessao-2026-03-14", Autor = "Marina", Texto = "Vocês enviam para todo o Brasil?", Resposta = "", CriadoEm = Utc("2026-10-19T11:10") });

        var agora = DateTime.UtcNow;
        context.Favoritos.AddRange(
            new Favorito { UsuarioId = c1.Id, ProdutoId = "sessao-2026-03-14", CriadoEm = agora },
            new Favorito { UsuarioId = c1.Id, ProdutoId = "sessao-2026-04-14", CriadoEm = agora },
            new Favorito { UsuarioId = c2.Id, ProdutoId = "sessao-2026-03-27", CriadoEm = agora },
            new Favorito { UsuarioId = c5.Id, ProdutoId = "sessao-2026-04-14", CriadoEm = agora });
    }
}

