using DeskMatch.BuildingBlocks.Exceptions;
using DeskMatch.CoreService.Application.Companies.Commands;
using Xunit;
using DeskMatch.CoreService.Application.Companies.Handlers;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Companies.Validators;
using DeskMatch.CoreService.Domain.Companies;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;

namespace DeskMatch.CoreService.Tests.Companies;

public class CompanyProfileTests
{
    private readonly Mock<ICompanyRepository> _repositoryMock;
    private readonly UpdateCompanyProfileCommandHandler _handler;
    private readonly CompanyUpdateProfileDtoValidator _validator;

    public CompanyProfileTests()
    {
        _repositoryMock = new Mock<ICompanyRepository>();
        _handler = new UpdateCompanyProfileCommandHandler(_repositoryMock.Object);
        _validator = new CompanyUpdateProfileDtoValidator();
    }

    // Test 6 — Obtener perfil empresa (verifica que el handler proyecta correctamente)
    [Fact]
    public async Task HandleAsync_WhenCompanyExists_ReturnsCorrectProfile()
    {
        var ownerId = Guid.NewGuid();
        var company = BuildCompany(ownerId, name: "Acme Corp", description: "Descripción",
            contactEmail: "contacto@acme.com", websiteUrl: "https://acme.com");

        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateCompanyProfileCommand(
            ownerId, "Acme Corp", "Descripción", "contacto@acme.com", "https://acme.com", null, null);

        var result = await _handler.HandleAsync(command);

        result.Name.Should().Be("Acme Corp");
        result.ContactEmail.Should().Be("contacto@acme.com");
        result.WebsiteUrl.Should().Be("https://acme.com");
        result.IsVerified.Should().BeFalse();
    }

    // Test 7 — Actualizar perfil empresa
    [Fact]
    public async Task HandleAsync_UpdatesCompanyAndCallsSaveChanges()
    {
        var ownerId = Guid.NewGuid();
        var company = BuildCompany(ownerId, name: "Viejo Nombre");

        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateCompanyProfileCommand(
            ownerId, "Nuevo Nombre", "Nueva descripción", "nuevo@email.com", "https://nuevo.com", null, null);

        var result = await _handler.HandleAsync(command);

        result.Name.Should().Be("Nuevo Nombre");
        result.Description.Should().Be("Nueva descripción");
        _repositoryMock.Verify(r => r.Update(It.IsAny<Company>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test 8 — Rechazar descripción > 500 caracteres
    [Fact]
    public void Validator_WhenDescriptionExceeds500Chars_HasValidationError()
    {
        var dto = new Application.Companies.Dtos.CompanyUpdateProfileDto(
            "Empresa", new string('A', 501), null, null, null, null);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    // Test 9 — Rechazar email inválido
    [Theory]
    [InlineData("no-es-email")]
    [InlineData("@sindominio.com")]
    [InlineData("sin-arroba")]
    public void Validator_WhenContactEmailInvalid_HasValidationError(string email)
    {
        var dto = new Application.Companies.Dtos.CompanyUpdateProfileDto(
            "Empresa", "Desc", email, null, null, null);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }

    // Test 10 — Rechazar URL inválida
    [Theory]
    [InlineData("ftp://sitio.com")]   // protocolo no válido
    [InlineData("sitio.com")]          // sin protocolo
    [InlineData("http//malformada")]   // malformada
    public void Validator_WhenWebsiteUrlInvalid_HasValidationError(string url)
    {
        var dto = new Application.Companies.Dtos.CompanyUpdateProfileDto(
            "Empresa", "Desc", null, url, null, null);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl);
    }

    [Theory]
    [InlineData("https://acme.com")]
    [InlineData("http://acme.com")]
    [InlineData("https://sub.acme.com/path")]
    public void Validator_WhenWebsiteUrlValid_HasNoValidationError(string url)
    {
        var dto = new Application.Companies.Dtos.CompanyUpdateProfileDto(
            "Empresa", "Desc", null, url, null, null);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.WebsiteUrl);
    }

    // Test 11 — Devolver 404 cuando la empresa no existe (equivalente a no ser owner)
    [Fact]
    public async Task HandleAsync_WhenCompanyNotFoundForOwner_ThrowsNotFoundException()
    {
        var ownerId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var command = new UpdateCompanyProfileCommand(ownerId, "Nombre", "Desc", null, null, null, null);

        var act = () => _handler.HandleAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test 12 — No permitir modificar IsVerified
    [Fact]
    public async Task HandleAsync_DoesNotModifyIsVerified()
    {
        var ownerId = Guid.NewGuid();
        var company = BuildCompany(ownerId, name: "Empresa");
        company.IsVerified = false;

        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateCompanyProfileCommand(
            ownerId, "Empresa", "Desc", null, null, null, null);

        var result = await _handler.HandleAsync(command);

        result.IsVerified.Should().BeFalse();
        company.IsVerified.Should().BeFalse();
    }

    // Test 13 — Sanitización XSS en Description y Name
    [Fact]
    public async Task HandleAsync_StripesHtmlTagsFromDescriptionAndName()
    {
        var ownerId = Guid.NewGuid();
        var company = BuildCompany(ownerId, name: "Empresa");

        _repositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateCompanyProfileCommand(
            ownerId,
            "<b>Empresa</b>",
            "<script>alert('xss')</script>Descripción legítima",
            null,
            null,
            null,
            null);

        var result = await _handler.HandleAsync(command);

        result.Name.Should().NotContain("<b>");
        result.Name.Should().Contain("Empresa");
        result.Description.Should().NotContain("<script>");
        result.Description.Should().Contain("Descripción legítima");
    }

    private static Company BuildCompany(
        Guid ownerId,
        string name = "Test Company",
        string? description = null,
        string? contactEmail = null,
        string? websiteUrl = null)
    {
        var company = new Company(Guid.NewGuid())
        {
            Name = name,
            Description = description,
            ContactEmail = contactEmail,
            WebsiteUrl = websiteUrl,
            OwnerId = ownerId,
            IsActive = true
        };
        return company;
    }
}
