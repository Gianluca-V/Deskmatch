using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Companies;

public class Company : AggregateRoot<Guid>
{
    public Company(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    private Company() { } // EF Core

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsVerified { get; set; } = false;
    public Guid? OwnerId { get; set; }
    public bool IsActive { get; set; } = true;

    // KYB 
    public KybStatus KybStatus { get; set; } = KybStatus.NotSubmitted;
    public string? TaxId { get; set; }
    public string? LegalName { get; set; }
    public string? RegistrationDocumentUrl { get; set; }
    public DateTime? KybSubmittedAt { get; set; }
    public DateTime? KybReviewedAt { get; set; }
    public Guid? KybReviewedByAdminId { get; set; }
    public string? KybRejectionReason { get; set; }

    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;

    public void SubmitForKybReview(string taxId, string legalName, string? registrationDocumentUrl)
    {
        TaxId = taxId;
        LegalName = legalName;
        RegistrationDocumentUrl = registrationDocumentUrl;
        KybStatus = KybStatus.Pending;
        KybSubmittedAt = DateTime.UtcNow;
        KybReviewedAt = null;
        KybReviewedByAdminId = null;
        KybRejectionReason = null;
        MarkAsUpdated();
    }

    public void ApproveKyb(Guid adminId)
    {
        KybStatus = KybStatus.Approved;
        IsVerified = true;
        KybReviewedAt = DateTime.UtcNow;
        KybReviewedByAdminId = adminId;
        KybRejectionReason = null;
        MarkAsUpdated();
    }

    public void RejectKyb(Guid adminId, string reason)
    {
        KybStatus = KybStatus.Rejected;
        IsVerified = false;
        KybReviewedAt = DateTime.UtcNow;
        KybReviewedByAdminId = adminId;
        KybRejectionReason = reason;
        MarkAsUpdated();
    }
}