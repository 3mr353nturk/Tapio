using Tapio.Domain.Common;

namespace Tapio.Domain.Entities;

public class Campaign : BaseEntity
{
    public Guid BusinessId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Business Business { get; set; } = null!;
}
