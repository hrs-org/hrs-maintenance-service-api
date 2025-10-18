using HRS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRS.Infrastructure.Configuration;

public class ItemMaintenanceConfiguration : IEntityTypeConfiguration<ItemMaintenance>
{
    public void Configure(EntityTypeBuilder<ItemMaintenance> builder)
    {
        builder.ToTable("ItemMaintenances");
        builder.HasKey(x => x.Id);

        // 仅标量列与索引（无导航关系）
        builder.HasIndex(x => new { x.ItemId, x.Type });
        builder.HasIndex(x => x.RentalOrderId);

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Quantity).IsRequired();

        builder.Property(x => x.Remarks).HasMaxLength(250);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
            .ValueGeneratedOnAddOrUpdate();
    }
}
