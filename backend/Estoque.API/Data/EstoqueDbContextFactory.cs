using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Estoque.API.Data;

public class EstoqueDbContextFactory : IDesignTimeDbContextFactory<EstoqueDbContext>
{
    public EstoqueDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EstoqueDbContext>();
        optionsBuilder.UseMySql(
            "Server=localhost;Port=3306;Database=korp_estoque;User=root;Password=root;",
            ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=korp_estoque;User=root;Password=root;")
        );
        return new EstoqueDbContext(optionsBuilder.Options);
    }
}