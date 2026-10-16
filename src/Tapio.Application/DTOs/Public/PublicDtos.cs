using Tapio.Domain.Enums;

namespace Tapio.Application.DTOs.Public;

public record PublicBusinessResponse(
    string Name, string Slug, string? LogoUrl, string? CoverImageUrl, string? Description,
    IReadOnlyList<PublicActionDto> Actions, IReadOnlyList<PublicCampaignDto> Campaigns);

public record PublicActionDto(ActionType Type, string Title, string Value, int DisplayOrder);
public record PublicCampaignDto(string Title, string? Description, string? ImageUrl);
public record PublicEventRequest(string BusinessSlug, string? StandCode, EventType EventType, string SessionId);
