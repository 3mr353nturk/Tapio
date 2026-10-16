using Tapio.Domain.Enums;

namespace Tapio.Application.DTOs.Admin;

public record AdminCreateBusinessRequest(
    string Name, string? Category, string? Phone, string? Email,
    string? Address, string? City, string? District);

public record AdminUpdateBusinessRequest(
    string Name, string? Category, string? Phone, string? Email,
    string? Address, string? City, string? District);

public record AdminBusinessListItem(
    Guid Id, string Name, string Slug, string? Category, bool IsActive,
    BusinessRegistrationSource RegistrationSource, DateTimeOffset CreatedDate);

public record AdminBusinessDetail(
    Guid Id, string Name, string Slug, string? Category, string? Phone, string? Email,
    string? Address, string? City, string? District, bool IsActive,
    BusinessRegistrationSource RegistrationSource, DateTimeOffset CreatedDate);

public record AdminCreateBusinessResponse(Guid BusinessId, string Slug);

public record AdminUserListItem(Guid Id, string Email, string FirstName, string LastName, UserRole Role, bool IsActive, bool EmailVerified);
public record AssignBusinessUserRequest(Guid BusinessId, BusinessUserRole Role);

public record AdminStandResponse(
    Guid Id, string StandCode, Guid? BusinessId, string? BusinessName, bool IsActive, DateTimeOffset CreatedDate);

public record AssignStandRequest(Guid BusinessId);
public record AdminStatsResponse(int TotalBusinesses, int ActiveBusinesses, int TotalStands, int UnassignedStands, int TotalEventsToday);
