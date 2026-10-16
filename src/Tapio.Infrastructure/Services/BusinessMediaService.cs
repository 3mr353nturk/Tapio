using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Tapio.Application.Common.Exceptions;
using Tapio.Application.DTOs.Business;
using Tapio.Application.Services;
using Tapio.Infrastructure.Persistence;

namespace Tapio.Infrastructure.Services;

public class BusinessMediaService(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    IPublicBusinessCache cache,
    IWebHostEnvironment env) : IBusinessMediaService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public Task<MediaUploadResponse> UploadLogoAsync(Stream content, string contentType, string fileName, CancellationToken ct = default) =>
        SaveAsync(content, contentType, fileName, "logo", ct);

    public Task<MediaUploadResponse> UploadCoverAsync(Stream content, string contentType, string fileName, CancellationToken ct = default) =>
        SaveAsync(content, contentType, fileName, "cover", ct);

    private async Task<MediaUploadResponse> SaveAsync(
        Stream content,
        string contentType,
        string fileName,
        string kind,
        CancellationToken ct)
    {
        if (!AllowedTypes.Contains(contentType))
            throw new ValidationException("Yalnızca JPEG, PNG, WebP veya GIF yükleyebilirsiniz.");

        var businessId = currentUser.BusinessId ?? throw new UnauthorizedAppException();
        var business = await db.Businesses.FirstAsync(b => b.Id == businessId, ct);
        var slug = business.Slug;

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5)
            ext = contentType switch
            {
                "image/png" => ".png",
                "image/webp" => ".webp",
                "image/gif" => ".gif",
                _ => ".jpg"
            };

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", businessId.ToString("N"));
        Directory.CreateDirectory(dir);

        var relative = $"/uploads/{businessId:N}/{kind}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(dir, kind + ext.ToLowerInvariant());

        await using (var fs = File.Create(fullPath))
            await content.CopyToAsync(fs, ct);

        if (kind == "logo") business.LogoUrl = relative;
        else business.CoverImageUrl = relative;

        business.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await cache.InvalidateAsync(slug, ct);

        return new MediaUploadResponse(relative);
    }
}
