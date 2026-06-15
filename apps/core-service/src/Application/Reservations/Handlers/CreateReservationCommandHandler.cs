using System.Data;
using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Reservations.Commands;
using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Domain.Reservations;
using DeskMatch.CoreService.Infrastructure.Persistence;
using DeskMatch.Domain.CQRS;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DeskMatch.CoreService.Application.Reservations.Handlers;

public sealed class CreateReservationCommandHandler : ICommandHandler<CreateReservationCommand, Guid>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly CoreDbContext _dbContext;

    public CreateReservationCommandHandler(
        IWorkspaceRepository workspaceRepository,
        ICompanyRepository companyRepository,
        IReservationRepository reservationRepository,
        CoreDbContext dbContext)
    {
        _workspaceRepository = workspaceRepository;
        _companyRepository = companyRepository;
        _reservationRepository = reservationRepository;
        _dbContext = dbContext;
    }

    public async Task<Guid> HandleAsync(
        CreateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null || !workspace.IsActive)
            throw new NotFoundException("Workspace", command.WorkspaceId);

        var company = await _companyRepository.GetByIdAsync(workspace.CompanyId, cancellationToken);
        if (company is null)
            throw new NotFoundException("Company", workspace.CompanyId);

        if (company.OwnerId.HasValue && company.OwnerId.Value == command.GuestId)
            throw new ForbiddenException("No podés reservar un espacio de tu propia empresa.");

        if (command.StartTime <= DateTimeOffset.UtcNow)
            throw new BadRequestException("No se pueden crear reservas en el pasado.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        try
        {
            // TODO: US-14 — Step 1: Validar horarios operativos contra WorkspaceSchedule
            // TODO: US-14 — Step 2: Validar que no haya WorkspaceBlock activo en el período

            var hasOverlap = await _reservationRepository.HasOverlapAsync(
                command.WorkspaceId, command.StartTime, command.EndTime, cancellationToken);

            if (hasOverlap)
                throw new ConflictException("El espacio ya está reservado en ese horario.");

            var totalPrice = CalculatePrice(
                workspace.PricePerHour, workspace.PricePerDay,
                command.StartTime, command.EndTime);

            var reservation = new Reservation(Guid.NewGuid())
            {
                WorkspaceId = command.WorkspaceId,
                GuestId = command.GuestId,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                TotalPrice = totalPrice,
                Status = ReservationStatus.Confirmed
            };

            await _reservationRepository.AddAsync(reservation, cancellationToken);
            await _reservationRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return reservation.Id;
        }
        catch (PostgresException ex) when (ex.SqlState == "40001")
        {
            // Serialization failure: dos requests concurrentes ganaron la misma ventana
            throw new ConflictException("El espacio fue reservado simultáneamente. Intentá con otro horario.");
        }
        catch (ConflictException)
        {
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static decimal CalculatePrice(
        decimal pricePerHour,
        decimal? pricePerDay,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        var durationHours = (decimal)(end - start).TotalHours;

        if (pricePerDay.HasValue && durationHours >= 8)
        {
            var days = (decimal)Math.Ceiling((double)durationHours / 24);
            return pricePerDay.Value * days;
        }

        return pricePerHour * durationHours;
    }
}
