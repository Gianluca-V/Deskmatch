using DeskMatch.AuthService.Application.Users.Dtos;
using FluentValidation;

namespace DeskMatch.AuthService.Application.Users.Validators;

public class UserUpdateProfileDtoValidator : AbstractValidator<UserUpdateProfileDto>
{
    public UserUpdateProfileDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("El nombre completo es requerido.")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("El teléfono no puede superar los 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("La ubicación no puede superar los 100 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}
