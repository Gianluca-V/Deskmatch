using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Dashboard.Dtos;
using DeskMatch.CoreService.Application.Dashboard.Interfaces;

namespace DeskMatch.CoreService.Application.Dashboard.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardResponseDto> GetHostDashboardAsync(Guid hostId, CancellationToken ct)
    {
        var isHost = await _repository.IsHostAsync(hostId, ct);
        if (!isHost)
            throw new ForbiddenException("El usuario no tiene permisos de Host.");

        // Ejecución secuencial: DbContext no es thread-safe.
        var kpis = await _repository.GetKpisAsync(hostId, ct);
        var popularWorkspaces = await _repository.GetPopularWorkspacesAsync(hostId, 3, ct);
        var recentReservations = await _repository.GetRecentReservationsAsync(hostId, 10, ct);

        var startDate = DateTime.UtcNow.Date.AddDays(-29);
        var dailyReservations = await _repository.GetDailyReservationsTrendAsync(hostId, startDate, ct);
        var revenueByWorkspace = await _repository.GetRevenueGroupedByWorkspaceAsync(hostId, ct);

        var dailyChart = FillDailyGaps(startDate, DateTime.UtcNow.Date, dailyReservations);

        return new DashboardResponseDto(
            kpis.TotalRevenue,
            kpis.ActiveReservationsCount,
            kpis.TotalWorkspacesCount,
            popularWorkspaces,
            recentReservations,
            dailyChart,
            revenueByWorkspace
        );
    }

    /// <summary>
    /// Rellena con ceros los días sin reservas para que el gráfico de líneas
    /// del frontend no presente saltos temporales engañosos.
    /// </summary>
    private static IReadOnlyList<DailyReservationDto> FillDailyGaps(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<DailyReservationDto> existing)
    {
        var dict = existing.ToDictionary(x => x.Date);
        var result = new List<DailyReservationDto>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var key = date.ToString("yyyy-MM-dd");
            result.Add(dict.TryGetValue(key, out var dto) ? dto : new DailyReservationDto(key, 0));
        }

        return result;
    }
}
