using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tapio.Application.Common;
using Tapio.Application.DTOs.Public;
using Tapio.Application.Services;

namespace Tapio.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicBusinessController(IPublicBusinessService service) : ControllerBase
{
    [HttpGet("businesses/{slug}")]
    public async Task<ActionResult<ApiResponse<PublicBusinessResponse>>> Get(string slug, CancellationToken ct) =>
        Ok(ApiResponse<PublicBusinessResponse>.Ok(await service.GetBySlugAsync(slug, ct)));
}

[ApiController]
[Route("api/public/events")]
public class PublicEventController(IPublicEventService service) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("public-events")]
    public async Task<ActionResult<ApiResponse<object>>> Post([FromBody] PublicEventRequest request, CancellationToken ct)
    {
        await service.RecordEventAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}
