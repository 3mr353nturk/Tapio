using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tapio.Application.Services;
using Tapio.Infrastructure.Caching;
using Tapio.Infrastructure.Identity;
using Tapio.Infrastructure.Persistence;
using Tapio.Infrastructure.Services;

namespace Tapio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost,1433;Database=Tapio;User Id=sa;Password=Tapio123!;TrustServerCertificate=True";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
            services.AddSingleton<IPublicBusinessCache, RedisPublicBusinessCache>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<IPublicBusinessCache, NullPublicBusinessCache>();
        }

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, LogEmailService>();
        services.AddScoped<IStandCodeGenerator, StandCodeGenerator>();
        services.AddScoped<IPublicBusinessService, PublicBusinessService>();
        services.AddScoped<IPublicEventService, PublicEventService>();
        services.AddScoped<IBusinessProfileService, BusinessProfileService>();
        services.AddScoped<IBusinessMediaService, BusinessMediaService>();
        services.AddScoped<IBusinessActionService, BusinessActionService>();
        services.AddScoped<IBusinessCampaignService, BusinessCampaignService>();
        services.AddScoped<IBusinessStandService, BusinessStandService>();
        services.AddScoped<IBusinessAnalyticsService, BusinessAnalyticsService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IAdminBusinessService, AdminBusinessService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminStandService, AdminStandService>();
        services.AddScoped<IAdminStatsService, AdminStatsService>();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole(nameof(Domain.Enums.UserRole.Admin)))
            .AddPolicy("BusinessUserOnly", p => p.RequireRole(nameof(Domain.Enums.UserRole.BusinessUser)));

        return services;
    }
}
