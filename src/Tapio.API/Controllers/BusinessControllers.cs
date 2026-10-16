using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tapio.Application.Common;
using Tapio.Application.Common.Exceptions;
using Tapio.Application.DTOs.Business;
using Tapio.Application.Services;

namespace Tapio.API.Controllers;

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business/media")]
public class BusinessMediaController(IBusinessMediaService service) : ControllerBase
{
    [HttpPost("logo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<MediaUploadResponse>>> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) throw new ValidationException("Dosya seçilmedi.");
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<MediaUploadResponse>.Ok(
            await service.UploadLogoAsync(stream, file.ContentType, file.FileName, ct)));
    }

    [HttpPost("cover")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<MediaUploadResponse>>> UploadCover(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) throw new ValidationException("Dosya seçilmedi.");
        await using var stream = file.OpenReadStream();
        return Ok(ApiResponse<MediaUploadResponse>.Ok(
            await service.UploadCoverAsync(stream, file.ContentType, file.FileName, ct)));
    }
}

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business")]
public class BusinessProfileController(IBusinessProfileService service) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<BusinessProfileResponse>>> Get(CancellationToken ct) =>
        Ok(ApiResponse<BusinessProfileResponse>.Ok(await service.GetAsync(ct)));

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<BusinessProfileResponse>>> Update([FromBody] UpdateBusinessProfileRequest request, CancellationToken ct) =>
        Ok(ApiResponse<BusinessProfileResponse>.Ok(await service.UpdateAsync(request, ct)));
}

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business/actions")]
public class BusinessActionsController(IBusinessActionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BusinessActionResponse>>>> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<BusinessActionResponse>>.Ok(await service.GetAllAsync(ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BusinessActionResponse>>> Create([FromBody] CreateActionRequest request, CancellationToken ct) =>
        Ok(ApiResponse<BusinessActionResponse>.Ok(await service.CreateAsync(request, ct)));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BusinessActionResponse>>> Update(Guid id, [FromBody] UpdateActionRequest request, CancellationToken ct) =>
        Ok(ApiResponse<BusinessActionResponse>.Ok(await service.UpdateAsync(id, request, ct)));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business/campaigns")]
public class BusinessCampaignsController(IBusinessCampaignService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CampaignResponse>>>> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<CampaignResponse>>.Ok(await service.GetAllAsync(ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CampaignResponse>>> Create([FromBody] CreateCampaignRequest request, CancellationToken ct) =>
        Ok(ApiResponse<CampaignResponse>.Ok(await service.CreateAsync(request, ct)));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CampaignResponse>>> Update(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken ct) =>
        Ok(ApiResponse<CampaignResponse>.Ok(await service.UpdateAsync(id, request, ct)));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business/stands")]
public class BusinessStandsController(IBusinessStandService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StandResponse>>>> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<StandResponse>>.Ok(await service.GetAllAsync(ct)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StandResponse>>> Get(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<StandResponse>.Ok(await service.GetByIdAsync(id, ct)));
}

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business/analytics")]
public class BusinessAnalyticsController(IBusinessAnalyticsService service) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<AnalyticsSummaryResponse>>> Summary([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct) =>
        Ok(ApiResponse<AnalyticsSummaryResponse>.Ok(await service.GetSummaryAsync(from, to, ct)));

    [HttpGet("daily")]
    public async Task<ActionResult<ApiResponse<AnalyticsDailyResponse>>> Daily([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct) =>
        Ok(ApiResponse<AnalyticsDailyResponse>.Ok(await service.GetDailyAsync(from, to, ct)));

    [HttpGet("actions")]
    public async Task<ActionResult<ApiResponse<AnalyticsActionsResponse>>> Actions([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct) =>
        Ok(ApiResponse<AnalyticsActionsResponse>.Ok(await service.GetActionsAsync(from, to, ct)));
}

[Authorize(Policy = "BusinessUserOnly")]
[ApiController]
[Route("api/business/onboarding")]
public class BusinessOnboardingController(IOnboardingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<OnboardingResponse>>> Get(CancellationToken ct) =>
        Ok(ApiResponse<OnboardingResponse>.Ok(await service.GetAsync(ct)));
}
