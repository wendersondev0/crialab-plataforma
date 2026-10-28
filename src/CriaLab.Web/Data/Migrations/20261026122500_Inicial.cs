using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CriaLab.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asp_net_roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome_completo = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    telefone_contato = table.Column<string>(type: "text", nullable: true),
                    documento = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_loja",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    frete_gratis_a_partir = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_a_vista = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    parcelas = table.Column<int>(type: "integer", nullable: false),
                    repassar_frete = table.Column<bool>(type: "boolean", nullable: false),
                    taxa_falha = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    frete_canal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_kwh_atual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_configuracoes_loja", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contas_financeiras",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    baixada = table.Column<bool>(type: "boolean", nullable: false),
                    comprovante = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contas_financeiras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cupons",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cupons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "depoimentos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    autor = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    pode_aparecer = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_depoimentos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "descontos_orcamento",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    recorrente = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    volume = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_descontos_orcamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "enderecos_cliente",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    rotulo = table.Column<string>(type: "text", nullable: false),
                    cep = table.Column<string>(type: "text", nullable: false),
                    linha = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enderecos_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estoque_insumos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    unidade = table.Column<string>(type: "text", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    minimo = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    imagem = table.Column<string>(type: "text", nullable: false),
                    adquirido_em = table.Column<DateOnly>(type: "date", nullable: true),
                    fornecedor = table.Column<string>(type: "text", nullable: false),
                    origem_planilha = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estoque_insumos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faixas_cep",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    de = table.Column<int>(type: "integer", nullable: false),
                    ate = table.Column<int>(type: "integer", nullable: false),
                    pac = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pac_dias = table.Column<int>(type: "integer", nullable: false),
                    sedex = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sedex_dias = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faixas_cep", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faixas_marketplace",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    canal = table.Column<string>(type: "text", nullable: false),
                    de = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    fixo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faixas_marketplace", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fechamentos_energia",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    kwh_total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tarifa = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notas = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fechamentos_energia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lancamentos_financeiros",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<DateOnly>(type: "date", nullable: true),
                    competencia = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    tipo_insumo = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    link = table.Column<string>(type: "text", nullable: false),
                    kwh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    tarifa = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    estornado = table.Column<bool>(type: "boolean", nullable: false),
                    comprovante = table.Column<string>(type: "text", nullable: false),
                    origem = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lancamentos_financeiros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "loja_identidades",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    site = table.Column<string>(type: "text", nullable: false),
                    cnpj = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: false),
                    whatsapp = table.Column<string>(type: "text", nullable: false),
                    instagram = table.Column<string>(type: "text", nullable: false),
                    tiktok = table.Column<string>(type: "text", nullable: false),
                    youtube = table.Column<string>(type: "text", nullable: false),
                    shopee = table.Column<string>(type: "text", nullable: false),
                    mercado_livre = table.Column<string>(type: "text", nullable: false),
                    endereco = table.Column<string>(type: "text", nullable: false),
                    horario = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loja_identidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maquinas",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    imagem = table.Column<string>(type: "text", nullable: false),
                    watts = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    depreciacao_por_minuto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    manutencao_por_minuto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    energia_por_minuto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    controla_hora = table.Column<bool>(type: "boolean", nullable: false),
                    compra_competencia = table.Column<string>(type: "text", nullable: false),
                    adquirido_em = table.Column<DateOnly>(type: "date", nullable: true),
                    compra_valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    marca = table.Column<string>(type: "text", nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: false),
                    serial = table.Column<string>(type: "text", nullable: false),
                    local = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notas = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maquinas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "margens_canal",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_margens_canal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mensagens_contato",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mensagens_contato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menus_ajuda",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menus_ajuda", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "metas_financeiras",
                columns: table => new
                {
                    chave = table.Column<string>(type: "text", nullable: false),
                    valor_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_metas_financeiras", x => x.chave);
                });

            migrationBuilder.CreateTable(
                name: "modalidades",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    resumo = table.Column<string>(type: "text", nullable: false),
                    explicacao = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_modalidades", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "niveis_pintura",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    percentual = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    tempo = table.Column<string>(type: "text", nullable: false),
                    cores = table.Column<string>(type: "text", nullable: false),
                    acabamento = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_niveis_pintura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orcamentos_cliente",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    detalhes = table.Column<string>(type: "text", nullable: false),
                    arquivo_nome = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orcamentos_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_custo",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    unidade = table.Column<string>(type: "text", nullable: false),
                    preco = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    travado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parametros_custo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parceiros",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    logo = table.Column<string>(type: "text", nullable: false),
                    pode_aparecer = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parceiros", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_cupom = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    desconto_a_vista = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    frete_cobrado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    forma = table.Column<string>(type: "text", nullable: false),
                    endereco = table.Column<string>(type: "text", nullable: false),
                    cep = table.Column<string>(type: "text", nullable: false),
                    frete_nome = table.Column<string>(type: "text", nullable: false),
                    cupom = table.Column<string>(type: "text", nullable: false),
                    repassar_frete = table.Column<bool>(type: "boolean", nullable: false),
                    nota = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedidos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "referencias_maquina",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    energia_por_minuto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    depreciacao = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    manutencao = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_referencias_maquina", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_insumo_config",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipos_insumo_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insumo_cores",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    estoque_insumo_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    hex = table.Column<string>(type: "text", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    bobinas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insumo_cores", x => x.id);
                    table.ForeignKey(
                        name: "fk_insumo_cores_estoque_insumos_estoque_insumo_id",
                        column: x => x.estoque_insumo_id,
                        principalTable: "estoque_insumos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insumo_variantes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    estoque_insumo_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    unidade = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insumo_variantes", x => x.id);
                    table.ForeignKey(
                        name: "fk_insumo_variantes_estoque_insumos_estoque_insumo_id",
                        column: x => x.estoque_insumo_id,
                        principalTable: "estoque_insumos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "impressao_jobs",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    maquina_id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    peso_previsto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    peso_real = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sucesso = table.Column<bool>(type: "boolean", nullable: true),
                    motivo = table.Column<string>(type: "text", nullable: false),
                    kwh = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_impressao_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_impressao_jobs_maquinas_maquina_id",
                        column: x => x.maquina_id,
                        principalTable: "maquinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orcamentos_internos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    material = table.Column<string>(type: "text", nullable: false),
                    minutos = table.Column<int>(type: "integer", nullable: false),
                    peso_g = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    lixo_g = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    servico = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    margem_site = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    descontos_marcados_json = table.Column<string>(type: "text", nullable: false),
                    preco_kg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    preco_kg_origem = table.Column<string>(type: "text", nullable: false),
                    energia_por_minuto = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    depreciacao = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    manutencao = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    taxa_falha = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    enviado = table.Column<bool>(type: "boolean", nullable: false),
                    maquina_id = table.Column<string>(type: "text", nullable: true),
                    frete_canal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    pintura_json = table.Column<string>(type: "text", nullable: true),
                    realizado_json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orcamentos_internos", x => x.id);
                    table.ForeignKey(
                        name: "fk_orcamentos_internos_maquinas_maquina_id",
                        column: x => x.maquina_id,
                        principalTable: "maquinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    sessao = table.Column<string>(type: "text", nullable: false),
                    categoria_id = table.Column<string>(type: "text", nullable: false),
                    modalidade_id = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_comparativo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    preco_arquivo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    estoque = table.Column<int>(type: "integer", nullable: false),
                    minimo = table.Column<int>(type: "integer", nullable: false),
                    medidas = table.Column<string>(type: "text", nullable: false),
                    peso = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    material = table.Column<string>(type: "text", nullable: false),
                    prazo_dias = table.Column<int>(type: "integer", nullable: false),
                    com_arquivo = table.Column<bool>(type: "boolean", nullable: false),
                    arquivo = table.Column<string>(type: "text", nullable: true),
                    personalizavel = table.Column<bool>(type: "boolean", nullable: false),
                    vendas = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    oferta = table.Column<bool>(type: "boolean", nullable: false),
                    mais_vendido = table.Column<bool>(type: "boolean", nullable: false),
                    situacao = table.Column<string>(type: "text", nullable: false),
                    a_vista = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                    table.ForeignKey(
                        name: "fk_produtos_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_produtos_modalidades_modalidade_id",
                        column: x => x.modalidade_id,
                        principalTable: "modalidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedido_itens",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    pedido_id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    fisico = table.Column<bool>(type: "boolean", nullable: false),
                    arquivo = table.Column<bool>(type: "boolean", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cor_escolhida = table.Column<string>(type: "text", nullable: true),
                    tamanho_escolhido = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedido_itens_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pedido_ordens_servico",
                columns: table => new
                {
                    pedido_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    insumo_baixado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido_ordens_servico", x => x.pedido_id);
                    table.ForeignKey(
                        name: "fk_pedido_ordens_servico_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orcamento_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    orcamento_interno_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    preco = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orcamento_itens", x => x.id);
                    table.ForeignKey(
                        name: "fk_orcamento_itens_orcamentos_internos_orcamento_interno_id",
                        column: x => x.orcamento_interno_id,
                        principalTable: "orcamentos_internos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avaliacoes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    pedido_id = table.Column<string>(type: "text", nullable: true),
                    autor = table.Column<string>(type: "text", nullable: false),
                    nota = table.Column<int>(type: "integer", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    foto_do_produto = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avaliacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_avaliacoes_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favoritos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favoritos", x => x.id);
                    table.ForeignKey(
                        name: "fk_favoritos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "perguntas_produto",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    autor = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    resposta = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_perguntas_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_perguntas_produto_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_cores_legado",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    hex = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto_cores_legado", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_cores_legado_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_etapas",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    acresce = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto_etapas", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_etapas_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_fotos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    arquivo = table.Column<string>(type: "text", nullable: false),
                    sessao = table.Column<string>(type: "text", nullable: false),
                    legenda = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto_fotos", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_fotos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "produto_tamanhos_legado",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto_tamanhos_legado", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_tamanhos_legado_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variacao_eixos",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    produto_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_variacao_eixos", x => x.id);
                    table.ForeignKey(
                        name: "fk_variacao_eixos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variacao_opcoes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    variacao_eixo_id = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    hex = table.Column<string>(type: "text", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_variacao_opcoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_variacao_opcoes_variacao_eixos_variacao_eixo_id",
                        column: x => x.variacao_eixo_id,
                        principalTable: "variacao_eixos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "asp_net_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "asp_net_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "asp_net_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "asp_net_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "asp_net_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "asp_net_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_avaliacoes_produto_id",
                table: "avaliacoes",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_cupons_codigo",
                table: "cupons",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_enderecos_cliente_usuario_id",
                table: "enderecos_cliente",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_faixas_marketplace_canal",
                table: "faixas_marketplace",
                column: "canal");

            migrationBuilder.CreateIndex(
                name: "ix_favoritos_produto_id",
                table: "favoritos",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_favoritos_usuario_id_produto_id",
                table: "favoritos",
                columns: new[] { "usuario_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_impressao_jobs_maquina_id",
                table: "impressao_jobs",
                column: "maquina_id");

            migrationBuilder.CreateIndex(
                name: "ix_insumo_cores_estoque_insumo_id",
                table: "insumo_cores",
                column: "estoque_insumo_id");

            migrationBuilder.CreateIndex(
                name: "ix_insumo_variantes_estoque_insumo_id",
                table: "insumo_variantes",
                column: "estoque_insumo_id");

            migrationBuilder.CreateIndex(
                name: "ix_lancamentos_financeiros_competencia",
                table: "lancamentos_financeiros",
                column: "competencia");

            migrationBuilder.CreateIndex(
                name: "ix_mensagens_contato_status",
                table: "mensagens_contato",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_orcamento_itens_orcamento_interno_id",
                table: "orcamento_itens",
                column: "orcamento_interno_id");

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_cliente_usuario_id",
                table: "orcamentos_cliente",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_internos_maquina_id",
                table: "orcamentos_internos",
                column: "maquina_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_itens_pedido_id",
                table: "pedido_itens",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_status",
                table: "pedidos",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_usuario_id",
                table: "pedidos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_perguntas_produto_produto_id",
                table: "perguntas_produto",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_cores_legado_produto_id",
                table: "produto_cores_legado",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_etapas_produto_id",
                table: "produto_etapas",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_fotos_produto_id",
                table: "produto_fotos",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_tamanhos_legado_produto_id",
                table: "produto_tamanhos_legado",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_categoria_id",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_modalidade_id",
                table: "produtos",
                column: "modalidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_situacao",
                table: "produtos",
                column: "situacao");

            migrationBuilder.CreateIndex(
                name: "ix_variacao_eixos_produto_id",
                table: "variacao_eixos",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_variacao_opcoes_variacao_eixo_id",
                table: "variacao_opcoes",
                column: "variacao_eixo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asp_net_role_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_logins");

            migrationBuilder.DropTable(
                name: "asp_net_user_roles");

            migrationBuilder.DropTable(
                name: "asp_net_user_tokens");

            migrationBuilder.DropTable(
                name: "avaliacoes");

            migrationBuilder.DropTable(
                name: "configuracoes_loja");

            migrationBuilder.DropTable(
                name: "contas_financeiras");

            migrationBuilder.DropTable(
                name: "cupons");

            migrationBuilder.DropTable(
                name: "depoimentos");

            migrationBuilder.DropTable(
                name: "descontos_orcamento");

            migrationBuilder.DropTable(
                name: "enderecos_cliente");

            migrationBuilder.DropTable(
                name: "faixas_cep");

            migrationBuilder.DropTable(
                name: "faixas_marketplace");

            migrationBuilder.DropTable(
                name: "favoritos");

            migrationBuilder.DropTable(
                name: "fechamentos_energia");

            migrationBuilder.DropTable(
                name: "impressao_jobs");

            migrationBuilder.DropTable(
                name: "insumo_cores");

            migrationBuilder.DropTable(
                name: "insumo_variantes");

            migrationBuilder.DropTable(
                name: "lancamentos_financeiros");

            migrationBuilder.DropTable(
                name: "loja_identidades");

            migrationBuilder.DropTable(
                name: "margens_canal");

            migrationBuilder.DropTable(
                name: "mensagens_contato");

            migrationBuilder.DropTable(
                name: "menus_ajuda");

            migrationBuilder.DropTable(
                name: "metas_financeiras");

            migrationBuilder.DropTable(
                name: "niveis_pintura");

            migrationBuilder.DropTable(
                name: "orcamento_itens");

            migrationBuilder.DropTable(
                name: "orcamentos_cliente");

            migrationBuilder.DropTable(
                name: "parametros_custo");

            migrationBuilder.DropTable(
                name: "parceiros");

            migrationBuilder.DropTable(
                name: "pedido_itens");

            migrationBuilder.DropTable(
                name: "pedido_ordens_servico");

            migrationBuilder.DropTable(
                name: "perguntas_produto");

            migrationBuilder.DropTable(
                name: "produto_cores_legado");

            migrationBuilder.DropTable(
                name: "produto_etapas");

            migrationBuilder.DropTable(
                name: "produto_fotos");

            migrationBuilder.DropTable(
                name: "produto_tamanhos_legado");

            migrationBuilder.DropTable(
                name: "referencias_maquina");

            migrationBuilder.DropTable(
                name: "tipos_insumo_config");

            migrationBuilder.DropTable(
                name: "variacao_opcoes");

            migrationBuilder.DropTable(
                name: "asp_net_roles");

            migrationBuilder.DropTable(
                name: "asp_net_users");

            migrationBuilder.DropTable(
                name: "estoque_insumos");

            migrationBuilder.DropTable(
                name: "orcamentos_internos");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "variacao_eixos");

            migrationBuilder.DropTable(
                name: "maquinas");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "modalidades");
        }
    }
}
