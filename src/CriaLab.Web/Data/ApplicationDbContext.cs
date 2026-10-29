using CriaLab.Web.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CriaLab.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<LojaIdentidade> LojaIdentidades => Set<LojaIdentidade>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Modalidade> Modalidades => Set<Modalidade>();
    public DbSet<MenuAjuda> MenusAjuda => Set<MenuAjuda>();
    public DbSet<ConfiguracaoLoja> ConfiguracoesLoja => Set<ConfiguracaoLoja>();
    public DbSet<Cupom> Cupons => Set<Cupom>();
    public DbSet<FaixaCep> FaixasCep => Set<FaixaCep>();
    public DbSet<TipoInsumoConfig> TiposInsumoConfig => Set<TipoInsumoConfig>();
    public DbSet<Depoimento> Depoimentos => Set<Depoimento>();
    public DbSet<Parceiro> Parceiros => Set<Parceiro>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ProdutoFoto> ProdutoFotos => Set<ProdutoFoto>();
    public DbSet<ProdutoEtapaPersonalizacao> ProdutoEtapas => Set<ProdutoEtapaPersonalizacao>();
    public DbSet<VariacaoEixo> VariacaoEixos => Set<VariacaoEixo>();
    public DbSet<VariacaoOpcao> VariacaoOpcoes => Set<VariacaoOpcao>();
    public DbSet<ProdutoCorLegado> ProdutoCoresLegado => Set<ProdutoCorLegado>();
    public DbSet<ProdutoTamanhoLegado> ProdutoTamanhosLegado => Set<ProdutoTamanhoLegado>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<EnderecoCliente> EnderecosCliente => Set<EnderecoCliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<PedidoOs> PedidoOrdensServico => Set<PedidoOs>();
    public DbSet<OrcamentoCliente> OrcamentosCliente => Set<OrcamentoCliente>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
    public DbSet<PerguntaProduto> PerguntasProduto => Set<PerguntaProduto>();
    public DbSet<MensagemContato> MensagensContato => Set<MensagemContato>();
    public DbSet<Maquina> Maquinas => Set<Maquina>();
    public DbSet<EstoqueInsumo> EstoqueInsumos => Set<EstoqueInsumo>();
    public DbSet<InsumoCor> InsumoCores => Set<InsumoCor>();
    public DbSet<InsumoVariante> InsumoVariantes => Set<InsumoVariante>();
    public DbSet<ImpressaoJob> ImpressaoJobs => Set<ImpressaoJob>();
    public DbSet<ContaFinanceira> ContasFinanceiras => Set<ContaFinanceira>();
    public DbSet<LancamentoFinanceiro> LancamentosFinanceiros => Set<LancamentoFinanceiro>();
    public DbSet<MetaFinanceira> MetasFinanceiras => Set<MetaFinanceira>();
    public DbSet<FechamentoEnergia> FechamentosEnergia => Set<FechamentoEnergia>();
    public DbSet<ParametroCusto> ParametrosCusto => Set<ParametroCusto>();
    public DbSet<MargemCanal> MargensCanal => Set<MargemCanal>();
    public DbSet<DescontoOrcamento> DescontosOrcamento => Set<DescontoOrcamento>();
    public DbSet<ReferenciaMaquina> ReferenciasMaquina => Set<ReferenciaMaquina>();
    public DbSet<NivelPintura> NiveisPintura => Set<NivelPintura>();
    public DbSet<FaixaMarketplace> FaixasMarketplace => Set<FaixaMarketplace>();
    public DbSet<OrcamentoInterno> OrcamentosInternos => Set<OrcamentoInterno>();
    public DbSet<OrcamentoItem> OrcamentoItens => Set<OrcamentoItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("asp_net_users");
        builder.Entity<IdentityRole>().ToTable("asp_net_roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("asp_net_user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("asp_net_user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("asp_net_user_logins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("asp_net_role_claims");
        builder.Entity<IdentityUserToken<string>>().ToTable("asp_net_user_tokens");

        builder.Entity<LojaIdentidade>().HasKey(x => x.Id);

        builder.Entity<Categoria>().HasKey(x => x.Id);

        builder.Entity<Modalidade>().HasKey(x => x.Id);

        builder.Entity<MenuAjuda>().HasKey(x => x.Id);

        builder.Entity<ConfiguracaoLoja>().HasKey(x => x.Id);
        builder.Entity<ConfiguracaoLoja>().Property(x => x.FreteGratisAPartir).HasPrecision(18, 2);
        builder.Entity<ConfiguracaoLoja>().Property(x => x.DescontoAVista).HasPrecision(18, 4);
        builder.Entity<ConfiguracaoLoja>().Property(x => x.TaxaFalha).HasPrecision(18, 4);
        builder.Entity<ConfiguracaoLoja>().Property(x => x.FreteCanal).HasPrecision(18, 2);
        builder.Entity<ConfiguracaoLoja>().Property(x => x.PrecoKwhAtual).HasPrecision(18, 4);

        builder.Entity<Cupom>().HasKey(x => x.Id);
        builder.Entity<Cupom>().Property(x => x.Percentual).HasPrecision(18, 4);
        builder.Entity<Cupom>().HasIndex(x => x.Codigo).IsUnique();

        builder.Entity<FaixaCep>().HasKey(x => x.Id);
        builder.Entity<FaixaCep>().Property(x => x.Pac).HasPrecision(18, 2);
        builder.Entity<FaixaCep>().Property(x => x.Sedex).HasPrecision(18, 2);
        builder.Entity<FaixaCep>().Property(x => x.EntregaLocalPreco).HasPrecision(18, 2);

        builder.Entity<TipoInsumoConfig>().HasKey(x => x.Id);

        builder.Entity<Depoimento>().HasKey(x => x.Id);
        builder.Entity<Parceiro>().HasKey(x => x.Id);

        builder.Entity<Produto>().HasKey(x => x.Id);
        builder.Entity<Produto>().Property(x => x.Preco).HasPrecision(18, 2);
        builder.Entity<Produto>().Property(x => x.PrecoComparativo).HasPrecision(18, 2);
        builder.Entity<Produto>().Property(x => x.PrecoArquivo).HasPrecision(18, 2);
        builder.Entity<Produto>().Property(x => x.Peso).HasPrecision(18, 2);
        builder.Entity<Produto>().Property(x => x.Nota).HasPrecision(18, 2);
        builder.Entity<Produto>().HasIndex(x => x.CategoriaId);
        builder.Entity<Produto>().HasIndex(x => x.ModalidadeId);
        builder.Entity<Produto>().HasIndex(x => x.Situacao);
        builder.Entity<Produto>()
            .HasOne(x => x.Categoria)
            .WithMany(x => x.Produtos)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Produto>()
            .HasOne(x => x.Modalidade)
            .WithMany(x => x.Produtos)
            .HasForeignKey(x => x.ModalidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProdutoFoto>().HasKey(x => x.Id);
        builder.Entity<ProdutoFoto>()
            .HasOne(x => x.Produto)
            .WithMany(x => x.Fotos)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProdutoEtapaPersonalizacao>().HasKey(x => x.Id);
        builder.Entity<ProdutoEtapaPersonalizacao>().Property(x => x.Acresce).HasPrecision(18, 2);
        builder.Entity<ProdutoEtapaPersonalizacao>()
            .HasOne(x => x.Produto)
            .WithMany(x => x.EtapasPersonalizacao)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VariacaoEixo>().HasKey(x => x.Id);
        builder.Entity<VariacaoEixo>()
            .HasOne(x => x.Produto)
            .WithMany(x => x.Variacoes)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VariacaoOpcao>().HasKey(x => x.Id);
        builder.Entity<VariacaoOpcao>()
            .HasOne(x => x.VariacaoEixo)
            .WithMany(x => x.Opcoes)
            .HasForeignKey(x => x.VariacaoEixoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProdutoCorLegado>().HasKey(x => x.Id);
        builder.Entity<ProdutoCorLegado>()
            .HasOne(x => x.Produto)
            .WithMany(x => x.CoresLegado)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProdutoTamanhoLegado>().HasKey(x => x.Id);
        builder.Entity<ProdutoTamanhoLegado>()
            .HasOne(x => x.Produto)
            .WithMany(x => x.TamanhosLegado)
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favorito>().HasKey(x => x.Id);
        builder.Entity<Favorito>().HasIndex(x => new { x.UsuarioId, x.ProdutoId }).IsUnique();
        builder.Entity<Favorito>()
            .HasOne(x => x.Produto)
            .WithMany()
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EnderecoCliente>().HasKey(x => x.Id);
        builder.Entity<EnderecoCliente>().HasIndex(x => x.UsuarioId);

        builder.Entity<Pedido>().HasKey(x => x.Id);
        builder.Entity<Pedido>().Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Entity<Pedido>().Property(x => x.DescontoCupom).HasPrecision(18, 2);
        builder.Entity<Pedido>().Property(x => x.DescontoAVista).HasPrecision(18, 2);
        builder.Entity<Pedido>().Property(x => x.FreteCobrado).HasPrecision(18, 2);
        builder.Entity<Pedido>().Property(x => x.Total).HasPrecision(18, 2);
        builder.Entity<Pedido>().HasIndex(x => x.UsuarioId);
        builder.Entity<Pedido>().HasIndex(x => x.Status);

        builder.Entity<PedidoItem>().HasKey(x => x.Id);
        builder.Entity<PedidoItem>().Property(x => x.PrecoUnitario).HasPrecision(18, 2);
        builder.Entity<PedidoItem>()
            .HasOne(x => x.Pedido)
            .WithMany(x => x.Itens)
            .HasForeignKey(x => x.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PedidoOs>().HasKey(x => x.PedidoId);
        builder.Entity<PedidoOs>()
            .HasOne(x => x.Pedido)
            .WithOne(x => x.Os)
            .HasForeignKey<PedidoOs>(x => x.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrcamentoCliente>().HasKey(x => x.Id);
        builder.Entity<OrcamentoCliente>().Property(x => x.Valor).HasPrecision(18, 2);
        builder.Entity<OrcamentoCliente>().HasIndex(x => x.UsuarioId);

        builder.Entity<Avaliacao>().HasKey(x => x.Id);
        builder.Entity<Avaliacao>()
            .HasOne(x => x.Produto)
            .WithMany()
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PerguntaProduto>().HasKey(x => x.Id);
        builder.Entity<PerguntaProduto>()
            .HasOne(x => x.Produto)
            .WithMany()
            .HasForeignKey(x => x.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MensagemContato>().HasKey(x => x.Id);
        builder.Entity<MensagemContato>().HasIndex(x => x.Status);

        builder.Entity<Maquina>().HasKey(x => x.Id);
        builder.Entity<Maquina>().Property(x => x.Watts).HasPrecision(18, 2);
        builder.Entity<Maquina>().Property(x => x.DepreciacaoPorMinuto).HasPrecision(18, 4);
        builder.Entity<Maquina>().Property(x => x.ManutencaoPorMinuto).HasPrecision(18, 4);
        builder.Entity<Maquina>().Property(x => x.EnergiaPorMinuto).HasPrecision(18, 4);
        builder.Entity<Maquina>().Property(x => x.CompraValor).HasPrecision(18, 2);

        builder.Entity<EstoqueInsumo>().HasKey(x => x.Id);
        builder.Entity<EstoqueInsumo>().Property(x => x.Saldo).HasPrecision(18, 3);
        builder.Entity<EstoqueInsumo>().Property(x => x.Minimo).HasPrecision(18, 3);

        builder.Entity<InsumoCor>().HasKey(x => x.Id);
        builder.Entity<InsumoCor>().Property(x => x.Saldo).HasPrecision(18, 3);
        builder.Entity<InsumoCor>()
            .HasOne(x => x.EstoqueInsumo)
            .WithMany(x => x.Cores)
            .HasForeignKey(x => x.EstoqueInsumoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InsumoVariante>().HasKey(x => x.Id);
        builder.Entity<InsumoVariante>().Property(x => x.Saldo).HasPrecision(18, 3);
        builder.Entity<InsumoVariante>()
            .HasOne(x => x.EstoqueInsumo)
            .WithMany(x => x.Variantes)
            .HasForeignKey(x => x.EstoqueInsumoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ImpressaoJob>().HasKey(x => x.Id);
        builder.Entity<ImpressaoJob>().Property(x => x.PesoPrevisto).HasPrecision(18, 2);
        builder.Entity<ImpressaoJob>().Property(x => x.PesoReal).HasPrecision(18, 2);
        builder.Entity<ImpressaoJob>().Property(x => x.Kwh).HasPrecision(18, 4);
        builder.Entity<ImpressaoJob>()
            .HasOne(x => x.Maquina)
            .WithMany()
            .HasForeignKey(x => x.MaquinaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ContaFinanceira>().HasKey(x => x.Id);
        builder.Entity<ContaFinanceira>().Property(x => x.Valor).HasPrecision(18, 2);

        builder.Entity<LancamentoFinanceiro>().HasKey(x => x.Id);
        builder.Entity<LancamentoFinanceiro>().Property(x => x.Quantidade).HasPrecision(18, 3);
        builder.Entity<LancamentoFinanceiro>().Property(x => x.Valor).HasPrecision(18, 2);
        builder.Entity<LancamentoFinanceiro>().Property(x => x.Kwh).HasPrecision(18, 4);
        builder.Entity<LancamentoFinanceiro>().Property(x => x.Tarifa).HasPrecision(18, 4);
        builder.Entity<LancamentoFinanceiro>().HasIndex(x => x.Competencia);

        builder.Entity<MetaFinanceira>().HasKey(x => x.Chave);

        builder.Entity<FechamentoEnergia>().HasKey(x => x.Id);
        builder.Entity<FechamentoEnergia>().Property(x => x.KwhTotal).HasPrecision(18, 4);
        builder.Entity<FechamentoEnergia>().Property(x => x.Tarifa).HasPrecision(18, 4);
        builder.Entity<FechamentoEnergia>().Property(x => x.Valor).HasPrecision(18, 2);

        builder.Entity<ParametroCusto>().HasKey(x => x.Id);
        builder.Entity<ParametroCusto>().Property(x => x.Preco).HasPrecision(18, 4);

        builder.Entity<MargemCanal>().HasKey(x => x.Id);
        builder.Entity<MargemCanal>().Property(x => x.Percentual).HasPrecision(18, 4);

        builder.Entity<DescontoOrcamento>().HasKey(x => x.Id);
        builder.Entity<DescontoOrcamento>().Property(x => x.Recorrente).HasPrecision(18, 4);
        builder.Entity<DescontoOrcamento>().Property(x => x.Volume).HasPrecision(18, 4);

        builder.Entity<ReferenciaMaquina>().HasKey(x => x.Id);
        builder.Entity<ReferenciaMaquina>().Property(x => x.EnergiaPorMinuto).HasPrecision(18, 4);
        builder.Entity<ReferenciaMaquina>().Property(x => x.Depreciacao).HasPrecision(18, 4);
        builder.Entity<ReferenciaMaquina>().Property(x => x.Manutencao).HasPrecision(18, 4);

        builder.Entity<NivelPintura>().HasKey(x => x.Id);
        builder.Entity<NivelPintura>().Property(x => x.Percentual).HasPrecision(18, 4);

        builder.Entity<FaixaMarketplace>().HasKey(x => x.Id);
        builder.Entity<FaixaMarketplace>().Property(x => x.De).HasPrecision(18, 2);
        builder.Entity<FaixaMarketplace>().Property(x => x.Ate).HasPrecision(18, 2);
        builder.Entity<FaixaMarketplace>().Property(x => x.Percentual).HasPrecision(18, 4);
        builder.Entity<FaixaMarketplace>().Property(x => x.Fixo).HasPrecision(18, 2);
        builder.Entity<FaixaMarketplace>().HasIndex(x => x.Canal);

        builder.Entity<OrcamentoInterno>().HasKey(x => x.Id);
        builder.Entity<OrcamentoInterno>().Property(x => x.PesoG).HasPrecision(18, 2);
        builder.Entity<OrcamentoInterno>().Property(x => x.LixoG).HasPrecision(18, 2);
        builder.Entity<OrcamentoInterno>().Property(x => x.Servico).HasPrecision(18, 2);
        builder.Entity<OrcamentoInterno>().Property(x => x.MargemSite).HasPrecision(18, 4);
        builder.Entity<OrcamentoInterno>().Property(x => x.PrecoKg).HasPrecision(18, 4);
        builder.Entity<OrcamentoInterno>().Property(x => x.EnergiaPorMinuto).HasPrecision(18, 4);
        builder.Entity<OrcamentoInterno>().Property(x => x.Depreciacao).HasPrecision(18, 4);
        builder.Entity<OrcamentoInterno>().Property(x => x.Manutencao).HasPrecision(18, 4);
        builder.Entity<OrcamentoInterno>().Property(x => x.TaxaFalha).HasPrecision(18, 4);
        builder.Entity<OrcamentoInterno>().Property(x => x.FreteCanal).HasPrecision(18, 2);
        builder.Entity<OrcamentoInterno>()
            .HasOne(x => x.Maquina)
            .WithMany()
            .HasForeignKey(x => x.MaquinaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrcamentoItem>().HasKey(x => x.Id);
        builder.Entity<OrcamentoItem>().Property(x => x.Quantidade).HasPrecision(18, 3);
        builder.Entity<OrcamentoItem>().Property(x => x.Preco).HasPrecision(18, 4);
        builder.Entity<OrcamentoItem>()
            .HasOne(x => x.OrcamentoInterno)
            .WithMany(x => x.Itens)
            .HasForeignKey(x => x.OrcamentoInternoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
