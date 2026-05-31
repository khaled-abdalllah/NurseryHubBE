using Microsoft.EntityFrameworkCore;
using NurseryHub.Locations;
using NurseryHub.Nurseries;
using NurseryHub.Portal;
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
            b.Property(x => x.LogoData).HasColumnType("varbinary(max)");
            b.Property(x => x.LogoContentType).HasMaxLength(Nursery.MaxLogoContentTypeLength);
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
            b.Property(x => x.Description).HasMaxLength(NurseryClass.MaxDescriptionLength);
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

            b.Property(x => x.NurseryBranchId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(GradeCategory.MaxNameLength);
            b.Property(x => x.Icon).IsRequired().HasMaxLength(GradeCategory.MaxIconLength);
            b.Property(x => x.ColorToken).IsRequired().HasMaxLength(GradeCategory.MaxColorTokenLength);
            b.Property(x => x.Description).HasMaxLength(GradeCategory.MaxDescriptionLength);
            b.Property(x => x.IsActive).HasDefaultValue(true);

            b.HasIndex(x => x.NurseryBranchId);
            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId, x.Name }).IsUnique();

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ParentContact>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "ParentContacts", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.FatherName).IsRequired().HasMaxLength(ParentContact.MaxFullNameLength);
            b.Property(x => x.FatherIdentityNumber).IsRequired().HasMaxLength(ParentContact.MaxIdentityNumberLength);
            b.Property(x => x.FatherPhoneNumber).IsRequired().HasMaxLength(ParentContact.MaxPhoneNumberLength);
            b.Property(x => x.MotherName).IsRequired().HasMaxLength(ParentContact.MaxFullNameLength);
            b.Property(x => x.MotherIdentityNumber).IsRequired().HasMaxLength(ParentContact.MaxIdentityNumberLength);
            b.Property(x => x.MotherPhoneNumber).IsRequired().HasMaxLength(ParentContact.MaxPhoneNumberLength);

            b.HasIndex(x => x.TenantId);
        });

        builder.Entity<StudentApplication>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "StudentApplications", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ChildFullName).IsRequired().HasMaxLength(StudentApplication.MaxChildFullNameLength);
            b.Property(x => x.Gender).IsRequired().HasMaxLength(16);
            b.Property(x => x.ParentFullName).IsRequired().HasMaxLength(StudentApplication.MaxParentFullNameLength);
            b.Property(x => x.ParentPhoneNumber).IsRequired().HasMaxLength(StudentApplication.MaxPhoneNumberLength);
            b.Property(x => x.SecondaryPhoneNumber).HasMaxLength(StudentApplication.MaxPhoneNumberLength);
            b.Property(x => x.Email).HasMaxLength(StudentApplication.MaxEmailLength);
            b.Property(x => x.Notes).HasMaxLength(StudentApplication.MaxNotesLength);
            b.Property(x => x.Status).HasConversion<int>();

            b.HasIndex(x => x.ParentPhoneNumber);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.BirthDate);

            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.CreationTime });

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<GradeCategory>()
                .WithMany()
                .HasForeignKey(x => x.RequestedGradeCategoryId)
                .OnDelete(DeleteBehavior.NoAction);
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
            b.Property(x => x.EmergencyContactNumber).IsRequired().HasMaxLength(Student.MaxPhoneNumberLength);
            b.Property(x => x.HealthNotes).HasMaxLength(Student.MaxHealthNotesLength);
            b.Property(x => x.DietaryRestrictions).HasMaxLength(Student.MaxDietaryRestrictionsLength);
            b.Property(x => x.AllergyNotes).HasMaxLength(Student.MaxAllergyNotesLength);
            b.Property(x => x.WeightKg).HasColumnType("decimal(5,2)");
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
            b.HasIndex(x => x.ParentId);

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<NurseryClass>()
                .WithMany()
                .HasForeignKey(x => x.NurseryClassId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.Parent)
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Payments", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.PaymentNumber).IsRequired().HasMaxLength(Payment.MaxPaymentNumberLength);
            b.Property(x => x.PaymentMethod).HasConversion<int>();
            b.Property(x => x.PaymentPurpose).HasConversion<int>();
            b.Property(x => x.CustomPaymentPurpose).HasMaxLength(Payment.MaxCustomPurposeLength);
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Notes).HasMaxLength(Payment.MaxNotesLength);

            b.HasIndex(x => new { x.TenantId, x.PaymentNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.PaymentDate });
            b.HasIndex(x => new { x.TenantId, x.StudentId });
            b.HasIndex(x => new { x.TenantId, x.GradeId });
            b.HasIndex(x => new { x.TenantId, x.ClassId });

            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<GradeCategory>()
                .WithMany()
                .HasForeignKey(x => x.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<NurseryClass>()
                .WithMany()
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Expense>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Expenses", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ExpenseNumber).IsRequired().HasMaxLength(Expense.MaxExpenseNumberLength);
            b.Property(x => x.Title).IsRequired().HasMaxLength(Expense.MaxTitleLength);
            b.Property(x => x.ExpenseCategory).HasConversion<int>();
            b.Property(x => x.CustomExpenseCategory).HasMaxLength(Expense.MaxCustomCategoryLength);
            b.Property(x => x.VendorName).HasMaxLength(Expense.MaxVendorNameLength);
            b.Property(x => x.PaymentMethod).HasConversion<int>();
            b.Property(x => x.PaymentStatus).HasConversion<int>();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.ReceiptNumber).HasMaxLength(Expense.MaxReceiptNumberLength);
            b.Property(x => x.Notes).HasMaxLength(Expense.MaxNotesLength);

            b.HasIndex(x => x.NurseryBranchId);
            b.HasIndex(x => new { x.TenantId, x.ExpenseNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId, x.ExpenseDate });
            b.HasIndex(x => new { x.TenantId, x.ExpenseDate });
            b.HasIndex(x => new { x.TenantId, x.ExpenseCategory });
            b.HasIndex(x => new { x.TenantId, x.PaymentStatus });
            b.HasIndex(x => new { x.TenantId, x.PaymentMethod });

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Notification>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Notifications", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(Notification.MaxTitleLength);
            b.Property(x => x.Message).IsRequired().HasMaxLength(Notification.MaxMessageLength);
            b.Property(x => x.NotificationType).HasConversion<int>();
            b.Property(x => x.PriorityLevel).HasConversion<int>();
            b.Property(x => x.AudienceType).HasConversion<int>();
            b.Property(x => x.Status).HasConversion<int>();

            b.HasIndex(x => x.BranchId);
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.NotificationType });
            b.HasIndex(x => new { x.TenantId, x.CreationTime });

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.RelatedStudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<NotificationRecipient>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "NotificationRecipients", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.DeliveryStatus).HasConversion<int>();
            b.Property(x => x.FailureReason).HasMaxLength(NotificationRecipient.MaxFailureReasonLength);

            b.HasIndex(x => x.NotificationId);
            b.HasIndex(x => new { x.NotificationId, x.ParentUserId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.DeliveryStatus });

            b.HasOne<Notification>()
                .WithMany()
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Attendance>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "Attendances", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Date).HasColumnType("date");
            b.Property(x => x.Status).HasConversion<int>();

            b.HasIndex(x => new { x.StudentId, x.Date }).IsUnique();

            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyFollowupBook>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "DailyFollowupBooks", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ReportDate).HasColumnType("date");
            b.Property(x => x.OverallMood).HasConversion<int>();
            b.Property(x => x.TeacherNote).IsRequired().HasMaxLength(DailyFollowupBook.MaxTeacherNoteLength);
            b.Property(x => x.SleepDuration).HasMaxLength(DailyFollowupBook.MaxSleepDurationLength);
            b.Property(x => x.MoodAfterWaking).HasConversion<int?>();
            b.Property(x => x.IsDraft).HasDefaultValue(true);
            b.Property(x => x.SentToParent).HasDefaultValue(false);
            b.Property(x => x.IsVisibleToParent).HasDefaultValue(false);

            b.HasIndex(x => new { x.StudentId, x.ReportDate }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NurseryBranchId, x.ReportDate });

            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<NurseryBranch>()
                .WithMany()
                .HasForeignKey(x => x.NurseryBranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DailyFollowupSubjectEntry>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "DailyFollowupSubjectEntries", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.SubjectName).IsRequired().HasMaxLength(DailyFollowupSubjectEntry.MaxSubjectNameLength);
            b.Property(x => x.SubjectIcon).IsRequired().HasMaxLength(DailyFollowupSubjectEntry.MaxSubjectIconLength);
            b.Property(x => x.Notes).HasMaxLength(DailyFollowupSubjectEntry.MaxNotesLength);

            b.HasIndex(x => new { x.DailyFollowupBookId, x.SortOrder });

            b.HasOne<DailyFollowupBook>()
                .WithMany(x => x.Subjects)
                .HasForeignKey(x => x.DailyFollowupBookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyFollowupActivityEntry>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "DailyFollowupActivityEntries", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ActivityType).HasConversion<int>();

            b.HasIndex(x => new { x.DailyFollowupBookId, x.ActivityType }).IsUnique();

            b.HasOne<DailyFollowupBook>()
                .WithMany(x => x.Activities)
                .HasForeignKey(x => x.DailyFollowupBookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DailyFollowupMealEntry>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "DailyFollowupMealEntries", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.MealType).HasConversion<int>();
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.Notes).HasMaxLength(DailyFollowupMealEntry.MaxNotesLength);

            b.HasIndex(x => new { x.DailyFollowupBookId, x.MealType }).IsUnique();

            b.HasOne<DailyFollowupBook>()
                .WithMany(x => x.Meals)
                .HasForeignKey(x => x.DailyFollowupBookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ParentStudent>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "ParentStudents", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasIndex(x => new { x.TenantId, x.ParentUserId });
            b.HasIndex(x => new { x.TenantId, x.StudentId });
            b.HasIndex(x => new { x.TenantId, x.ParentUserId, x.StudentId }).IsUnique();

            b.HasOne<Volo.Abp.Identity.IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.ParentUserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Student>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Volo.Abp.TenantManagement.Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
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

        builder.Entity<PackageSubscriptionInquiry>(b =>
        {
            b.ToTable(NurseryHubConsts.DbTablePrefix + "PackageSubscriptionInquiries", NurseryHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.PackageTier).HasConversion<int>();
            b.Property(x => x.NurseryName).IsRequired().HasMaxLength(PackageSubscriptionInquiryConsts.MaxNurseryNameLength);
            b.Property(x => x.ContactName).IsRequired().HasMaxLength(PackageSubscriptionInquiryConsts.MaxContactNameLength);
            b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(PackageSubscriptionInquiryConsts.MaxPhoneNumberLength);
            b.Property(x => x.Email).IsRequired().HasMaxLength(PackageSubscriptionInquiryConsts.MaxEmailLength);
            b.Property(x => x.Message).HasMaxLength(PackageSubscriptionInquiryConsts.MaxMessageLength);

            b.HasIndex(x => x.CreationTime);
            b.HasIndex(x => x.PackageTier);
        });
    }
}
