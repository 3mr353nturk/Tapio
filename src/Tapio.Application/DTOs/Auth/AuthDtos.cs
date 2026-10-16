using Tapio.Domain.Enums;

namespace Tapio.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password);
public record RegisterRequest(
    string FirstName, string LastName, string Email, string Phone, string Password,
    string BusinessName, string Category, string BusinessPhone, string Address, string City, string District);
public record VerifyEmailRequest(string Token);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record RefreshTokenRequest(string RefreshToken);

public record UserInfoDto(Guid Id, string Email, string FirstName, string LastName, UserRole Role, Guid? BusinessId, string? BusinessSlug, bool EmailVerified);
public record LoginResponse(string AccessToken, UserInfoDto User);
public record RegisterResponse(string AccessToken, UserInfoDto User, BusinessSummaryDto Business);
public record BusinessSummaryDto(string Name, string Slug);
