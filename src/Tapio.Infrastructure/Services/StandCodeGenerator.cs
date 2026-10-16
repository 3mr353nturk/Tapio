using Microsoft.EntityFrameworkCore;
using Tapio.Application.Services;
using Tapio.Infrastructure.Persistence;

namespace Tapio.Infrastructure.Services;

public class StandCodeGenerator(ApplicationDbContext db) : IStandCodeGenerator
{
    public async Task<string> GenerateAsync(CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEXT VALUE FOR dbo.StandCodeSequence";
        var result = await command.ExecuteScalarAsync(ct);
        var seq = Convert.ToInt64(result);
        return $"TAP-{seq:D6}";
    }
}
