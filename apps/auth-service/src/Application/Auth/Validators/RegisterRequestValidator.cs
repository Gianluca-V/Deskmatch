using FluentValidation;

namespace DeskMatch.AuthService.Application.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Debe contener al menos una mayúscula.")
            .Matches("[a-z]").WithMessage("Debe contener al menos una minúscula.")
            .Matches("[0-9]").WithMessage("Debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Debe contener al menos un carácter especial.");

        RuleFor(x => x.Name)
            .MaximumLength(256).WithMessage("El nombre no puede superar los 256 caracteres.")
            .When(x => x.Name is not null);

        RuleFor(x => x.FirstName)
            .MaximumLength(128).WithMessage("El nombre no puede superar los 128 caracteres.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(128).WithMessage("El apellido no puede superar los 128 caracteres.")
            .When(x => x.LastName is not null);
    }
}