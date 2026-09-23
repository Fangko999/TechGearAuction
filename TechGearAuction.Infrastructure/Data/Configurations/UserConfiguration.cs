using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(255);
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

