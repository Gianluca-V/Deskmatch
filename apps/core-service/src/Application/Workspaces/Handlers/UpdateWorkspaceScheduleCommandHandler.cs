using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class UpdateWorkspaceScheduleCommandHandler : ICommandHandler<UpdateWorkspaceScheduleCommand>
{
    private readonly IWorkspaceScheduleRepository _scheduleRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICompanyRepository _companyRepository;

    public UpdateWorkspaceScheduleCommandHandler(
        IWorkspaceScheduleRepository scheduleRepository,
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository)
    {
        _scheduleRepository = scheduleRepository;
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
    }

    public async Task HandleAsync(
        UpdateWorkspaceScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        await VerifyOwnershipAsync(command.WorkspaceId, command.UserId, cancellationToken);

        var schedule = await _scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken)
            ?? throw new KeyNotFoundException("Horario no encontrado.");

        schedule.DayOfWeek = command.DayOfWeek;
        schedule.OpenTime = TimeOnly.Parse(command.OpenTime);
        schedule.CloseTime = TimeOnly.Parse(command.CloseTime);
        schedule.IsAvailable = command.IsAvailable;

        await _scheduleRepository.UpdateAsync(schedule, cancellationToken);
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