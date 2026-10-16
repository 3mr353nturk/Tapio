namespace Tapio.Application.Services;

public interface IStandCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken ct = default);
}
