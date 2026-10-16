using Tapio.Domain.Enums;

namespace Tapio.Domain.Entities;

public class TapioEvent
{
    public long Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid? StandId { get; set; }
    public EventType EventType { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public Business Business { get; set; } = null!;
    public TapioStand? Stand { get; set; }
}
