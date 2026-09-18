using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LojaWeb_2.Models;

namespace LojaWeb_2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }

        public DbSet<ProdutoCor> ProdutoCores { get; set; }

        public DbSet<ProdutoImagem> ProdutoImagens { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Estoque> Estoques { get; set; }

        public DbSet<Marca> Marcas { get; set; }

        public DbSet<Combo> Combos { get; set; }

        public DbSet<ComboItem> ComboItens { get; set; }

        public DbSet<Newsletter> Newsletters { get; set; }

        public DbSet<CampanhaNewsletter> CampanhasNewsletter { get; set; }

        public DbSet<Fornecedor> Fornecedores { get; set; }

        public DbSet<Cliente> Clientes {  get; set; }

        public DbSet<CarrinhoItem> CarrinhoItens { get; set; }

        public DbSet<CarrinhoComboItem> CarrinhoComboItens { get; set; }

        public DbSet<Venda> Vendas { get; set; }

        public DbSet<ItemVenda> ItensVenda { get; set; }

        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }

        public DbSet<LancamentoFinanceiro> LancamentosFinanceiros { get; set; }

        public DbSet<ContaReceber> ContasReceber { get; set; }

        public DbSet<ContaPagar> ContasPagar { get; set; }

        public DbSet<FechamentoCaixa> FechamentosCaixa { get; set; }

        public DbSet<ConfiguracaoLoja> ConfiguracoesLoja { get; set; }

        public DbSet<CotacaoFrete> CotacoesFrete { get; set; }

        public DbSet<CarrinhoComboSelecao> CarrinhoComboSelecoes
        {
            get;
            set;
        }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // PREÇO DO PRODUTO

            modelBuilder.Entity<Produto>()
                .Property(p => p.Preco)
                .HasPrecision(18, 2);


            // RELAÇÃO PRODUTO → CATEGORIA

            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);


            // RELAÇÃO PRODUTO → MARCA

            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Marca)
                .WithMany(m => m.Produtos)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);


            // RELAÇÃO ESTOQUE → PRODUTO

            modelBuilder.Entity<Estoque>()
                .HasOne(e => e.Produto)
                .WithMany(p => p.Estoques)
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            // RELAÇÃO ESTOQUE → MARCA

            modelBuilder.Entity<Estoque>()
                .HasOne(e => e.Marca)
                .WithMany()
                .HasForeignKey(e => e.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);


            // RELAÇÃO MOVIMENTAÇÃO → MARCA

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Marca)
                .WithMany()
                .HasForeignKey(m => m.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);


            // RELAÇÃO MOVIMENTAÇÃO → PRODUTO

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Produto)
                .WithMany()
                .HasForeignKey(m => m.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);


            // RELAÇÃO MOVIMENTAÇÃO → FORNECEDOR

            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasOne(m => m.Fornecedor)
                .WithMany()
                .HasForeignKey(m => m.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tabela Cliente já existente no banco
            modelBuilder.Entity<Cliente>()
             .ToTable("Cliente");

            // RELAÇÃO VENDA → CLIENTE
            modelBuilder.Entity<Venda>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ItemVenda → Venda
            modelBuilder.Entity<ItemVenda>()
                .HasOne(i => i.Venda)
                .WithMany(v => v.Itens)
                .HasForeignKey(i => i.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ItemVenda → Produto
            modelBuilder.Entity<ItemVenda>()
                .HasOne(i => i.Produto)
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ITEM VENDA - VALORES FINANCEIROS

            modelBuilder.Entity<ItemVenda>()
                .Property(i => i.PrecoUnitario)
                .HasPrecision(18, 2)
                .ValueGeneratedNever();

            modelBuilder.Entity<ItemVenda>()
                .Property(i => i.CustoUnitario)
                .HasPrecision(18, 2)
                .ValueGeneratedNever();

            modelBuilder.Entity<ItemVenda>()
                .Property(i => i.Subtotal)
                .HasPrecision(18, 2)
                .ValueGeneratedNever();

            // =========================================
            // FINANCEIRO - PRECISÃO MONETÁRIA
            // =========================================

            modelBuilder.Entity<LancamentoFinanceiro>()
                .Property(l => l.Valor)
                .HasPrecision(18, 2);


            modelBuilder.Entity<ContaReceber>()
                .Property(c => c.Valor)
                .HasPrecision(18, 2);


            modelBuilder.Entity<ContaPagar>()
                .Property(c => c.Valor)
                .HasPrecision(18, 2);


            modelBuilder.Entity<FechamentoCaixa>()
                .Property(c => c.SaldoInicial)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FechamentoCaixa>()
                .Property(c => c.TotalEntradas)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FechamentoCaixa>()
                .Property(c => c.TotalSaidas)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FechamentoCaixa>()
                .Property(c => c.SaldoEsperado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FechamentoCaixa>()
                .Property(c => c.SaldoInformado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FechamentoCaixa>()
                .Property(c => c.Diferenca)
                .HasPrecision(18, 2);

            // =========================================
            // CONTA A RECEBER → VENDA
            // =========================================

            modelBuilder.Entity<ContaReceber>()
                .HasOne(c => c.Venda)
                .WithMany()
                .HasForeignKey(c => c.VendaId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // CONTA A PAGAR → FORNECEDOR
            // =========================================

            modelBuilder.Entity<ContaPagar>()
                .HasOne(c => c.Fornecedor)
                .WithMany()
                .HasForeignKey(c => c.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================================
            // CARRINHO → CLIENTE
            // =========================================

            modelBuilder.Entity<CarrinhoItem>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================================
            // CARRINHO → PRODUTO
            // =========================================

            modelBuilder.Entity<CarrinhoItem>()
                .HasOne(c => c.Produto)
                .WithMany()
                .HasForeignKey(c => c.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarrinhoComboSelecao>()
                   .HasOne(x => x.CarrinhoComboItem)
                   .WithMany(x => x.Selecoes)
                   .HasForeignKey(x => x.CarrinhoComboItemId)
                   .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarrinhoComboSelecao>()
                .HasOne(x => x.ComboItem)
                .WithMany()
                .HasForeignKey(x => x.ComboItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CarrinhoComboSelecao>()
                .HasOne(x => x.Produto)
                .WithMany()
                .HasForeignKey(x => x.ProdutoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CotacaoFrete>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================================
            // RELACIONAMENTO COR → PRODUTO
            // =========================================

            modelBuilder.Entity<ProdutoCor>()
                .HasOne(pc => pc.Produto)
                .WithMany(p => p.Cores)
                .HasForeignKey(pc => pc.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);


            //Relacionamento Imagem →  Cor
            modelBuilder.Entity<ProdutoImagem>()
                .HasOne(pi => pi.ProdutoCor)
                .WithMany(pc => pc.Imagens)
                .HasForeignKey(pi => pi.ProdutoCorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProdutoCor>()
                .HasOne(pc => pc.ImagemPrincipal)
                .WithMany()
                .HasForeignKey(pc => pc.ImagemPrincipalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}