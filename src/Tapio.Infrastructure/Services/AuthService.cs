using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tapio.Application.Common.Exceptions;
using Tapio.Application.Common.Helpers;
using Tapio.Application.DTOs.Auth;
using Tapio.Application.Services;
using Tapio.Domain.Entities;
using Tapio.Domain.Enums;
using Tapio.Infrastructure.Identity;
using Tapio.Infrastructure.Persistence;

namespace Tapio.Infrastructure.Services;

public class AuthService(
    ApplicationDbContext db,
    JwtTokenService jwtTokenService,
    IEmailService emailService,
    ICurrentUserService currentUser,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct)
            ?? throw new UnauthorizedAppException("E-posta veya şifre hatalı.");

        if (!user.IsActive || user.PasswordHash is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("E-posta veya şifre hatalı.");

        return await BuildLoginResponseAsync(user, ct);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ValidationException("Bu e-posta adresi zaten kayıtlı.");

        var now = DateTimeOffset.UtcNow;
        var slug = await GenerateUniqueSlugAsync(request.BusinessName, ct);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Role = UserRole.BusinessUser,
            IsActive = true,
            EmailVerified = false,
            CreatedDate = now,
            UpdatedDate = now
        };

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = request.BusinessName,
            Slug = slug,
            Category = request.Category,
            Phone = request.BusinessPhone,
            Address = request.Address,
            City = request.City,
            District = request.District,
            IsActive = true,
            RegistrationSource = BusinessRegistrationSource.SelfService,
            CreatedDate = now,
            UpdatedDate = now
        };

        var businessUser = new BusinessUser
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            BusinessId = business.Id,
            Role = BusinessUserRole.Owner,
            CreatedDate = now
        };

        var verifyToken = TokenHelper.GenerateToken();
        db.Users.Add(user);
        db.Businesses.Add(business);
        db.BusinessUsers.Add(businessUser);
        db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHelper.HashToken(verifyToken),
            ExpiresAt = now.AddDays(1),
            CreatedDate = now
        });
        await db.SaveChangesAsync(ct);
        await emailService.SendVerificationEmailAsync(email, verifyToken, ct);

        var accessToken = jwtTokenService.CreateAccessToken(user, business.Id);
        var userInfo = MapUserInfo(user, business.Id, business.Slug);
        return new RegisterResponse(accessToken, userInfo, new BusinessSummaryDto(business.Name, business.Slug));
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = TokenHelper.HashToken(refreshToken);
        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash && r.RevokedAt == null, ct)
            ?? throw new UnauthorizedAppException("Geçersiz refresh token.");

        if (stored.ExpiresAt < DateTimeOffset.UtcNow)
            throw new UnauthorizedAppException("Refresh token süresi dolmuş.");

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return await BuildLoginResponseAsync(stored.User, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = TokenHelper.HashToken(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
        if (stored is not null)
        {
            stored.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<UserInfoDto> GetMeAsync(CancellationToken ct = default)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAppException();
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        var bu = await db.BusinessUsers.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        string? slug = null;
        if (bu is not null)
            slug = await db.Businesses.Where(b => b.Id == bu.BusinessId).Select(b => b.Slug).FirstAsync(ct);
        return MapUserInfo(user, bu?.BusinessId, slug);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var hash = TokenHelper.HashToken(request.Token);
        var token = await db.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new ValidationException("Geçersiz doğrulama bağlantısı.");

        if (token.ExpiresAt < DateTimeOffset.UtcNow)
            throw new ValidationException("Doğrulama bağlantısının süresi dolmuş.");

        token.User.EmailVerified = true;
        token.User.UpdatedDate = DateTimeOffset.UtcNow;
        db.EmailVerificationTokens.Remove(token);
        await db.SaveChangesAsync(ct);
    }

    public async Task ResendVerificationAsync(CancellationToken ct = default)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedAppException();
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        if (user.EmailVerified) return;

        var now = DateTimeOffset.UtcNow;
        var oldTokens = await db.EmailVerificationTokens.Where(t => t.UserId == userId).ToListAsync(ct);
        db.EmailVerificationTokens.RemoveRange(oldTokens);

        var token = TokenHelper.GenerateToken();
        db.EmailVerificationTokens.Add(new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = TokenHelper.HashToken(token),
            ExpiresAt = now.AddDays(1),
            CreatedDate = now
        });
        await db.SaveChangesAsync(ct);
        await emailService.SendVerificationEmailAsync(user.Email, token, ct);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);
        if (user is null) return;

        var now = DateTimeOffset.UtcNow;
        var token = TokenHelper.GenerateToken();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHelper.HashToken(token),
            ExpiresAt = now.AddHours(1),
            IsUsed = false,
            CreatedDate = now
        });
        await db.SaveChangesAsync(ct);
        await emailService.SendPasswordResetEmailAsync(user.Email, token, ct);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var hash = TokenHelper.HashToken(request.Token);
        var token = await db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash && !t.IsUsed, ct)
            ?? throw new ValidationException("Geçersiz şifre sıfırlama bağlantısı.");

        if (token.ExpiresAt < DateTimeOffset.UtcNow)
            throw new ValidationException("Şifre sıfırlama bağlantısının süresi dolmuş.");

        token.User.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        token.User.UpdatedDate = DateTimeOffset.UtcNow;
        token.IsUsed = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var token = TokenHelper.GenerateToken();
        var now = DateTimeOffset.UtcNow;
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = TokenHelper.HashToken(token),
            ExpiresAt = now.AddDays(_jwt.RefreshTokenDays),
            CreatedDate = now
        });
        await db.SaveChangesAsync(ct);
        return token;
    }

    private async Task<LoginResponse> BuildLoginResponseAsync(User user, CancellationToken ct)
    {
        var bu = await db.BusinessUsers.FirstOrDefaultAsync(x => x.UserId == user.Id, ct);
        var accessToken = jwtTokenService.CreateAccessToken(user, bu?.BusinessId);
        string? slug = null;
        if (bu is not null)
            slug = await db.Businesses.Where(b => b.Id == bu.BusinessId).Select(b => b.Slug).FirstOrDefaultAsync(ct);
        return new LoginResponse(accessToken, MapUserInfo(user, bu?.BusinessId, slug));
    }

    private static UserInfoDto MapUserInfo(User user, Guid? businessId, string? slug) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Role, businessId, slug, user.EmailVerified);

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var i = 2;
        while (await db.Businesses.AnyAsync(b => b.Slug == slug, ct))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }
}
