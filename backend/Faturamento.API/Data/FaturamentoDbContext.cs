using Faturamento.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.API.Data;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options)
        : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNota> ItensNota => Set<ItemNota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Numero).IsRequired();
            entity.HasIndex(n => n.Numero).IsUnique();
            entity.Property(n => n.Status).IsRequired().HasConversion<int>();
            entity.Property(n => n.DataCriacao).IsRequired();

            entity.HasMany(n => n.Itens)
                  .WithOne()
                  .HasForeignKey(i => i.NotaFiscalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemNota>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProdutoDescricao).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Quantidade).IsRequired();
        });
    }
}