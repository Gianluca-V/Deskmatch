using DeskMatch.CoreService.Application.Reservations.Dtos;
using FluentValidation;

namespace DeskMatch.CoreService.Application.Reservations.Validators;

public sealed class CreateReservationRequestDtoValidator : AbstractValidator<CreateReservationRequestDto>
{
    public CreateReservationRequestDtoValidator()
    {
        RuleFor(x => x.StartTime)
            .GreaterThan(_ => DateTimeOffset.UtcNow)
            .WithMessage("La fecha de inicio debe ser futura.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("La fecha de fin debe ser posterior a la de inicio.");

        RuleFor(x => x)
            .Must(x => (x.EndTime - x.StartTime).TotalHours >= 1)
            .WithMessage("La duración mínima de una reserva es 1 hora.")
            .When(x => x.EndTime > x.StartTime);
    }
}
