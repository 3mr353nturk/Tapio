using Microsoft.Extensions.Logging;
using Tapio.Application.Services;

namespace Tapio.Infrastructure.Services;

public class LogEmailService(ILogger<LogEmailService> logger) : IEmailService
{
    public Task SendVerificationEmailAsync(string email, string token, CancellationToken ct = default)
    {
        logger.LogInformation("Verification email to {Email}: /verify-email?token={Token}", email, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken ct = default)
    {
        logger.LogInformation("Password reset email to {Email}: /reset-password?token={Token}", email, token);
        return Task.CompletedTask;
    }

}
