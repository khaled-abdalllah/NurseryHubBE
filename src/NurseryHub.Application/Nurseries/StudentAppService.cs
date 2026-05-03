using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NurseryHub.Permissions;
using NurseryHub.Security;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;

namespace NurseryHub.Nurseries;

[Authorize(Roles = NurseryHubRoles.Admin)]
public class StudentAppService
    : CrudAppService<
            Student,
            StudentDto,
            Guid,
            GetStudentsInput,
            CreateUpdateStudentDto,
            CreateUpdateStudentDto>,
        IStudentAppService
{
    private readonly IRepository<NurseryBranch, Guid> _nurseryBranchRepository;
    private readonly IRepository<NurseryClass, Guid> _nurseryClassRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly NurseryMediaOptions _mediaOptions;

    public StudentAppService(
        IRepository<Student, Guid> repository,
        IRepository<NurseryBranch, Guid> nurseryBranchRepository,
        IRepository<NurseryClass, Guid> nurseryClassRepository,
        ITenantRepository tenantRepository,
        IOptions<NurseryMediaOptions> mediaOptions) : base(repository)
    {
        _nurseryBranchRepository = nurseryBranchRepository;
        _nurseryClassRepository = nurseryClassRepository;
        _tenantRepository = tenantRepository;
        _mediaOptions = mediaOptions.Value;
    }

    protected override string? GetPolicyName { get; set; } = NurseryHubPermissions.Students.Default;
    protected override string? GetListPolicyName { get; set; } = NurseryHubPermissions.Students.Default;
    protected override string? CreatePolicyName { get; set; } = NurseryHubPermissions.Students.Create;
    protected override string? UpdatePolicyName { get; set; } = NurseryHubPermissions.Students.Edit;
    protected override string? DeletePolicyName { get; set; } = NurseryHubPermissions.Students.Delete;

    public override async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentsInput input)
    {
        var students = await Repository.GetQueryableAsync();
        var classes = await _nurseryClassRepository.GetQueryableAsync();
        var tenantName = CurrentTenant.Name;

        var filtered = students
            .WhereIf(input.NurseryBranchId.HasValue, x => x.NurseryBranchId == input.NurseryBranchId)
            .WhereIf(input.NurseryClassId.HasValue, x => x.NurseryClassId == input.NurseryClassId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.FullName.Contains(input.Filter!) ||
                x.FatherName.Contains(input.Filter!) ||
                x.MotherName.Contains(input.Filter!) ||
                x.FatherPhoneNumber.Contains(input.Filter!) ||
                x.MotherPhoneNumber.Contains(input.Filter!));

        var query =
            from student in filtered
            join nurseryClass in classes on student.NurseryClassId equals nurseryClass.Id into classJoin
            from nurseryClass in classJoin.DefaultIfEmpty()
            select new
            {
                student.Id,
                student.NurseryBranchId,
                student.NurseryClassId,
                NurseryClassName = nurseryClass != null ? nurseryClass.Name : null,
                student.FullName,
                student.BirthDate,
                student.Gender,
                student.BloodType,
                student.Religion,
                student.HomeAddress,
                student.FatherName,
                student.FatherIdentityNumber,
                student.FatherPhoneNumber,
                student.MotherName,
                student.MotherIdentityNumber,
                student.MotherPhoneNumber,
                student.EmergencyContactNumber,
                student.EnrollmentDate,
                student.HealthNotes,
                student.DietaryRestrictions,
                student.ToiletTrainingStatus,
                student.AttendsSunday,
                student.AttendsMonday,
                student.AttendsTuesday,
                student.AttendsWednesday,
                student.AttendsThursday,
                student.AttendsFriday,
                student.AttendsSaturday,
                student.MedicalNotes,
                student.ProfileImageFileName,
                student.IsActive,
                student.CreationTime,
                student.CreatorId,
                student.LastModificationTime,
                student.LastModifierId,
                student.IsDeleted,
                student.DeleterId,
                student.DeletionTime,
            };

        var totalCount = await AsyncExecuter.CountAsync(query);
        var rawItems = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.FullName).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rawItems.Select(x => new StudentDto
        {
            Id = x.Id,
            NurseryBranchId = x.NurseryBranchId,
            NurseryClassId = x.NurseryClassId,
            NurseryClassName = x.NurseryClassName,
            FullName = x.FullName,
            BirthDate = x.BirthDate,
            Gender = x.Gender,
            BloodType = x.BloodType,
            Religion = x.Religion,
            HomeAddress = x.HomeAddress,
            FatherName = x.FatherName,
            FatherIdentityNumber = x.FatherIdentityNumber,
            FatherPhoneNumber = x.FatherPhoneNumber,
            MotherName = x.MotherName,
            MotherIdentityNumber = x.MotherIdentityNumber,
            MotherPhoneNumber = x.MotherPhoneNumber,
            EmergencyContactNumber = x.EmergencyContactNumber,
            EnrollmentDate = x.EnrollmentDate,
            HealthNotes = x.HealthNotes,
            DietaryRestrictions = x.DietaryRestrictions,
            ToiletTrainingStatus = x.ToiletTrainingStatus,
            AttendsSunday = x.AttendsSunday,
            AttendsMonday = x.AttendsMonday,
            AttendsTuesday = x.AttendsTuesday,
            AttendsWednesday = x.AttendsWednesday,
            AttendsThursday = x.AttendsThursday,
            AttendsFriday = x.AttendsFriday,
            AttendsSaturday = x.AttendsSaturday,
            MedicalNotes = x.MedicalNotes,
            ProfileImageFileName = x.ProfileImageFileName,
            ProfileImageUrl = ResolveStudentImageDisplayUrl(x.ProfileImageFileName, tenantName),
            IsActive = x.IsActive,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId,
            IsDeleted = x.IsDeleted,
            DeleterId = x.DeleterId,
            DeletionTime = x.DeletionTime,
        }).ToList();

        return new PagedResultDto<StudentDto>(totalCount, items);
    }

    public override async Task<StudentDto> GetAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);
        var nurseryClass = entity.NurseryClassId.HasValue
            ? await _nurseryClassRepository.FindAsync(entity.NurseryClassId.Value)
            : null;

        var dto = MapToGetOutputDto(entity);
        dto.NurseryClassName = nurseryClass?.Name;
        dto.ProfileImageUrl = ResolveStudentImageDisplayUrl(dto.ProfileImageFileName, CurrentTenant.Name);
        return dto;
    }

    public virtual async Task<StudentDto> UploadImageAsync(Guid id, UploadStudentImageInput input)
    {
        var entity = await GetEntityByIdAsync(id);
        var file = input.File;
        var ext = Path.GetExtension(file.FileName)!.ToLowerInvariant();
        var tenantName = await GetTenantNameAsync(entity.TenantId);
        var directory = GetStudentDirectoryPath(tenantName);
        Directory.CreateDirectory(directory);

        DeleteStoredStudentImageIfExists(tenantName, entity.ProfileImageFileName);

        var fileName = $"{GuidGenerator.Create()}{ext}";
        var physicalPath = Path.Combine(directory, fileName);
        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream);
        }

        entity.SetProfileImageFileName(fileName);
        await Repository.UpdateAsync(entity, autoSave: true);

        var dto = MapToGetOutputDto(entity);
        dto.ProfileImageUrl = ResolveStudentImageDisplayUrl(entity.ProfileImageFileName, tenantName);
        return dto;
    }

    protected override async Task<Student> MapToEntityAsync(CreateUpdateStudentDto createInput)
    {
        await ValidateReferencesAsync(createInput.NurseryBranchId, createInput.NurseryClassId);

        return new Student(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            createInput.NurseryBranchId,
            createInput.FullName,
            createInput.BirthDate,
            createInput.Gender,
            createInput.BloodType,
            createInput.Religion,
            createInput.HomeAddress,
            createInput.FatherName,
            createInput.FatherIdentityNumber,
            createInput.FatherPhoneNumber,
            createInput.MotherName,
            createInput.MotherIdentityNumber,
            createInput.MotherPhoneNumber,
            createInput.EmergencyContactNumber,
            createInput.EnrollmentDate,
            createInput.HealthNotes,
            createInput.DietaryRestrictions,
            createInput.ToiletTrainingStatus,
            createInput.AttendsSunday,
            createInput.AttendsMonday,
            createInput.AttendsTuesday,
            createInput.AttendsWednesday,
            createInput.AttendsThursday,
            createInput.AttendsFriday,
            createInput.AttendsSaturday,
            createInput.MedicalNotes,
            profileImageFileName: null,
            createInput.IsActive,
            createInput.NurseryClassId);
    }

    protected override async Task MapToEntityAsync(CreateUpdateStudentDto updateInput, Student entity)
    {
        await ValidateReferencesAsync(updateInput.NurseryBranchId, updateInput.NurseryClassId);

        entity.SetClass(updateInput.NurseryClassId);
        entity.SetFullName(updateInput.FullName);
        entity.SetBirthDate(updateInput.BirthDate);
        entity.SetGender(updateInput.Gender);
        entity.SetBloodType(updateInput.BloodType);
        entity.SetReligion(updateInput.Religion);
        entity.SetHomeAddress(updateInput.HomeAddress);
        entity.SetFatherInfo(updateInput.FatherName, updateInput.FatherIdentityNumber, updateInput.FatherPhoneNumber);
        entity.SetMotherInfo(updateInput.MotherName, updateInput.MotherIdentityNumber, updateInput.MotherPhoneNumber);
        entity.SetEmergencyContactNumber(updateInput.EmergencyContactNumber);
        entity.SetEnrollmentDate(updateInput.EnrollmentDate);
        entity.SetHealthNotes(updateInput.HealthNotes);
        entity.SetDietaryRestrictions(updateInput.DietaryRestrictions);
        entity.SetToiletTrainingStatus(updateInput.ToiletTrainingStatus);
        entity.SetAttendingDays(
            updateInput.AttendsSunday,
            updateInput.AttendsMonday,
            updateInput.AttendsTuesday,
            updateInput.AttendsWednesday,
            updateInput.AttendsThursday,
            updateInput.AttendsFriday,
            updateInput.AttendsSaturday);
        entity.SetMedicalNotes(updateInput.MedicalNotes);
        entity.SetIsActive(updateInput.IsActive);

        await Task.CompletedTask;
    }

    private async Task ValidateReferencesAsync(Guid nurseryBranchId, Guid? nurseryClassId)
    {
        if (!await _nurseryBranchRepository.AnyAsync(x => x.Id == nurseryBranchId))
        {
            throw new BusinessException("NurseryHub:Student:BranchNotFound");
        }

        if (!nurseryClassId.HasValue)
        {
            return;
        }

        var nurseryClass = await _nurseryClassRepository.FindAsync(nurseryClassId.Value);
        if (nurseryClass == null || nurseryClass.NurseryBranchId != nurseryBranchId)
        {
            throw new BusinessException("NurseryHub:Student:ClassInvalidForBranch");
        }
    }

    protected override StudentDto MapToGetOutputDto(Student entity)
    {
        return new StudentDto
        {
            Id = entity.Id,
            NurseryBranchId = entity.NurseryBranchId,
            NurseryClassId = entity.NurseryClassId,
            FullName = entity.FullName,
            BirthDate = entity.BirthDate,
            Gender = entity.Gender,
            BloodType = entity.BloodType,
            Religion = entity.Religion,
            HomeAddress = entity.HomeAddress,
            FatherName = entity.FatherName,
            FatherIdentityNumber = entity.FatherIdentityNumber,
            FatherPhoneNumber = entity.FatherPhoneNumber,
            MotherName = entity.MotherName,
            MotherIdentityNumber = entity.MotherIdentityNumber,
            MotherPhoneNumber = entity.MotherPhoneNumber,
            EmergencyContactNumber = entity.EmergencyContactNumber,
            EnrollmentDate = entity.EnrollmentDate,
            HealthNotes = entity.HealthNotes,
            DietaryRestrictions = entity.DietaryRestrictions,
            ToiletTrainingStatus = entity.ToiletTrainingStatus,
            AttendsSunday = entity.AttendsSunday,
            AttendsMonday = entity.AttendsMonday,
            AttendsTuesday = entity.AttendsTuesday,
            AttendsWednesday = entity.AttendsWednesday,
            AttendsThursday = entity.AttendsThursday,
            AttendsFriday = entity.AttendsFriday,
            AttendsSaturday = entity.AttendsSaturday,
            MedicalNotes = entity.MedicalNotes,
            ProfileImageFileName = entity.ProfileImageFileName,
            ProfileImageUrl = ResolveStudentImageDisplayUrl(entity.ProfileImageFileName, CurrentTenant.Name),
            IsActive = entity.IsActive,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime,
        };
    }

    private string GetStudentDirectoryPath(string? tenantName)
    {
        var safeTenantName = string.IsNullOrWhiteSpace(tenantName) ? "host" : tenantName;
        return Path.Combine(_mediaOptions.MediaRootPath, safeTenantName, "students");
    }

    private string? ResolveStudentImageDisplayUrl(string? fileName, string? tenantName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var safeTenantName = string.IsNullOrWhiteSpace(tenantName) ? "host" : tenantName;
        var baseUrl = _mediaOptions.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{safeTenantName}/students/{fileName}";
    }

    private void DeleteStoredStudentImageIfExists(string? tenantName, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var directory = GetStudentDirectoryPath(tenantName);
        var path = Path.Combine(directory, Path.GetFileName(fileName));
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private async Task<string?> GetTenantNameAsync(Guid? tenantId)
    {
        if (!tenantId.HasValue)
        {
            return CurrentTenant.Name;
        }

        var tenant = await _tenantRepository.FindAsync(tenantId.Value);
        return tenant?.Name ?? CurrentTenant.Name;
    }
}
