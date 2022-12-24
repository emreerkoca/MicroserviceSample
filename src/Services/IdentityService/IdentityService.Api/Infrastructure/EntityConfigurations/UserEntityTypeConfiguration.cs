using IdentityService.Api.Core.Domain;
using IdentityService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Api.Infrastructure.EntityConfigurations
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User", IdentityContext.DEFAULT_SCHEMA);

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Email)
                .IsRequired();

            builder.Property(m => m.Password)
                .IsRequired();

            builder.Property(m => m.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(m => m.IsEmailConfirmed)
                .HasDefaultValue(false);

            builder.Property(m => m.CreateDate)
                .IsRequired();
        }
    }
}
