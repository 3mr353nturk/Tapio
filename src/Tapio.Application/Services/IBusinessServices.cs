using Tapio.Application.Common;
using Tapio.Application.DTOs.Admin;
using Tapio.Application.DTOs.Business;
using Tapio.Application.DTOs.Public;

namespace Tapio.Application.Services;

public interface IPublicBusinessService
{
    Task<PublicBusinessResponse> GetBySlugAsync(string slug, CancellationToken ct = default);
}

public interface IPublicEventService
{
    Task RecordEventAsync(PublicEventRequest request, CancellationToken ct = default);
}

public interface IBusinessProfileService
{
    Task<BusinessProfileResponse> GetAsync(CancellationToken ct = default);
    Task<BusinessProfileResponse> UpdateAsync(UpdateBusinessProfileRequest request, CancellationToken ct = default);
}

public interface IBusinessMediaService
{
    Task<MediaUploadResponse> UploadLogoAsync(Stream content, string contentType, string fileName, CancellationToken ct = default);
    Task<MediaUploadResponse> UploadCoverAsync(Stream content, string contentType, string fileName, CancellationToken ct = default);
}

public interface IBusinessActionService
{
    Task<IReadOnlyList<BusinessActionResponse>> GetAllAsync(CancellationToken ct = default);
    Task<BusinessActionResponse> CreateAsync(CreateActionRequest request, CancellationToken ct = default);
    Task<BusinessActionResponse> UpdateAsync(Guid id, UpdateActionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IBusinessCampaignService
{
    Task<IReadOnlyList<CampaignResponse>> GetAllAsync(CancellationToken ct = default);
    Task<CampaignResponse> CreateAsync(CreateCampaignRequest request, CancellationToken ct = default);
    Task<CampaignResponse> UpdateAsync(Guid id, UpdateCampaignRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IBusinessStandService
{
    Task<IReadOnlyList<StandResponse>> GetAllAsync(CancellationToken ct = default);
    Task<StandResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IBusinessAnalyticsService
{
    Task<AnalyticsSummaryResponse> GetSummaryAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<AnalyticsDailyResponse> GetDailyAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<AnalyticsActionsResponse> GetActionsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
}

public interface IOnboardingService
{
    Task<OnboardingResponse> GetAsync(CancellationToken ct = default);
}

public interface IAdminBusinessService
{
    Task<PagedResult<AdminBusinessListItem>> GetAllAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default);
    Task<AdminBusinessDetail> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminCreateBusinessResponse> CreateAsync(AdminCreateBusinessRequest request, CancellationToken ct = default);
    Task<AdminBusinessDetail> UpdateAsync(Guid id, AdminUpdateBusinessRequest request, CancellationToken ct = default);
    Task SetStatusAsync(Guid id, bool isActive, CancellationToken ct = default);
}

public interface IAdminUserService
{
    Task<PagedResult<AdminUserListItem>> GetAllAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default);
    Task AssignBusinessAsync(Guid userId, AssignBusinessUserRequest request, CancellationToken ct = default);
}

public interface IAdminStandService
{
    Task<PagedResult<AdminStandResponse>> GetAllAsync(int page, int pageSize, bool? unassignedOnly, string? search = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default);
    Task<AdminStandResponse> CreateAsync(CancellationToken ct = default);
    Task<AdminStandResponse> AssignAsync(Guid standId, AssignStandRequest request, CancellationToken ct = default);
    Task<AdminStandResponse> UnassignAsync(Guid standId, CancellationToken ct = default);
    Task SetStatusAsync(Guid standId, bool isActive, CancellationToken ct = default);
}

public interface IAdminStatsService
{
    Task<AdminStatsResponse> GetStatsAsync(CancellationToken ct = default);
}
