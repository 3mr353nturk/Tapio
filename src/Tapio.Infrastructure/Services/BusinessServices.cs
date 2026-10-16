using Microsoft.EntityFrameworkCore;
using Tapio.Application.Common.Exceptions;
using Tapio.Application.Common.Helpers;
using Tapio.Application.DTOs.Business;
using Tapio.Application.DTOs.Public;
using Tapio.Application.Services;
using Tapio.Domain.Entities;
using Tapio.Domain.Enums;
using Tapio.Infrastructure.Persistence;

namespace Tapio.Infrastructure.Services;

public class PublicBusinessService(ApplicationDbContext db, IPublicBusinessCache cache) : IPublicBusinessService
{
    public async Task<PublicBusinessResponse> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var cached = await cache.GetAsync(slug, ct);
        if (cached is not null) return cached;

        var now = DateTimeOffset.UtcNow;
        var business = await db.Businesses
            .AsNoTracking()
            .Where(b => b.Slug == slug && b.IsActive)
            .Select(b => new
            {
                b.Name, b.Slug, b.LogoUrl, b.CoverImageUrl, b.Description,
                Actions = b.Actions.Where(a => a.IsEnabled).OrderBy(a => a.DisplayOrder)
                    .Select(a => new PublicActionDto(a.Type, a.Title, a.Value, a.DisplayOrder)).ToList(),
                Campaigns = b.Campaigns
                    .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
                    .Select(c => new PublicCampaignDto(c.Title, c.Description, c.ImageUrl)).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("İşletme bulunamadı.");

        var response = new PublicBusinessResponse(
            business.Name, business.Slug, business.LogoUrl, business.CoverImageUrl, business.Description,
            business.Actions, business.Campaigns);

        await cache.SetAsync(slug, response, ct);
        return response;
    }
}

public class PublicEventService(ApplicationDbContext db) : IPublicEventService
{
    public async Task RecordEventAsync(PublicEventRequest request, CancellationToken ct = default)
    {
        var business = await db.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == request.BusinessSlug && b.IsActive, ct);
        if (business is null) return;

        Guid? standId = null;
        if (!string.IsNullOrWhiteSpace(request.StandCode))
        {
            var stand = await db.TapioStands
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StandCode == request.StandCode && s.IsActive && s.BusinessId == business.Id, ct);
            if (stand is null) return;
            standId = stand.Id;
        }

        db.TapioEvents.Add(new TapioEvent
        {
            BusinessId = business.Id,
            StandId = standId,
            EventType = request.EventType,
            SessionId = request.SessionId,
            CreatedDate = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}

public class BusinessProfileService(ApplicationDbContext db, ICurrentUserService currentUser, IPublicBusinessCache cache) : IBusinessProfileService
{
    public async Task<BusinessProfileResponse> GetAsync(CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var business = await db.Businesses.AsNoTracking().FirstAsync(b => b.Id == businessId, ct);
        return MapProfile(business);
    }

    public async Task<BusinessProfileResponse> UpdateAsync(UpdateBusinessProfileRequest request, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var business = await db.Businesses.FirstAsync(b => b.Id == businessId, ct);
        var oldSlug = business.Slug;

        business.Name = request.Name;
        business.Category = request.Category;
        business.LogoUrl = request.LogoUrl;
        business.CoverImageUrl = request.CoverImageUrl;
        business.Description = request.Description;
        business.Phone = request.Phone;
        business.Email = request.Email;
        business.WhatsAppNumber = request.WhatsAppNumber;
        business.InstagramUrl = request.InstagramUrl;
        business.GoogleMapsUrl = request.GoogleMapsUrl;
        business.GoogleReviewUrl = request.GoogleReviewUrl;
        business.MenuUrl = request.MenuUrl;
        business.Address = request.Address;
        business.City = request.City;
        business.District = request.District;
        business.UpdatedDate = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(oldSlug, ct);
        if (oldSlug != business.Slug)
            await cache.InvalidateAsync(business.Slug, ct);

        return MapProfile(business);
    }

    private static BusinessProfileResponse MapProfile(Business b) => new(
        b.Id, b.Name, b.Slug, b.Category, b.LogoUrl, b.CoverImageUrl, b.Description,
        b.Phone, b.Email, b.WhatsAppNumber, b.InstagramUrl, b.GoogleMapsUrl, b.GoogleReviewUrl,
        b.MenuUrl, b.Address, b.City, b.District);
}

public class BusinessActionService(ApplicationDbContext db, ICurrentUserService currentUser, IPublicBusinessCache cache) : IBusinessActionService
{
    private async Task<string> GetSlugAsync(CancellationToken ct)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        return await db.Businesses.Where(b => b.Id == businessId).Select(b => b.Slug).FirstAsync(ct);
    }

    public async Task<IReadOnlyList<BusinessActionResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        return await db.BusinessActions.AsNoTracking()
            .Where(a => a.BusinessId == businessId)
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new BusinessActionResponse(a.Id, a.Type, a.Title, a.Value, a.DisplayOrder, a.IsEnabled))
            .ToListAsync(ct);
    }

    public async Task<BusinessActionResponse> CreateAsync(CreateActionRequest request, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var now = DateTimeOffset.UtcNow;
        var action = new BusinessAction
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Type = request.Type,
            Title = request.Title,
            Value = request.Value,
            DisplayOrder = request.DisplayOrder,
            IsEnabled = request.IsEnabled,
            CreatedDate = now,
            UpdatedDate = now
        };
        db.BusinessActions.Add(action);
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(await GetSlugAsync(ct), ct);
        return new BusinessActionResponse(action.Id, action.Type, action.Title, action.Value, action.DisplayOrder, action.IsEnabled);
    }

    public async Task<BusinessActionResponse> UpdateAsync(Guid id, UpdateActionRequest request, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var action = await db.BusinessActions.FirstOrDefaultAsync(a => a.Id == id && a.BusinessId == businessId, ct)
            ?? throw new NotFoundException();
        action.Title = request.Title;
        action.Value = request.Value;
        action.DisplayOrder = request.DisplayOrder;
        action.IsEnabled = request.IsEnabled;
        action.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(await GetSlugAsync(ct), ct);
        return new BusinessActionResponse(action.Id, action.Type, action.Title, action.Value, action.DisplayOrder, action.IsEnabled);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var action = await db.BusinessActions.FirstOrDefaultAsync(a => a.Id == id && a.BusinessId == businessId, ct)
            ?? throw new NotFoundException();
        db.BusinessActions.Remove(action);
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(await GetSlugAsync(ct), ct);
    }
}

public class BusinessCampaignService(ApplicationDbContext db, ICurrentUserService currentUser, IPublicBusinessCache cache) : IBusinessCampaignService
{
    private async Task<string> GetSlugAsync(CancellationToken ct)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        return await db.Businesses.Where(b => b.Id == businessId).Select(b => b.Slug).FirstAsync(ct);
    }

    public async Task<IReadOnlyList<CampaignResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        return await db.Campaigns.AsNoTracking()
            .Where(c => c.BusinessId == businessId)
            .OrderByDescending(c => c.CreatedDate)
            .Select(c => new CampaignResponse(c.Id, c.Title, c.Description, c.ImageUrl, c.StartDate, c.EndDate, c.IsActive))
            .ToListAsync(ct);
    }

    public async Task<CampaignResponse> CreateAsync(CreateCampaignRequest request, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var now = DateTimeOffset.UtcNow;
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedDate = now,
            UpdatedDate = now
        };
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(await GetSlugAsync(ct), ct);
        return new CampaignResponse(campaign.Id, campaign.Title, campaign.Description, campaign.ImageUrl, campaign.StartDate, campaign.EndDate, campaign.IsActive);
    }

    public async Task<CampaignResponse> UpdateAsync(Guid id, UpdateCampaignRequest request, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId, ct)
            ?? throw new NotFoundException();
        campaign.Title = request.Title;
        campaign.Description = request.Description;
        campaign.ImageUrl = request.ImageUrl;
        campaign.StartDate = request.StartDate;
        campaign.EndDate = request.EndDate;
        campaign.IsActive = request.IsActive;
        campaign.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(await GetSlugAsync(ct), ct);
        return new CampaignResponse(campaign.Id, campaign.Title, campaign.Description, campaign.ImageUrl, campaign.StartDate, campaign.EndDate, campaign.IsActive);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == id && c.BusinessId == businessId, ct)
            ?? throw new NotFoundException();
        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(await GetSlugAsync(ct), ct);
    }
}

public class BusinessStandService(ApplicationDbContext db, ICurrentUserService currentUser) : IBusinessStandService
{
    public async Task<IReadOnlyList<StandResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        return await db.TapioStands.AsNoTracking()
            .Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.StandCode)
            .Select(s => new StandResponse(s.Id, s.StandCode, s.IsActive, s.CreatedDate))
            .ToListAsync(ct);
    }

    public async Task<StandResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        return await db.TapioStands.AsNoTracking()
            .Where(s => s.Id == id && s.BusinessId == businessId)
            .Select(s => new StandResponse(s.Id, s.StandCode, s.IsActive, s.CreatedDate))
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException();
    }
}

public class BusinessAnalyticsService(ApplicationDbContext db, ICurrentUserService currentUser) : IBusinessAnalyticsService
{
    private static readonly EventType[] ActionEvents =
    [
        EventType.MenuClick, EventType.WhatsAppClick, EventType.InstagramClick,
        EventType.GoogleReviewClick, EventType.GoogleMapsClick, EventType.PhoneClick, EventType.CampaignClick
    ];

    private (DateTimeOffset from, DateTimeOffset to) ResolveRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        var end = to ?? DateTimeOffset.UtcNow;
        var start = from ?? end.AddDays(-30);
        return (start, end);
    }

    public async Task<AnalyticsSummaryResponse> GetSummaryAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var (start, end) = ResolveRange(from, to);
        var now = DateTimeOffset.UtcNow;

        var stats = await db.TapioEvents.AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.CreatedDate >= start && e.CreatedDate <= end)
            .GroupBy(e => e.EventType)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var pageViews = stats.FirstOrDefault(s => s.Key == EventType.PageView)?.Count ?? 0;
        var actionClicks = stats.Where(s => ActionEvents.Contains(s.Key)).Sum(s => s.Count);
        var topAction = stats.Where(s => ActionEvents.Contains(s.Key)).OrderByDescending(s => s.Count).FirstOrDefault()?.Key.ToString();

        var activeCampaigns = await db.Campaigns.CountAsync(
            c => c.BusinessId == businessId && c.IsActive && c.StartDate <= now && c.EndDate >= now, ct);
        var activeStands = await db.TapioStands.CountAsync(
            s => s.BusinessId == businessId && s.IsActive, ct);

        return new AnalyticsSummaryResponse(pageViews, actionClicks, topAction, activeCampaigns, activeStands);
    }

    public async Task<AnalyticsDailyResponse> GetDailyAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var (start, end) = ResolveRange(from, to);

        var items = await db.TapioEvents.AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.CreatedDate >= start && e.CreatedDate <= end)
            .GroupBy(e => e.CreatedDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                PageViews = g.Count(x => x.EventType == EventType.PageView),
                ActionClicks = g.Count(x => ActionEvents.Contains(x.EventType))
            })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        return new AnalyticsDailyResponse(items.Select(i => new DailyStatDto(DateOnly.FromDateTime(i.Date), i.PageViews, i.ActionClicks)).ToList());
    }

    public async Task<AnalyticsActionsResponse> GetActionsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var (start, end) = ResolveRange(from, to);

        var items = await db.TapioEvents.AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.CreatedDate >= start && e.CreatedDate <= end && ActionEvents.Contains(e.EventType))
            .GroupBy(e => e.EventType)
            .Select(g => new ActionStatDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        return new AnalyticsActionsResponse(items);
    }
}

public class OnboardingService(ApplicationDbContext db, ICurrentUserService currentUser) : IOnboardingService
{
    public async Task<OnboardingResponse> GetAsync(CancellationToken ct = default)
    {
        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var now = DateTimeOffset.UtcNow;

        var b = await db.Businesses.AsNoTracking()
            .Include(x => x.Stands)
            .Include(x => x.Campaigns)
            .FirstAsync(x => x.Id == businessId, ct);

        var steps = new List<OnboardingStepDto>
        {
            new("info", "İşletme bilgileri", !string.IsNullOrWhiteSpace(b.Name) && !string.IsNullOrWhiteSpace(b.Category) && !string.IsNullOrWhiteSpace(b.Address), false),
            new("logo", "Logo", !string.IsNullOrWhiteSpace(b.LogoUrl), true),
            new("cover", "Kapak", !string.IsNullOrWhiteSpace(b.CoverImageUrl), true),
            new("whatsapp", "WhatsApp", !string.IsNullOrWhiteSpace(b.WhatsAppNumber), true),
            new("instagram", "Instagram", !string.IsNullOrWhiteSpace(b.InstagramUrl), true),
            new("menu", "Menü", !string.IsNullOrWhiteSpace(b.MenuUrl), true),
            new("review", "Google Review", !string.IsNullOrWhiteSpace(b.GoogleReviewUrl), true),
            new("maps", "Google Maps", !string.IsNullOrWhiteSpace(b.GoogleMapsUrl), true),
            new("campaign", "Kampanya", b.Campaigns.Any(c => c.IsActive && c.StartDate <= now && c.EndDate >= now), true),
            new("stand", "Stand", b.Stands.Any(s => s.IsActive), true)
        };

        return new OnboardingResponse(steps, steps.Count(s => s.Completed), steps.Count);
    }
}
