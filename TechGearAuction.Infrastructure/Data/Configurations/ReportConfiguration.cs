using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.ReportedUser).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

