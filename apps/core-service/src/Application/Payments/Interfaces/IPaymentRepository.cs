using DeskMatch.CoreService.Domain.Payments;

namespace DeskMatch.CoreService.Application.Payments.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
