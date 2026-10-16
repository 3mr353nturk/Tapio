using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Tapio.Application.Common;
using Tapio.Application.DTOs.Auth;
using Tapio.Application.Services;
using Tapio.Infrastructure.Identity;

namespace Tapio.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IWebHostEnvironment env, IOptions<JwtSettings> jwtOptions) : ControllerBase
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        var refresh = await authService.CreateRefreshTokenAsync(result.User.Id, ct);
        SetRefreshCookie(refresh);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("register")]
    [EnableRateLimiting("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        var refresh = await authService.CreateRefreshTokenAsync(result.User.Id, ct);
        SetRefreshCookie(refresh);
        return Ok(ApiResponse<RegisterResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var token) || string.IsNullOrEmpty(token))
            return Unauthorized(ApiResponse<LoginResponse>.Fail("UNAUTHORIZED", "Refresh token bulunamadı."));

        var result = await authService.RefreshAsync(token, ct);
        var refresh = await authService.CreateRefreshTokenAsync(result.User.Id, ct);
        SetRefreshCookie(refresh);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var token) && !string.IsNullOrEmpty(token))
            await authService.LogoutAsync(token, ct);
        Response.Cookies.Delete("refreshToken");
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> Me(CancellationToken ct) =>
        Ok(ApiResponse<UserInfoDto>.Ok(await authService.GetMeAsync(ct)));

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        await authService.VerifyEmailAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "E-posta doğrulandı." }));
    }

    [Authorize]
    [HttpPost("resend-verification")]
    public async Task<ActionResult<ApiResponse<object>>> ResendVerification(CancellationToken ct)
    {
        await authService.ResendVerificationAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Doğrulama e-postası gönderildi." }));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await authService.ForgotPasswordAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Şifre sıfırlama bağlantısı gönderildi." }));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await authService.ResetPasswordAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(new { message = "Şifre güncellendi." }));
    }

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenDays),
            Path = "/"
        });
    }
}
