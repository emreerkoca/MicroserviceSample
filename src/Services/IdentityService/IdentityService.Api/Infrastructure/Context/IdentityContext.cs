using IdentityService.Api.Core.Domain;
using IdentityService.Api.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Infrastructure.Context
{
    public class IdentityContext : DbContext
    {
        public const string DEFAULT_SCHEMA = "identity";

        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        }
    }
}
