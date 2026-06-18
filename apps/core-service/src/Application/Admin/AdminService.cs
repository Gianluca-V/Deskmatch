using DeskMatch.CoreService.Application.Admin.Dtos;
using DeskMatch.CoreService.Application.Admin.Interfaces;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Domain.Audit;

namespace DeskMatch.CoreService.Application.Admin;

public class AdminService : IAdminService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public AdminService(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AdminCompanyDto>> GetCompaniesAsync(
        int skip, int take, CancellationToken ct = default)
    {
        var (companies, total) = await _companyRepository.GetPagedAsync(skip, take, ct);

        var items = companies.Select(c => new AdminCompanyDto(
            c.Id, c.Name, c.ContactEmail, c.IsVerified,
            c.KybStatus, c.TaxId, c.LegalName,
            c.KybSubmittedAt, c.KybReviewedAt, c.OwnerId, c.IsActive))
            .ToList();

        return new PagedResult<AdminCompanyDto>(items, total, skip, take);
    }

    public async Task<AdminCompanyDto> ToggleCompanyVerificationAsync(
        Guid companyId, Guid adminId, string? reason, CancellationToken ct = default)
    {
        var company = await _companyRepository.GetByIdAsync(companyId, ct)
            ?? throw new KeyNotFoundException($"Company {companyId} not found.");

        if (company.IsVerified)
            company.RejectKyb(adminId, reason ?? "Reverted by admin.");
        else
            company.ApproveKyb(adminId);

        await _companyRepository.SaveChangesAsync(ct);

        var action = company.IsVerified ? "VERIFY_COMPANY" : "UNVERIFY_COMPANY";
        var log = new AuditLog(Guid.NewGuid(), adminId, action, "Company", companyId, reason);
        await _auditLogRepository.AddAsync(log, ct);
        await _auditLogRepository.SaveChangesAsync(ct);

        return new AdminCompanyDto(
            company.Id, company.Name, company.ContactEmail, company.IsVerified,
            company.KybStatus, company.TaxId, company.LegalName,
            company.KybSubmittedAt, company.KybReviewedAt, company.OwnerId, company.IsActive);
    }
}