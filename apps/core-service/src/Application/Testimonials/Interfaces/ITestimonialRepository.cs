using DeskMatch.CoreService.Domain.Testimonials;
using DeskMatch.Domain.Abstractions;

namespace DeskMatch.CoreService.Application.Testimonials.Interfaces;

public interface ITestimonialRepository : IRepository<Testimonial, Guid>
{
    Task<IReadOnlyList<Testimonial>> GetActiveTestimonialsAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
