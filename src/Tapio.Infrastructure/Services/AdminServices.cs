using Microsoft.EntityFrameworkCore;
using Tapio.Application.Common;
using Tapio.Application.Common.Exceptions;
using Tapio.Application.Common.Helpers;
using Tapio.Application.DTOs.Admin;
using Tapio.Application.Services;
using Tapio.Domain.Entities;
using Tapio.Domain.Enums;
using Tapio.Infrastructure.Persistence;

namespace Tapio.Infrastructure.Services;

public class AdminBusinessService(ApplicationDbContext db) : IAdminBusinessService
{
    public async Task<PagedResult<AdminBusinessListItem>> GetAllAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default)
    {
        var query = db.Businesses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(term) ||
                b.Slug.ToLower().Contains(term) ||
                (b.Category != null && b.Category.ToLower().Contains(term)));
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "name" => sortDesc ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
            "slug" => sortDesc ? query.OrderByDescending(b => b.Slug) : query.OrderBy(b => b.Slug),
            "category" => sortDesc ? query.OrderByDescending(b => b.Category) : query.OrderBy(b => b.Category),
            "createddate" => sortDesc ? query.OrderByDescending(b => b.CreatedDate) : query.OrderBy(b => b.CreatedDate),
            _ => sortDesc ? query.OrderByDescending(b => b.CreatedDate) : query.OrderBy(b => b.CreatedDate)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new AdminBusinessListItem(b.Id, b.Name, b.Slug, b.Category, b.IsActive, b.RegistrationSource, b.CreatedDate))
            .ToListAsync(ct);
        return new PagedResult<AdminBusinessListItem> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<AdminBusinessDetail> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Businesses.AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new AdminBusinessDetail(b.Id, b.Name, b.Slug, b.Category, b.Phone, b.Email, b.Address, b.City, b.District, b.IsActive, b.RegistrationSource, b.CreatedDate))
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException();
    }

    public async Task<AdminCreateBusinessResponse> CreateAsync(AdminCreateBusinessRequest request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var slug = await GenerateUniqueSlugAsync(request.Name, ct);

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug,
            Category = request.Category,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            City = request.City,
            District = request.District,
            IsActive = true,
            RegistrationSource = BusinessRegistrationSource.Admin,
            CreatedDate = now,
            UpdatedDate = now
        };

        db.Businesses.Add(business);
        await db.SaveChangesAsync(ct);

        return new AdminCreateBusinessResponse(business.Id, business.Slug);
    }

    public async Task<AdminBusinessDetail> UpdateAsync(Guid id, AdminUpdateBusinessRequest request, CancellationToken ct = default)
    {
        var business = await db.Businesses.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException();
        business.Name = request.Name;
        business.Category = request.Category;
        business.Phone = request.Phone;
        business.Email = request.Email;
        business.Address = request.Address;
        business.City = request.City;
        business.District = request.District;
        business.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task SetStatusAsync(Guid id, bool isActive, CancellationToken ct = default)
    {
        var business = await db.Businesses.FirstOrDefaultAsync(b => b.Id == id, ct) ?? throw new NotFoundException();
        business.IsActive = isActive;
        business.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

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

public class AdminUserService(ApplicationDbContext db) : IAdminUserService
{
    public async Task<PagedResult<AdminUserListItem>> GetAllAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default)
    {
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "email" => sortDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "firstname" => sortDesc ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => sortDesc ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "role" => sortDesc ? query.OrderByDescending(u => u.Role) : query.OrderBy(u => u.Role),
            _ => sortDesc ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new AdminUserListItem(u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.IsActive, u.EmailVerified))
            .ToListAsync(ct);
        return new PagedResult<AdminUserListItem> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task AssignBusinessAsync(Guid userId, AssignBusinessUserRequest request, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct) ?? throw new NotFoundException();
        var business = await db.Businesses.FirstOrDefaultAsync(b => b.Id == request.BusinessId, ct) ?? throw new NotFoundException();

        if (user.Role != UserRole.BusinessUser)
            throw new ValidationException("Yalnızca işletme kullanıcıları işletmeye atanabilir.");

        if (await db.BusinessUsers.AnyAsync(bu => bu.UserId == userId && bu.BusinessId == request.BusinessId, ct))
            throw new ValidationException("Kullanıcı zaten bu işletmeye atanmış.");

        db.BusinessUsers.Add(new BusinessUser
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BusinessId = business.Id,
            Role = request.Role,
            CreatedDate = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}

public class AdminStandService(ApplicationDbContext db, IStandCodeGenerator standCodeGenerator) : IAdminStandService
{
    public async Task<PagedResult<AdminStandResponse>> GetAllAsync(int page, int pageSize, bool? unassignedOnly, string? search = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default)
    {
        var query = db.TapioStands.AsNoTracking().AsQueryable();
        if (unassignedOnly == true)
            query = query.Where(s => s.BusinessId == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.StandCode.ToLower().Contains(term) ||
                (s.Business != null && s.Business.Name.ToLower().Contains(term)));
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "standcode" => sortDesc ? query.OrderByDescending(s => s.StandCode) : query.OrderBy(s => s.StandCode),
            "businessname" => sortDesc ? query.OrderByDescending(s => s.Business!.Name) : query.OrderBy(s => s.Business!.Name),
            "createddate" => sortDesc ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate),
            _ => sortDesc ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new AdminStandResponse(s.Id, s.StandCode, s.BusinessId, s.Business != null ? s.Business.Name : null, s.IsActive, s.CreatedDate))
            .ToListAsync(ct);
        return new PagedResult<AdminStandResponse> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<AdminStandResponse> CreateAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var standCode = await standCodeGenerator.GenerateAsync(ct);
        var stand = new TapioStand
        {
            Id = Guid.NewGuid(),
            StandCode = standCode,
            BusinessId = null,
            IsActive = true,
            CreatedDate = now,
            UpdatedDate = now
        };
        db.TapioStands.Add(stand);
        await db.SaveChangesAsync(ct);
        return new AdminStandResponse(stand.Id, stand.StandCode, null, null, stand.IsActive, stand.CreatedDate);
    }

    public async Task<AdminStandResponse> AssignAsync(Guid standId, AssignStandRequest request, CancellationToken ct = default)
    {
        var stand = await db.TapioStands.FirstOrDefaultAsync(s => s.Id == standId, ct) ?? throw new NotFoundException();
        var business = await db.Businesses.FirstOrDefaultAsync(b => b.Id == request.BusinessId, ct) ?? throw new NotFoundException();
        stand.BusinessId = business.Id;
        stand.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return new AdminStandResponse(stand.Id, stand.StandCode, business.Id, business.Name, stand.IsActive, stand.CreatedDate);
    }

    public async Task<AdminStandResponse> UnassignAsync(Guid standId, CancellationToken ct = default)
    {
        var stand = await db.TapioStands.FirstOrDefaultAsync(s => s.Id == standId, ct) ?? throw new NotFoundException();
        stand.BusinessId = null;
        stand.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return new AdminStandResponse(stand.Id, stand.StandCode, null, null, stand.IsActive, stand.CreatedDate);
    }

    public async Task SetStatusAsync(Guid standId, bool isActive, CancellationToken ct = default)
    {
        var stand = await db.TapioStands.FirstOrDefaultAsync(s => s.Id == standId, ct) ?? throw new NotFoundException();
        stand.IsActive = isActive;
        stand.UpdatedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public class AdminStatsService(ApplicationDbContext db) : IAdminStatsService
{
    public async Task<AdminStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return new AdminStatsResponse(
            await db.Businesses.CountAsync(ct),
            await db.Businesses.CountAsync(b => b.IsActive, ct),
            await db.TapioStands.CountAsync(ct),
            await db.TapioStands.CountAsync(s => s.BusinessId == null, ct),
            await db.TapioEvents.CountAsync(e => e.CreatedDate >= today && e.CreatedDate < tomorrow, ct));
    }
}
