using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using DeskMatch.Domain.CQRS;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class UpdateWeeklyScheduleCommandHandler : ICommandHandler<UpdateWeeklyScheduleCommand>
{
    private readonly CoreDbContext _context;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICompanyRepository _companyRepository;

    public UpdateWeeklyScheduleCommandHandler(
        CoreDbContext context,
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository)
    {
        _context = context;
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
    }

    public async Task HandleAsync(
        UpdateWeeklyScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        await VerifyOwnershipAsync(command.WorkspaceId, command.UserId, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var day in command.Days)
            {
                var existing = await _context.WorkspaceSchedules
                    .FirstOrDefaultAsync(s =>
                        s.WorkspaceId == command.WorkspaceId &&
                        s.DayOfWeek == day.DayOfWeek,
                        cancellationToken);

                if (existing is not null)
                {
                    existing.OpenTime = TimeOnly.Parse(day.OpenTime);
                    existing.CloseTime = TimeOnly.Parse(day.CloseTime);
                    existing.IsAvailable = day.IsAvailable;
                }
                else
                {
                    var newSchedule = new WorkspaceSchedule(Guid.NewGuid())
                    {
                        WorkspaceId = command.WorkspaceId,
                        DayOfWeek = day.DayOfWeek,
                        OpenTime = TimeOnly.Parse(day.OpenTime),
                        CloseTime = TimeOnly.Parse(day.CloseTime),
                        IsAvailable = day.IsAvailable
                    };
                    await _context.WorkspaceSchedules.AddAsync(newSchedule, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task VerifyOwnershipAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException("Workspace no encontrado.");

        var company = await _companyRepository.GetByOwnerIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("No pertenecés a ninguna empresa.");

        if (workspace.CompanyId != company.Id)
            throw new UnauthorizedAccessException("No tenés permiso para modificar este workspace.");
    }
}