using System;
using Xunit;

namespace NurseryHub.Nurseries;

public class CreateUpdateStudentDtoValidatorTests
{
    private static CreateUpdateStudentDtoValidator CreateValidator() => new();

    private static CreateUpdateStudentDto BuildValidDto()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var birth = today.AddYears(-3);

        return new CreateUpdateStudentDto
        {
            NurseryBranchId = Guid.NewGuid(),
            FullName = "محمد علي",
            BirthDate = birth,
            Gender = "Male",
            FatherName = "أحمد محمد",
            FatherPhoneNumber = "01012345678",
            EmergencyContactNumber = "01212345678",
            EnrollmentDate = today,
            AttendsSunday = true,
            AttendsMonday = false,
            AttendsTuesday = false,
            AttendsWednesday = false,
            AttendsThursday = false,
            AttendsFriday = false,
            AttendsSaturday = false,
            IsActive = true,
        };
    }

    [Fact]
    public void Valid_dto_should_pass()
    {
        var result = CreateValidator().Validate(BuildValidDto());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Invalid_father_phone_should_fail()
    {
        var dto = BuildValidDto();
        dto.FatherPhoneNumber = "123";
        var result = CreateValidator().Validate(dto);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Future_birth_date_should_fail()
    {
        var dto = BuildValidDto();
        dto.BirthDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var result = CreateValidator().Validate(dto);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void No_attendance_day_should_fail()
    {
        var dto = BuildValidDto();
        dto.AttendsSunday = false;
        var result = CreateValidator().Validate(dto);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Optional_father_national_id_empty_should_pass()
    {
        var dto = BuildValidDto();
        dto.FatherIdentityNumber = null;
        var result = CreateValidator().Validate(dto);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Father_national_id_wrong_length_should_fail()
    {
        var dto = BuildValidDto();
        dto.FatherIdentityNumber = "123";
        var result = CreateValidator().Validate(dto);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Parent_portal_with_mother_source_and_empty_mother_phone_should_fail()
    {
        var dto = BuildValidDto();
        dto.CreateParentPortalAccount = true;
        dto.ParentLoginUsernameSource = ParentPortalLoginUsernameSource.MotherPhone;
        dto.MotherPhoneNumber = null;
        var result = CreateValidator().Validate(dto);
        Assert.False(result.IsValid);
    }
}
