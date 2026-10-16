using Tapio.Domain.Common;
using Tapio.Domain.Enums;

namespace Tapio.Domain.Entities;

public class BusinessAction : BaseEntity
{
    public Guid BusinessId { get; set; }
    public ActionType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Business Business { get; set; } = null!;
}
