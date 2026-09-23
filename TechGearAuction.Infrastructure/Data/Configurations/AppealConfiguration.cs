using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data.Configurations;

public class AppealConfiguration : IEntityTypeConfiguration<Appeal>
{
    public void Configure(EntityTypeBuilder<Appeal> builder)
    {
        builder.HasIndex(a => a.ReportId).IsUnique().HasFilter("[DeletedAt] IS NULL");
        
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

