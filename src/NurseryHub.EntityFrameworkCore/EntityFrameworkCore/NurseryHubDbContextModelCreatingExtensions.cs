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
            b.HasIndex(x => new { x.TenantId, x.GradeCategoryId });

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<GradeCategory>()
                .WithMany()
                .HasForeignKey(x => x.GradeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GradeCategory>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "GradeCategories", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(GradeCategory.MaxNameLength);
            b.Property(x => x.Icon).IsRequired().HasMaxLength(GradeCategory.MaxIconLength);
            b.Property(x => x.ColorToken).IsRequired().HasMaxLength(GradeCategory.MaxColorTokenLength);
            b.Property(x => x.Description).HasMaxLength(GradeCategory.MaxDescriptionLength);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        });

        builder.Entity<Student>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Students", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.FullName).IsRequired().HasMaxLength(Student.MaxFullNameLength);
            b.Property(x => x.Gender).IsRequired().HasMaxLength(16);
            b.Property(x => x.BloodType).HasMaxLength(Student.MaxBloodTypeLength);
            b.Property(x => x.Religion).HasMaxLength(Student.MaxReligionLength);
            b.Property(x => x.HomeAddress).HasMaxLength(Student.MaxHomeAddressLength);
            b.Property(x => x.FatherName).IsRequired().HasMaxLength(Student.MaxFullNameLength);
            b.Property(x => x.FatherIdentityNumber).IsRequired().HasMaxLength(Student.MaxIdentityNumberLength);
            b.Property(x => x.FatherPhoneNumber).IsRequired().HasMaxLength(Student.MaxPhoneNumberLength);
            b.Property(x => x.MotherName).IsRequired().HasMaxLength(Student.MaxFullNameLength);
            b.Property(x => x.MotherIdentityNumber).IsRequired().HasMaxLength(Student.MaxIdentityNumberLength);
            b.Property(x => x.MotherPhoneNumber).IsRequired().HasMaxLength(Student.MaxPhoneNumberLength);
            b.Property(x => x.EmergencyContactNumber).IsRequired().HasMaxLength(Student.MaxPhoneNumberLength);
            b.Property(x => x.HealthNotes).HasMaxLength(Student.MaxHealthNotesLength);
            b.Property(x => x.DietaryRestrictions).HasMaxLength(Student.MaxDietaryRestrictionsLength);
            b.Property(x => x.ToiletTrainingStatus).HasMaxLength(Student.MaxToiletTrainingStatusLength);
            b.Property(x => x.AttendsSunday).HasDefaultValue(false);
            b.Property(x => x.AttendsMonday).HasDefaultValue(false);
            b.Property(x => x.AttendsTuesday).HasDefaultValue(false);
            b.Property(x => x.AttendsWednesday).HasDefaultValue(false);
            b.Property(x => x.AttendsThursday).HasDefaultValue(false);
            b.Property(x => x.AttendsFriday).HasDefaultValue(false);
            b.Property(x => x.AttendsSaturday).HasDefaultValue(false);
            b.Property(x => x.MedicalNotes).HasMaxLength(Student.MaxMedicalNotesLength);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId, x.FullName });
            b.HasIndex(x => new { x.TenantId, x.NurseryClassId });

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<NurseryClass>()
                .WithMany()
                .HasForeignKey(x => x.NurseryClassId)
                .OnDelete(DeleteBehavior.NoAction);
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
