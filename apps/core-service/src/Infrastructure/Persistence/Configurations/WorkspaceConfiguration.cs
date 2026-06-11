using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("Workspaces");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(w => w.Description)
            .HasColumnType("text");

        builder.Property(w => w.Address)
            .HasMaxLength(512);

        builder.Property(w => w.City)
            .HasMaxLength(128);

        builder.Property(w => w.Country)
            .HasMaxLength(128);

        builder.Property(w => w.Capacity)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(w => w.PricePerHour)
            .IsRequired()
            .HasPrecision(10, 2)
            .HasDefaultValue(0);

        builder.Property(w => w.PricePerDay)
            .HasPrecision(10, 2);

        builder.Property(w => w.PricePerMonth)
            .HasPrecision(10, 2);

        builder.Property(w => w.Amenities)
            .HasColumnType("text[]");

        builder.Property(w => w.Images)
            .HasColumnType("text[]");

        builder.Property(w => w.Rating)
            .HasPrecision(3, 2);

        builder.Property(w => w.ReviewCount)
            .HasDefaultValue(0);

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Ignore(w => w.DynamicAttributes);
    }
}
