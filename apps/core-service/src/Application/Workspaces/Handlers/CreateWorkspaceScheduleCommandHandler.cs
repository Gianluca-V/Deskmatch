using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class CreateWorkspaceScheduleCommandHandler : ICommandHandler<CreateWorkspaceScheduleCommand, Guid>
{
    private readonly IWorkspaceScheduleRepository _scheduleRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreateWorkspaceScheduleCommandHandler(
        IWorkspaceScheduleRepository scheduleRepository,
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository)
    {
        _scheduleRepository = scheduleRepository;
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Guid> HandleAsync(
        CreateWorkspaceScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        await VerifyOwnershipAsync(command.WorkspaceId, command.UserId, cancellationToken);

        var schedule = new WorkspaceSchedule(Guid.NewGuid())
        {
            WorkspaceId = command.WorkspaceId,
            DayOfWeek = command.DayOfWeek,
            OpenTime = TimeOnly.Parse(command.OpenTime),
            CloseTime = TimeOnly.Parse(command.CloseTime),
            IsAvailable = command.IsAvailable
        };

        await _scheduleRepository.AddAsync(schedule, cancellationToken);
        return schedule.Id;
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