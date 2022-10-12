using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogServiceApi.Infrastructure.Context
{
    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
                .UseSqlServer("Server=container_sqlserver;Integrated Security=true;Initial Catalog=MicroserviceSampleCatalog;User ID=sa;Password=123456!");

            return new CatalogContext(optionsBuilder.Options);
        }
    }
}
