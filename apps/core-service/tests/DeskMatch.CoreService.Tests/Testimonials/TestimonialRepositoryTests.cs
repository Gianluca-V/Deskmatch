using DeskMatch.CoreService.Domain.Testimonials;
using DeskMatch.CoreService.Infrastructure.Persistence;
using DeskMatch.CoreService.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DeskMatch.CoreService.Tests.Testimonials;

public class TestimonialRepositoryTests
{
    private static CoreDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseInMemoryDatabase($"TestimonialTest_{Guid.NewGuid()}")
            .Options;

        var context = new CoreDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static Testimonial BuildTestimonial(
        string authorName,
        string role,
        string content,
        int displayOrder,
        bool isActive = true)
    {
        var testimonial = new Testimonial(Guid.NewGuid())
        {
            AuthorName = authorName,
            Role = role,
            Content = content,
            DisplayOrder = displayOrder,
            IsActive = isActive
        };
        return testimonial;
    }

    // ──────────────────────────────────────────────
    // Test 1 — GetActiveTestimonialsAsync returns only active, ordered by DisplayOrder
    // ──────────────────────────────────────────────
    [Fact]
    public async Task GetActiveTestimonialsAsync_WhenCalled_ReturnsActiveTestimonialsOrderedByDisplayOrder()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new TestimonialRepository(context);

        context.Testimonials.AddRange(
            BuildTestimonial("Juan López", "Developer", "Testimonio 1", 2),
            BuildTestimonial("Andrea Sánchez", "Diseñadora", "Testimonio 2", 1),
            BuildTestimonial("Diego Fernández", "Gestor", "Testimonio 3", 3),
            BuildTestimonial("Inactivo", "Test", "No debe aparecer", 4, isActive: false)
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetActiveTestimonialsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(t => t.DisplayOrder);
        result.First().AuthorName.Should().Be("Andrea Sánchez");
        result.Last().AuthorName.Should().Be("Diego Fernández");
    }

    // ──────────────────────────────────────────────
    // Test 2 — GetActiveTestimonialsAsync returns empty when no active testimonials
    // ──────────────────────────────────────────────
    [Fact]
    public async Task GetActiveTestimonialsAsync_WhenNoActiveTestimonials_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new TestimonialRepository(context);

        context.Testimonials.AddRange(
            BuildTestimonial("Inactivo 1", "Test", "No debe aparecer", 1, isActive: false),
            BuildTestimonial("Inactivo 2", "Test", "No debe aparecer", 2, isActive: false)
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetActiveTestimonialsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────
    // Test 3 — GetByIdAsync returns the correct testimonial
    // ──────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_WhenTestimonialExists_ReturnsTestimonial()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new TestimonialRepository(context);

        var testimonial = BuildTestimonial("Juan López", "Developer", "Test content", 1);
        context.Testimonials.Add(testimonial);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(testimonial.Id);

        // Assert
        result.Should().NotBeNull();
        result!.AuthorName.Should().Be("Juan López");
        result.Role.Should().Be("Developer");
    }

    // ──────────────────────────────────────────────
    // Test 4 — AddAsync adds testimonial to database
    // ──────────────────────────────────────────────
    [Fact]
    public async Task AddAsync_WhenCalled_AddsTestimonialToDatabase()
    {
        // Arrange
        var context = CreateDbContext();
        var repository = new TestimonialRepository(context);

        var testimonial = BuildTestimonial("Nuevo", "Test", "Nuevo testimonio", 1);

        // Act
        var result = await repository.AddAsync(testimonial);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await context.Testimonials.FindAsync(testimonial.Id);
        saved.Should().NotBeNull();
        saved!.AuthorName.Should().Be("Nuevo");
    }
}
