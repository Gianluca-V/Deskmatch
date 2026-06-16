using DeskMatch.CoreService.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeskMatch.CoreService.Infrastructure.Persistence.Configurations;

public class WorkspaceBlockConfiguration : IEntityTypeConfiguration<WorkspaceBlock>
{
    public void Configure(EntityTypeBuilder<WorkspaceBlock> builder)
    {
        builder.ToTable("WorkspaceBlocks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BlockStart).IsRequired();
        builder.Property(x => x.BlockEnd).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500);

        builder.HasCheckConstraint("CK_WorkspaceBlock_Times", "\"BlockEnd\" > \"BlockStart\"");

        builder.HasOne(x => x.Workspace)
               .WithMany()
               .HasForeignKey(x => x.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}