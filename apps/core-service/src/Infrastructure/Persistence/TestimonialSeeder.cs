using DeskMatch.CoreService.Domain.Testimonials;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Infrastructure.Persistence;

public static class TestimonialSeeder
{
    private static readonly IReadOnlyList<(string AuthorName, string Role, string Content, int DisplayOrder)> SeedData =
    [
        ("Juan López", "Developer",
         "Perfecto para trabajar desde diferentes lugares. La interfaz es intuitiva y rápida.",
         1),
        ("Andrea Sánchez", "Diseñadora Gráfica",
         "Encontré el espacio ideal para mi estudio. Proceso de reserva muy sencillo.",
         2),
        ("Diego Fernández", "Gestor Empresarial",
         "Excelente herramienta para maximizar el uso de nuestros espacios ociosos.",
         3)
    ];

    public static async Task SeedTestimonialsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();

        if (await db.Testimonials.AnyAsync())
            return; // Ya hay datos, no duplicar

        var testimonials = SeedData.Select(data => new Testimonial(Guid.NewGuid())
        {
            AuthorName = data.AuthorName,
            Role = data.Role,
            Content = data.Content,
            DisplayOrder = data.DisplayOrder,
            IsActive = true
        }).ToList();

        db.Testimonials.AddRange(testimonials);
        await db.SaveChangesAsync();
    }
}
