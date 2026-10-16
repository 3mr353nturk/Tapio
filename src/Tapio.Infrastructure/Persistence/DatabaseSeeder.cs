using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tapio.Domain.Entities;
using Tapio.Domain.Enums;
using Tapio.Infrastructure.Services;

namespace Tapio.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (db.Database.IsSqlServer())
        {
            await db.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'StandCodeSequence')
                CREATE SEQUENCE dbo.StandCodeSequence AS BIGINT START WITH 1 INCREMENT BY 1;
                """);
        }

        if (await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
            return;

        var now = DateTimeOffset.UtcNow;
        db.Users.Add(new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "admin@tapio.app",
            PasswordHash = PasswordHasher.Hash("Admin123!"),
            FirstName = "Tapio",
            LastName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            EmailVerified = true,
            CreatedDate = now,
            UpdatedDate = now
        });
        await db.SaveChangesAsync();
    }
}
