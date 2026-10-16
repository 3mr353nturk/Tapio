using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tapio.Application.Common;
using Tapio.Application.DTOs.Admin;
using Tapio.Application.Services;

namespace Tapio.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/businesses")]
public class AdminBusinessesController(IAdminBusinessService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminBusinessListItem>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default) =>
        Ok(ApiResponse<PagedResult<AdminBusinessListItem>>.Ok(await service.GetAllAsync(page, pageSize, search, sortBy, sortDesc, ct)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminBusinessDetail>>> Get(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<AdminBusinessDetail>.Ok(await service.GetByIdAsync(id, ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminCreateBusinessResponse>>> Create([FromBody] AdminCreateBusinessRequest request, CancellationToken ct) =>
        Ok(ApiResponse<AdminCreateBusinessResponse>.Ok(await service.CreateAsync(request, ct)));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminBusinessDetail>>> Update(Guid id, [FromBody] AdminUpdateBusinessRequest request, CancellationToken ct) =>
        Ok(ApiResponse<AdminBusinessDetail>.Ok(await service.UpdateAsync(id, request, ct)));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<object>>> SetStatus(Guid id, [FromBody] bool isActive, CancellationToken ct)
    {
        await service.SetStatusAsync(id, isActive, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

}

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController(IAdminUserService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserListItem>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default) =>
        Ok(ApiResponse<PagedResult<AdminUserListItem>>.Ok(await service.GetAllAsync(page, pageSize, search, sortBy, sortDesc, ct)));

    [HttpPost("{userId:guid}/businesses")]
    public async Task<ActionResult<ApiResponse<object>>> Assign(Guid userId, [FromBody] AssignBusinessUserRequest request, CancellationToken ct)
    {
        await service.AssignBusinessAsync(userId, request, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/stands")]
public class AdminStandsController(IAdminStandService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminStandResponse>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? unassignedOnly = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default) =>
        Ok(ApiResponse<PagedResult<AdminStandResponse>>.Ok(await service.GetAllAsync(page, pageSize, unassignedOnly, search, sortBy, sortDesc, ct)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminStandResponse>>> Create(CancellationToken ct) =>
        Ok(ApiResponse<AdminStandResponse>.Ok(await service.CreateAsync(ct)));

    [HttpPut("{id:guid}/assign")]
    public async Task<ActionResult<ApiResponse<AdminStandResponse>>> Assign(Guid id, [FromBody] AssignStandRequest request, CancellationToken ct) =>
        Ok(ApiResponse<AdminStandResponse>.Ok(await service.AssignAsync(id, request, ct)));

    [HttpPut("{id:guid}/unassign")]
    public async Task<ActionResult<ApiResponse<AdminStandResponse>>> Unassign(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<AdminStandResponse>.Ok(await service.UnassignAsync(id, ct)));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<object>>> SetStatus(Guid id, [FromBody] bool isActive, CancellationToken ct)
    {
        await service.SetStatusAsync(id, isActive, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/admin/stats")]
public class AdminStatsController(IAdminStatsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AdminStatsResponse>>> Get(CancellationToken ct) =>
        Ok(ApiResponse<AdminStatsResponse>.Ok(await service.GetStatsAsync(ct)));
}
