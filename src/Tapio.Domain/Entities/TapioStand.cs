using Tapio.Domain.Common;

namespace Tapio.Domain.Entities;

public class TapioStand : BaseEntity
{
    public Guid? BusinessId { get; set; }
    public string StandCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Business? Business { get; set; }
    public ICollection<TapioEvent> Events { get; set; } = [];
}
