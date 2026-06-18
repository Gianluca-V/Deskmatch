using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class DeleteWorkspaceBlockCommandHandler : ICommandHandler<DeleteWorkspaceBlockCommand>
{
    private readonly IWorkspaceBlockRepository _blockRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICompanyRepository _companyRepository;

    public DeleteWorkspaceBlockCommandHandler(
        IWorkspaceBlockRepository blockRepository,
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository)
    {
        _blockRepository = blockRepository;
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
    }

    public async Task HandleAsync(
        DeleteWorkspaceBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        await VerifyOwnershipAsync(command.WorkspaceId, command.UserId, cancellationToken);

        var block = await _blockRepository.GetByIdAsync(command.BlockId, cancellationToken)
            ?? throw new KeyNotFoundException("Bloqueo no encontrado.");

        await _blockRepository.DeleteAsync(command.BlockId, cancellationToken);
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