using Tapio.Domain.Enums;

namespace Tapio.Application.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? Role { get; }
    Guid? BusinessId { get; }
    bool IsAuthenticated { get; }
}
