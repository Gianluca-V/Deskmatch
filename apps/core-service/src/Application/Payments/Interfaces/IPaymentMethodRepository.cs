using DeskMatch.CoreService.Domain.Payments;

namespace DeskMatch.CoreService.Application.Payments.Interfaces;

public interface IPaymentMethodRepository
{
    Task AddAsync(PaymentMethod method, CancellationToken cancellationToken = default);
    Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentMethod>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
