using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.Property(a => a.RowVersion).IsRowVersion();
        
        builder.ToTable(t => t.HasCheckConstraint("CK_Auction_BuyNowPrice", "[BuyNowPrice] IS NULL OR [BuyNowPrice] >= [StartPrice]"));
        
        builder.Property(a => a.StartPrice).HasPrecision(18, 2);
        builder.Property(a => a.CurrentPrice).HasPrecision(18, 2);
        builder.Property(a => a.BidIncrement).HasPrecision(18, 2);
        builder.Property(a => a.BuyNowPrice).HasPrecision(18, 2);

        builder.HasOne(a => a.Seller).WithMany().HasForeignKey(a => a.SellerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Winner).WithMany().HasForeignKey(a => a.WinnerId).OnDelete(DeleteBehavior.Restrict);
        
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

