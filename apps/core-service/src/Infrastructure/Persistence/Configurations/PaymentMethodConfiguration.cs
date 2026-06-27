using DeskMatch.CoreService.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.UserId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pm => pm.LastFourDigits)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(pm => pm.ExpiryMonth)
            .IsRequired();

        builder.Property(pm => pm.ExpiryYear)
            .IsRequired();

        builder.Property(pm => pm.CardHolderName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pm => pm.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(pm => pm.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(pm => pm.UserId)
            .HasDatabaseName("IX_PaymentMethods_UserId");
    }
}
