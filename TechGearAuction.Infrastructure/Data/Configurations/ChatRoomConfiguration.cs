using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.HasIndex(c => c.AuctionId).IsUnique().HasFilter("[DeletedAt] IS NULL");
        
        builder.HasQueryFilter(e => e.DeletedAt == null);
    }
}

