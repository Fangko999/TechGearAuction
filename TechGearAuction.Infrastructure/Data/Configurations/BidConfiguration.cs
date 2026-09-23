using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasIndex(b => new { b.AuctionId, b.BidAmount }).IsUnique().HasFilter("[DeletedAt] IS NULL");
        
        builder.Property(b => b.BidAmount).HasPrecision(18, 2);

        builder.HasOne(b => b.Bidder).WithMany().HasForeignKey(b => b.BidderId).OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

