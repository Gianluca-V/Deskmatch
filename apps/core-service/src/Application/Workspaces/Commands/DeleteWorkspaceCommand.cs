using DeskMatch.Domain.CQRS;

namespace DeskMatch.CoreService.Application.Workspaces.Commands;

public sealed record DeleteWorkspaceCommand(Guid Id) : ICommand;
