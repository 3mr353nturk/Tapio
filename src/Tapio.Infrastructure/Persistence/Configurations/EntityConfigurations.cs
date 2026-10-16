using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tapio.Domain.Entities;

namespace Tapio.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
    }
}

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.WhatsAppNumber).HasMaxLength(20);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
    }
}

public class BusinessUserConfiguration : IEntityTypeConfiguration<BusinessUser>
{
    public void Configure(EntityTypeBuilder<BusinessUser> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.BusinessId }).IsUnique();
        builder.HasOne(x => x.User).WithMany(x => x.BusinessUsers).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Business).WithMany(x => x.BusinessUsers).HasForeignKey(x => x.BusinessId);
    }
}

public class TapioStandConfiguration : IEntityTypeConfiguration<TapioStand>
{
    public void Configure(EntityTypeBuilder<TapioStand> builder)
    {
        builder.HasIndex(x => x.StandCode).IsUnique();
        builder.Property(x => x.StandCode).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.Business).WithMany(x => x.Stands).HasForeignKey(x => x.BusinessId).IsRequired(false);
    }
}

public class BusinessActionConfiguration : IEntityTypeConfiguration<BusinessAction>
{
    public void Configure(EntityTypeBuilder<BusinessAction> builder)
    {
        builder.HasIndex(x => x.BusinessId);
        builder.Property(x => x.Title).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
    }
}

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasIndex(x => x.BusinessId);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
    }
}

public class TapioEventConfiguration : IEntityTypeConfiguration<TapioEvent>
{
    public void Configure(EntityTypeBuilder<TapioEvent> builder)
    {
        builder.HasIndex(x => new { x.BusinessId, x.CreatedDate });
        builder.HasIndex(x => x.StandId);
        builder.HasIndex(x => x.EventType);
        builder.Property(x => x.SessionId).HasMaxLength(64).IsRequired();
    }
}

public class BusinessInvitationConfiguration : IEntityTypeConfiguration<BusinessInvitation>
{
    public void Configure(EntityTypeBuilder<BusinessInvitation> builder)
    {
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
    }
}

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}

