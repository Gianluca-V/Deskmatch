using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", t =>
            t.HasCheckConstraint("CK_Reservation_Dates", "\"EndTime\" > \"StartTime\""));

        builder.HasKey(r => r.Id);

        builder.Property(r => r.WorkspaceId)
            .IsRequired();

        builder.Property(r => r.GuestId)
            .IsRequired();

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.DepositPercentage)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(r => r.DepositAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.DepositPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.FullyPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(r => new { r.WorkspaceId, r.StartTime, r.EndTime })
            .HasDatabaseName("IX_Reservations_Workspace_Dates");

        builder.HasOne<Workspace>()
            .WithMany()
            .HasForeignKey(r => r.WorkspaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
