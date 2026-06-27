using DeskMatch.CoreService.Domain.Payments;
using DeskMatch.CoreService.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReservationId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PaymentType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.PaymentMethod)
            .HasMaxLength(256);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.PaidAt);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(p => p.ReservationId)
            .HasDatabaseName("IX_Payments_ReservationId");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_Payments_UserId");

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
