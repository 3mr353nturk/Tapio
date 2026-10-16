using Tapio.Domain.Enums;

namespace Tapio.Application.DTOs.Business;

public record BusinessProfileResponse(
    Guid Id, string Name, string Slug, string? Category, string? LogoUrl, string? CoverImageUrl,
    string? Description, string? Phone, string? Email, string? WhatsAppNumber, string? InstagramUrl,
    string? GoogleMapsUrl, string? GoogleReviewUrl, string? MenuUrl, string? Address, string? City, string? District);

public record UpdateBusinessProfileRequest(
    string Name, string? Category, string? LogoUrl, string? CoverImageUrl, string? Description,
    string? Phone, string? Email, string? WhatsAppNumber, string? InstagramUrl,
    string? GoogleMapsUrl, string? GoogleReviewUrl, string? MenuUrl, string? Address, string? City, string? District);

public record BusinessActionResponse(Guid Id, ActionType Type, string Title, string Value, int DisplayOrder, bool IsEnabled);
public record CreateActionRequest(ActionType Type, string Title, string Value, int DisplayOrder, bool IsEnabled);
public record UpdateActionRequest(string Title, string Value, int DisplayOrder, bool IsEnabled);

public record CampaignResponse(Guid Id, string Title, string? Description, string? ImageUrl, DateTimeOffset StartDate, DateTimeOffset EndDate, bool IsActive);
public record CreateCampaignRequest(string Title, string? Description, string? ImageUrl, DateTimeOffset StartDate, DateTimeOffset EndDate, bool IsActive);
public record UpdateCampaignRequest(string Title, string? Description, string? ImageUrl, DateTimeOffset StartDate, DateTimeOffset EndDate, bool IsActive);

public record StandResponse(Guid Id, string StandCode, bool IsActive, DateTimeOffset CreatedDate);

public record AnalyticsSummaryResponse(int PageViews, int ActionClicks, string? TopAction, int ActiveCampaigns, int ActiveStands);
public record DailyStatDto(DateOnly Date, int PageViews, int ActionClicks);
public record ActionStatDto(EventType EventType, int Count);
public record AnalyticsDailyResponse(IReadOnlyList<DailyStatDto> Items);
public record AnalyticsActionsResponse(IReadOnlyList<ActionStatDto> Items);

public record OnboardingStepDto(string Key, string Label, bool Completed, bool Optional);
public record OnboardingResponse(IReadOnlyList<OnboardingStepDto> Steps, int CompletedCount, int TotalCount);
