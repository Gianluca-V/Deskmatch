using DeskMatch.CoreService.Application.Payments.Interfaces;
using DeskMatch.CoreService.Domain.Payments;
using DeskMatch.CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly CoreDbContext _dbContext;

    public PaymentMethodRepository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PaymentMethod method, CancellationToken cancellationToken = default)
    {
        await _dbContext.PaymentMethods.AddAsync(method, cancellationToken);
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethod>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PaymentMethods
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var method = await _dbContext.PaymentMethods.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (method != null)
        {
            _dbContext.PaymentMethods.Remove(method);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
