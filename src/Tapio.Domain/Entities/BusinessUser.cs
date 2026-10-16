using Tapio.Domain.Enums;

namespace Tapio.Domain.Entities;

public class BusinessUser
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BusinessId { get; set; }
    public BusinessUserRole Role { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public User User { get; set; } = null!;
    public Business Business { get; set; } = null!;
}
