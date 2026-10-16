using Tapio.Domain.Common;
using Tapio.Domain.Enums;

namespace Tapio.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; }

    public ICollection<BusinessUser> BusinessUsers { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
