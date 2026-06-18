using DeskMatch.CoreService.Domain.Testimonials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.AuthorName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Role)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(t => t.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(t => t.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // ── Indexes ──
        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_Testimonials_IsActive");

        builder.HasIndex(t => t.DisplayOrder)
            .HasDatabaseName("IX_Testimonials_DisplayOrder");
    }
}
