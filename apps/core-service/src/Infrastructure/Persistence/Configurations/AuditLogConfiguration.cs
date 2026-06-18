using DeskMatch.CoreService.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.TargetType)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(a => a.Reason)
            .HasMaxLength(512);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}