using DeskMatch.CoreService.Application.Companies.Dtos;
using FluentValidation;

namespace DeskMatch.CoreService.Application.Companies.Validators;

public class CompanyUpdateProfileDtoValidator : AbstractValidator<CompanyUpdateProfileDto>
{
    public CompanyUpdateProfileDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la empresa es requerido.")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("El email de contacto es requerido.")
            .EmailAddress().WithMessage("El email de contacto no tiene un formato válido.");

        RuleFor(x => x.WebsiteUrl)
            .Must(BeAValidUrl).WithMessage("El sitio web debe ser una URL válida con http o https.")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
