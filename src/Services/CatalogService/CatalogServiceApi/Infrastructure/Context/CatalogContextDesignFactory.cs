using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogServiceApi.Infrastructure.Context
{
    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
                .UseSqlServer("Data Source=localhost,1433;Persist Security Info=True;Initial Catalog=CatalogDb;User ID=sa;Password=123456Ee");

            return new CatalogContext(optionsBuilder.Options);
        }
    }
}
