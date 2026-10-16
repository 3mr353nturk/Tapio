using Tapio.Domain.Common;
using Tapio.Domain.Enums;

namespace Tapio.Domain.Entities;

public class Business : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? InstagramUrl { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public string? GoogleReviewUrl { get; set; }
    public string? MenuUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public bool IsActive { get; set; } = true;
    public BusinessRegistrationSource RegistrationSource { get; set; }

    public ICollection<BusinessUser> BusinessUsers { get; set; } = [];
    public ICollection<TapioStand> Stands { get; set; } = [];
    public ICollection<BusinessAction> Actions { get; set; } = [];
    public ICollection<Campaign> Campaigns { get; set; } = [];
    public ICollection<TapioEvent> Events { get; set; } = [];
    public ICollection<BusinessInvitation> Invitations { get; set; } = [];
}
