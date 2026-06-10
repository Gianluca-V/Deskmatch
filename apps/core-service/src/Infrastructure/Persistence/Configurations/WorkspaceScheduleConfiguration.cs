using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class WorkspaceScheduleConfiguration : IEntityTypeConfiguration<WorkspaceSchedule>
{
    public void Configure(EntityTypeBuilder<WorkspaceSchedule> builder)
    {
        builder.ToTable("WorkspaceSchedules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek).IsRequired();
        builder.Property(x => x.OpenTime).IsRequired();
        builder.Property(x => x.CloseTime).IsRequired();
        builder.Property(x => x.IsAvailable).HasDefaultValue(true);

        builder.HasOne(x => x.Workspace)
               .WithMany()
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}