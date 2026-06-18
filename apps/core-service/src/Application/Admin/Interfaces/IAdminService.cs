using DeskMatch.CoreService.Application.Admin.Dtos;

namespace DeskMatch.CoreService.Application.Admin.Interfaces;

public interface IAdminService
{
    Task<PagedResult<AdminCompanyDto>> GetCompaniesAsync(int skip, int take, CancellationToken ct = default);
    Task<AdminCompanyDto> ToggleCompanyVerificationAsync(Guid companyId, Guid adminId, string? reason, CancellationToken ct = default);
}