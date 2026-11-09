
using ButterScott.Models;
using Microsoft.EntityFrameworkCore;

namespace ButterScott.Data
{
    public class ButterScottContext : DbContext
    {
        public ButterScottContext(DbContextOptions<ButterScottContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<ItemPedido> ItensPedido { get; set; } = null!;
        public DbSet<Endereco> Enderecos { get; set; } = null!;
        public DbSet<Entrega> Entregas { get; set; } = null!;
        public DbSet<Cupom> Cupons { get; set; } = null!;
        public DbSet<CupomUtilizado> CuponsUtilizados { get; set; } = null!;
        public DbSet<Favorito> Favoritos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("categorias");
                entity.HasKey(c => c.id_categoria);

                entity.Property(c => c.id_categoria)
                    .HasColumnName("id_categoria")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.nome)
                    .HasColumnName("nome")
                    .HasMaxLength(80)
                    .IsRequired();

                entity.Property(c => c.descricao)
                    .HasColumnName("descricao")
                    .HasMaxLength(255)
                    .IsRequired(false);

                entity.Property(c => c.ativa)
                    .HasColumnName("ativa")
                    .HasDefaultValue(true)
                    .IsRequired();
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("produtos");
                entity.HasKey(p => p.id_produto);

                entity.Property(p => p.id_produto)
                    .HasColumnName("id_produto")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.id_categoria)
                    .HasColumnName("id_categoria")
                    .IsRequired();

                entity.HasOne<Categoria>()
                    .WithMany()
                    .HasForeignKey(p => p.id_categoria);

                entity.Property(p => p.nome)
                    .HasColumnName("nome")
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(p => p.descricao)
                    .HasColumnName("descricao")
                    .HasColumnType("text")
                    .IsRequired(false);

                entity.Property(p => p.sabor)
                    .HasColumnName("sabor")
                    .HasMaxLength(80)
                    .IsRequired();

                entity.Property(p => p.preco)
                    .HasColumnName("preco")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(p => p.estoque)
                    .HasColumnName("estoque")
                    .HasDefaultValue(0u)
                    .IsRequired();

                entity.Property(p => p.imagem_url)
                    .HasColumnName("imagem_url")
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(p => p.ativo)
                    .HasColumnName("ativo")
                    .HasDefaultValue(true)
                    .IsRequired();

                entity.Property(p => p.data_criacao)
                    .HasColumnName("data_criacao")
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.data_atualizacao)
                    .HasColumnName("data_atualizacao")
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("clientes");
                entity.HasKey(c => c.id_cliente);

                entity.Property(c => c.id_cliente)
                    .HasColumnName("id_cliente")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.nome)
                    .HasColumnName("nome")
                    .HasMaxLength(120)
                    .IsRequired();

                entity.Property(c => c.email)
                    .HasColumnName("email")
                    .HasMaxLength(180)
                    .IsRequired();

                entity.HasIndex(c => c.email)
                    .IsUnique();

                entity.Property(c => c.senha_hash)
                    .HasColumnName("senha_hash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(c => c.telefone)
                    .HasColumnName("telefone")
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(c => c.data_cadastro)
                    .HasColumnName("data_cadastro")
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.ativo)
                    .HasColumnName("ativo")
                    .HasDefaultValue(true)
                    .IsRequired();
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("pedidos");
                entity.HasKey(p => p.id_pedido);

                entity.Property(p => p.id_pedido)
                    .HasColumnName("id_pedido")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.id_cliente)
                    .HasColumnName("id_cliente")
                    .IsRequired();

                entity.Property(p => p.data_pedido)
                    .HasColumnName("data_pedido")
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.status)
                    .HasColumnName("status")
                    .HasColumnType("enum('Pendente','Confirmado','Em preparo','Saiu para entrega','Entregue','Cancelado')")
                    .HasDefaultValue("Pendente")
                    .IsRequired();

                entity.Property(p => p.valor_subtotal)
                    .HasColumnName("valor_subtotal")
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0m);

                entity.Property(p => p.valor_entrega)
                    .HasColumnName("valor_entrega")
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0m);

                entity.Property(p => p.valor_desconto)
                    .HasColumnName("valor_desconto")
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0m);

                entity.Property(p => p.valor_total)
                    .HasColumnName("valor_total")
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0m);

                entity.Property(p => p.observacoes)
                    .HasColumnName("observacoes")
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(p => p.id_cliente);
            });

            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.ToTable("itens_pedido");
                entity.HasKey(i => i.id_item);

                entity.Property(i => i.id_item)
                    .HasColumnName("id_item")
                    .ValueGeneratedOnAdd();

                entity.Property(i => i.id_pedido)
                    .HasColumnName("id_pedido")
                    .IsRequired();

                entity.Property(i => i.id_produto)
                    .HasColumnName("id_produto")
                    .IsRequired();

                entity.Property(i => i.quantidade)
                    .HasColumnName("quantidade")
                    .IsRequired();

                entity.Property(i => i.preco_unitario)
                    .HasColumnName("preco_unitario")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(i => i.subtotal)
                    .HasColumnName("subtotal")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.HasOne<Pedido>()
                    .WithMany()
                    .HasForeignKey(i => i.id_pedido);

                entity.HasOne<Produto>()
                    .WithMany()
                    .HasForeignKey(i => i.id_produto);
            });

            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.ToTable("enderecos");
                entity.HasKey(e => e.id_endereco);

                entity.Property(e => e.id_endereco)
                    .HasColumnName("id_endereco")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.id_cliente)
                    .HasColumnName("id_cliente")
                    .IsRequired();

                entity.Property(e => e.identificacao)
                    .HasColumnName("identificacao")
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.rua)
                    .HasColumnName("rua")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.numero)
                    .HasColumnName("numero")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.complemento)
                    .HasColumnName("complemento")
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.bairro)
                    .HasColumnName("bairro")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.cidade)
                    .HasColumnName("cidade")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.estado)
                    .HasColumnName("estado")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.cep)
                    .HasColumnName("cep")
                    .HasMaxLength(15)
                    .IsRequired();

                entity.Property(e => e.principal)
                    .HasColumnName("principal")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(e => e.id_cliente);
            });

            modelBuilder.Entity<Entrega>(entity =>
            {
                entity.ToTable("entregas");
                entity.HasKey(e => e.id_entrega);

                entity.Property(e => e.id_entrega)
                    .HasColumnName("id_entrega")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.id_pedido)
                    .HasColumnName("id_pedido")
                    .IsRequired();

                entity.Property(e => e.id_endereco)
                    .HasColumnName("id_endereco")
                    .IsRequired();

                entity.Property(e => e.status)
                    .HasColumnName("status")
                    .HasMaxLength(30)
                    .HasDefaultValue("Aguardando")
                    .IsRequired();

                entity.Property(e => e.codigo_rastreamento)
                    .HasColumnName("codigo_rastreamento")
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.data_saida)
                    .HasColumnName("data_saida")
                    .IsRequired(false);

                entity.Property(e => e.data_entrega)
                    .HasColumnName("data_entrega")
                    .IsRequired(false);

                entity.Property(e => e.observacoes)
                    .HasColumnName("observacoes")
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.HasOne<Pedido>()
                    .WithMany()
                    .HasForeignKey(e => e.id_pedido);

                entity.HasOne<Endereco>()
                    .WithMany()
                    .HasForeignKey(e => e.id_endereco);
            });

            // Mapeamento de cupons
            modelBuilder.Entity<Cupom>(entity =>
            {
                entity.ToTable("cupons");
                entity.HasKey(c => c.id_cupom);

                entity.Property(c => c.id_cupom)
                    .HasColumnName("id_cupom")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.codigo)
                    .HasColumnName("codigo")
                    .HasMaxLength(40)
                    .IsRequired();

                entity.HasIndex(c => c.codigo)
                    .IsUnique();

                entity.Property(c => c.descricao)
                    .HasColumnName("descricao")
                    .HasMaxLength(255)
                    .IsRequired(false);

                entity.Property(c => c.tipo_desconto)
                    .HasColumnName("tipo_desconto")
                    .HasColumnType("enum('Percentual','ValorFixo')")
                    .IsRequired();

                entity.Property(c => c.valor_desconto)
                    .HasColumnName("valor_desconto")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(c => c.valor_minimo_pedido)
                    .HasColumnName("valor_minimo_pedido")
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0m);

                entity.Property(c => c.data_inicio)
                    .HasColumnName("data_inicio")
                    .HasColumnType("datetime")
                    .IsRequired(false);

                entity.Property(c => c.data_fim)
                    .HasColumnName("data_fim")
                    .HasColumnType("datetime")
                    .IsRequired(false);

                entity.Property(c => c.limite_uso)
                    .HasColumnName("limite_uso")
                    .IsRequired(false);

                entity.Property(c => c.usos_realizados)
                    .HasColumnName("usos_realizados")
                    .HasDefaultValue(0u)
                    .IsRequired();

                entity.Property(c => c.ativo)
                    .HasColumnName("ativo")
                    .HasDefaultValue(true)
                    .IsRequired();
            });

            // Registro de utilização dos cupons
            modelBuilder.Entity<CupomUtilizado>(entity =>
            {
                entity.ToTable("cupons_utilizados");

                entity.HasKey(c => new { c.id_cupom, c.id_pedido });

                entity.Property(c => c.id_cupom)
                    .HasColumnName("id_cupom");

                entity.Property(c => c.id_pedido)
                    .HasColumnName("id_pedido");

                entity.Property(c => c.id_cliente)
                    .HasColumnName("id_cliente")
                    .IsRequired();

                entity.Property(c => c.data_utilizacao)
                    .HasColumnName("data_utilizacao")
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAdd();

                entity.Property(c => c.valor_desconto_aplicado)
                    .HasColumnName("valor_desconto_aplicado")
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.HasOne<Cupom>()
                    .WithMany()
                    .HasForeignKey(c => c.id_cupom);

                entity.HasOne<Pedido>()
                    .WithMany()
                    .HasForeignKey(c => c.id_pedido);

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(c => c.id_cliente);
            });

            // Mapeamento de favoritos
            modelBuilder.Entity<Favorito>(entity =>
            {
                entity.ToTable("favoritos");

                entity.HasKey(f => new { f.id_cliente, f.id_produto });

                entity.Property(f => f.id_cliente)
                    .HasColumnName("id_cliente")
                    .IsRequired();

                entity.Property(f => f.id_produto)
                    .HasColumnName("id_produto")
                    .IsRequired();

                entity.Property(f => f.data_adicao)
                    .HasColumnName("data_adicao")
                    .HasColumnType("datetime")
                    .ValueGeneratedOnAdd();

                entity.HasOne<Cliente>()
                    .WithMany()
                    .HasForeignKey(f => f.id_cliente);

                entity.HasOne<Produto>()
                    .WithMany()
                    .HasForeignKey(f => f.id_produto);
            });
        }
    }
}
