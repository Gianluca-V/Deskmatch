using DeskMatch.AuthService.Application.Users;
using DeskMatch.AuthService.Application.Users.Dtos;
using Xunit;
using DeskMatch.AuthService.Application.Users.Validators;
using DeskMatch.AuthService.Infrastructure.Identity;
using DeskMatch.BuildingBlocks.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DeskMatch.AuthService.Tests.Users;

public class UserProfileServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly UserProfileService _sut;
    private readonly UserUpdateProfileDtoValidator _validator;

    public UserProfileServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new UserProfileService(_userManagerMock.Object);
        _validator = new UserUpdateProfileDtoValidator();
    }

    // Test 1 — Obtener perfil de usuario
    [Fact]
    public async Task GetProfileAsync_WhenUserExists_ReturnsCorrectProfile()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId, name: "Juan Pérez", email: "juan@test.com",
            phoneNumber: "+5491112345678", location: "Buenos Aires");

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        var result = await _sut.GetProfileAsync(userId);

        result.Id.Should().Be(userId);
        result.FullName.Should().Be("Juan Pérez");
        result.Email.Should().Be("juan@test.com");
        result.PhoneNumber.Should().Be("+5491112345678");
        result.Location.Should().Be("Buenos Aires");
    }

    [Fact]
    public async Task GetProfileAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.GetProfileAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Test 2 — Actualizar perfil de usuario
    [Fact]
    public async Task UpdateProfileAsync_WithValidData_UpdatesAndReturnsProfile()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId, name: "Viejo Nombre", email: "user@test.com");
        var dto = new UserUpdateProfileDto("Nuevo Nombre", "+5491187654321", "Córdoba, Argentina");

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateProfileAsync(userId, dto);

        result.FullName.Should().Be("Nuevo Nombre");
        result.PhoneNumber.Should().Be("+5491187654321");
        result.Location.Should().Be("Córdoba, Argentina");
    }

    // Test 3 — Rechazar teléfono inválido
    [Theory]
    [InlineData("123456789")]       // sin + prefix
    [InlineData("+0123456789")]     // empieza con 0
    [InlineData("tel:+5491112345")] // prefijo no numérico
    [InlineData("+")]               // solo plus
    public void Validator_WhenPhoneNumberInvalid_HasValidationError(string phone)
    {
        var dto = new UserUpdateProfileDto("Juan Pérez", phone, null);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("+5491112345678")]
    [InlineData("+15551234567")]
    [InlineData("+447911123456")]
    public void Validator_WhenPhoneNumberValidE164_HasNoValidationError(string phone)
    {
        var dto = new UserUpdateProfileDto("Juan Pérez", phone, null);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    // Test 4 — No permitir modificar email
    [Fact]
    public async Task UpdateProfileAsync_DoesNotModifyEmail()
    {
        var userId = Guid.NewGuid();
        var originalEmail = "original@test.com";
        var user = BuildUser(userId, name: "Juan", email: originalEmail);
        var dto = new UserUpdateProfileDto("Juan", null, null);

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateProfileAsync(userId, dto);

        result.Email.Should().Be(originalEmail);
        _userManagerMock.Verify(
            m => m.ChangeEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    // Test 5 — Verificar extracción de userId desde UserManager (no desde request)
    [Fact]
    public async Task UpdateProfileAsync_LoadsUserByIdFromSystem_NotFromRequest()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId, name: "Juan", email: "juan@test.com");
        var dto = new UserUpdateProfileDto("Nuevo", null, null);

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.UpdateProfileAsync(userId, dto);

        _userManagerMock.Verify(m => m.FindByIdAsync(userId.ToString()), Times.Once);
        _userManagerMock.Verify(m => m.FindByIdAsync(It.Is<string>(s => s != userId.ToString())), Times.Never);
    }

    // Sanitización XSS (test 13 parcial — Auth)
    [Fact]
    public async Task UpdateProfileAsync_StripesHtmlTagsFromFullName()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId, name: "Viejo", email: "user@test.com");
        var dto = new UserUpdateProfileDto("<script>alert('xss')</script>Juan", null, null);

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateProfileAsync(userId, dto);

        result.FullName.Should().NotContain("<script>");
        result.FullName.Should().NotContain("</script>");
        result.FullName.Should().Contain("Juan");
    }

    // Validación: teléfono vacío → persiste null
    [Fact]
    public async Task UpdateProfileAsync_WhenPhoneIsEmpty_PersistsNull()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId, name: "Juan", email: "juan@test.com",
            phoneNumber: "+5491112345678");
        var dto = new UserUpdateProfileDto("Juan", "", null);

        _userManagerMock
            .Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateProfileAsync(userId, dto);

        result.PhoneNumber.Should().BeNull();
    }

    private static ApplicationUser BuildUser(
        Guid id,
        string name = "Test User",
        string email = "test@test.com",
        string? phoneNumber = null,
        string? location = null) =>
        new()
        {
            Id = id,
            Name = name,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            Location = location,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
}
