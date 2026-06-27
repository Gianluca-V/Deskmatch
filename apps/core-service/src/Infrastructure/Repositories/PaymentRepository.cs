using DeskMatch.CoreService.Application.Payments.Interfaces;
using DeskMatch.CoreService.Domain.Payments;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly CoreDbContext _dbContext;

    public PaymentRepository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Where(p => p.ReservationId == reservationId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
