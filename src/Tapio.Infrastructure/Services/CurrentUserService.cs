using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Tapio.Application.Services;
using Tapio.Domain.Enums;
using Tapio.Infrastructure.Identity;

namespace Tapio.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User?.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var claims = User?.Claims ?? [];
            return JwtTokenService.GetRoleFromClaims(claims);
        }
    }

    public Guid? BusinessId
    {
        get
        {
            var val = User?.FindFirst("businessId")?.Value;
            return Guid.TryParse(val, out var id) ? id : null;
        }
    }
}
