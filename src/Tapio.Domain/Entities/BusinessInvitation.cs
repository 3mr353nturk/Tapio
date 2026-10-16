using Tapio.Domain.Enums;

namespace Tapio.Domain.Entities;

public class BusinessInvitation
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public Guid InvitedByUserId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public Business Business { get; set; } = null!;
}
