using Microsoft.EntityFrameworkCore;
using NurseryHub.Locations;
using NurseryHub.Nurseries;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace NurseryHub.EntityFrameworkCore;

public static class NurseryHubDbContextModelCreatingExtensions
{
    public static void ConfigureNurseryHub(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Nursery>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Nurseries", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(Nursery.MaxNameLength);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(Nursery.MaxPhoneNumberLength);
            b.Property(x => x.Email).IsRequired().HasMaxLength(Nursery.MaxEmailLength);
            b.Property(x => x.LogoUrl).HasMaxLength(Nursery.MaxLogoUrlLength);
            b.Property(x => x.WebsiteUrl).HasMaxLength(Nursery.MaxWebsiteUrlLength);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Name });
            b.HasIndex(x => new { x.TenantId, x.Email });
        });

        builder.Entity<Governorate>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Governorates", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(Governorate.MaxCodeLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(Governorate.MaxNameLength);
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(Governorate.MaxNameLength);

            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<City>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Cities", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code).IsRequired().HasMaxLength(City.MaxCodeLength);
            b.Property(x => x.NameEn).IsRequired().HasMaxLength(City.MaxNameLength);
            b.Property(x => x.NameAr).IsRequired().HasMaxLength(City.MaxNameLength);

            b.HasIndex(x => new { x.GovernorateId, x.NameEn });
            b.HasIndex(x => x.Code).IsUnique();

            b.HasOne<Governorate>()
                .WithMany()
                .HasForeignKey(x => x.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<NurseryBranch>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "NurseryBranches", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(NurseryBranch.MaxNameLength);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(NurseryBranch.MaxPhoneNumberLength);
            b.Property(x => x.AddressLine).HasMaxLength(NurseryBranch.MaxAddressLength);
            b.Property(x => x.FacebookLink).HasMaxLength(NurseryBranch.MaxFacebookLinkLength);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.NurseryId, x.Name });
            b.HasIndex(x => new { x.TenantId, x.GovernorateId, x.CityId });

            b.HasOne<Nursery>()
                .WithMany()
                .HasForeignKey(x => x.NurseryId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Governorate>()
                .WithMany()
                .HasForeignKey(x => x.GovernorateId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<City>()
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<NurseryClass>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "NurseryClasses", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(NurseryClass.MaxNameLength);
            b.Property(x => x.Capacity).IsRequired();
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId, x.Name });

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserBranch>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "UserBranches", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.IsPrimary).HasDefaultValue(false);

            b.HasIndex(x => new { x.TenantId, x.UserId, x.NurseryBranchId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId });

            b.HasOne<Volo.Abp.Identity.IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
