namespace Tapio.Application.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token, CancellationToken ct = default);
    Task SendPasswordResetEmailAsync(string email, string token, CancellationToken ct = default);
}
