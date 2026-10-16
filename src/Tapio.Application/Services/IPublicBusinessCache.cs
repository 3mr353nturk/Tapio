using Tapio.Application.DTOs.Public;

namespace Tapio.Application.Services;

public interface IPublicBusinessCache
{
    Task<PublicBusinessResponse?> GetAsync(string slug, CancellationToken ct = default);
    Task SetAsync(string slug, PublicBusinessResponse response, CancellationToken ct = default);
    Task InvalidateAsync(string slug, CancellationToken ct = default);
}
