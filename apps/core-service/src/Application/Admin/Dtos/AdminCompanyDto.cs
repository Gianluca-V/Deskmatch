using DeskMatch.CoreService.Domain.Companies;

namespace DeskMatch.CoreService.Application.Admin.Dtos;

public sealed record AdminCompanyDto(
    Guid Id,
    string Name,
    string? ContactEmail,
    bool IsVerified,
    KybStatus KybStatus,
    string? TaxId,
    string? LegalName,
    DateTime? KybSubmittedAt,
    DateTime? KybReviewedAt,
    Guid? OwnerId,
    bool IsActive
);