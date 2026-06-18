using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Workspaces;
using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Handlers;

public sealed class CreateWorkspaceBlockCommandHandler : ICommandHandler<CreateWorkspaceBlockCommand, Guid>
{
    private readonly IWorkspaceBlockRepository _blockRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreateWorkspaceBlockCommandHandler(
        IWorkspaceBlockRepository blockRepository,
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository)
    {
        _blockRepository = blockRepository;
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Guid> HandleAsync(
        CreateWorkspaceBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        await VerifyOwnershipAsync(command.WorkspaceId, command.UserId, cancellationToken);

        if (command.BlockEnd <= command.BlockStart)
            throw new ArgumentException("BlockEnd debe ser posterior a BlockStart.");

        var block = new WorkspaceBlock(Guid.NewGuid())
        {
            WorkspaceId = command.WorkspaceId,
            BlockStart = command.BlockStart.UtcDateTime,
            BlockEnd = command.BlockEnd.UtcDateTime,
            Reason = command.Reason
        };

        return await _blockRepository.AddAsync(block, cancellationToken);
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